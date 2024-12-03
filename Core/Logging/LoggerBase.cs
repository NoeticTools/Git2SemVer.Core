namespace NoeticTools.Git2SemVer.Core.Logging;

public abstract class LoggerBase
{
    private const string LogScopeIndent = "  ";

    public string Errors => string.Join("\n", ErrorMessages);

    public bool HasError { get; protected set; }

    public string LogPrefix { get; private set; } = "";

    public IDisposable EnterLogScope()
    {
        LogPrefix += LogScopeIndent;
        return new UsingScope(LeaveLogScope);
    }

    private void LeaveLogScope()
    {
        LogPrefix = LogPrefix.Substring(0, LogPrefix.Length - LogScopeIndent.Length);
    }

    protected readonly List<string> ErrorMessages = [];

    protected string IndentLines(string message)
    {
        message = message.Replace("\n", Environment.NewLine + LogPrefix);
        return LogPrefix + message;
    }
}