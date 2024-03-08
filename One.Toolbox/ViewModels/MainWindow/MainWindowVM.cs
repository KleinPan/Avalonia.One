using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Helpers;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Dashboard;
using One.Toolbox.ViewModels.Setting;
using One.Toolbox.Views.Dashboard;
using One.Toolbox.Views.Settings;

using System.Collections.ObjectModel;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainWindowVM : BaseVM
{
    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MainMenuItemVM> _navigationItems = new();

    //[ObservableProperty]
    //private ObservableCollection<MenuItem> _trayMenuItems = new();

    [ObservableProperty]
    private MainMenuItemVM currentMenuItem;

    public MainWindowVM()
    {
    }

    public override void InitializeViewModel()
    {
        ApplicationTitle = "One.Toolbox";

        NavigationItems = new ObservableCollection<MainMenuItemVM>
        {
            //https://avaloniaui.github.io/icons.html
            //https://www.xicons.org/#/
            //https://pictogrammers.com/library/mdi/
            new()
            {
                Header = "Home",
                Icon =ResourceHelper. FindObjectResource("home_regular"),
                Content = new DashboardPage(){DataContext =App.Current!.Services.GetService<DashboardPageVM>()} ,
            },

            new()
            {
                 Header="Settings",
                 Icon =ResourceHelper. FindObjectResource("settings_regular"),
                 Content=new SettingsPage(){DataContext=App.Current.Services.GetService<SettingsPageVM>()},
            },
        };
        CurrentMenuItem = NavigationItems.First();

        base.InitializeViewModel();
    }

    #region 框架逻辑

    partial void OnCurrentMenuItemChanged(MainMenuItemVM? oldValue, MainMenuItemVM newValue)
    {
        if (oldValue != null)
        {
            var vm = oldValue.Content.DataContext as BaseVM;
            vm.OnNavigatedLeave();
        }
        if (newValue != null)
        {
            var vmNew = newValue.Content.DataContext as BaseVM;
            vmNew.OnNavigatedEnter();
        }
    }

    #endregion 框架逻辑
}