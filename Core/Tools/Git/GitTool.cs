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
    private readonly SemVersion _assumedLowestGitVersion = new(2, 34, 1);
    private readonly ICommitsCache? _cache;
    private readonly IGitResponseParser _gitResponseParser;
    private readonly IGitProcessCli _inner;
    private readonly ILogger _logger;
    private int _commitsReadCountFromHead;
    private Commit? _head;
    private bool _initialised;

    public GitTool(ILogger logger)
    {
        _cache = new CommitsCache();
        _inner = new GitProcessCli(logger);
        _gitResponseParser = new GitResponseParser(_cache, new ConventionalCommitsParser(), logger);
        _logger = logger;
    }

    public string WorkingDirectory
    {
        get => _inner.WorkingDirectory;
        set => _inner.WorkingDirectory = value;
    }

    public string BranchName => GetBranchName();

    public ICommitsCache Cache
    {
        get
        {
            if (_cache == null)
            {
                PrimeCache();
            }

            return _cache!;
        }
    }

    public bool HasLocalChanges => GetHasLocalChanges();

    public Commit Head
    {
        get
        {
            if (_head == null)
            {
                PrimeCache();
            }

            return _head!;
        }
        private set => _head = value;
    }

    public Commit Get(CommitId commitId)
    {
        return Get(commitId.Sha);
    }

    public Commit Get(string commitSha)
    {
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
        return GetCommits(x => x.ReachableFrom(commitSha)
                                .Take(takeCount ?? DefaultTakeLimit));
    }

    public IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount)
    {
        var commits = GetCommits(x => x.ReachableFromHead()
                                .Skip(skipCount)
                                .Take(takeCount));
        if (skipCount == 0)
        {
            _head = commits[0];
        }

        return commits;
    }

    public IReadOnlyList<Commit> GetCommits(Action<IGitRevisionsBuilder> rangeBuilderAction)
    {
        var rangeBuilder = new GitRevisionsBuilder();
        rangeBuilderAction(rangeBuilder);
        return GetCommitsFromGitLog(rangeBuilder.GetArgs());
    }

    public async Task<IReadOnlyList<Commit>> GetCommitsAsync(string commitSha, int? takeCount = null)
    {
        return await GetCommitsAsync(x => x.ReachableFrom(commitSha)
                                           .Take(takeCount ?? DefaultTakeLimit));
    }

    public async Task<IReadOnlyList<Commit>> GetCommitsAsync(int skipCount, int takeCount)
    {
        var commits = await GetCommitsAsync(x => x.ReachableFromHead()
                                           .Skip(skipCount)
                                           .Take(takeCount));
        if (skipCount == 0)
        {
            _head = commits[0];
        }

        return commits;
    }

    public async Task<IReadOnlyList<Commit>> GetCommitsAsync(Action<IGitRevisionsBuilder> rangeBuilderAction)
    {
        var rangeBuilder = new GitRevisionsBuilder();
        rangeBuilderAction(rangeBuilder);
        return await GetCommitsFromGitLogAsync(rangeBuilder.GetArgs());
    }

    public IReadOnlyList<Commit> GetContributingCommits(CommitId commit, CommitId prior)
    {
        return GetCommits(x => x.ReachableFrom(commit)
                                .NotReachableFrom(prior));
    }

    public async Task<IReadOnlyList<Commit>> GetContributingCommitsAsync(CommitId commit, CommitId prior)
    {
        return await GetCommitsAsync(x => x.ReachableFrom(commit)
                                           .NotReachableFrom(prior));
    }

    public string Run(string arguments)
    {
        return Task.Run(() => RunAsync(arguments)).Result;
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

    private string GetBranchName()
    {
        return Task.Run(GetBranchNameAsync).Result;
    }

    private async Task<string> GetBranchNameAsync()
    {
        var stdOutput = await RunAsync("status -b -s --porcelain");
        return _gitResponseParser.ParseStatusResponseBranchName(stdOutput);
    }

    /// <summary>
    ///     Get next set of commits from head.
    /// </summary>
    private async Task<IReadOnlyList<Commit>> GetCommitsAsync()
    {
        var commits = await GetCommitsAsync(_commitsReadCountFromHead, DefaultTakeLimit);
        if (_commitsReadCountFromHead == 0)
        {
            if (commits.Count == 0)
            {
                throw new Git2SemVerGitOperationException("Unable to get commits. Either new repository and no commits or problem accessing git.");
            }
            _head = commits[0];
        }
        _commitsReadCountFromHead += commits.Count;
        return commits;
    }

    private IReadOnlyList<Commit> GetCommitsFromGitLog(string scopeArguments = "", IGitResponseParser? customParser = null)
    {
        return Task.Run(() => GetCommitsFromGitLogAsync(scopeArguments, customParser)).Result;
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

    private bool GetHasLocalChanges()
    {
        return Task.Run(GetHasLocalChangesAsync).Result;
    }

    private async Task<bool> GetHasLocalChangesAsync()
    {
        var stdOutput = await RunAsync("status -u -s --porcelain");
        return stdOutput.Length > 0;
    }

    /// <summary>
    ///     Get a semantic version representation of the Git version.
    /// </summary>
    private SemVersion? GetVersion()
    {
        return Task.Run(GetVersionAsync).Result;
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

    private void PrimeCache()
    {
        Task.Run(PrimeCacheAsync).Wait();
    }

    private async Task PrimeCacheAsync()
    {
        if (_initialised)
        {
            return;
        }

        _initialised = true;

        var gitVersion = await GetVersionAsync();
        if (gitVersion != null &&
            gitVersion.ComparePrecedenceTo(_assumedLowestGitVersion) < 0)
        {
            _logger.LogError($"Git version {_assumedLowestGitVersion} or later is required.");
        }

        var commits = await GetCommitsAsync();
        Cache.Add(commits.ToArray());
    }
}