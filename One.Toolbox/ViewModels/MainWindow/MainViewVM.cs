using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Helpers;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.BingImage;
using One.Toolbox.ViewModels.Dashboard;
using One.Toolbox.ViewModels.DataProcess;
using One.Toolbox.ViewModels.Note;
using One.Toolbox.ViewModels.Setting;
using One.Toolbox.Views.BingImage;
using One.Toolbox.Views.Dashboard;
using One.Toolbox.Views.DataProcess;
using One.Toolbox.Views.Note;
using One.Toolbox.Views.Settings;

using System.Collections.ObjectModel;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainViewVM : BaseVM
{
    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MainMenuItemVM> _navigationItems =
    [
        new MainMenuItemVM()
        {
             Header = "Home",
             Icon =ResourceHelper. FindObjectResource("home_regular"),
        },
        new()
        {
            Header = "Images",
            Icon =ResourceHelper. FindObjectResource("image_library_regular"),
        },
        new()
        {
            Header = "Texts",
            Icon =ResourceHelper. FindObjectResource("text_number_format_regular"),
        },
        new()
        {
            Header = "Notes",
            Icon =ResourceHelper. FindObjectResource("notepad_regular"),
        },
        new()
        {
             Dock=Avalonia.Controls.Dock.Bottom,
             Header="Settings",
             Icon =ResourceHelper. FindObjectResource("settings_regular"),
        },
    ];

    [ObservableProperty]
    private MainMenuItemVM currentMenuItem;

    public MainViewVM()
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
                Header = "Images",
                Icon =ResourceHelper. FindObjectResource("image_library_regular"),
                Content = new BingImagePage(){DataContext =App.Current!.Services.GetService<BingImagePageVM>()} ,
            },
            new()
            {
                Header = "Texts",
                Icon =ResourceHelper. FindObjectResource("text_number_format_regular"),
                Content = new DataProcessPage(){DataContext =App.Current!.Services.GetService<DataProcessPageVM>()} ,
            },
            new()
            {
                Header = "Notes",
                Icon =ResourceHelper. FindObjectResource("notepad_regular"),
                Content = new NotePage(){DataContext =App.Current!.Services.GetService<NotePageVM>()} ,
            },
            new()
            {
                 Header="Settings",
                 Dock=Avalonia.Controls.Dock.Bottom,
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