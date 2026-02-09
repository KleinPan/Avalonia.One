using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Microsoft.Extensions.DependencyInjection;

namespace One.Toolbox.Services;

public interface INotifyService
{
    public void ShowErrorMessage(string message, string title = "Error");

    public void ShowWarnMessage(string message, string title = "Warning");

    public void ShowInfoMessage(string message, string title = "Info");
}

public class NotifyService : INotifyService
{
    private static WindowNotificationManager? _manager;
    private static TopLevel topLevel;
    private static TrayIcon NotifyIcon;

    public NotifyService(Window target)
    {
        topLevel = target;
        _manager = new WindowNotificationManager(topLevel) { MaxItems = 2, Position = NotificationPosition.TopCenter };
    }

    public void ShowErrorMessage(string message, string title = "Error")
    {
        _manager?.Show(new Notification(title, message, NotificationType.Error));
    }

    public void ShowWarnMessage(string message, string title = "Warning")
    {
        _manager?.Show(new Notification(title, message, NotificationType.Warning));
    }

    public void ShowInfoMessage(string message, string title = "Info")
    {
        _manager?.Show(new Notification(title, message, NotificationType.Information));
    }

    public void InitializeLogo()
    {
        return;
        // 初始化Icon
        NotifyIcon = new TrayIcon();

        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://One.Toolbox/Assets/icons8-knife-100.ico")));
        NotifyIcon.Icon = new WindowIcon(bitmap);

        NotifyIcon.ToolTipText = "One.Toolbox";
        NotifyIcon.IsVisible = true;
        NotifyIcon.Clicked += NotifyIcon_Clicked;
        NotifyIcon.Menu = new NativeMenu();
        NotifyIcon.Menu.Add(new NativeMenuItem() { Header = "Exit" });
    }

    private static void NotifyIcon_Clicked(object? sender, EventArgs e)
    {
        topLevel.IsVisible = true;
    }
}

internal class NotifyHelper
{
    //public static NotifyHelper Instance = new Lazy<NotifyHelper>(() => new NotifyHelper()).Value;

    public static void ShowWarnMessage(string message)
    {
        App.Current!.Services.GetService<INotifyService>()!.ShowWarnMessage(message);
    }

    public static void ShowErrorMessage(string message)
    {
        App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(message);
    }

    public static void ShowInfoMessage(string message)
    {
        App.Current!.Services.GetService<INotifyService>()!.ShowInfoMessage(message);
    }
}