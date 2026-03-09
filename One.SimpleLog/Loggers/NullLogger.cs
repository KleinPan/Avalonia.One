using One.SimpleLog.Enum;

namespace One.SimpleLog.Loggers;

internal sealed class NullLogger : BaseLogger
{
    internal override void Log(string message, LogLevel level)
    {
    }
}
