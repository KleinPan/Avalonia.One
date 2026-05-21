using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Markup.I18n;
using One.SimpleLog;
using One.SimpleLog.Extensions;
using One.SimpleLog.Loggers;
using One.Toolbox.Services;
using One.Toolbox.Services.Navigation;
using One.Toolbox.ViewModels.IconBoard;
using One.Toolbox.ViewModels.MainWindow;
using One.Toolbox.ViewModels.Setting;
using One.Toolbox.Views;

using System.Globalization;

namespace One.Toolbox;

public partial class App : Application
{
    public static LoggerWrapper logger = LogManager.GetLogger();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public new static App? Current => Application.Current as App;

    /// <summary>Gets the <see cref="IServiceProvider"/> instance to resolve application services.</summary>
    public IServiceProvider Services { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation. Without this line you will get
        // duplicate validations from both Avalonia and CT
        //BindingPlugins.DataValidators.RemoveAt(0);

        I18nManager.Instance.AddResource(typeof(App).Assembly);
        I18nManager.Instance.Culture = CultureInfo.CurrentUICulture;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Services = ConfigureServices(desktop);

            MainWindowVM vm = new MainWindowVM();
            desktop.MainWindow = new MainWindow { DataContext = vm };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = new MainViewVM() };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>Configures the services for the application.</summary>
    private static IServiceProvider ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow!));
        services.AddSingleton<INotifyService>(x => new NotifyService(desktop.MainWindow!));
        services.AddSingleton<SettingService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<BingImageService>();
        services.AddSingleton<CloudSettingSyncService>();

        services.AddSingleton<MainWindowVM>();
        services.AddSingleton<MainViewVM>();
        services.AddSingleton<CloudSettingsVM>();
        services.AddSingleton<IconBoardPageVM>();

        ToolPageRegistry.RegisterAll(services);

        return services.BuildServiceProvider();
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
        else
        {
            // 调试：如果没进 if，说明生命周期类型不对
            System.Diagnostics.Debug.WriteLine("无法获取 Desktop 模式生命周期");

            // 环境兜底（不推荐作为首选，但在调试时有用）
              Environment.Exit(0); 
        }
    }

    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.IsVisible = true;
        }
    }

    public virtual void WriteDebugLog(string msg)
    {
        logger.WithPatternProperty("ThreadID", Thread.CurrentThread.ManagedThreadId.ToString()).Debug(msg);
    }
}
