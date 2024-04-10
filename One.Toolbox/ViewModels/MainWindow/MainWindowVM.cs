using Avalonia;
using Avalonia.Styling;

using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Setting;

using System.Globalization;
using System.Threading;

namespace One.Toolbox.ViewModels.MainWindow
{
    public partial class MainWindowVM : BaseVM
    {
        [ObservableProperty]
        private string a;

        [ObservableProperty]
        private MainViewVM mainViewVM;

        //[ObservableProperty]
        //private SettingsPageVM settingsPageVM;

        [ObservableProperty]
        private string _appVersion = string.Empty;

        public MainWindowVM()
        {
            //MainViewVM = new MainViewVM();
            //MainViewVM.InitializeViewModel();
        }

        /// <summary> 目前不能动态切换，只能在启动前 </summary>
        /// <param name="obj"> </param>
        [RelayCommand]
        private void SetLanguage(object obj)
        {
            var language = obj as string;
            Assets.Languages.Resource.Culture = new CultureInfo(language);
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

        public override void InitializeViewModel()
        {
            //AppVersion = $"v{GetAssemblyVersion()} .NET 8.0";
            AppVersion = $"v{One.Core.Helpers.AssemblyHelper.Instance.ProductVersion} .NET 8.0";

            MainViewVM = App.Current!.Services.GetService<MainViewVM>()!;
            //SettingsPageVM = App.Current!.Services.GetService<SettingsPageVM>()!;
            base.InitializeViewModel();

            MainViewVM.InitializeViewModel();
        }
    }
}