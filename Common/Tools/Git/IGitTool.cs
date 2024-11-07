namespace NoeticTools.Common.Tools.Git;

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

    /// <summary>
    ///     Get commits working from head downwards (to older commits).
    /// </summary>
    IReadOnlyList<Commit> GetCommits(int skipCount, int takeCount);

    /// <summary>
    ///     Invoke Git with given arguments.
    /// </summary>
    (int returnCode, string stdOutput) Run(string arguments);
}