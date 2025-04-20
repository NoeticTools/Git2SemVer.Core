using NoeticTools.Git2SemVer.Core.ConventionCommits;


#pragma warning disable NUnit2045

namespace NoeticTools.Git2SemVer.Core.Tests.ConventionalCommits;

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
internal class ConventionalCommitsParserTests
{
    private ConventionalCommitsParser _target;

    [SetUp]
    public void SetUp()
    {
        _target = new ConventionalCommitsParser();
    }

    [TestCase(
                 """
                 Body - paragraph1

                 Body - paragraph2

                 Body - paragraph2
                 """,
                 """
                 Body - paragraph1

                 Body - paragraph2

                 Body - paragraph2
                 """,
                 "",
                 false)]
    [TestCase(
                 """
                 Body - paragraph1
                 """,
                 "Body - paragraph1",
                 "",
                 false)]
    [TestCase(
                 """
                 Body - paragraph1

                 BREAKING CHANGE: Oops
                 """,
                 "Body - paragraph1",
                 "BREAKING CHANGE|Oops",
                 true)]
    [TestCase(
                 """
                 Body - paragraph1

                 BREAKING CHANGE: Oops very sorry

                 """,
                 "Body - paragraph1",
                 "BREAKING CHANGE|Oops very sorry",
                 true)]
    [TestCase(
                 """
                 Body - paragraph1

                 BREAKING CHANGE: Oops very sorry
                 ref: 1234
                 """,
                 "Body - paragraph1",
                 """
                 BREAKING CHANGE|Oops very sorry
                 ref|1234
                 """,
                 true)]
    [TestCase(
                 """
                 Body - paragraph1
                 """,
                 "Body - paragraph1",
                 "",
                 false)]
    public void BodyMultiLineBodyAndFooterTest(string messageBody,
                                               string expectedBody,
                                               string expectedFooter,
                                               bool hasBreakingChange)
    {
        var result = _target.Parse("feat: Added a real nice feature", messageBody);

        Assert.That(result.ApiChangeFlags.FunctionalityChange);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.EqualTo(hasBreakingChange));
        Assert.That(result.ChangeDescription, Is.EqualTo("Added a real nice feature"));
        Assert.That(result.Body, Is.EqualTo(expectedBody));
        var keyValuePairs = GetExpectedKeyValuePairs(expectedFooter);

        Assert.That(result.FooterKeyValues, Is.EquivalentTo(keyValuePairs.ToLookup(k => k.key, v => v.value)));
    }

    [TestCase(
                 """
                 Body - paragraph1

                 Body - paragraph2

                 Body - paragraph2
                 """,
                 """
                 Body - paragraph1

                 Body - paragraph2

                 Body - paragraph2
                 """)]
    [TestCase(
                 """
                 Body - paragraph1
                 """,
                 """
                 Body - paragraph1
                 """)]
    public void BodyWithFooterTest(string messageBody,
                                   string expectedBody)
    {
        var result = _target.Parse("feat: Added a real nice feature", messageBody);

        Assert.That(result.ApiChangeFlags.FunctionalityChange, Is.True);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.False);
        Assert.That(result.ChangeDescription, Is.EqualTo("Added a real nice feature"));
        Assert.That(result.Body, Is.EqualTo(expectedBody));
        Assert.That(result.FooterKeyValues, Is.Empty);
    }

    [TestCase(
                 "BREAKING CHANGE: Oops very sorry",
                 "BREAKING CHANGE|Oops very sorry",
                 true)]
    [TestCase(
                 """
                 BREAKING CHANGE: Oops very sorry
                 refs: 12345
                 """,
                 """
                 BREAKING CHANGE|Oops very sorry
                 refs|12345
                 """,
                 true)]
    public void FooterWithoutBodyTest(string messageBody,
                                      string expectedFooter,
                                      bool hasBreakingChange)
    {
        var result = _target.Parse("feat: Added a real nice feature", messageBody);

        Assert.That(result.ApiChangeFlags.FunctionalityChange, Is.True);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.EqualTo(hasBreakingChange));
        Assert.That(result.ChangeDescription, Is.EqualTo("Added a real nice feature"));
        Assert.That(result.Body, Is.EqualTo(""));
        var keyValuePairs = GetExpectedKeyValuePairs(expectedFooter);

        Assert.That(result.FooterKeyValues, Is.EquivalentTo(keyValuePairs.ToLookup(k => k.key, v => v.value)));
    }

    [TestCase("feat:")]
    [TestCase("feat:\n")]
    [TestCase("feat: ")]
    [TestCase("feat: \n")]
    [TestCase("feat:  ")]
    [TestCase("fix:  ")]
    [TestCase("fix!:  ")]
    [TestCase("fix(a scope):  ")]
    public void MalformedSubjectLineConventionalCommitInfoTest(string commitSubject)
    {
        var result = _target.Parse(commitSubject, "");

        Assert.That(result.ApiChangeFlags.None, Is.True);
        Assert.That(result.ChangeNoun, Is.Empty);
    }

    [TestCase("feat(widget): Did something", "widget")]
    [TestCase("feat(widget-a): Did something", "widget-a")]
    [TestCase("feat(widget)!: Did something", "widget")]
    [TestCase("fix(widget-1): Did something", "widget-1")]
    [TestCase("refactor(widget-b): Did something", "widget-b")]
    public void ScopedChangeTypeTest(string messageSubject, string expectedScope)
    {
        var result = _target.Parse(messageSubject, "");

        Assert.That(result.ChangeScope, Is.EqualTo(expectedScope));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("feat")]
    [TestCase("feat! This is a commit without conventional commit info")]
    [TestCase("This is a commit without conventional commit info!")]
    public void SubjectLineWithoutConventionalCommitInfoTest(string commitSubject)
    {
        var result = _target.Parse(commitSubject, "");

        Assert.That(result.ApiChangeFlags.FunctionalityChange, Is.False);
        Assert.That(result.ApiChangeFlags.Fix, Is.False);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.False);
        Assert.That(result.ApiChangeFlags.None, Is.True);
        Assert.That(result.ChangeNoun, Is.Empty);
    }

    [TestCase("fix: Fixed nasty bug", "Fixed nasty bug", false)]
    [TestCase("fix(scope)!: Fixed nasty bug", "Fixed nasty bug", true)]
    public void SubjectWithBugFixTest(string messageSubject,
                                      string expectedChangeDescription,
                                      bool hasBreakingChange)
    {
        var result = _target.Parse(messageSubject, "");

        Assert.That(result.ApiChangeFlags.FunctionalityChange, Is.False);
        Assert.That(result.ApiChangeFlags.Fix, Is.True);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.EqualTo(hasBreakingChange));
        Assert.That(result.ApiChangeFlags.None, Is.False);
        Assert.That(result.ChangeDescription, Is.EqualTo(expectedChangeDescription));
        Assert.That(result.Body, Is.Empty);
        Assert.That(result.FooterKeyValues, Is.Empty);
        Assert.That(result.ChangeNoun, Is.EqualTo("fix"));
    }

    [TestCase("feat: Added a real nice feature", "Added a real nice feature", false)]
    [TestCase("feat: Added a real nice feature (#24)", "Added a real nice feature (#24)", false)]
    [TestCase("feat!: Added a real nice feature", "Added a real nice feature", true)]
    public void SubjectWithFeatureTest(string messageSubject,
                                       string expectedChangeDescription,
                                       bool hasBreakingChange)
    {
        var result = _target.Parse(messageSubject, "");

        Assert.That(result.ApiChangeFlags.FunctionalityChange, Is.True);
        Assert.That(result.ApiChangeFlags.Fix, Is.False);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.EqualTo(hasBreakingChange));
        Assert.That(result.ApiChangeFlags.None, Is.False);
        Assert.That(result.ChangeDescription, Is.EqualTo(expectedChangeDescription));
        Assert.That(result.Body, Is.Empty);
        Assert.That(result.FooterKeyValues, Is.Empty);
        Assert.That(result.ChangeNoun, Is.EqualTo("feat"));
    }

    [TestCase("build: Build work", "build", "Build work")]
    [TestCase("refactor: Did something", "refactor", "Did something")]
    public void SubjectWithOtherNounTest(string messageSubject,
                                         string expectedNoun,
                                         string expectedChangeDescription)
    {
        var result = _target.Parse(messageSubject, "");

        Assert.That(result.ChangeNoun, Is.EqualTo(expectedNoun));
        Assert.That(result.ApiChangeFlags.FunctionalityChange, Is.False);
        Assert.That(result.ApiChangeFlags.Fix, Is.False);
        Assert.That(result.ApiChangeFlags.BreakingChange, Is.False);
        Assert.That(result.ApiChangeFlags.None, Is.True);
        Assert.That(result.ChangeDescription, Is.EqualTo(expectedChangeDescription));
        Assert.That(result.Body, Is.Empty);
        Assert.That(result.FooterKeyValues, Is.Empty);
    }

    private static List<(string key, string value)> GetExpectedKeyValuePairs(string expectedFooter)
    {
        var expectedFooterLines = expectedFooter.Split('\n');
        var keyValuePairs = new List<(string key, string value)>();
        foreach (var line in expectedFooterLines)
        {
            if (line.Length == 0)
            {
                continue;
            }

            var elements = line.Split('|');
            keyValuePairs.Add((key: elements[0], value: elements[1].Trim()));
        }

        return keyValuePairs;
    }
}