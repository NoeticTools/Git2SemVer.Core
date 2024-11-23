namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public interface ICommitObfuscator
{
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
    string GetObfuscatedLogLine(string graph, Commit? commit);

    string GetObfuscatedSha(string sha);
}