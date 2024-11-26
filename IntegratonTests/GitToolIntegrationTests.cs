using System.Diagnostics;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;


namespace NoeticTools.Git2SemVer.Core.IntegrationTests;

[TestFixture, NonParallelizable]
public class GitToolIntegrationTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CanInvokeGitTest()
    {
        var logger = new ConsoleLogger();
        var target = new GitTool(logger);

        Assert.That(target.BranchName, Is.Not.Empty);
    }

    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(20)]
    public void PerformanceTest(int numberToLoad)
    {
        var logger = new ConsoleLogger();
        var target = new GitTool(logger);
        var stopwatch = Stopwatch.StartNew();

        target.GetCommits(0, numberToLoad);

        stopwatch.Stop();
        logger.LogInfo($"Reading {numberToLoad} took {stopwatch.ElapsedMilliseconds/numberToLoad}ms per commit.");
    }

    [Test]
    public void ContributingCommitsTest()
    {
        using var logger = new NUnitLogger(false);
        var target = new GitTool(logger);
        var commit = target.Head!;
        for (var count = 0; count < 10; count++)
        {
            commit = target.Get(commit.Parents[0]);
        }

        var contributingCommits = target.GetContributingCommits(after: commit.CommitId, to: target.Head.CommitId);

        Assert.That(contributingCommits.Count, Is.AtLeast(10));
    }
}