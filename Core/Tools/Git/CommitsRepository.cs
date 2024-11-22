using NoeticTools.Common.Exceptions;


namespace NoeticTools.Common.Tools.Git;

#pragma warning disable CS1591
public sealed class CommitsRepository : ICommitsRepository
{
    private const int GitCommitsReadMaxCount = 200;
    private readonly Dictionary<CommitId, Commit> _commits;
    private readonly IGitTool _gitTool;

    public CommitsRepository(IGitTool gitTool)
    {
        var commits = gitTool.GetCommits(0, GitCommitsReadMaxCount);
        if (commits.Count == 0)
        {
            throw new Git2SemVerGitOperationException("Unable to get commits. Either new repository and no commits or problem accessing git.");
        }

        Head = commits[0];
        _commits = commits.ToDictionary(k => k.CommitId, v => v);
        _gitTool = gitTool;
    }

    public Commit Head { get; }

    public Commit Get(CommitId commitId)
    {
        while (true)
        {
            if (_commits.TryGetValue(commitId, out var commit))
            {
                return commit;
            }

            var commits = _gitTool.GetCommits(_commits.Count, GitCommitsReadMaxCount);
            if (commits.Count == 0)
            {
                throw new Git2SemVerRepositoryException("Unable to read further git commits.");
            }

            foreach (var readCommit in commits)
            {
                _commits.Add(readCommit.CommitId, readCommit);
            }
        }
    }
}