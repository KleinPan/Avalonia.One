using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using One.SimpleLog.Configurations;
using One.SimpleLog.Enum;

namespace One.SimpleLog.Loggers;

internal class FileLogger : BaseLogger
{
    private readonly object _lock = new object();

    internal override void Log(string message, LogLevel level)
    {
        if (!IsWriteable(level))
            return;

        var logPath = ResolveLogPath();
        var newMsg = ReplacePatternProperty(message);

        lock (_lock)
        {
            CheckDirectory(logPath);
            CheckRollBackups(logPath);
            File.AppendAllText(logPath, newMsg);
        }
    }

    private string ResolveLogPath()
    {
        var logPath = LogConfigHelper.GetLogFile();
        logPath = ReplacePredefinedProperty(logPath);
        logPath = ReplaceTargetProperty(logPath);
        return logPath;
    }

    /// <summary>
    /// 检查是否需要滚动日志
    /// </summary>
    private static void CheckRollBackups(string logPath)
    {
        var maxRollTime = LogConfigHelper.GetMaxRollTime();
        var maxRollSize = LogConfigHelper.GetMaxRollSize();
        var file = new FileInfo(logPath);
        if (!file.Exists)
            return;

        var fileSizeInBytes = file.Length;
        var timeDifference = DateTime.Now - file.LastWriteTime;

        if ((maxRollTime > 0 && timeDifference.TotalSeconds >= maxRollTime) || (maxRollSize > 0 && fileSizeInBytes >= maxRollSize))
            RollBackups(logPath);
    }

    /// <summary>
    /// 滚动日志
    /// </summary>
    private static void RollBackups(string logPath)
    {
        var now = DateTime.Now;
        var strNow = $"{now:yyyy-MM-dd_HH-mm-ss}";
        var maxRollBackups = LogConfigHelper.GetMaxRollBackups();
        try
        {
            if (maxRollBackups > 0)
                DeleteOldRollBackups(logPath, maxRollBackups);

            File.Move(logPath, $"{logPath}.{strNow}");
        }
        catch (Exception ex)
        {
            var errorMessage = $"{strNow} RollError {ex.Message} {ex.StackTrace}{Environment.NewLine}";
            File.AppendAllText(logPath, errorMessage);
        }
    }

    /// <summary>
    /// 删除旧的滚动日志，保持最大滚动日志数量
    /// </summary>
    /// <param name="maxRollBackups">最大日志数量</param>
    private static void DeleteOldRollBackups(string logPath, int maxRollBackups)
    {
        var fileName = Path.GetFileName(logPath);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new NullReferenceException($"GetFileName Error: logPath is {logPath}");

        var fileDir = Path.GetDirectoryName(logPath);
        if (string.IsNullOrWhiteSpace(fileDir))
            fileDir = Environment.CurrentDirectory;

        var regexPattern = $@"^{Regex.Escape(fileName)}\.\d{{4}}-\d{{2}}-\d{{2}}_\d{{2}}-\d{{2}}-\d{{2}}$";
        var files = Directory.GetFiles(fileDir)
            .Where(f => Regex.IsMatch(Path.GetFileName(f), regexPattern))
            .OrderByDescending(Path.GetFileName)
            .ToArray();

        if (files.Length < maxRollBackups)
            return;

        for (int i = maxRollBackups - 1; i < files.Length; i++)
            File.Delete(files[i]);
    }

    private static void CheckDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
