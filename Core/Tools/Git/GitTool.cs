using System.Text.RegularExpressions;
using Injectio.Attributes;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;
using NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;
using Semver;


#pragma warning disable SYSLIB1045
#pragma warning disable CS1591

namespace NoeticTools.Git2SemVer.Core.Tools.Git;

[RegisterTransient]
public class GitTool : IGitTool
{
    private const int DefaultTakeLimit = 300;
    //private readonly SemVersion _assumedLowestGitVersion = new(2, 34, 1);
    private readonly IGitResponseParser _gitResponseParser;
    private readonly IGitProcessCli _inner;
    private readonly ILogger _logger;
    private int _commitsReadCountFromHead;
    private Commit? _head;
    private readonly ICommitsCache? _cache;
    private bool _initialised = false;

    public GitTool(ILogger logger)
        : this(new CommitsCache(), new GitProcessCli(logger), logger)
    {
    }

    //public GitTool(ICommitsCache cache, ILogger logger)
    //    : this(cache, new GitProcessCli(logger), logger)
    //{
    //}

    public GitTool(ICommitsCache cache, IGitProcessCli inner, ILogger logger)
    //    : this(cache, inner, new GitResponseParser(cache, new ConventionalCommitsParser(), logger), logger)
    //{
    //}

    //public GitTool(ICommitsCache cache, IGitProcessCli inner, IGitResponseParser gitResponseParser, ILogger logger)
    {
        _cache = cache;
        _inner = inner;
        _gitResponseParser = new GitResponseParser(cache, new ConventionalCommitsParser(), logger);
        _logger = logger;

        //var gitVersion = GetVersion();
        //if (gitVersion != null &&
        //    gitVersion.ComparePrecedenceTo(_assumedLowestGitVersion) < 0)
        //{
        //    _logger.LogError($"Git must be version {_assumedLowestGitVersion} or later.");
        //}

        //Initialise();
    }

    private void Initialise()
    {
        if (_initialised)
        {
            return;
        }
        _initialised = true;

        var task = Task.Run(GetCommitsAsync);
        //task.Wait();
        //if (task.Status != TaskStatus.RanToCompletion)
        //{
        //    _logger.LogError("Unable to initialise git tool.");
        //    return;
        //}

        var commits = task.Result;
        if (commits.Count == 0)
        {
            throw new Git2SemVerGitOperationException("Unable to get commits. Either new repository and no commits or problem accessing git.");
        }

        Head = commits[0];
        Cache.Add(commits.ToArray());
    }

    public string BranchName => GetBranchName();

    public ICommitsCache Cache
    {
        get
        {
            if (!_initialised)
            {
                Initialise();
            }
            return _cache!;
        }
    }

    public bool HasLocalChanges => GetHasLocalChanges();

    public Commit Head
    {
        get
        {
            if (!_initialised)
            {
                Initialise();
            }
            return _head!;
        }
        private set => _head = value;
    }

    public Commit Get(CommitId commitId)
    {
        if (!_initialised)
        {
            Initialise();
        }

        return Get(commitId.Sha);
    }

    public Commit Get(string commitSha)
    {
        if (!_initialised)
        {
            Initialise();
        }

        if (Cache.TryGet(commitSha, out var existingCommit))
        {
            return existingCommit;
        }

        var commits = GetCommits(commitSha);
        if (commits.Count == 0)
        {
            throw new Git2SemVerRepositoryException($"Unable to find git commit '{commitSha}' in the repository.");
        }

        Cache.Add(commits);
        return Cache.Get(commitSha);
    }

    public IReadOnlyList<Commit> GetCommits(string commitSha, int? takeCount = null)
    {
        if (!_initialised)
        {
            Initialise();
        }

        return GetCommits(x => x.ReachableFrom(commitSha)
                                .Take(takeCount ?? DefaultTakeLimit));
    }

    public async Task<IReadOnlyList<Commit>> GetCommitsAsync(int skipCount, int takeCount)
    {
        return await GetCommitsAsync(x => x.ReachableFromHead()
                                .Skip(skipCount)
                                .Take(takeCount));
    }

    public IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount)
    {
        if (!_initialised)
        {
            Initialise();
        }

        return GetCommits(x => x.ReachableFromHead()
                                .Skip(skipCount)
                                .Take(takeCount));
    }

    public async Task<IReadOnlyList<Commit>> GetCommitsAsync(Action<IGitRevisionsBuilder> rangeBuilderAction)
    {
        var rangeBuilder = new GitRevisionsBuilder();
        rangeBuilderAction(rangeBuilder);
        return await GetCommitsFromGitLogAsync(rangeBuilder.GetArgs());
    }

    public IReadOnlyList<Commit> GetCommits(Action<IGitRevisionsBuilder> rangeBuilderAction)
    {
        if (!_initialised)
        {
            Initialise();
        }

        var rangeBuilder = new GitRevisionsBuilder();
        rangeBuilderAction(rangeBuilder);
        return GetCommitsFromGitLog(rangeBuilder.GetArgs());
    }

    public IReadOnlyList<Commit> GetContributingCommits(CommitId head, CommitId prior)
    {
        if (!_initialised)
        {
            Initialise();
        }

        return GetCommits(x => x.ReachableFrom(head)
                                .NotReachableFrom(prior));
    }

    public async Task<string> RunAsync(string arguments)
    {
        var outWriter = new StringWriter();
        var errorWriter = new StringWriter();

        var returnCode = await _inner.RunAsync(arguments, outWriter, errorWriter);

        if (returnCode != 0)
        {
            throw new Git2SemVerGitOperationException($"Command 'git {arguments}' returned non-zero return code: {returnCode}");
        }

        var errorOutput = errorWriter.ToString();
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            _logger.LogError($"Git command '{arguments}' returned error: {errorOutput}");
        }

        var stdOutput = outWriter.ToString();
        if (string.IsNullOrWhiteSpace(stdOutput))
        {
            _logger.LogWarning("No response from git command.");
        }
        return stdOutput;
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

        var stdOutput = outWriter.ToString();
        if (string.IsNullOrWhiteSpace(stdOutput))
        {
            _logger.LogWarning("No response from git command.");
        }
        return stdOutput;
    }

    private async Task<string> GetBranchNameAsync()
    {
        var stdOutput = await RunAsync("status -b -s --porcelain");

        return _gitResponseParser.ParseStatusResponseBranchName(stdOutput);
    }

    private string GetBranchName()
    {
        var stdOutput = Run("status -b -s --porcelain");

        return _gitResponseParser.ParseStatusResponseBranchName(stdOutput);
    }

    /// <summary>
    ///     Get next set of commits from head.
    /// </summary>
    private async Task<IReadOnlyList<Commit>> GetCommitsAsync()
    {
        var commits = await GetCommitsAsync(_commitsReadCountFromHead, DefaultTakeLimit);
        _commitsReadCountFromHead += commits.Count;
        return commits;
    }

    /// <summary>
    ///     Get next set of commits from head.
    /// </summary>
    private IReadOnlyList<Commit> GetCommits()
    {
        var commits = GetCommits(_commitsReadCountFromHead, DefaultTakeLimit);
        _commitsReadCountFromHead += commits.Count;
        return commits;
    }

    private async Task<IReadOnlyList<Commit>> GetCommitsFromGitLogAsync(string scopeArguments = "", IGitResponseParser? customParser = null)
    {
        var parser = customParser ?? _gitResponseParser;
        var stdOutput = await RunAsync($"log {parser.FormatArgs} {scopeArguments}");
        var lines = stdOutput.Split(parser.RecordSeparator);
        var commits = lines.Select(line => parser.ParseGitLogLine(line)).OfType<Commit>().ToList();
        _logger.LogTrace("Read {0} commits from git history.", commits.Count);
        return commits;
    }

    private IReadOnlyList<Commit> GetCommitsFromGitLog(string scopeArguments = "", IGitResponseParser? customParser = null)
    {
        var parser = customParser ?? _gitResponseParser;
        var stdOutput = Run($"log {parser.FormatArgs} {scopeArguments}");
        var lines = stdOutput.Split(parser.RecordSeparator);
        var commits = lines.Select(line => parser.ParseGitLogLine(line)).OfType<Commit>().ToList();
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

        return _gitResponseParser.ParseGitVersionResponse(response);
    }

    /// <summary>
    ///     Get a semantic version representation of the Git version.
    /// </summary>
    private async Task<SemVersion?> GetVersionAsync()
    {
        var process = new ProcessCli(_logger);
        var (returnCode, response) = await process.RunAsync("git", "--version");
        if (returnCode != 0)
        {
            _logger.LogError($"Unable to read git version. Return code was '{returnCode}'. Git may not be executable from current directory.");
        }

        return _gitResponseParser.ParseGitVersionResponse(response);
    }
}