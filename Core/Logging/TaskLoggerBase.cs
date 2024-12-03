namespace NoeticTools.Git2SemVer.Core.Logging;

public abstract class TaskLoggerBase : LoggerBase, ILogger
{
    private readonly ITaskLoggerAdapter _adapter;

    protected TaskLoggerBase(ITaskLoggerAdapter adapter)
    {
        _adapter = adapter;
    }

    /// <summary>
    ///     This logger ignores the set log level as the underlying task logger sets the logging level.
    /// </summary>
    public override LoggingLevel Level
    {
        get => LoggingLevel.Trace;
        set { }
    }

    public void Dispose()
    {
    }

    public void Log(LoggingLevel level, string message)
    {
        if (Level < level)
        {
            return;
        }

        var lookup = new Dictionary<LoggingLevel, Action<string>>
        {
            { LoggingLevel.Trace, LogTrace },
            { LoggingLevel.Debug, LogDebug },
            { LoggingLevel.Info, LogInfo },
            { LoggingLevel.Warning, LogWarning },
            { LoggingLevel.Error, LogError }
        };

        lookup[level](message);
    }

    public void LogDebug(string message)
    {
        _adapter.LogDebug(LogPrefix + message);
    }

    public void LogDebug(Func<string> messageGenerator)
    {
        if (Level < LoggingLevel.Debug)
        {
            return;
        }

        LogDebug(messageGenerator());
    }

    public void LogDebug(string message, params object[] messageArgs)
    {
        LogDebug(string.Format(message, messageArgs));
    }

    public void LogError(string message)
    {
        HasError = true;
        ErrorMessages.Add(message);
        _adapter.LogError(message);
    }

    public void LogError(string message, params object[] messageArgs)
    {
        LogError(string.Format(message, messageArgs));
    }

    public void LogError(Exception exception)
    {
        HasError = true;
        ErrorMessages.Add($"Exception: {exception.Message}\nStack trace: {exception.StackTrace}");
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

    public void LogTrace(Func<string> messageGenerator)
    {
        if (Level < LoggingLevel.Trace)
        {
            return;
        }

        LogTrace(messageGenerator());
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
}