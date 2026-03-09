using One.SimpleLog.Configurations;
using One.SimpleLog.Loggers;

namespace One.SimpleLog;

public static class LogManager
{
    private static readonly object Locker = new();
    private static LoggerWrapper? logger;

    /// <summary>
    /// 获得日志记录器
    /// </summary>
    public static LoggerWrapper GetLogger()
    {
        if (logger != null)
            return logger;

        lock (Locker)
        {
            if (logger != null)
                return logger;

            logger = LogConfigHelper.GetTarget() switch
            {
                "Console" => new LoggerWrapper(new ConsoleLogger()),
                "File" => new LoggerWrapper(new FileLogger()),
                _ => new LoggerWrapper(new NullLogger()),
            };

            return logger;
        }
    }
}
