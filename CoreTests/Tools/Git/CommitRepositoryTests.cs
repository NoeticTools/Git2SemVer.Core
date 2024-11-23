using Moq;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Tools.Git;


#pragma warning disable NUnit2045

namespace NoeticTools.Git2SemVer.Core.Tests.Tools.Git;

[Parallelizable]
internal class CommitRepositoryTests
{
    private readonly List<Commit> _commits;

    public CommitRepositoryTests()
    {
        var obfuscator = new CommitObfuscator();
        _commits =
        [
            new Commit("001", ["002"], "Summary1", "Body", "Refs", new CommitMessageMetadata(), obfuscator),
            new Commit("002", ["003", "010"], "Summary2", "Body", "Refs", new CommitMessageMetadata(), obfuscator),

            // branch 1
            new Commit("003", ["004"], "Summary3", "Body", "Refs", new CommitMessageMetadata(), obfuscator),
            new Commit("004", ["051"], "Summary4", "Body", "Refs", new CommitMessageMetadata(), obfuscator),

            // branch 2
            new Commit("010", ["011"], "Summary3", "Body", "Refs", new CommitMessageMetadata(), obfuscator),
            new Commit("011", ["051"], "Summary4", "Body", "Refs", new CommitMessageMetadata(), obfuscator),

            new Commit("051", [], "Summary4", "Body", "Refs", new CommitMessageMetadata(), obfuscator)
        ];
    }

    [Test]
    public void LinksEachCommitToChildCommits()
    {
        var gitTool = new Mock<IGitTool>();
        gitTool.Setup(x => x.GetCommits(0, 200)).Returns(_commits);

        var target = new CommitsRepository(gitTool.Object);

        gitTool.Verify(x => x.GetCommits(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        var obfuscator = new CommitObfuscator();
        Assert.That(target.Get(new CommitId("001", obfuscator)).Summary, Is.EqualTo("Summary1"));
        Assert.That(target.Get(new CommitId("004", obfuscator)).Summary, Is.EqualTo("Summary4"));
        gitTool.Verify(x => x.GetCommits(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }
}