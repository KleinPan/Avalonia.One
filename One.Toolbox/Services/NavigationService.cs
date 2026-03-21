using Avalonia.Controls;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Helpers;
using One.Toolbox.ViewModels.MainWindow;
using One.Toolbox.Views.BingImage;
using One.Toolbox.Views.Dashboard;
using One.Toolbox.Views.DataProcess;
using One.Toolbox.Views.DiffViewer;
using One.Toolbox.Views.HashTool;
using One.Toolbox.Views.Note;
using One.Toolbox.Views.QRCode;
using One.Toolbox.Views.RegularTester;
using One.Toolbox.Views.Settings;
using One.Toolbox.Views.Todo;
using One.Toolbox.Views.UnixTimeConverter;
using One.Toolbox.Views;

using System.Collections.ObjectModel;

namespace One.Toolbox.Services;

public sealed class NavigationService
{
    private readonly IServiceProvider services;

    public NavigationService(IServiceProvider services)
    {
        this.services = services;
    }

    public ObservableCollection<MainMenuItemVM> CreateNavigationItems()
    {
        return
        [
            CreateItem("Home", "home_regular", () => CreatePage<DashboardPage, ViewModels.Dashboard.DashboardPageVM>()),
            CreateItem("Serialport", "SerialPort24Filled", () => CreatePage<SerialportPage, ViewModels.Serialport.SerialportPageVM>()),
            CreateItem("NetTool", "organization_regular", () => CreatePage<NetToolPage, ViewModels.NetTool.NetToolPageVM>()),
            CreateItem("Images", "image_library_regular", () => CreatePage<BingImagePage, ViewModels.BingImage.BingImagePageVM>()),
            CreateItem("Texts", "text_edit_style_regular", () => CreatePage<DataProcessPage, ViewModels.DataProcess.DataProcessPageVM>()),
            CreateItem("DiffViewer", "text_number_format_regular", () => CreatePage<DiffViewerPage, ViewModels.DiffViewer.DiffViewerPageVM>()),
            CreateItem("Notes", "notepad_regular", () => CreatePage<NotePage, ViewModels.Note.NotePageVM>()),
            CreateItem("Todo", "text_bullet_list_regular", () => CreatePage<TodoPage, ViewModels.Todo.TodoPageVM>()),
            CreateItem("HashTools", "premium_regular", () => CreatePage<HashToolPage, ViewModels.HashTool.HashToolPageVM>()),
            CreateItem(I18nManager.GetString(Language.RegTestTool)!, "teddy_regular", () => CreatePage<RegularTesterPage, ViewModels.RegularTester.RegularTesterPageVM>()),
            CreateItem("QRCode", "qr_code_regular", () => CreatePage<QRCodePage, ViewModels.QRCode.QRCodePageVM>()),
            CreateItem(I18nManager.GetString(Language.TimeConvert)!, "timer_regular", () => CreatePage<UnixTimeConverterPage, ViewModels.UnixTimeConverter.UnixTimeConverterVM>()),
            CreateItem("Settings", "settings_regular", () => CreatePage<SettingsPage, ViewModels.Setting.SettingsPageVM>(), Dock.Bottom),
        ];
    }

    private MainMenuItemVM CreateItem(string header, string iconResourceKey, Func<UserControl> factory, Dock dock = Dock.Top)
    {
        return new MainMenuItemVM
        {
            Header = header,
            Icon = ResourceHelper.FindObjectResource(iconResourceKey),
            ContentFactory = factory,
            Dock = dock,
        };
    }

    private TPage CreatePage<TPage, TViewModel>()
        where TPage : UserControl, new()
    {
        return new TPage
        {
            DataContext = services.GetService<TViewModel>(),
        };
    }
}
