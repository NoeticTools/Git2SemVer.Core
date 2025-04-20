namespace NoeticTools.Git2SemVer.Core.ConventionCommits;

public interface IConventionalCommitsParser
{
    /// <summary>
    /// Parse summary and message body for conventional commit metadata.
    /// </summary>
    /// <remarks>
    /// If no conventional commit metadata found an empty metadata object is returned.
    /// </remarks>
    CommitMessageMetadata Parse(string commitSummary, string commitMessageBody);
}