﻿using System.Text.RegularExpressions;


namespace NoeticTools.Git2SemVer.Core.ConventionCommits;

public sealed class ConventionalCommitsParser : IConventionalCommitsParser
{
    private readonly Regex _bodyRegex = new("""
                                            \A
                                            (
                                              (
                                                (?<footer> 
                                                  (BREAKING(\s|-)CHANGE | \w(\w|-)* )
                                                  :\s+ 
                                                  (\w|\#)(\w|\s)*?
                                                  (\n|\r\n)?
                                                )*
                                              )
                                              |
                                              ( 
                                                (?<body>.*?)
                                                ( 
                                                  (\n|\r\n) 
                                                  ( 
                                                    (\n|\r\n) 
                                                    (?<footer> 
                                                      (BREAKING(\s|-)CHANGE | \w(\w|-)* )
                                                      :\s+ 
                                                      (\w|\#)(\w|\s)*?
                                                      (\n|\r\n)?
                                                    )*
                                                  )?
                                                )?
                                                (\n|\r\n)?
                                              )
                                            )
                                            \Z 
                                            """,
                                            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private readonly Regex _summaryRegex = new("""
                                               \A
                                                 (?<changeNoun>(fix|feat|build|chore|ci|docs|style|refactor|perf|test))
                                                   (\((?<scopeNoun>[\w\-\.]+)\))?(?<breakFlag>!)?: \s+(?<desc>\S.*?)
                                               \Z
                                               """,
                                               RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

    public CommitMessageMetadata Parse(string commitSummary, string commitMessageBody)
    {
        var summaryMatch = _summaryRegex.Match(commitSummary);
        if (!summaryMatch.Success)
        {
            return new CommitMessageMetadata();
        }

        var changeNoun = summaryMatch.GetGroupValue("changeNoun");
        var breakingChangeFlagged = summaryMatch.GetGroupValue("breakFlag").Length > 0;
        var changeDescription = summaryMatch.GetGroupValue("desc");
        var changeScope = summaryMatch.GetGroupValue("scopeNoun");

        var bodyMatch = _bodyRegex.Match(commitMessageBody);
        var body = bodyMatch.GetGroupValue("body");
        var footerGroup = bodyMatch.Groups["footer"];
        var keyValuePairs = GetFooterKeyValuePairs(footerGroup);

        return new CommitMessageMetadata(changeNoun, changeScope, breakingChangeFlagged, changeDescription, body, keyValuePairs);
    }

    private static List<(string key, string value)> GetFooterKeyValuePairs(Group footerGroup)
    {
        var keyValuePairs = new List<(string key, string value)>();
        if (!footerGroup.Success)
        {
            return keyValuePairs;
        }

        foreach (Capture capture in footerGroup.Captures)
        {
            var line = capture.Value;
            var elements = line.Split(':');
            keyValuePairs.Add((key: elements[0], value: elements[1].Trim()));
        }

        return keyValuePairs;
    }
}