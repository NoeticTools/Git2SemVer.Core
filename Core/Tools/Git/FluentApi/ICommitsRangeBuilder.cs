namespace NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;

public interface ICommitsRangeBuilder
{
    CommitsRangeBuilder ReachableFrom(CommitId commitId);
    CommitsRangeBuilder ReachableFrom(string commitSha);
    CommitsRangeBuilder ExcludingReachableFrom(CommitId commitId, bool includeCommit = false);
    CommitsRangeBuilder ExcludingReachableFrom(string commitSha, bool includeCommit = false);
}