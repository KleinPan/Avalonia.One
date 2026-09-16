using System;

using Avalonia;

using One.Toolbox.Helpers;

namespace One.Toolbox.Desktop;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code before AppMain is called: things aren't initialized yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        StartupTracker.Start(); // 启动性能打点，必须最先调用

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
#if DEBUG
            .LogToTrace() // Trace 日志仅 Debug 启用，Release 减少输出开销
#endif
            ;
}