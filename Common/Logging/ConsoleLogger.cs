using Injectio.Attributes;
using Spectre.Console;


namespace NoeticTools.Common.Logging;

[RegisterTransient]
public class ConsoleLogger : ILogger
{
    private const string LogScopeIndent = "  ";
    private readonly List<string> _errorMessages = [];

    public string Errors => string.Join("\n", _errorMessages);

    public bool HasError { get; private set; }

    public LoggingLevel Level { get; set; } = LoggingLevel.Info;

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
        if (Level <= level)
        {
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
    }

    public void LogDebug(string message)
    {
        if (Level <= LoggingLevel.Debug)
        {
            Console.Out.WriteLine(LogPrefix + message);
        }
    }

    public void LogDebug(string message, params object[] messageArgs)
    {
        LogDebug(string.Format(message, messageArgs));
    }

    public void LogError(string message)
    {
        HasError = true;
        _errorMessages.Add(message);
        AnsiConsole.MarkupLine("[red]" + message + "[/]");
    }

    public void LogError(string message, params object[] messageArgs)
    {
        LogError(LogPrefix + string.Format(message, messageArgs));
    }

    public void LogError(Exception exception)
    {
        HasError = true;
        var message = $"Exception - {exception.Message}\nStack trace: {exception.StackTrace}";
        LogError(message);
    }

    public void LogInfo(string message)
    {
        Console.Out.WriteLine(LogPrefix + message);
    }

    public void LogInfo(string message, params object[] messageArgs)
    {
        LogInfo(string.Format(message, messageArgs));
    }

    public void LogTrace(string message)
    {
        if (Level >= LoggingLevel.Trace)
        {
            Console.Out.WriteLine(LogPrefix + message);
        }
    }

    public void LogTrace(string message, params object[] messageArgs)
    {
        if (Level >= LoggingLevel.Trace)
        {
            LogTrace(string.Format(message, messageArgs));
        }
    }

    public void LogWarning(string message)
    {
        if (Level >= LoggingLevel.Warning)
        {
            Console.Out.WriteLine(LogPrefix + message);
            AnsiConsole.MarkupLine("[fuchsia]" + message + "[/]");
        }
    }

    public void LogWarning(string format, params object[] args)
    {
        LogWarning(string.Format(format, args));
    }

    public void LogWarning(Exception exception)
    {
        LogWarning($"Exception - {exception.Message}");
    }

    private void LeaveLogScope()
    {
        LogPrefix = LogPrefix.Substring(0, LogPrefix.Length - LogScopeIndent.Length);
    }
}