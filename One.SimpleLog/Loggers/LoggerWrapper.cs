using One.SimpleLog.Configurations;
using One.SimpleLog.Enum;

using System.Diagnostics;

namespace One.SimpleLog.Loggers;

public class LoggerWrapper
{
    private readonly BaseLogger baseLogger;

    internal LoggerWrapper(BaseLogger baselogger)
    {
        this.baseLogger = baselogger;
    }

    internal void Log(string message, LogLevel level)
    {
        var pattern = LogConfigHelper.GetPattern();

        var newMessage = pattern
            .Replace("%Date", DateTime.Now.ToString(LogConfigHelper.GetDateFormat()))
            .Replace("%date", DateTime.Now.ToString(LogConfigHelper.GetDateFormat()))
            .Replace("%Level", level.ToString().ToUpper())
            .Replace("%level", level.ToString().ToUpper())
            .Replace("%Stack", $"{new StackTrace(true)}")
            .Replace("%stack", $"{new StackTrace(true)}")
            .Replace("%Message", message)
            .Replace("%message", message)
            .Replace("%Newline", Environment.NewLine)
            .Replace("%newline", Environment.NewLine);

        baseLogger.Log(newMessage, level);
    }

    internal void SetTargetProperty(string propertyName, string propertyValue)
    {
        baseLogger.targetDic[propertyName] = propertyValue;
    }

    internal void SetPatternProperty(string propertyName, string propertyValue)
    {
        baseLogger.patternDic[propertyName] = propertyValue;
    }

    public void Debug(string message)
    {
        Log(message, LogLevel.Debug);
    }

    public void Debug(Exception exception, string? message = null)
    {
        Log(FormatExceptionMessage(exception, message), LogLevel.Debug);
    }

    public void Info(string message)
    {
        Log(message, LogLevel.Info);
    }

    public void Info(Exception exception, string? message = null)
    {
        Log(FormatExceptionMessage(exception, message), LogLevel.Info);
    }

    public void Warn(string message)
    {
        Log(message, LogLevel.Warn);
    }

    public void Warn(Exception exception, string? message = null)
    {
        Log(FormatExceptionMessage(exception, message), LogLevel.Warn);
    }

    public void Error(string message)
    {
        Log(message, LogLevel.Error);
    }

    public void Error(Exception exception, string? message = null)
    {
        Log(FormatExceptionMessage(exception, message), LogLevel.Error);
    }

    public void Fatal(string message)
    {
        Log(message, LogLevel.Fatal);
    }

    public void Fatal(Exception exception, string? message = null)
    {
        Log(FormatExceptionMessage(exception, message), LogLevel.Fatal);
    }

    private static string FormatExceptionMessage(Exception exception, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return exception.ToString();

        return $"{message}{Environment.NewLine}{exception}";
    }
}
