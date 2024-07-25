using One.SimpleLog.Configurations;
using One.SimpleLog.Datas;
using One.SimpleLog.Enum;
using One.SimpleLog.Helpers;

using System;
using System.Collections.Generic;

namespace One.SimpleLog.Loggers;

public abstract class BaseLogger
{
    internal Dictionary<string, string> targetDic = new();

    internal Dictionary<string, string> patternDic = new();
    internal abstract void Log(string message, LogLevel level);

    internal virtual void SetProperty(string propertyName, string propertyValue) { }

    internal virtual bool IsWriteable(LogLevel level)
    {
        return LogConfigHelper.GetLevel() switch
        {
            "Debug" => true,
            "Info" => level >= LogLevel.Info,
            "Warn" => level >= LogLevel.Warn,
            "Error" => level >= LogLevel.Error,
            "Fatal" => level >= LogLevel.Fatal,
            _ => false,
        };
    }

    internal string ReplaceTargetProperty(string inputString, string propertyName = "property")
    {
        return LogHelper.ReplaceTargetProperty(targetDic, inputString, propertyName);
    }

    internal string ReplacePatternProperty(string inputString, string propertyName = "property")
    {
        return LogHelper.ReplaceTargetProperty(patternDic, inputString, propertyName);
    }

    internal string ReplacePredefinedProperty(string inputString)
    {
        var func = new Func<string, string>(arg1 => DateTime.Now.ToString(arg1));
        var temp = LogHelper.ReplacePredefinedProperty(PredefinedData.PredefinedTimeFormate, inputString, func);

        return temp;
    }
}
