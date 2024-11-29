using System.Text.RegularExpressions;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
public class GitLogCommitParser : GitLogCommitParserBase, IGitLogCommitParser
{
    public GitLogCommitParser(ICommitsRepository cache, IConventionalCommitsParser conventionalCommitParser)
        : base(cache, conventionalCommitParser)
    {
        FormatArgs = "--graph --pretty=\"format:%x1f.|%H|%P|%x02%s%x03|%x02%b%x03|%d|%x1e\"";
    }

    public string FormatArgs { get; }

    public string ParseToLogLineWithGraph(string line)
    {
        var (commit, graph) = ParseCommitAndGraph(line);

        // todo if required

        throw new NotImplementedException();
    }

    public Commit? Parse(string line)
    {
        return ParseCommitAndGraph(line).commit;
    }
}