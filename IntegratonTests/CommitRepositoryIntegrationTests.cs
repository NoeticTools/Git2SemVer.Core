using NoeticTools.Git2SemVer.Core.Tools.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoeticTools.Git2SemVer.Core.IntegrationTests
{
    [TestFixture]
    internal class CommitRepositoryIntegrationTests
    {

        [Test]
        public void ContributingCommitsTest()
        {
            using var logger = new NUnitLogger(false);
            var target = new CommitsRepository(new GitTool(logger));
            var commit = target.Head!;
            for (var count = 0; count < 10; count++)
            {
                commit = target.Get(commit.Parents[0]);
            }

            var contributingCommits = target.GetContributingCommits(after: commit.CommitId, to: target.Head.CommitId);

            Assert.That(contributingCommits.Count, Is.AtLeast(10));
        }
    }
}
