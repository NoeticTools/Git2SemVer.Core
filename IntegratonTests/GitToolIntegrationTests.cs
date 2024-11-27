using System.Diagnostics;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;


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

        var contributingCommits = _target.GetContributingCommits(after: commit.CommitId, to: _target.Head.CommitId);

        Assert.That(contributingCommits.Count, Is.AtLeast(10));
    }

    [TestCase(3)]
    [TestCase(1)]
    [TestCase(0)]
    public void GetCommitsFromShaTest(int count)
    {
        var commit = GetCommitAtIndex(_target, 5);

        var commits = _target.GetCommits(commit.CommitId.Id, count);

        Assert.That(commits.Count, Is.EqualTo(count));
        if (commits.Count > 0)
        {
            Assert.That(commits[0], Is.SameAs(commit));
        }
    }

    private static Commit GetCommitAtIndex(GitTool _target, int index)
    {
        var commit = _target.Head!;
        for (var count = 0; count < index; count++)
        {
            commit = _target.Get(commit.Parents[0]);
        }
        return commit;
    }
}