namespace NoeticTools.Common.Tools.DotnetCli;

public sealed class DotNetSolutionCommands : IDotNetSolutionCommands
{
    private readonly DotNetTool _inner;

    internal DotNetSolutionCommands(DotNetTool inner)
    {
        _inner = inner;
    }

    public (int returnCode, string stdOutput) AddProject(string projectName)
    {
        return _inner.Run($"sln add {projectName}");
    }

    public (int returnCode, string stdOutput) AddProject(string solutionName, string projectName)
    {
        return _inner.Run($"sln {solutionName} add {projectName}");
    }

    public (int returnCode, IReadOnlyList<string> project) GetProjects()
    {
        var result = _inner.Run("sln list");
        return (result.returnCode, result.stdOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
    }

    public (int returnCode, IReadOnlyList<string> projects) GetProjects(string solutionName)
    {
        var result = _inner.Run($"sln {solutionName} list");
        return (result.returnCode, result.stdOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
    }

    public (int returnCode, string stdOutput) RemoveProject(string projectName)
    {
        return _inner.Run($"sln remove {projectName}");
    }

    public (int returnCode, string stdOutput) RemoveProject(string solutionName, string projectName)
    {
        return _inner.Run($"sln {solutionName} remove {projectName}");
    }
}