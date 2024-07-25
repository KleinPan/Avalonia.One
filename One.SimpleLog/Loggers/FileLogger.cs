using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using One.SimpleLog.Configurations;
using One.SimpleLog.Enum;
using static System.Net.Mime.MediaTypeNames;

namespace One.SimpleLog.Loggers;

internal class FileLogger : BaseLogger
{
    private readonly object _lock = new object();
    string logPath;

    internal override void Log(string message, LogLevel level)
    {
        logPath = LogConfigHelper.GetLogFile();

        logPath = ReplacePredefinedProperty(logPath);
        logPath = ReplaceTargetProperty(logPath);

        if (IsWriteable(level))
        {
            var newMsg = ReplacePatternProperty(message);
            lock (_lock)
            {
                CheckRollBackups();
                CheckDirectory(logPath);
                File.AppendAllText(logPath, newMsg);
            }
        }
    }

    /// <summary>
    /// 检查是否需要滚动日志
    /// </summary>
    private static void CheckRollBackups()
    {
        var maxRollTime = LogConfigHelper.GetMaxRollTime();
        var maxRollSize = LogConfigHelper.GetMaxRollSize();
        var file = new FileInfo(LogConfigHelper.GetLogFile());
        if (!file.Exists)
            return;

        var fileSizeInBytes = file.Length;
        var timeDifference = DateTime.Now - file.CreationTime;

        if ((maxRollTime > 0 && timeDifference.TotalSeconds >= maxRollTime) || (maxRollSize > 0 && fileSizeInBytes >= maxRollSize))
        {
            RollBackups();
        }
    }

    /// <summary>
    /// 滚动日志
    /// </summary>
    private static void RollBackups()
    {
        var now = DateTime.Now;
        var strNow = $"{now:yyyy-MM-dd_HH-mm-ss}";
        var logPath = LogConfigHelper.GetLogFile();
        var maxRollBackups = LogConfigHelper.GetMaxRollBackups();
        try
        {
            if (maxRollBackups > 0)
                DeleteOldRollBackups(maxRollBackups);

            File.Move(logPath, $"{logPath}.{strNow}");
            File.Create(logPath).Close();
            File.SetCreationTime(logPath, now);
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
    private static void DeleteOldRollBackups(int maxRollBackups)
    {
        var logPath = LogConfigHelper.GetLogFile();

        var fileName = Path.GetFileName(logPath);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new NullReferenceException($"GetFileName Error: logPath is {logPath}");

        var fileDir = Path.GetDirectoryName(logPath);
        if (string.IsNullOrWhiteSpace(fileDir))
            fileDir = Environment.CurrentDirectory;

        // 过滤日志备份文件
        var regexPattern = $@"^{Regex.Escape(fileName)}\..*";
        var files = Directory.GetFiles(fileDir).Where(f => Regex.IsMatch(Path.GetFileName(f), regexPattern)).ToArray();

        if (files == null)
            return;

        if (files.Length >= maxRollBackups)
        {
            // 按创建时间排序
            Array.Sort(files, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            for (int i = 0; i <= files.Length - maxRollBackups; i++)
                File.Delete(files[i]);
        }
    }

    private static void CheckDirectory(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
    }
}
