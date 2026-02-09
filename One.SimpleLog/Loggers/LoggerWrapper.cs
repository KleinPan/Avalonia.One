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
            .Replace("%Date", DateTime.Now.ToString(LogConfigHelper.GetDateFormat())) // FormatException
            .Replace("%Level", level.ToString().ToUpper())
            .Replace("%Stack", $"{new StackTrace(true)}")
            .Replace("%Message", message)
            .Replace("%Newline", Environment.NewLine);

        //.Replace("%thread",$"{Thread.CurrentThread.Name ?? ""}:{Environment.CurrentManagedThreadId}") //线程没有名称就不打印

        baseLogger?.Log(newMessage, level);
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

    public void Info(string message)
    {
        Log(message, LogLevel.Info);
    }

    public void Warn(string message)
    {
        Log(message, LogLevel.Warn);
    }

    public void Error(string message)
    {
        Log(message, LogLevel.Error);
    }

    public void Fatal(string message)
    {
        Log(message, LogLevel.Fatal);
    }
}