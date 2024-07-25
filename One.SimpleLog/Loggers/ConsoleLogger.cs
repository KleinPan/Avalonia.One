using System;
using System.Diagnostics;

using One.SimpleLog.Enum;

namespace One.SimpleLog.Loggers;

internal class ConsoleLogger : BaseLogger
{
    internal override void Log(string message, LogLevel level)
    {
        Console.ForegroundColor = level switch
        {
            LogLevel.Debug => ConsoleColor.DarkGreen,
            LogLevel.Info => ConsoleColor.Gray,
            LogLevel.Warn => ConsoleColor.DarkYellow,
            LogLevel.Error => ConsoleColor.DarkRed,
            LogLevel.Fatal => ConsoleColor.Magenta,
            _ => ConsoleColor.White,
        };
        if (IsWriteable(level))
        {
            var newMes = ReplacePredefinedProperty(message);
            newMes = ReplacePatternProperty(newMes);
            Console.WriteLine(newMes);
        }

        Console.ResetColor(); // finally reset
    }
}
