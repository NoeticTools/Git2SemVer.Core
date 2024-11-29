using Moq;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;


namespace NoeticTools.Git2SemVer.Core.Tests.Tools.Git;

[TestFixture]
internal class LoggingGitLogCommitParserIntegrationTests
{
    private LoggingGitLogCommitParserIntegration _target;

    [SetUp]
    public void SetUp()
    {
        var cache = new CommitsCache();
        var gitTool = new Mock<IGitTool>();
        _target = new LoggingGitLogCommitParserIntegration(cache, new ConventionalCommitsParser(), new CommitObfuscator());
    }

    [Test]
    public void NoConventionalCommitInfoLogLineTest()
    {
        const string expected = "*               \u001f.|0001|0002 0003|\u0002REDACTED\u0003|\u0002\u0003||";
        var commit = new Commit("commitSha", ["parent1", "parent2"], "Summary line", "", "", new CommitMessageMetadata());

        var result = _target.GetLogLine("* ", commit);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void WithConventionalCommitSummaryLogLineTest()
    {
        const string summary = "feat!: REDACTED";
        const string expected = $"|\\              \u001f.|0001|0002 0003|\u0002{summary}\u0003|\u0002\u0003||";
        var commit = new Commit("commitSha",
                                ["parent1", "parent2"],
                                summary, "", "",
                                new CommitMessageMetadata("feat", true, "Big red feature\nRecommended", "", []));

        var result = _target.GetLogLine(@"|\  ", commit);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void WithFooterValuesLogLineTest()
    {
        const string summary = "fix: Fixed";
        var footerKeyValues = new List<(string key, string value)>
        {
            ("BREAKING CHANGE", "Oops my bad"),
            ("refs", "#0001"),
            ("refs", "#0002")
        };
        var expected =
            "|\\              \u001f.|0001|0002 0003|\u0002fix: REDACTED\u0003|\u0002BREAKING CHANGE: Oops my bad\nrefs: #0001\nrefs: #0002\u0003||";
        var commit = new Commit("commitSha",
                                ["parent1", "parent2"],
                                summary, "", "",
                                new CommitMessageMetadata("feat", true, "Big red feature", "", footerKeyValues));

        var result = _target.GetLogLine(@"|\  ", commit);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void WithRefsLogLineTest()
    {
        const string summary = "fix: Fixed";
        var footerKeyValues = new List<(string key, string value)>();
        var expected =
            "|\\              \u001f.|0001|0002 0003|\u0002fix: REDACTED\u0003|\u0002\u0003| (HEAD -> REDACTED_BRANCH, origin/main)|";
        var commit = new Commit("commitSha",
                                ["parent1", "parent2"],
                                summary, "", "HEAD -> REDACTED_BRANCH, origin/main",
                                new CommitMessageMetadata("feat", true, "Big red feature", "", footerKeyValues));

        var result = _target.GetLogLine(@"|\  ", commit);

        Assert.That(result, Is.EqualTo(expected));
    }
}