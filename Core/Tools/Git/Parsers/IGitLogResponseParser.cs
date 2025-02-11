namespace NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

public interface IGitLogResponseParser
{
    /// <summary>
    ///     Pars.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    Commit? ParseGitLogLine(string line);
}