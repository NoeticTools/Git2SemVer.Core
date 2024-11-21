using NoeticTools.Common.Logging;
using NoeticTools.Common.Tools.Git;


namespace NoeticTools.IntegrationTests
{
    public class GitToolIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanInvokeGit()
        {
            var logger = new ConsoleLogger();
            var target = new GitTool(logger);

            Assert.That(target.BranchName, Is.Not.Empty);
        }
    }
}