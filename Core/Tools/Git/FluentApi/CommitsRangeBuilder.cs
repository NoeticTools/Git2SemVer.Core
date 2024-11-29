namespace NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;

#pragma warning disable CS1591
public class CommitsRangeBuilder : ICommitsRangeBuilder
{
    private readonly List<string> _commitRangeShas = [];

    public ICommitsRangeBuilder ExcludingReachableFrom(CommitId commitId, bool includeCommit = false)
    {
        return ExcludingReachableFrom(commitId.Sha, includeCommit);
    }

    public ICommitsRangeBuilder ExcludingReachableFrom(string commitSha, bool includeCommit = false)
    {
        _commitRangeShas.Add($"\"^{commitSha}{(includeCommit ? "^@" : "")}\"");
        return this;
    }

    public ICommitsRangeBuilder ExcludingReachableFrom(CommitId[] commitIds, bool includeCommit = false)
    {
        return ExcludingReachableFrom(commitIds.Select(x => x.Sha).ToArray(), includeCommit);
    }

    public ICommitsRangeBuilder ExcludingReachableFrom(string[] commitShas, bool includeCommit = false)
    {
        foreach (var sha in commitShas)
        {
            ExcludingReachableFrom(sha, includeCommit);
        }

        return this;
    }

    public ICommitsRangeBuilder ReachableFrom(CommitId commitId)
    {
        return ReachableFrom(commitId.Sha);
    }

    public ICommitsRangeBuilder ReachableFrom(string commitSha)
    {
        _commitRangeShas.Add(commitSha);
        return this;
    }

    public ICommitsRangeBuilder ReachableFromHead()
    {
        _commitRangeShas.Add("HEAD");
        return this;
    }

    internal string ToArgs()
    {
        return _commitRangeShas.Aggregate("", (current, sha) => current + $" {sha}");
    }
}