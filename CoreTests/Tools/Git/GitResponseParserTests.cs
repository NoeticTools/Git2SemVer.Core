using Moq;
using NoeticTools.Git2SemVer.Core.ConventionCommits;
using NoeticTools.Git2SemVer.Core.Logging;
using NoeticTools.Git2SemVer.Core.Tools.Git;
using NoeticTools.Git2SemVer.Core.Tools.Git.Parsers;


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