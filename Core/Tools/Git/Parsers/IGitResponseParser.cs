using Semver;


namespace NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

public interface IGitResponseParser : IGitLogResponseParser
{
    /// <summary>
    ///     The format arguments for the git log command to use like: `git log &lt;Format>`.
    /// </summary>
    string FormatArgs { get; }

    char RecordSeparator { get; }

    SemVersion? ParseGitVersionResponse(string response);
    string ParseStatusResponseBranchName(string stdOutput);
}