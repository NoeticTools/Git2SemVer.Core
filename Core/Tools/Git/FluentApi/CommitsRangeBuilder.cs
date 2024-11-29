namespace NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;

#pragma warning disable CS1591
public class CommitsRangeBuilder : ICommitsRangeBuilder
{
    private readonly List<string> _commitRangeShas = [];

    public CommitsRangeBuilder ReachableFrom(CommitId commitId)
    {
        return ReachableFrom(commitId.Sha);
    }

    public CommitsRangeBuilder ReachableFrom(string commitSha)
    {
        _commitRangeShas.Add(commitSha);
        return this;
    }

    public CommitsRangeBuilder ExcludingReachableFrom(CommitId commitId, bool includeCommit = false)
    {
        return ExcludingReachableFrom(commitId.Sha, includeCommit);
    }

    public CommitsRangeBuilder ExcludingReachableFrom(string commitSha, bool includeCommit = false)
    {
        _commitRangeShas.Add($"\"^{commitSha}{(includeCommit ? "^@" : "")}\"");
        return this;
    }

    internal string ToArgs()
    {
        return _commitRangeShas.Aggregate("", (current, sha) => current + $" {sha}");
    }
}