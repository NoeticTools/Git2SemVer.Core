namespace NoeticTools.Common.Tools.DotnetCli;

public interface IDotNetTool
{
    IDotNetProjectCommands Projects { get; }

    IDotNetSolutionCommands Solution { get; }

    (int returnCode, string stdOutput) Run(string commandLineArguments);

    /// <summary>
    ///     Run dotnet cli with provided command line arguments.
    /// </summary>
    int Run(string commandLineArguments,
            TextWriter standardOut, TextWriter? errorOut = null);
}