using NoeticTools.Git2SemVer.Core.Exceptions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
// todo - merge this class into GitTool.
public sealed class CommitsRepository : ICommitsRepository
{
    private const int GitCommitsReadMaxCount = 200;
    private readonly Dictionary<string, Commit> _commits;
    private readonly IGitTool _gitTool;

    public CommitsRepository(IGitTool gitTool)
    {
        var commits = gitTool.GetCommits(0, GitCommitsReadMaxCount);
        if (commits.Count == 0)
        {
            throw new Git2SemVerGitOperationException("Unable to get commits. Either new repository and no commits or problem accessing git.");
        }

        Head = commits[0];
        _commits = commits.ToDictionary(k => k.CommitId.Id, v => v);
        _gitTool = gitTool;
    }

    public Commit Head { get; }

    public Commit Get(CommitId commitId)
    {
        return Get(commitId.Id);
    }

    public Commit Get(string commitSha)
    {
        while (true)
        {
            if (_commits.TryGetValue(commitSha, out var commit))
            {
                return commit;
            }

            var commits = _gitTool.GetCommits(_commits.Count, GitCommitsReadMaxCount);
            if (commits.Count == 0)
            {
                throw new Git2SemVerRepositoryException("Unable to read further git commits.");
            }

            foreach (var readCommit in commits) _commits.Add(readCommit.CommitId.Id, readCommit);
        }
    }

    /// <summary>
    ///     Get all commits contributing to code at a commit after a prior commit.
    /// </summary>
    public IReadOnlyList<Commit> GetContributingCommits(CommitId after, CommitId to)
    {
        var arguments = $"log {after.Id}..{to.Id} --pretty=\"format:%H\"";
        var result = _gitTool.Run(arguments);
        if (result.returnCode != 0)
        {
            throw new Git2SemVerGitOperationException($"Command 'git {arguments}' returned non zero return code {result.returnCode}.");
        }

        if (result.stdOutput.Length == 0)
        {
            return [];
        }

        var lines = result.stdOutput.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0);
        return lines.Select(hash => Get(hash!.Trim())).ToList();
    }
}