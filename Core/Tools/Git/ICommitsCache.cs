namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public interface ICommitsCache
{
    bool TryGet(CommitId commitId, out Commit commit);
    bool TryGet(string commitSha, out Commit commit1);
    void Add(params Commit[] commits);
    void Add(IReadOnlyList<Commit> commits);
    Commit Get(CommitId commitId);
    Commit Get(string commitSha);
}