using Avalonia.Controls;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Markup.I18n;
using One.Toolbox.Helpers;
using One.Toolbox.Services.Navigation;
using One.Toolbox.ViewModels.MainWindow;

using System.Collections.ObjectModel;

namespace One.Toolbox.Services;

public sealed class NavigationService
{
    private readonly IServiceProvider services;
    private readonly ToolPageRegistry registry;

    public NavigationService(IServiceProvider services, ToolPageRegistry registry)
    {
        this.services = services;
        this.registry = registry;
    }

    public ObservableCollection<MainMenuItemVM> CreateNavigationItems()
    {
        var items = new ObservableCollection<MainMenuItemVM>();
        foreach (var d in registry.Descriptors)
        {
            items.Add(CreateItem(d));
        }
        return items;
    }

    private MainMenuItemVM CreateItem(ToolPageDescriptor d)
    {
        return new MainMenuItemVM
        {
            Header = I18nManager.GetString(d.I18nKey) ?? d.I18nKey,
            Icon = ResourceHelper.FindObjectResource(d.IconKey),
            ContentFactory = () => CreatePage(d),
            Dock = d.Dock,
        };
    }

    private UserControl CreatePage(ToolPageDescriptor d)
    {
        var page = (UserControl)Activator.CreateInstance(d.ViewType)!;
        page.DataContext = services.GetService(d.ViewModelType);
        return page;
    }
}
