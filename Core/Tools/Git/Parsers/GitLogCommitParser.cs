using NoeticTools.Git2SemVer.Core.ConventionCommits;


namespace NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

#pragma warning disable CS1591
public class GitLogCommitParser : GitLogCommitParserBase, IGitLogCommitParser
{
    public GitLogCommitParser(ICommitsCache cache, IConventionalCommitsParser conventionalCommitParser)
        : base(cache, conventionalCommitParser)
    {
    }

    public Commit? Parse(string line)
    {
        return ParseCommitAndGraph(line).commit;
    }
}