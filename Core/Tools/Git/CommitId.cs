// ReSharper disable ReplaceSubstringWithRangeIndexer

using NoeticTools.Git2SemVer.Core.Exceptions;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

public sealed class CommitId : IEquatable<CommitId>, IEquatable<string>
{
    private const int ShortShaLength = 7;

    public CommitId(string sha)
    {
        if (sha.Length == 0)
        {
            throw new Git2SemVerGitLogParsingException("Empty commit SHA.");
        }

        Sha = sha;
        ShortSha = sha.Length < 7 ? sha : sha.Substring(0, ShortShaLength);
    }

    public string Sha { get; }

    public string ShortSha { get; }

    public bool Equals(string? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        return Sha.Equals(other);
    }

    public bool Equals(CommitId? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Sha == other.Sha;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is CommitId other && Equals(other));
    }

    public override int GetHashCode()
    {
        return Sha.GetHashCode();
    }
}