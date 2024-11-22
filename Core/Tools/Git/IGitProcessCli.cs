namespace NoeticTools.Common.Tools.Git;

public interface IGitProcessCli
{
    string WorkingDirectory { get; set; }

    /// <summary>
    ///     Run dotnet cli with provided command line arguments.
    /// </summary>
    int Run(string commandLineArguments,
            TextWriter standardOut, TextWriter errorOut);
}