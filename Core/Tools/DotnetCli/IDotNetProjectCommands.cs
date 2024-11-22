namespace NoeticTools.Common.Tools.DotnetCli;

public interface IDotNetProjectCommands
{
    void New(string template, string projectPath, string language = "C#");
    void NewClassLib(string projectPath, string language = "C#");
    void NewConsole(string projectPath, string language = "C#");
}