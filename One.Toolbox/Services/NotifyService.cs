using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using Microsoft.Extensions.DependencyInjection;

namespace One.Toolbox.Services;

public interface INotifyService
{
    public void ShowErrorMessage(string message);

    public void ShowWarnMessage(string message);

    public void ShowInfoMessage(string message);
}

public class NotifyService : INotifyService
{
    private static WindowNotificationManager? _manager;
    private static TopLevel topLevel;

    public NotifyService(Window target)
    {
        topLevel = target;
        _manager = new WindowNotificationManager(topLevel) { MaxItems = 3 };
    }

    public void ShowErrorMessage(string message)
    {
        _manager?.Show(new Notification("Error", message, NotificationType.Error));
    }

    public void ShowWarnMessage(string message)
    {
        _manager?.Show(new Notification("Warn", message, NotificationType.Warning));
    }

    public void ShowInfoMessage(string message)
    {
        _manager?.Show(new Notification("Info", message, NotificationType.Information));
    }
}

internal class ServiceHelper
{
    public static ServiceHelper Instance = new Lazy<ServiceHelper>(() => new ServiceHelper()).Value;

    public void ShowWarnMessage(string message)
    {
        App.Current!.Services.GetService<INotifyService>()!.ShowWarnMessage(message);
    }

    public void ShowErrorMessage(string message)
    {
        App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(message);
    }
    public void ShowInfoMessage(string message)
    {
        App.Current!.Services.GetService<INotifyService>()!.ShowInfoMessage(message);

    }
}