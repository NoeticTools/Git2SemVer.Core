using Injectio.Attributes;
using Spectre.Console;


namespace NoeticTools.Git2SemVer.Core.Logging;

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
        if (Level < LoggingLevel.Debug)
        {
            return;
        }

        message = IndentLines(message);
        Console.Out.WriteLine(message);
    }

    public void LogDebug(string message, params object[] messageArgs)
    {
        if (Level < LoggingLevel.Debug)
        {
            return;
        }

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
        if (Level < LoggingLevel.Info)
        {
            return;
        }

        message = IndentLines(message);
        Console.Out.WriteLine(message);
    }

    public void LogInfo(string message, params object[] messageArgs)
    {
        if (Level < LoggingLevel.Info)
        {
            return;
        }

        LogInfo(string.Format(message, messageArgs));
    }

    public void LogTrace(string message)
    {
        if (Level < LoggingLevel.Trace)
        {
            return;
        }

        message = IndentLines(message);
        AnsiConsole.MarkupLine("[grey50]" + message + "[/]");
    }

    public void LogTrace(string message, params object[] messageArgs)
    {
        if (Level < LoggingLevel.Trace)
        {
            return;
        }

        LogTrace(string.Format(message, messageArgs));
    }

    public void LogWarning(string message)
    {
        if (Level < LoggingLevel.Warning)
        {
            return;
        }

        message = IndentLines(message);
        AnsiConsole.MarkupLine("[fuchsia]" + message + "[/]");
    }

    public void LogWarning(string format, params object[] args)
    {
        if (Level < LoggingLevel.Warning)
        {
            return;
        }

        LogWarning(string.Format(format, args));
    }

    public void LogWarning(Exception exception)
    {
        if (Level < LoggingLevel.Warning)
        {
            return;
        }

        LogWarning($"Exception - {exception.Message}");
    }

    private string IndentLines(string message)
    {
        message = message.Replace(Environment.NewLine, Environment.NewLine + LogPrefix);
        return LogPrefix + message;
    }

    private void LeaveLogScope()
    {
        LogPrefix = LogPrefix.Substring(0, LogPrefix.Length - LogScopeIndent.Length);
    }
}