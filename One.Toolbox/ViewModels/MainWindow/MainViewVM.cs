using Microsoft.Extensions.DependencyInjection;

using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Helpers;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.BingImage;
using One.Toolbox.ViewModels.Dashboard;
using One.Toolbox.ViewModels.DataProcess;
using One.Toolbox.ViewModels.FileMonitor;
using One.Toolbox.ViewModels.HashTool;
using One.Toolbox.ViewModels.IconBoard;
using One.Toolbox.ViewModels.Note;
using One.Toolbox.ViewModels.QRCode;
using One.Toolbox.ViewModels.RegularTester;
using One.Toolbox.ViewModels.Setting;
using One.Toolbox.ViewModels.UnixTimeConverter;
using One.Toolbox.Views;
using One.Toolbox.Views.BingImage;
using One.Toolbox.Views.Dashboard;
using One.Toolbox.Views.DataProcess;
using One.Toolbox.Views.FileMonitor;
using One.Toolbox.Views.HashTool;
using One.Toolbox.Views.IconBoard;
using One.Toolbox.Views.Note;
using One.Toolbox.Views.QRCode;
using One.Toolbox.Views.RegularTester;
using One.Toolbox.Views.Settings;
using One.Toolbox.Views.UnixTimeConverter;

using System.Collections.ObjectModel;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainViewVM : BaseVM
{
    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MainMenuItemVM> _navigationItems =
    [
        new MainMenuItemVM() { Header = "Home", Icon = ResourceHelper.FindObjectResource("home_regular"), },
        new() { Header = "Images", Icon = ResourceHelper.FindObjectResource("image_library_regular"), },
        new() { Header = "Texts", Icon = ResourceHelper.FindObjectResource("text_number_format_regular"), },
        new() { Header = "Notes", Icon = ResourceHelper.FindObjectResource("notepad_regular"), },
        new() { Header = "HashTools", Icon = ResourceHelper.FindObjectResource("premium_regular"), },
        new()
        {
            Dock = Avalonia.Controls.Dock.Bottom,
            Header = "Settings",
            Icon = ResourceHelper.FindObjectResource("settings_regular"),
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
                Icon = ResourceHelper.FindObjectResource("home_regular"),
                Content = new DashboardPage() { DataContext = App.Current!.Services.GetService<DashboardPageVM>() },
            },
            new()
            {
                Header = "Images",
                Icon = ResourceHelper.FindObjectResource("image_library_regular"),
                Content = new BingImagePage() { DataContext = App.Current!.Services.GetService<BingImagePageVM>() },
            },
            new()
            {
                Header = "Texts",
                Icon = ResourceHelper.FindObjectResource("text_edit_style_regular"),
                Content = new DataProcessPage() { DataContext = App.Current!.Services.GetService<DataProcessPageVM>() },
            },
            new()
            {
                Header = "Notes",
                Icon = ResourceHelper.FindObjectResource("notepad_regular"),
                Content = new NotePage() { DataContext = App.Current!.Services.GetService<NotePageVM>() },
            },
            new()
            {
                Header = "HashTools",
                Icon = ResourceHelper.FindObjectResource("premium_regular"),
                Content = new HashToolPage() { DataContext = App.Current!.Services.GetService<HashToolPageVM>() },
            },
            new()
            {
                Header = I18nManager.GetString(Language.RegTestTool),//待研究，更换语言后这里不更新
                Icon = ResourceHelper.FindObjectResource("teddy_regular"),
                Content = new RegularTesterPage() { DataContext = App.Current!.Services.GetService<RegularTesterVM>() },
            },
            new()
            {
                Header = "IconBoard",
                Icon = ResourceHelper.FindObjectResource("icons_regular"),
                Content = new IconBoardPage() { DataContext = App.Current!.Services.GetService<IconBoardPageVM>() },
            },
            new()
            {
                Header = "QRCode",
                Icon = ResourceHelper.FindObjectResource("qr_code_regular"),
                Content = new QRCodePage() { DataContext = App.Current!.Services.GetService<QRCodePageVM>() },
            },
            new()
            {
                Header =  I18nManager.GetString(Language.TimeConvert),
                Icon = ResourceHelper.FindObjectResource("timer_regular"),
                Content = new UnixTimeConverterPage() { DataContext = App.Current!.Services.GetService<UnixTimeConverterVM>() },
            },

            new()
            {
                Header = "Settings",
                Dock = Avalonia.Controls.Dock.Bottom,
                Icon = ResourceHelper.FindObjectResource("settings_regular"),
                Content = new SettingsPage() { DataContext = App.Current.Services.GetService<SettingsPageVM>() },
            },
        };
        //判断平台
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            NavigationItems.Add(new()
            {
                Header = "FileMonitor",
                Icon = ResourceHelper.FindObjectResource("lock_regular"),
                Content = new FileMonitorPage() { DataContext = App.Current!.Services.GetService<FileMonitorPageVM>() },
            });
        }
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