namespace NoeticTools.Git2SemVer.Core.ConventionCommits;

public interface IConventionalCommitsParser
{
    CommitMessageMetadata Parse(string commitSummary, string commitMessageBody);
}