namespace NoeticTools.Git2SemVer.Core.ConventionCommits;

public class CommitMessageMetadata
{
    private const string FeatureNoun = "feat";
    private const string FixNoun = "fix";

    public CommitMessageMetadata(string changeNoun, 
                                 string changeScope, 
                                 bool breakingChangeFlagged, 
                                 string changeDescription, 
                                 string body,
                                 List<(string key, string value)> footerKeyValues)
    {
        ChangeNoun = changeNoun;
        ChangeScope = changeScope;
        ChangeDescription = changeDescription;
        Body = body;
        FooterKeyValues = footerKeyValues.ToLookup(k => k.key, v => v.value);

        var apiChanges = new ApiChanges
        {
            FunctionalityChange = FeatureNoun.Equals(changeNoun),
            Fix = FixNoun.Equals(changeNoun),
            BreakingChange = breakingChangeFlagged ||
                             FooterKeyValues.Contains("BREAKING-CHANGE") ||
                             FooterKeyValues.Contains("BREAKING CHANGE")
        };
        ApiChangeFlags = apiChanges;
    }

    public CommitMessageMetadata() : this("", "", false, "", "", [])
    {
    }

    public ApiChanges ApiChangeFlags { get; }

    public string Body { get; }

    public string ChangeNoun { get; }

    public string ChangeScope { get; }

    public string ChangeDescription { get; }

    public ILookup<string, string> FooterKeyValues { get; }
}