using NoeticTools.Common.Logging;
using NoeticTools.Common.Tools.Git;


namespace NoeticTools.Git2SemVer.Core.IntegrationTests;

public class GitToolIntegrationTests
{
    [Test]
    public void CanInvokeGit()
    {
        var logger = new ConsoleLogger();
        var target = new GitTool(logger);

        Assert.That(target.BranchName, Is.Not.Empty);
    }

    [SetUp]
    public void Setup()
    {
    }
}
