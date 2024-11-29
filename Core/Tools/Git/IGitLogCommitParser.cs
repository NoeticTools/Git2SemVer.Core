namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public interface IGitLogCommitParser
{
    /// <summary>
    ///     Pars.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    Commit? Parse(string line);

    /// <summary>
    ///     The format arguments for the git log command to use like: `git log &lt;Format>`.
    /// </summary>
    string FormatArgs { get; }

    char RecordSeparator { get; }

    string ParseToLogLineWithGraph(string line);
}