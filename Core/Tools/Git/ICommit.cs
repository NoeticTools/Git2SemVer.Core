using System.Text.Json.Serialization;
using Semver;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

[JsonDerivedType(typeof(Commit), "Commit")]
public interface ICommit
{
    CommitId CommitId { get; }

    string MessageBody { get; }

    CommitId[] Parents { get; }

    string Refs { get; }

    SemVersion? ReleasedVersion { get; }

    string Summary { get; }
}