namespace NoeticTools.Common.Logging;

/// <summary>
///     An internally used interface.
/// </summary>
public interface ITaskLoggerAdapter
{
    void LogDebug(string message);
    void LogError(string message);
    void LogError(Exception exception);
    void LogInfo(string message);
    void LogTrace(string message);
    void LogWarning(string message);
    void LogWarning(Exception exception);
}