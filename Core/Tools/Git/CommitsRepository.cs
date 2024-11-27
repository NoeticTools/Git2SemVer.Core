using NoeticTools.Git2SemVer.Core.Exceptions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
// todo - merge this class into GitTool.
public sealed class CommitsRepository : ICommitsRepository
{
    private readonly Dictionary<string, Commit> _commitsBySha = [];

    public Commit Get(CommitId commitId)
    {
        return Get(commitId.Id);
    }

    public Commit Get(string commitSha)
    {
        if (!TryGet(commitSha, out var commit))
        {
            throw new Git2SemVerRepositoryException($"Commit {commitSha} not found in the repository. Did you mean to use 'TryGet'?");
        }
        return commit;
    }

    public bool TryGet(CommitId commitId, out Commit commit)
    {
        return TryGet(commitId.Id, out commit);
    }

    public bool TryGet(string commitSha, out Commit commit1)
    {
        return _commitsBySha.TryGetValue(commitSha, out commit1);
    }

    public void Add(params Commit[] commits)
    {
        foreach (var commit in commits)
        {
            if (!_commitsBySha.ContainsKey(commit.CommitId.Id))
            {
                _commitsBySha.Add(commit.CommitId.Id, commit);
            }
        }
    }

    public void Add(IReadOnlyList<Commit> commits)
    {
        Add(commits.ToArray());
    }
}