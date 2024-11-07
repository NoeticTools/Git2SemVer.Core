using Injectio.Attributes;
using NoeticTools.Common.Logging;


namespace NoeticTools.Common.Tools.DotnetCli;

/// <summary>
///     Help for executing dotnet cli commands.
/// </summary>
[RegisterTransient]
public sealed class DotNetTool : IDotNetTool
{
    private readonly ProcessCli _inner;

    public DotNetTool() : this(new ProcessCli(new NullTaskLogger()))
    {
    }

    public DotNetTool(ProcessCli inner)
    {
        _inner = inner;
    }

    public IDotNetProjectCommands Projects => new DotNetProjectCommands(this);

    public IDotNetSolutionCommands Solution => new DotNetSolutionCommands(this);

    /// <summary>
    ///     Command time limit in milliseconds.
    /// </summary>
    public int TimeLimitMilliseconds
    {
        get => _inner.TimeLimitMilliseconds;
        set => _inner.TimeLimitMilliseconds = value;
    }

    /// <summary>
    ///     Build solution with build caching disabled.
    /// </summary>
    /// <remarks>
    ///     See: <see href="https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build">dotnet build</see>
    /// </remarks>
    public (int returnCode, string stdOutput) Build(string solution, string configuration, params string[] arguments)
    {
        _inner.Logger.LogInfo($"Building solution {solution}.");
        var dotNetCommandLine =
            $"build {solution} --configuration {configuration} --disable-build-servers {string.Join(" ", arguments)}";
        return Run(dotNetCommandLine);
    }

    /// <summary>
    ///     Pack project. Generates NuGet package.
    /// </summary>
    public (int returnCode, string stdOutput) Pack(string project, string configuration, params string[] arguments)
    {
        _inner.Logger.LogInfo($"Packing project {project}.");
        var dotNetCommandLine = $"pack {project} --configuration {configuration} {string.Join(" ", arguments)}";
        return Run(dotNetCommandLine);
    }

    /// <summary>
    ///     Run dotnet cli with provided command line arguments.
    /// </summary>
    public int Run(string commandLineArguments,
                   TextWriter standardOut, TextWriter? errorOut = null)
    {
        return _inner.Run("dotnet", commandLineArguments, standardOut, errorOut);
    }

    public (int returnCode, string stdOutput) Run(string commandLineArguments)
    {
        return _inner.Run("dotnet", commandLineArguments);
    }
}