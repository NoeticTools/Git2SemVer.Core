using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NoeticTools.Common.ConventionCommits;
using NoeticTools.Common.Tools.Git;
#pragma warning disable NUnit2045


namespace NoeticTools.CommonTests.Tools.Git
{
    [Parallelizable]
    internal class CommitRepositoryTests
    {
        private readonly List<Commit> _commits =
        [
            new("001", ["002"], "Summary1", "Body", "Refs", new CommitMessageMetadata()),
            new("002", ["003", "010"], "Summary2", "Body", "Refs", new CommitMessageMetadata()),

            // branch 1
            new("003", ["004"], "Summary3", "Body", "Refs", new CommitMessageMetadata()),
            new("004", ["051"], "Summary4", "Body", "Refs", new CommitMessageMetadata()),

            // branch 2
            new("010", ["011"], "Summary3", "Body", "Refs", new CommitMessageMetadata()),
            new("011", ["051"], "Summary4", "Body", "Refs", new CommitMessageMetadata()),

            new("051", [], "Summary4", "Body", "Refs", new CommitMessageMetadata()),
        ];

        [Test]
        public void LinksEachCommitToChildCommits()
        {
            var gitTool = new Mock<IGitTool>();
            gitTool.Setup(x => x.GetCommits(0, 200)).Returns(_commits);

            var target = new CommitsRepository(gitTool.Object);

            gitTool.Verify(x => x.GetCommits(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            Assert.That(target.Get(new CommitId("001")).Summary, Is.EqualTo("Summary1"));
            Assert.That(target.Get(new CommitId("004")).Summary, Is.EqualTo("Summary4"));
            gitTool.Verify(x => x.GetCommits(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }
    }
}
