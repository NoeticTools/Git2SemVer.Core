using NoeticTools.Git2SemVer.Core.Logging;


namespace NoeticTools.Git2SemVer.Core.Tools;

public interface IProcessCli
{
    ILogger Logger { get; }

    /// <summary>
    ///     Process run time limit in milliseconds.
    /// </summary>
    int TimeLimitMilliseconds { get; set; }

    /// <summary>
    ///     The directory from which Git will be invoked.
    /// </summary>
    string WorkingDirectory { get; set; }

    (int returnCode, string stdOutput) Run(string application, string commandLineArguments);

    /// <summary>
    ///     Run dotnet cli with provided command line arguments.
    /// </summary>
    int Run(string application, string commandLineArguments,
            TextWriter standardOut, TextWriter? errorOut = null);

    Task<(int returnCode, string stdOutput)> RunAsync(string application, string commandLineArguments);

    /// <summary>
    ///     Run dotnet cli with provided command line arguments.
    /// </summary>
    Task<int> RunAsync(string application, string commandLineArguments,
                       TextWriter standardOut, TextWriter? errorOut = null);
}