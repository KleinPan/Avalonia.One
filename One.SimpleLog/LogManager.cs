using One.SimpleLog.Configurations;
using One.SimpleLog.Loggers;

namespace One.SimpleLog;

public static class LogManager
{
    /// <summary>
    /// 获得日志记录器
    /// </summary>
    public static LoggerWrapper GetLogger()
    {
        switch (LogConfigHelper.GetTarget())
        {
            case "Console":
                return new LoggerWrapper(new ConsoleLogger());
            case "File":
                return new LoggerWrapper(new FileLogger());
            default:
                return null;
        }
    }
}
