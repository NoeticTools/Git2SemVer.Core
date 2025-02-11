using Moq;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;
using Semver;


namespace NoeticTools.Git2SemVer.Core.Tests.Tools.Git;

[TestFixture]
internal class GitResponseParserTests
{
    private GitResponseParser _parser;

    [SetUp]
    public void SetUp()
    {
        var cache = new CommitsCache();
        var conventionalCommitsParser = new Mock<IConventionalCommitsParser>();
        _parser = new GitResponseParser(cache, conventionalCommitsParser.Object, new ConsoleLogger());
    }

    [TestCase("git version 2.41.0.8", "2.41.0+8")]
    [TestCase("git version 222.41.0", "222.41.0")]
    [TestCase("git version 2.41.0.windows.1", "2.41.0+windows.1")]
    [TestCase("git version 2.39.5 (Apple Git-154)", "2.39.5")]
    [TestCase("git version 2.41.0 garbage", "2.41.0")]
    public void ParseVersionResponseTest(string response, string expected)
    {
        var result = _parser.ParseGitVersionResponse(response);

        Assert.That(result!.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void ParseStatusResponseTest()
    {
        const string response = """
                                ## My-Branch/Thing_A
                                 M Git2SemVer.IntegrationTests/Resources/Scripts/ForceProperties1.csx
                                 D Git2SemVer.MSBuild/Framework/ReadOnlyList.cs
                                 D Git2SemVer.MSBuild/Versioning/Generation/Builders/IVersioningContext.cs
                                ?? CommonTests/Tools/Git/
                                ?? Git2SemVer.MSBuild/Versioning/Generation/ApiChanges.cs
                                ?? Git2SemVer.MSBuild/Versioning/Generation/VersioningMode.cs
                                """;

        var result = _parser.ParseStatusResponseBranchName(response);

        Assert.That(result, Is.EqualTo("My-Branch/Thing_A"));
    }
}