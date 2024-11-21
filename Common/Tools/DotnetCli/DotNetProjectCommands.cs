namespace NoeticTools.Common.Tools.DotnetCli;

public sealed class DotNetProjectCommands : IDotNetProjectCommands
{
    private readonly DotNetTool _inner;

    internal DotNetProjectCommands(DotNetTool inner)
    {
        _inner = inner;
    }

    public void NewClassLib(string projectPath, string language = "C#")
    {
        New("classlib", projectPath, language);
    }

    public void New(string template, string projectPath, string language = "C#")
    {
        var commandLineArguments = $"new {template} --framework net8.0 --language {language} --output {projectPath}";
        _inner.Run(commandLineArguments);
    }
}