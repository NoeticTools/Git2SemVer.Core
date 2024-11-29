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
    private const int NextSetReadMaxCount = 300;
    public const char RecordSeparator = CharacterConstants.RS;
    private readonly SemVersion _assumedLowestGitVersion = new(2, 0, 0); // Tested with 2.41.0. Do not expect compatibility below 2.0.0.
    private readonly ICommitsRepository _cache;
    private readonly IGitLogCommitParser _commitLogParser;
    private readonly IGitProcessCli _inner;
    private readonly ILogger _logger;
    private int _commitsReadCountFromHead;

    public GitTool(ILogger logger)
        : this(new CommitsRepository(), logger)
    {
    }

    public GitTool(ICommitsRepository cache, ILogger logger)
        : this(cache, new GitProcessCli(logger), logger)
    {
    }

    public GitTool(ICommitsRepository cache, IGitProcessCli inner, ILogger logger)
        : this(cache, inner, new GitLogCommitParser(cache, new ConventionalCommitsParser()), logger)
    {
    }

    public GitTool(ICommitsRepository cache, IGitProcessCli inner, IGitLogCommitParser commitLogParser, ILogger logger)
    {
        _cache = cache;
        _inner = inner;
        _commitLogParser = commitLogParser;
        _logger = logger;

        var gitVersion = GetVersion();
        if (gitVersion != null &&
            gitVersion.ComparePrecedenceTo(_assumedLowestGitVersion) < 0)
        {
            _logger.LogError($"Git must be version {_assumedLowestGitVersion} or later.");
        }

        var commits = GetCommits();
        if (commits.Count == 0)
        {
            throw new Git2SemVerGitOperationException("Unable to get commits. Either new repository and no commits or problem accessing git.");
        }

        Head = commits[0];
        cache.Add(commits.ToArray());

        BranchName = GetBranchName();
        HasLocalChanges = GetHasLocalChanges();
    }

    public string BranchName { get; }

    public bool HasLocalChanges { get; }

    public Commit Head { get; }

    public Commit Get(CommitId commitId)
    {
        return Get(commitId.Id);
    }

    public Commit Get(string commitSha)
    {
        if (_cache.TryGet(commitSha, out var existingCommit))
        {
            return existingCommit;
        }

        var commits = GetCommits(commitSha);
        if (commits.Count == 0)
        {
            throw new Git2SemVerRepositoryException($"Unable to find git commit '{commitSha}' in the repository.");
        }

        _cache.Add(commits);
        return _cache.Get(commitSha);
    }

    public IReadOnlyList<Commit> GetCommits(string commitSha, int? maxCount = null)
    {
        maxCount ??= NextSetReadMaxCount;
        var commits = GetCommitsFromGitLog($"{commitSha}  --max-count={maxCount}");
        _logger.LogTrace($"Read {commits.Count} commits from git history starting at '{commitSha}'.");
        return commits;
    }

    public IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount)
    {
        var commits = GetCommitsFromGitLog($"--skip={skipCount}  --max-count={takeCount}");
        _logger.LogTrace($"Read {commits.Count} commits from git history. Skipped {skipCount}.");
        return commits;
    }

    public IReadOnlyList<Commit> GetCommitsInRange(CommitId head, params CommitId[] startingCommits)
    {
        return GetCommitsInRange(head.Id, startingCommits.Select(x => x.Id).ToArray());
    }

    public IReadOnlyList<Commit> GetCommitsInRange(string head, params string[] startingCommits)
    {
        var commitsRange = $"{head}";
        foreach (var startingCommit in startingCommits)
        {
            commitsRange += $" \"^{startingCommit}^@\"";
        }
        var arguments = $"log {commitsRange} --pretty=\"format:%H\"";
        var stdOutput = Run(arguments);
        return GetCommitsFromCommitShaList(stdOutput);
    }

    public IReadOnlyList<Commit> GetContributingCommits(CommitId head, CommitId prior)
    {
        var arguments = $"log {head.Id} \"^{prior.Id}\" --pretty=\"format:%H\"";
        var stdOutput = Run(arguments);
        return GetCommitsFromCommitShaList(stdOutput);
    }

    private IReadOnlyList<Commit> GetCommitsFromCommitShaList(string stdOutput)
    {
        if (stdOutput.Length == 0)
        {
            return [];
        }

        var lines = stdOutput.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0);
        return lines.Select(hash => Get(hash!.Trim())).ToList();
    }

    public string Run(string arguments)
    {
        var outWriter = new StringWriter();
        var errorWriter = new StringWriter();

        var returnCode = _inner.Run(arguments, outWriter, errorWriter);

        if (returnCode != 0)
        {
            throw new Git2SemVerGitOperationException($"Command 'git {arguments}' returned non-zero return code: {returnCode}");
        }

        var errorOutput = errorWriter.ToString();
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            _logger.LogError($"Git command '{arguments}' returned error: {errorOutput}");
        }

        return outWriter.ToString();
    }

    private string GetBranchName()
    {
        var stdOutput = Run("status -b -s --porcelain");

        return ParseStatusResponseBranchName(stdOutput);
    }

    /// <summary>
    ///     Get next set of commits from head.
    /// </summary>
    private IReadOnlyList<Commit> GetCommits()
    {
        var commits = GetCommits(_commitsReadCountFromHead, NextSetReadMaxCount);
        _commitsReadCountFromHead += commits.Count;
        return commits;
    }

    private IReadOnlyList<Commit> GetCommitsFromGitLog(string scopeArguments = "")
    {
        var stdOutput = Run($"log {_commitLogParser.FormatArgs} {scopeArguments}");
        var lines = stdOutput.Split(RecordSeparator);
        var commits = lines.Select(line => _commitLogParser.Parse(line)).OfType<Commit>().ToList();
        _logger.LogTrace("Read {0} commits from git history.", commits.Count);
        return commits;
    }

    private bool GetHasLocalChanges()
    {
        var stdOutput = Run("status -u -s --porcelain");
        return stdOutput.Length > 0;
    }

    /// <summary>
    ///     Get a semantic version representation of the Git version.
    /// </summary>
    private SemVersion? GetVersion()
    {
        var process = new ProcessCli(_logger);
        var (returnCode, response) = process.Run("git", "--version");
        if (returnCode != 0)
        {
            _logger.LogError($"Unable to read git version. Return code was '{returnCode}'. Git may not be executable from current directory.");
        }

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

    internal static string ParseStatusResponseBranchName(string stdOutput)
    {
        var regex = new Regex(@"^## (?<branchName>[a-zA-Z0-9!$*\._\/-]+?)(\.\.\..*)?\s*?$", RegexOptions.Multiline);
        var match = regex.Match(stdOutput);

        if (!match.Success)
        {
            throw new Git2SemVerGitOperationException($"Unable to read branch name from Git status response '{stdOutput}'.\n");
        }

        return match.Groups["branchName"].Value;
    }
}