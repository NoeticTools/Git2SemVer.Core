using System.Text;
using NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;

namespace NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;

#pragma warning disable CS1591
/// <summary>
///     Build git <see href="https://git-scm.com/docs/gitrevisions">revision range arguments</see>.
/// </summary>
public sealed class GitRevisionsBuilder : IGitRevisionsBuilder
{
    private readonly StringBuilder _args = new();

    public IGitResponseParser? Parser { get; private set; }

    public IGitRevisionsBuilder NotReachableFrom(CommitId commitId, bool inclusive = false)
    {
        return NotReachableFrom(commitId.Sha, inclusive);
    }

    public IGitRevisionsBuilder NotReachableFrom(string commitSha, bool inclusive = false)
    {
        AppendArgs($"\"^{commitSha}{(inclusive ? "^@" : "")}\"");
        return this;
    }

    public IGitRevisionsBuilder NotReachableFrom(CommitId[] commitIds, bool inclusive = false)
    {
        return NotReachableFrom(commitIds.Select(x => x.Sha).ToArray(), inclusive);
    }

    public IGitRevisionsBuilder NotReachableFrom(string[] commitShas, bool inclusive = false)
    {
        foreach (var sha in commitShas)
        {
            NotReachableFrom(sha, inclusive);
        }
        return this;
    }

    public IGitRevisionsBuilder ReachableFrom(CommitId commitId)
    {
        return ReachableFrom(commitId.Sha);
    }

    public IGitRevisionsBuilder ReachableFrom(string commitSha)
    {
        AppendArgs(commitSha);
        return this;
    }

    public IGitRevisionsBuilder ReachableFrom(CommitId[] commitIds)
    {
        foreach (var commitId in commitIds)
        {
            ReachableFrom(commitId.Sha);
        }
        return this;
    }

    public IGitRevisionsBuilder ReachableFrom(string[] commitShas)
    {
        foreach (var sha in commitShas)
        {
            ReachableFrom(sha);
        }
        return this;
    }

    private void AppendArgs(string args)
    {
        if (_args.Length > 0)
        {
            _args.Append(" ");
        }
        _args.Append(args);
    }

    public IGitRevisionsBuilder ReachableFromHead()
    {
        AppendArgs("HEAD");
        return this;
    }

    public IGitRevisionsBuilder Skip(int skipCount)
    {
        AppendArgs($"--skip={skipCount}");
        return this;
    }

    public IGitRevisionsBuilder Take(int takeCount)
    {
        AppendArgs($"--max-count={takeCount}");
        return this;
    }

    public IGitRevisionsBuilder With(IGitResponseParser customParser)
    {
        Parser = customParser;
        return this;
    }

    internal string GetArgs()
    {
        return _args.ToString();
    }
}