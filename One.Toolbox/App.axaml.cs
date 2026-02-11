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
using One.Toolbox.ViewModels.DataProcess;
using One.Toolbox.ViewModels.HashTool;
using One.Toolbox.ViewModels.IconBoard;
using One.Toolbox.ViewModels.MainWindow;
using One.Toolbox.ViewModels.QRCode;
using One.Toolbox.ViewModels.RegularTester;
using One.Toolbox.ViewModels.Serialport;
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
        BindingPlugins.DataValidators.RemoveAt(0);

        I18nManager.Instance.Culture = new CultureInfo("zh-CN");

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

        // 核心服务注册
        services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow!));
        services.AddSingleton<INotifyService>(x => new NotifyService(desktop.MainWindow!));
        services.AddSingleton<SettingService>();

        // Views and ViewModels
        services.AddSingleton<MainWindowVM>();
        services.AddSingleton<MainViewVM>();
        services.AddSingleton<One.Toolbox.ViewModels.Dashboard.DashboardPageVM>();
        services.AddSingleton<DataProcessPageVM>();
        services.AddSingleton<One.Toolbox.ViewModels.Setting.SettingsPageVM>();
        services.AddSingleton<One.Toolbox.ViewModels.Note.NotePageVM>();
        services.AddSingleton<One.Toolbox.ViewModels.Setting.CloudSettingsVM>();
        services.AddSingleton<One.Toolbox.ViewModels.BingImage.BingImagePageVM>();
        services.AddSingleton<QRCodePageVM>();
        services.AddSingleton<HashToolPageVM>();
        services.AddSingleton<IconBoardPageVM>();
        services.AddSingleton<One.Toolbox.ViewModels.UnixTimeConverter.UnixTimeConverterVM>();
        services.AddSingleton<RegularTesterPageVM>();
 
        services.AddSingleton<SerialportPageVM>();

        return services.BuildServiceProvider();
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
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