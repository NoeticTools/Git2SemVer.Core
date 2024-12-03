namespace NoeticTools.Git2SemVer.Core.Logging;

public sealed class NoDisposeLoggerDecorator : ILogger
{
    private readonly ILogger _inner;

    public NoDisposeLoggerDecorator(ILogger inner)
    {
        _inner = inner;
        Level = LoggingLevel.Info;
    }

    public string Errors => _inner.Errors;

    public bool HasError => _inner.HasError;

    public LoggingLevel Level
    {
        get => _inner.Level;
        set => _inner.Level = value;
    }

    public string LogPrefix => _inner.LogPrefix;

    public void Dispose()
    {
        // do nothing
    }

    public IDisposable EnterLogScope()
    {
        return _inner.EnterLogScope();
    }

    public void Log(LoggingLevel level, string message)
    {
        _inner.Log(level, message);
    }

    public void LogDebug(string message)
    {
        _inner.LogDebug(message);
    }

    public void LogDebug(Func<string> messageGenerator)
    {
        _inner.LogDebug(messageGenerator());
    }

    public void LogDebug(string message, params object[] messageArgs)
    {
        _inner.LogDebug(message, messageArgs);
    }

    public void LogError(string message)
    {
        _inner.LogError(message);
    }

    public void LogError(string message, params object[] messageArgs)
    {
        _inner.LogError(message, messageArgs);
    }

    public void LogError(Exception exception)
    {
        _inner.LogError(exception);
    }

    public void LogInfo(string message)
    {
        _inner.LogInfo(message);
    }

    public void LogInfo(string message, params object[] messageArgs)
    {
        _inner.LogInfo(message, messageArgs);
    }

    public void LogTrace(string message)
    {
        _inner.LogTrace(message);
    }

    public void LogTrace(Func<string> messageGenerator)
    {
        _inner.LogTrace(messageGenerator);
    }

    public void LogTrace(string message, params object[] messageArgs)
    {
        _inner.LogTrace(message, messageArgs);
    }

    public void LogWarning(string message)
    {
        _inner.LogWarning(message);
    }

    public void LogWarning(string format, params object[] args)
    {
        _inner.LogWarning(format, args);
    }

    public void LogWarning(Exception exception)
    {
        _inner.LogWarning(exception);
    }
}