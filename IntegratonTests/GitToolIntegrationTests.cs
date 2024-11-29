using System.Diagnostics;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Core.Tools.Git.FluentApi;
#pragma warning disable NUnit2045


namespace NoeticTools.Git2SemVer.Core.IntegrationTests;

[TestFixture, NonParallelizable]
public class GitToolIntegrationTests
{
    private GitTool _target;
    private ConsoleLogger _logger;

    [SetUp]
    public void SetUp()
    {
        _logger = new ConsoleLogger();
        _target = new GitTool(_logger);
    }

    [TearDown]
    public void TearDown()
    {
        _logger.Dispose();
    }

    [Test]
    public void CanInvokeGitTest()
    {
        Assert.That(_target.BranchName, Is.Not.Empty);
    }

    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    public void MeasurePerformanceTest(int numberToLoad)
    {
        var stopwatch = Stopwatch.StartNew();

        _target.GetCommits(0, numberToLoad);

        stopwatch.Stop();
        _logger.LogInfo($"Reading {numberToLoad} took {stopwatch.ElapsedMilliseconds/numberToLoad}ms per commit.");
    }

    [Test]
    public void ContributingCommitsTest()
    {
        var commit = GetCommitAtIndex(_target, 10);

        var contributingCommits = _target.GetContributingCommits(head: _target.Head.CommitId, prior: commit.CommitId);

        Assert.That(contributingCommits, Has.Count.AtLeast(10));
    }

    [TestCase(3)]
    [TestCase(1)]
    [TestCase(0)]
    public void GetCommitsFromShaTest(int count)
    {
        var commit = GetCommitAtIndex(_target, 5);

        var commits = _target.GetCommits(commit.CommitId.Sha, count);

        Assert.That(commits, Has.Count.EqualTo(count));
        Assert.That(commits[0], Is.SameAs(commit));
    }

    [Test]
    public void GetCommitsInRangeUsingFluentApi()
    {
        var commit = GetCommitAtIndex(_target, 5);
        var headCommitId = _target.Head.CommitId;

        var commits = _target.GetCommits(x => x.ReachableFrom(headCommitId)
                                               .ExcludingReachableFrom(commit.CommitId));
        var commitsInclusive = _target.GetCommits(x => x.ReachableFrom(headCommitId)
                                                        .ExcludingReachableFrom(commit.CommitId, includeCommit: true));

        Assert.That(commits, Has.Count.AtLeast(5));
        Assert.That(commits[0].CommitId.ShortSha, Is.SameAs(headCommitId.ShortSha));
        Assert.That(commitsInclusive, Has.Count.EqualTo(commits.Count + 1));
    }

    private static Commit GetCommitAtIndex(GitTool target, int index)
    {
        var commit = target.Head!;
        for (var count = 0; count < index; count++)
        {
            commit = target.Get(commit.Parents[0]);
        }
        return commit;
    }
}