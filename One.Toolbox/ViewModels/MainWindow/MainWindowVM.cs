using Avalonia;
using Avalonia.Styling;

using Microsoft.Extensions.DependencyInjection;

using One.Base.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainWindowVM : BaseVM
{
    [ObservableProperty]
    private string a;

    [ObservableProperty]
    private MainViewVM mainViewVM;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    private SettingService SettingService { get; set; }

    public MainWindowVM()
    {
        InitializeViewModel();
    }

    [RelayCommand]
    private void SetLanguage(object obj)
    {
        var language = obj as string;

        SettingService.ChangeLanguage(language.ToString());

        var a = I18nManager.GetString(Language.About);
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
        AppVersion = $"v{AssemblyHelper.Instance.ProductVersion} .NET 9.0";

        MainViewVM = App.Current!.Services.GetService<MainViewVM>()!;
        SettingService = App.Current!.Services.GetService<Services.SettingService>()!;

        MainViewVM.InitializeViewModel();
    }
}