using System.Diagnostics;

using One.SimpleLog;
using One.SimpleLog.Extensions;
using One.SimpleLog.Loggers;

namespace One.Toolbox.Helpers;

/// <summary>启动性能打点：记录关键节点耗时（总耗时 + 相对上一节点增量），用于建立启动基线和量化优化效果。日志输出到 SimpleLog 文件，同时写入调试器输出。</summary>
public static class StartupTracker
{
    private static LoggerWrapper? logger;
    private static long startTimestamp;
    private static long lastTimestamp;
    private static bool firstNavigationMarked;

    /// <summary>在 Main 入口最先调用，开始计时。日志初始化在计时开始前完成，不计入耗时。</summary>
    public static void Start()
    {
        logger = LogManager.GetLogger();
        startTimestamp = Stopwatch.GetTimestamp();
        lastTimestamp = startTimestamp;
    }

    /// <summary>记录一个启动节点：输出自启动以来的总耗时和相对上一节点的增量。</summary>
    public static void Mark(string milestone)
    {
        if (startTimestamp == 0)
        {
            return;
        }

        var now = Stopwatch.GetTimestamp();
        var total = Stopwatch.GetElapsedTime(startTimestamp, now).TotalMilliseconds;
        var delta = Stopwatch.GetElapsedTime(lastTimestamp, now).TotalMilliseconds;
        lastTimestamp = now;

        var message = $"[Startup] {milestone} | 总耗时: {total:F1}ms | 增量: {delta:F1}ms";

        logger?.WithPatternProperty("ThreadID", Environment.CurrentManagedThreadId.ToString()).Info(message);
        Debug.WriteLine(message);
    }

    /// <summary>仅记录一次首次页面导航（首页开始加载，启动路径收尾）。</summary>
    public static void MarkFirstNavigation()
    {
        if (firstNavigationMarked)
        {
            return;
        }

        firstNavigationMarked = true;
        Mark("首次页面导航（首页开始加载）");
    }
}
