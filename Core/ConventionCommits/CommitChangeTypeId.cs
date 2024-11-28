namespace NoeticTools.Git2SemVer.Core.ConventionCommits;

public enum CommitChangeTypeId
{
    Unknown = 0,
    None,
    Feature,
    Fix,
    Build,
    Chore,
    ContinuousIntegration,
    Documentation,
    Style,
    Refactoring,
    Performance,
    Testing,
}