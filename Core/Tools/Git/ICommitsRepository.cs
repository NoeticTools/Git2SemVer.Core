namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public interface ICommitsRepository
{
    Commit Head { get; }

    Commit Get(CommitId commitId);
}