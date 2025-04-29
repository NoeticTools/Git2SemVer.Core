using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;
using NoeticTools.Git2SemVer.Core.Logging;
using Semver;
using System.Text.RegularExpressions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

#pragma warning disable CS1591
public class GitResponseParser : GitLogCommitParserBase, IGitResponseParser
{
    private readonly ILogger _logger;

    public GitResponseParser(ICommitsCache cache, IConventionalCommitsParser conventionalCommitParser, ILogger logger)
        : base(cache, conventionalCommitParser)
    {
        _logger = logger;
    }

    public Commit? ParseGitLogLine(string line)
    {
        return ParseCommitAndGraph(line).commit;
    }

    public string ParseStatusResponseBranchName(string stdOutput)
    {
        var regex = new Regex(@"^## (?<branchName>[a-zA-Z0-9!$*\._\/-]+?)(\.\.\..*)?\s*?$", RegexOptions.Multiline);
        var match = regex.Match(stdOutput);

        if (!match.Success)
        {
            throw new Git2SemVerGitOperationException($"Unable to read branch name from Git status response '{stdOutput}'.\n");
        }

        return match.Groups["branchName"].Value;
    }

    public SemVersion? ParseGitVersionResponse(string response)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.LogError("Unable to parse git --version response: No git response received.");
                return null;
            }

            var regex = new Regex(@"^git version (?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)((\.(?<metadata>.*?)$)|$|(\s.*$))");
            var match = regex.Match(response.Trim());
            if (!match.Success)
            {
                _logger.LogWarning($"Unable to parse git --version response: '{response}'.");
                return null;
            }

            var major = int.Parse(match.Groups["major"].Value);
            var minor = int.Parse(match.Groups["minor"].Value);
            var patch = int.Parse(match.Groups["patch"].Value);
            var version = new SemVersion(major, minor, patch);
            var metadata = match.Groups["metadata"].Value;
            version = version.WithMetadataParsedFrom(metadata);
            _logger.LogDebug("Git version (in Semver format) is '{0}'", version.ToString());
            return version;
        }
        catch (Exception exception)
        {
            _logger.LogWarning($"Unable to parse git --version response: '{response}'. Exception: {exception.Message}.");
            return null;
        }
    }
}