namespace NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;

/// <summary>
///     Build git <see href="https://git-scm.com/docs/gitrevisions">revision range arguments</see>.
/// </summary>
public interface IGitRevisionsBuilder
{
    IGitRevisionsBuilder ReachableFrom(CommitId commitId);
    IGitRevisionsBuilder ReachableFrom(string commitSha);
    IGitRevisionsBuilder NotReachableFrom(CommitId commitId, bool inclusive = false);
    IGitRevisionsBuilder NotReachableFrom(string commitSha, bool inclusive = false);
    IGitRevisionsBuilder NotReachableFrom(CommitId[] commitIds, bool inclusive = false);
    IGitRevisionsBuilder NotReachableFrom(string[] commitShas, bool inclusive = false);
    IGitRevisionsBuilder ReachableFromHead();
    IGitRevisionsBuilder Skip(int skipCount);
    IGitRevisionsBuilder Take(int takeCount);
    IGitRevisionsBuilder ReachableFrom(CommitId[] commitIds);
    IGitRevisionsBuilder ReachableFrom(string[] commitShas);
}