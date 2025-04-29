﻿using Injectio.Attributes;
using NoeticTools.Git2SemVer.Core.Logging;


namespace NoeticTools.Git2SemVer.Core.Tools.Git;

[RegisterTransient]
public class GitProcessCli : IGitProcessCli
{
    private readonly string _gitPath;
    private readonly IProcessCli _inner;

    public GitProcessCli(ILogger logger)
    {
        _inner = new ProcessCli(logger);

        var teamCityGitPath = Environment.GetEnvironmentVariable("TEAMCITY_GIT_PATH") ?? "";
        _gitPath = teamCityGitPath.Length > 0 ? teamCityGitPath : "git";
        //logger.LogTrace($"Using git path: '{_gitPath}'");
        logger.LogInfo($"Using git path: '{_gitPath}'");
    }

    public string WorkingDirectory
    {
        get => _inner.WorkingDirectory;
        set => _inner.WorkingDirectory = value;
    }

    public int Run(string commandLineArguments,
                   TextWriter standardOut, TextWriter errorOut)
    {
        return _inner.Run(_gitPath, commandLineArguments, standardOut, errorOut);
    }
}