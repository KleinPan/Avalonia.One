using Avalonia.Controls;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainViewVM : BaseVM
{
    [ObservableProperty]
    private string applicationTitle = string.Empty;

    [ObservableProperty]
    private bool isPaneOpen;

    [ObservableProperty]
    private double paneWidth = 50;

    [ObservableProperty]
    private ObservableCollection<MainMenuItemVM> navigationItems =
    [
        new MainMenuItemVM { Header = "Home", Icon = ResourceHelper.FindObjectResource("home_regular") },
        new MainMenuItemVM { Header = "Images", Icon = ResourceHelper.FindObjectResource("image_library_regular") },
        new MainMenuItemVM { Header = "Texts", Icon = ResourceHelper.FindObjectResource("text_number_format_regular") },
        new MainMenuItemVM { Header = "Notes", Icon = ResourceHelper.FindObjectResource("notepad_regular") },
        new MainMenuItemVM { Header = "HashTools", Icon = ResourceHelper.FindObjectResource("premium_regular") },
        new MainMenuItemVM
        {
            Dock = Dock.Bottom,
            Header = "Settings",
            Icon = ResourceHelper.FindObjectResource("settings_regular"),
        },
    ];

    [ObservableProperty]
    private MainMenuItemVM? currentMenuItem;

    private readonly NavigationService navigationService;

    public MainViewVM()
    {
        navigationService = App.Current!.Services.GetService<NavigationService>()!;
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
        PaneWidth = IsPaneOpen ? 200 : 50;
    }

    public void InitializeViewModel()
    {
        ApplicationTitle = "One.Toolbox";
        NavigationItems = navigationService.CreateNavigationItems();

        if (NavigationItems.Count > 0)
        {
            CurrentMenuItem = NavigationItems.First();
        }
    }

    partial void OnCurrentMenuItemChanged(MainMenuItemVM? oldValue, MainMenuItemVM? newValue)
    {
        if (oldValue != null)
        {
            var oldContent = oldValue.Content;
            if (oldContent?.DataContext is BasePageVM vmOld)
            {
                vmOld.OnNavigatedLeave();
            }
        }

        if (newValue == null)
        {
            return;
        }

        var newContent = newValue.EnsureContent();
        if (newContent.DataContext is BasePageVM vmNew)
        {
            vmNew.OnNavigatedEnter(newContent);
        }
    }
}
