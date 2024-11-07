namespace NoeticTools.Common.Tools.DotnetCli;

public interface IDotNetSolutionCommands
{
    /// <summary>
    ///     Add existing project to solution in current directory.
    /// </summary>
    /// <remarks>
    ///     See: <see href="https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln#commands">dotnet sln add</see>
    /// </remarks>
    (int returnCode, string stdOutput) AddProject(string projectName);

    /// <summary>
    ///     Add existing project to named solution in current directory.
    /// </summary>
    /// <remarks>
    ///     See: <see href="https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln#commands">dotnet sln add</see>
    /// </remarks>
    (int returnCode, string stdOutput) AddProject(string solutionName, string projectName);

    /// <summary>
    ///     Get list of project in solution in current directory.
    /// </summary>
    /// <remarks>
    ///     See: <see href="https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln#commands">dotnet sln list</see>
    /// </remarks>
    (int returnCode, IReadOnlyList<string> project) GetProjects();

    /// <summary>
    ///     Get list of project in named solution in current directory.
    /// </summary>
    /// <remarks>
    ///     See: <see href="https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln#commands">dotnet sln list</see>
    /// </remarks>
    (int returnCode, IReadOnlyList<string> projects) GetProjects(string solutionName);

    (int returnCode, string stdOutput) RemoveProject(string projectName);
    (int returnCode, string stdOutput) RemoveProject(string solutionName, string projectName);
}