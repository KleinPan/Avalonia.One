using Avalonia.Controls;
using Avalonia.Controls.Notifications;

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