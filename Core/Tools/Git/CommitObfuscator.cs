using System.Text.RegularExpressions;
using NoeticTools.Git2SemVer.Core.ConventionCommits;


#pragma warning disable SYSLIB1045

namespace NoeticTools.Git2SemVer.Core.Tools.Git;

#pragma warning disable CS1591
public sealed class CommitObfuscator : ICommitObfuscator
{
    private readonly Dictionary<string, string> _obfuscatedShaLookup = new();

    public void Clear()
    {
        _obfuscatedShaLookup.Clear();
    }

    /// <summary>
    ///     Create a partially obfuscated git log line for the build log.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Creates build log git log line that is more suitable for public viewing to diagnose faults.
    ///         Obfuscates some information such as commit ID, most git message summary test, and most git message body text.
    ///     </para>
    ///     <para>
    ///         The resulting log can be copy and pasted to build automatic tests.
    ///     </para>
    /// </remarks>
    public string GetObfuscatedLogLine(string graph, Commit? commit)
    {
        if (commit == null)
        {
            return graph;
        }

        var priorGraphLines = "";
        var graphLine = graph;
        if (graph.Contains("\n"))
        {
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

        var parentShas = commit.Parents.Length > 0 ? string.Join(" ", commit.Parents.Select(x => x.ObfuscatedSha)) : string.Empty;
        var sha = commit.CommitId.ObfuscatedSha;
        var summary = GetRedactedConventionalCommitSummary(commit);
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

    private string GetRedactedConventionalCommitSummary(Commit commit)
    {
        if (commit.Metadata.ChangeType == CommitChangeTypeId.Unknown)
        {
            return "REDACTED";
        }

        var colonPrefix = commit.Summary.IndexOf(':');
        var prefix = commit.Summary.Substring(0, colonPrefix + 1);
        return prefix + " REDACTED";
    }
}