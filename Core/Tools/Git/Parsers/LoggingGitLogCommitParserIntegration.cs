using NoeticTools.Git2SemVer.Core.ConventionCommits;
using System.Text.RegularExpressions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

public sealed class LoggingGitLogCommitParserIntegration
    : GitLogCommitParserBase, IGitLogCommitParser
{
    public LoggingGitLogCommitParserIntegration(ICommitsCache cache,
                                                IConventionalCommitsParser conventionalCommitParser,
                                                ICommitObfuscator? obfuscator = null)
        : base(cache, conventionalCommitParser)
    {
        _obfuscator = obfuscator;
    }

    private readonly List<string> _logLines = [];
    private readonly ICommitObfuscator? _obfuscator;

    public void Clear()
    {
        _logLines.Clear();
    }

    public string Log => _logLines.Aggregate("", (current, line) => current + Environment.NewLine + line);

    public Commit? Parse(string line)
    {
        var (commit, graph) = ParseCommitAndGraph(line);
        _logLines.Add(GetLogLine(graph, commit));
        return commit;
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
    internal string GetLogLine(string graph, Commit? commit)
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

    private string GetObfuscatedSha(string sha)
    {
        return _obfuscator?.GetObfuscatedSha(sha) ?? sha;
    }

    private string GetCommitSummary(Commit commit)
    {
        if (_obfuscator == null)
        {
            return commit.Summary;
        }
        else
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
}