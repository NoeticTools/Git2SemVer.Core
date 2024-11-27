using System.Text.RegularExpressions;
using Injectio.Attributes;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;
using NoeticTools.Git2SemVer.Core.Logging;
using Semver;


#pragma warning disable SYSLIB1045

namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
[RegisterTransient]
public class GitTool : IGitTool
{
    private readonly ICommitsRepository _cache;

    private const string GitLogParsingPattern =
        """
        ^(?<graph>[^\x1f$]*) 
          (\x1f\.\|
            (?<sha>[^\|]+) \|
            (?<parents>[^\|]*)? \|
            \x02(?<summary>[^\x03]*)?\x03 \|
            \x02(?<body>[^\x03]*)?\x03 \|
            (\s\((?<refs>.*?)\))?
           \|$)?
        """;

    private const char RecordSeparator = CharacterConstants.RS;
    private readonly SemVersion _assumedLowestGitVersion = new(2, 0, 0); // Tested with 2.41.0. Do not expect compatibility below 2.0.0.
    private readonly ConventionalCommitsParser _conventionalCommitParser;
    private readonly string _gitLogFormat;
    private readonly IGitProcessCli _inner;
    private readonly ILogger _logger;
    private readonly ICommitObfuscator _obfuscator;
    private const int NextSetReadMaxCount = 300;
    private int _commitsReadCountFromHead;

    public GitTool(ILogger logger) : this(new CommitsRepository(), logger)
    {
    }

    public GitTool(ICommitsRepository cache, ILogger logger)
        : this(cache, new GitProcessCli(logger), logger)
    {

    }

    public GitTool(ICommitsRepository cache, IGitProcessCli inner, ILogger logger)
    {
        _gitLogFormat = "%x1f.|%H|%P|%x02%s%x03|%x02%b%x03|%d|%x1e";
        _cache = cache;
        _inner = inner;
        _logger = logger;
        _conventionalCommitParser = new ConventionalCommitsParser();
        _obfuscator = new CommitObfuscator();

        var gitVersion = GetVersion();
        if (gitVersion != null &&
            gitVersion.ComparePrecedenceTo(_assumedLowestGitVersion) < 0)
        {
            _logger.LogError($"Git must be version {_assumedLowestGitVersion} or later.");
        }

        var commits = GetNextSetOfCommits();
        if (commits.Count == 0)
        {
            throw new Git2SemVerGitOperationException("Unable to get commits. Either new repository and no commits or problem accessing git.");
        }

        Head = commits[0];
        cache.Add(commits.ToArray());

        BranchName = GetBranchName();
        HasLocalChanges = GetHasLocalChanges();
    }

    public Commit Head { get; }

    public Commit Get(CommitId commitId)
    {
        return Get(commitId.Id);
    }

    public Commit Get(string commitSha)
    {
        while (true)
        {
            if (_cache.TryGet(commitSha, out var existingCommit))
            {
                return existingCommit;
            }

            // todo - this assumes that commits are read in sequence
            var commits = GetNextSetOfCommits();
            if (commits.Count == 0)
            {
                throw new Git2SemVerRepositoryException("Unable to read further git commits.");
            }

            _cache.Add(commits);
        }
    }

    /// <summary>
    ///     Get all commits contributing to code at a commit after a prior commit.
    /// </summary>
    public IReadOnlyList<Commit> GetContributingCommits(CommitId after, CommitId to)
    {
        var arguments = $"log {after.Id}..{to.Id} --pretty=\"format:%H\"";
        var result = Run(arguments);
        if (result.returnCode != 0)
        {
            throw new Git2SemVerGitOperationException($"Command 'git {arguments}' returned non zero return code {result.returnCode}.");
        }

        if (result.stdOutput.Length == 0)
        {
            return [];
        }

        var lines = result.stdOutput.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0);
        return lines.Select(hash => Get(hash!.Trim())).ToList();
    }

    public string BranchName { get; }

    public bool HasLocalChanges { get; }

    public string WorkingDirectory
    {
        get => _inner.WorkingDirectory;
        set => _inner.WorkingDirectory = value;
    }

    public IReadOnlyList<Commit> GetNextSetOfCommits()
    {
        var commits = GetCommits(_commitsReadCountFromHead, NextSetReadMaxCount);
        _commitsReadCountFromHead += commits.Count;
        return commits;
    }

    /// <summary>
    ///     Get commits working from head downwards (to older commits).
    /// </summary>
    internal IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount)
    {
        var commits = GetCommitsFromGitLog($"--skip={skipCount}  --max-count={takeCount}");
        _logger.LogTrace($"Read {commits.Count} commits from git history. Skipped {skipCount}.");
        return commits;
    }

    internal IReadOnlyList<Commit> GetCommitsFromGitLog(string scopeArguments)
    {
        var commits = new List<Commit>();

        var result = Run($"log --graph --pretty=\"format:{_gitLogFormat}\" {scopeArguments}");

        var lines = result.stdOutput.Split(RecordSeparator);

        foreach (var line in lines)
        {
            ParseGitLogLine(line, commits);
        }

        _logger.LogTrace($"Read {commits.Count} commits from git history.");

        return commits;
    }

    public void ParseGitLogLine(string line, List<Commit> commits)
    {
        line = line.Trim();
        var regex = new Regex(GitLogParsingPattern, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        var match = regex.Match(line);
        if (!match.Success)
        {
            throw new Git2SemVerGitLogParsingException($"Unable to parse Git log line {line}.");
        }

        var graph = match.GetGroupValue("graph");
        var sha = match.GetGroupValue("sha");
        var refs = match.GetGroupValue("refs");
        var parents = match.GetGroupValue("parents").Split(' ');
        var summary = match.GetGroupValue("summary");
        var body = match.GetGroupValue("body");

        if (!_cache.TryGet(sha!, out var commit))
        {
            var hasCommitMetadata = line.Contains($"{CharacterConstants.US}.|");
            if (hasCommitMetadata)
            {
                if (sha.Length == 0)
                {
                    throw new Git2SemVerGitLogParsingException($"Unable to read SHA from line: '{line}'");
                }
            }

            var commitMetadata = _conventionalCommitParser.Parse(summary, body);

            commit = hasCommitMetadata
                ? new Commit(sha, parents, summary, body, refs, commitMetadata)
                : null;

            if (commit != null)
            {
                commits.Add(commit);
            }
        }
    }

    public static string ParseStatusResponseBranchName(string stdOutput)
    {
        var regex = new Regex(@"^## (?<branchName>[a-zA-Z0-9!$*\._\/-]+?)(\.\.\..*)?\s*?$", RegexOptions.Multiline);
        var match = regex.Match(stdOutput);

        if (!match.Success)
        {
            throw new Git2SemVerGitOperationException($"Unable to read branch name from Git status response '{stdOutput}'.\n");
        }

        return match.Groups["branchName"].Value;
    }

    public (int returnCode, string stdOutput) Run(string arguments)
    {
        var outWriter = new StringWriter();
        var errorWriter = new StringWriter();

        var returnCode = _inner.Run(arguments, outWriter, errorWriter);

        if (returnCode != 0)
        {
            throw new Git2SemVerGitOperationException($"Git command '{arguments}' returned non-zero return code: {returnCode}");
        }

        var errorOutput = errorWriter.ToString();
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            _logger.LogError($"Git command '{arguments}' returned error: {errorOutput}");
        }

        return (returnCode, outWriter.ToString());
    }

    private string GetBranchName()
    {
        var result = Run("status -b -s --porcelain");

        return ParseStatusResponseBranchName(result.stdOutput);
    }

    private bool GetHasLocalChanges()
    {
        var result = Run("status -u -s --porcelain");
        return result.stdOutput.Length > 0;
    }

    /// <summary>
    ///     Get a semantic version representation of the Git version.
    /// </summary>
    private SemVersion? GetVersion()
    {
        var process = new ProcessCli(_logger);
        var result = process.Run("git", "--version");
        if (result.returnCode != 0)
        {
            _logger.LogError($"Unable to read git version. Return code was '{result.returnCode}'. Git may not be executable from current directory.");
        }

        var response = result.stdOutput;
        try
        {
            var regex = new Regex(@"^git version (?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)\.?(?<metadata>.*?)?$");
            var match = regex.Match(response.Trim());
            if (!match.Success)
            {
                _logger.LogWarning($"Unable to parse git --version response: '{response}'.");
                return null;
            }

            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = int.Parse(match.Groups["patch"].Value);
            var version = new SemVersion(major, minor, patch);
            var metadata = match.Groups["metadata"].Value;
            version = version.WithMetadataParsedFrom(metadata);
            _logger.LogDebug("Git version (in Semver format) is '{0}'", version.ToString());
            return version;
        }
        catch (Exception exception)
        {
            _logger.LogWarning($"Unable to parse git --version response: '{response}'. Exception: {exception.Message}.");
            return null;
        }
    }
}