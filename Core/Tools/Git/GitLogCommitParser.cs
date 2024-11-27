using System.Text.RegularExpressions;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
public class GitLogCommitParser : IGitLogCommitParser
{
    private readonly ICommitsRepository _cache;
    private readonly IConventionalCommitsParser _conventionalCommitParser;

    private const string GitLogParsingPattern =
        """
        ^(?<graph>[^\x1f$]*) 
          (\x1f\.\|
            (?<sha>[^\|]+) \|
            (?<parents>[^\|]*)? \|
            \x02(?<summary>[^\x03]*)?\x03 \|
            \x02(?<body>[^\x03]*)?\x03 \|
            (\s\((?<refs>.*?)\))?
           \|$)?
        """;

    public GitLogCommitParser(ICommitsRepository cache, IConventionalCommitsParser conventionalCommitParser)
    {
        _cache = cache;
        _conventionalCommitParser = conventionalCommitParser;
        FormatArgs = "--graph --pretty=\"format:%x1f.|%H|%P|%x02%s%x03|%x02%b%x03|%d|%x1e\"";
    }

    public string FormatArgs { get; }

    public Commit? Parse(string line)
    {
        // todo - move this out to its own parser

        line = line.Trim();
        var regex = new Regex(GitLogParsingPattern, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        var match = regex.Match(line);
        if (!match.Success)
        {
            throw new Git2SemVerGitLogParsingException($"Unable to parse Git log line {line}.");
        }

        var graph = match.GetGroupValue("graph");
        var sha = match.GetGroupValue("sha");
        var refs = match.GetGroupValue("refs");
        var parents = match.GetGroupValue("parents").Split(' ');
        var summary = match.GetGroupValue("summary");
        var body = match.GetGroupValue("body");

        if (_cache.TryGet(sha!, out var commit))
        {
            return commit;
        }

        var hasCommitMetadata = line.Contains($"{CharacterConstants.US}.|");
        if (hasCommitMetadata)
        {
            if (sha.Length == 0)
            {
                throw new Git2SemVerGitLogParsingException($"Unable to read SHA from line: '{line}'");
            }
        }

        var commitMetadata = _conventionalCommitParser.Parse(summary, body);

        commit = hasCommitMetadata
            ? new Commit(sha, parents, summary, body, refs, commitMetadata)
            : null;

        return commit;
    }
}