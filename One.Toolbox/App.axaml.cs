using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.ViewModels;
using One.Toolbox.ViewModels.DataProcess;
using One.Toolbox.ViewModels.FileMonitor;
using One.Toolbox.ViewModels.HashTool;
using One.Toolbox.ViewModels.IconBoard;
using One.Toolbox.ViewModels.MainWindow;
using One.Toolbox.ViewModels.QRCode;
using One.Toolbox.Views;

namespace One.Toolbox;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static new App? Current => Application.Current as App;

    /// <summary> Gets the <see cref="IServiceProvider"/> instance to resolve application services. </summary>
    public IServiceProvider Services { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation. Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new MainWindowVM();
            desktop.MainWindow = new MainWindow { DataContext = vm };

            Services = ConfigureServices(desktop);

            vm.InitializeViewModel();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = new MainViewVM() };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary> Configures the services for the application. </summary>
    private static IServiceProvider ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = new ServiceCollection();

        // App Host
        //Scoped 1指定将为每个作用域创建服务的新实例。 在 ASP.NET Core 应用中，会针对每个服务器请求创建一个作用域。
        //Singleton	0指定将创建该服务的单个实例。
        //Transient 2指定每次请求服务时，将创建该服务的新实例。
        services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));
        services.AddSingleton<INotifyService>(x => new NotifyService(desktop.MainWindow));
        //services.AddSingleton(new NotifyService(desktop.MainWindow));//这样也OK，获取的时候不用接口

        // Views and ViewModels
        services.AddSingleton<ViewModels.MainWindow.MainWindowVM>();

        services.AddSingleton<ViewModels.MainWindow.MainViewVM>();

        services.AddSingleton<ViewModels.Dashboard.DashboardPageVM>();

        services.AddSingleton<DataProcessPageVM>();

        services.AddSingleton<ViewModels.Setting.SettingsPageVM>();

        services.AddSingleton<ViewModels.Note.NotePageVM>();
        services.AddSingleton<ViewModels.Setting.CloudSettingsVM>();

        services.AddSingleton<ViewModels.BingImage.BingImagePageVM>();

        //Services
        services.AddSingleton<Services.SettingService>();

        services.AddSingleton<QRCodePageVM>();
        services.AddSingleton<HashToolPageVM>();
        services.AddSingleton<IconBoardPageVM>();
        services.AddSingleton<FileMonitorPageVM>();

        //多例
        //services.AddTransient<StickItemVM>();

        return services.BuildServiceProvider();
    }

    private void InitDataColelection() { }
}
