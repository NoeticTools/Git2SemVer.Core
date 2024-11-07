namespace NoeticTools.Common.Logging;

public abstract class TaskLoggerBase : ILogger
{
    private const string LogScopeIndent = "  ";
    private readonly ITaskLoggerAdapter _adapter;
    private readonly List<string> _errorMessages = [];

    protected TaskLoggerBase(ITaskLoggerAdapter adapter)
    {
        _adapter = adapter;
    }

    public string Errors => string.Join("\n", _errorMessages);

    public bool HasError { get; private set; }

    /// <summary>
    ///     This logger ignores the set log level as the underlying task logger sets the logging level.
    /// </summary>
    public LoggingLevel Level
    {
        get => LoggingLevel.Trace;
        set { }
    }

    public string LogPrefix { get; private set; } = "";

    public void Dispose()
    {
    }

    public IDisposable EnterLogScope()
    {
        LogPrefix += LogScopeIndent;
        return new UsingScope(LeaveLogScope);
    }

    public void Log(LoggingLevel level, string message)
    {
        throw new NotImplementedException();
    }

    public void LogDebug(string message)
    {
        _adapter.LogDebug(LogPrefix + message);
    }

    public void LogDebug(string message, params object[] messageArgs)
    {
        LogDebug(string.Format(message, messageArgs));
    }

    public void LogError(string message)
    {
        HasError = true;
        _errorMessages.Add(message);
        _adapter.LogError(message);
    }

    public void LogError(string message, params object[] messageArgs)
    {
        LogError(string.Format(message, messageArgs));
    }

    public void LogError(Exception exception)
    {
        HasError = true;
        _errorMessages.Add($"Exception: {exception.Message}\nStack trace: {exception.StackTrace}");
        _adapter.LogError(exception);
    }

    public void LogInfo(string message)
    {
        _adapter.LogInfo(LogPrefix + message);
    }

    public void LogInfo(string message, params object[] messageArgs)
    {
        LogDebug(string.Format(message, messageArgs));
    }

    public void LogTrace(string message)
    {
        _adapter.LogTrace(LogPrefix + message);
    }

    public void LogTrace(string message, params object[] messageArgs)
    {
        LogTrace(string.Format(message, messageArgs));
    }

    public void LogWarning(string message)
    {
        _adapter.LogWarning(LogPrefix + message);
    }

    public void LogWarning(string format, params object[] args)
    {
        LogWarning(string.Format(format, args));
    }

    public void LogWarning(Exception exception)
    {
        _adapter.LogWarning(exception);
    }

    private void LeaveLogScope()
    {
        LogPrefix = LogPrefix.Substring(0, LogPrefix.Length - LogScopeIndent.Length);
    }
}