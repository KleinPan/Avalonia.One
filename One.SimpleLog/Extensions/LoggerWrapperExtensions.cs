using One.SimpleLog.Loggers;

namespace One.SimpleLog.Extensions;

public static class LoggerWrapperExtensions
{
    public static LoggerWrapper WithTargetProperty(this LoggerWrapper wrapper, string propertyName, string propertyValue)
    {
        wrapper.SetTargetProperty(propertyName, propertyValue);
        return wrapper;
    }
    public static LoggerWrapper WithPatternProperty(this LoggerWrapper wrapper, string propertyName, string propertyValue)
    {
        wrapper.SetPatternProperty(propertyName, propertyValue);
        return wrapper;
    }
}
