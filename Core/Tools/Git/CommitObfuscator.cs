using System.Text;
using System.Text.RegularExpressions;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Exceptions;


#pragma warning disable SYSLIB1045

namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
public sealed class CommitObfuscator : GitLogCommitParserBase, ICommitObfuscator
{
    private readonly IGitTool _gitTool;
    private readonly Dictionary<string, string> _obfuscatedShaLookup = new();

    public CommitObfuscator(IGitTool gitTool, ICommitsRepository cache)
        : this (gitTool, cache, new ConventionalCommitsParser())
    {}

    public CommitObfuscator(IGitTool gitTool, ICommitsRepository cache, IConventionalCommitsParser conventionalCommitParser)
        : base(cache, conventionalCommitParser)
    {
        _gitTool = gitTool;
    }

    public string GetLogLine(string graph, Commit? commit)
    {
        if (commit == null)
        {
            return graph;
        }

        var priorGraphLines = "";
        var graphLine = graph;
        if (graph.Contains("\n"))
        {
            // todo - is this still needed?
            var lastNewLineIndex = graph.LastIndexOf('\n');
            priorGraphLines = graph.Substring(0, lastNewLineIndex + 1);
            graphLine = graph.Substring(lastNewLineIndex + 1);
        }

        var redactedRefs = new Regex(@"HEAD -> \S+?(?=[,\)])").Replace(commit.Refs, "HEAD -> REDACTED_BRANCH");
        var redactedRefs2 = new Regex(@"origin\/\S+?(?=[,\)])").Replace(redactedRefs, "origin/REDACTED_BRANCH");
        if (redactedRefs2.Length > 0)
        {
            redactedRefs2 = $" ({redactedRefs2})";
        }

        var sha = GetObfuscatedSha(commit.CommitId.Sha);
        var parentShas = commit.Parents.Length > 0 ? string.Join(" ", commit.Parents.Select(x => GetObfuscatedSha(x.Sha))) : string.Empty;
        var summary = GetCommitSummary(commit);
        var footer = string.Join("\n", commit.Metadata.FooterKeyValues.SelectMany((kv, _) => kv.Select(value => kv.Key + ": " + value)));

        return $"{priorGraphLines}{graphLine,-15} \u001f.|{sha}|{parentShas}|\u0002{summary}\u0003|\u0002{footer}\u0003|{redactedRefs2}|";
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

    private static string GetCommitSummary(Commit commit)
    {
        if (commit.Metadata.ChangeType == CommitChangeTypeId.Unknown)
        {
            return "UNKNOWN";
        }

        if (commit.Metadata.ChangeType == CommitChangeTypeId.None)
        {
            return "REDACTED";
        }

        var colonPrefix = commit.Summary.IndexOf(':');
        var prefix = commit.Summary.Substring(0, colonPrefix + 1);
        return prefix + " REDACTED";
    }
}