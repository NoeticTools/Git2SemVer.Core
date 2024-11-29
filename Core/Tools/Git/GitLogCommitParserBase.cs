using System.Text.RegularExpressions;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public class GitLogCommitParserBase(ICommitsRepository cache, 
                                    IConventionalCommitsParser conventionalCommitParser)
{
    internal const string GitLogParsingPattern =
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

    protected (Commit? commit, string graph) ParseCommitAndGraph(string line)
    {
        line = line.Trim();
        var regex = new Regex(GitLogCommitParser.GitLogParsingPattern, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
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

        if (cache.TryGet(sha!, out var commit))
        {
            return (commit, graph);
        }

        var hasCommitMetadata = line.Contains($"{CharacterConstants.US}.|");
        if (hasCommitMetadata)
        {
            if (sha.Length == 0)
            {
                throw new Git2SemVerGitLogParsingException($"Unable to read SHA from line: '{line}'");
            }
        }

        var commitMetadata = conventionalCommitParser.Parse(summary, body);

        commit = hasCommitMetadata
            ? new Commit(sha, parents, summary, body, refs, commitMetadata)
            : null;

        return (commit, graph);
    }
}