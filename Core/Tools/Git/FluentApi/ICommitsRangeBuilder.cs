namespace NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;

public interface ICommitsRangeBuilder
{
    ICommitsRangeBuilder ReachableFrom(CommitId commitId);
    ICommitsRangeBuilder ReachableFrom(string commitSha);
    ICommitsRangeBuilder ExcludingReachableFrom(CommitId commitId, bool includeCommit = false);
    ICommitsRangeBuilder ExcludingReachableFrom(string commitSha, bool includeCommit = false);
    ICommitsRangeBuilder ExcludingReachableFrom(CommitId[] commitIds, bool includeCommit = false);
    ICommitsRangeBuilder ExcludingReachableFrom(string[] commitShas, bool includeCommit = false);
    ICommitsRangeBuilder ReachableFromHead();
}