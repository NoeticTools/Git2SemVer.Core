using System.Text;
using System.Text.RegularExpressions;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;


#pragma warning disable SYSLIB1045

namespace NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

#pragma warning disable CS1591
    public sealed class CommitObfuscator : GitLogCommitParserBase, ICommitObfuscator
{
    private readonly IGitTool _gitTool;
    private readonly Dictionary<string, string> _obfuscatedShaLookup = new();

    public CommitObfuscator(IGitTool gitTool, ICommitsCache cache)
        : this(gitTool, cache, new ConventionalCommitsParser())
    { }

    public CommitObfuscator(IGitTool gitTool, ICommitsCache cache, IConventionalCommitsParser conventionalCommitParser)
        : base(cache, conventionalCommitParser)
    {
        _gitTool = gitTool;
    }

    public string GetObfuscatedSha(string sha)
    {
        if (_obfuscatedShaLookup.TryGetValue(sha, out var value))
        {
            return value;
        }

        var newValue = sha.Length > 6 ? (_obfuscatedShaLookup.Count + 1).ToString("D").PadLeft(4, '0') : sha;
        _obfuscatedShaLookup.Add(sha, newValue);
        return newValue;
    }
}