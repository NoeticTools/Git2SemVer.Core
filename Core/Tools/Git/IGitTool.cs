namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public interface IGitTool
{
    /// <summary>
    ///     The current head's branch name.
    /// </summary>

    string BranchName { get; }

    /// <summary>
    ///     True if there are uncommited local changes.
    /// </summary>
    bool HasLocalChanges { get; }

    ///// <summary>
    /////     Get commits working from head downwards (to older commits).
    ///// </summary>
    //IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount);

    /// <summary>
    ///     Invoke Git with given arguments.
    /// </summary>
    string Run(string arguments);

    Commit Head { get; }

    Commit Get(CommitId commitId);

    Commit Get(string commitSha);

    /// <summary>
    ///     Get all commits contributing to code at a commit after a prior commit.
    /// </summary>
    IReadOnlyList<Commit> GetContributingCommits(CommitId head, CommitId prior);

    /// <summary>
    ///     Get commits starting with given commit SHA and then parent (older) commits.
    /// </summary>
    IReadOnlyList<Commit> GetCommits(string commitSha, int? maxCount=null);

    /// <summary>
    ///     Get commits working from head downwards (to older commits).
    /// </summary>
    IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount);

    /// <summary>
    ///     Get commits reachable from head commit but not reachable from any given starting commit.
    ///     Inclusive, both the head commit and each starting commit are included (if reachable from the head commit).
    /// </summary>
    IReadOnlyList<Commit> GetCommitsInRange(CommitId head, params CommitId[] startingCommits);

    /// <summary>
    ///     Get commits reachable from head commit but not reachable from any given starting commit.
    ///     Inclusive, both the head commit and each starting commit are included (if reachable from the head commit).
    /// </summary>
    IReadOnlyList<Commit> GetCommitsInRange(string head, params string[] startingCommits);
}