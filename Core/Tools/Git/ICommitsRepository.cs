namespace NoeticTools.Common.Tools.Git;

public interface ICommitsRepository
{
    Commit Head { get; }

    Commit Get(CommitId commitId);
}