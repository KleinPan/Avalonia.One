using Avalonia;
using Avalonia.Styling;

using Microsoft.Extensions.DependencyInjection;

using One.Base.Helpers;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainWindowVM : BaseVM
{
    [ObservableProperty]
    private MainViewVM mainViewVM = null!;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    private SettingService SettingService { get; set; } = null!;

    public MainWindowVM()
    {
        InitializeViewModel();
    }

    [RelayCommand]
    private void SetLanguage(object obj)
    {
        if (obj is string language)
        {
            SettingService.ChangeLanguage(language);
        }
    }

    [RelayCommand]
    private void SetTheme(object obj)
    {
        var app = Application.Current;
        if (app is not null)
        {
            if (obj is string themeName)
            {
                if (themeName == "Dark")
                {
                    app.RequestedThemeVariant = ThemeVariant.Dark;
                }
                else if (themeName == "Light")
                {
                    app.RequestedThemeVariant = ThemeVariant.Light;
                }
                else
                {
                    //自动
                }
            }
        }
    }

    public void InitializeViewModel()
    {
        AppVersion = $"v{AssemblyHelper.Instance.ProductVersion} .NET 10.0";

        MainViewVM = App.Current!.Services.GetService<MainViewVM>()!;
        SettingService = App.Current!.Services.GetService<Services.SettingService>()!;

        MainViewVM.InitializeViewModel();
    }
}
