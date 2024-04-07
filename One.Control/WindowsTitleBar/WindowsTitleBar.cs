using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Reactive;

using One.Control.Extensions;

using System.Runtime.InteropServices;

namespace One.Control;

[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_MaximizeButton", typeof(Button))]
[TemplatePart("PART_MaximizeIcon", typeof(PathIcon))]
[TemplatePart("PART_MaximizeToolTip", typeof(ToolTip))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
[TemplatePart("PART_SystemChromeTitle", typeof(TextBlock))]
[TemplatePart("PART_TitleBar", typeof(Grid))]
[TemplatePart("PART_TitleBarBackground", typeof(StackPanel))]
[TemplatePart("PART_WindowIcon", typeof(Image))]
public class WindowsTitleBar : ContentControl
{
    private Button minimizeButton;
    private Button maximizeButton;
    private PathIcon maximizeIcon;
    private ToolTip maximizeToolTip;
    private Button closeButton;
    private Image windowIcon;

    private Grid titleBar;
    private StackPanel titleBarBackground;
    private TextBlock systemChromeTitle;
    private NativeMenuBar seamlessMenuBar;
    private NativeMenuBar defaultMenuBar;

    #region AvaloniaProperty

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<WindowsTitleBar, string>(nameof(Title), defaultValue: "Title");

    public string Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly StyledProperty<Bitmap> IconProperty = AvaloniaProperty.Register<WindowsTitleBar, Bitmap>(nameof(Icon));

    public Bitmap Icon
    {
        get { return GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public static readonly StyledProperty<object> CenterContentProperty = AvaloniaProperty.Register<WindowsTitleBar, object>(nameof(CenterContent));

    public object CenterContent
    {
        get { return GetValue(CenterContentProperty); }
        set { SetValue(CenterContentProperty, value); }
    }

    #endregion AvaloniaProperty

    /// <summary> 窗口移动 </summary>
    public event EventHandler<PointerPressedEventArgs> OnPointerMouseHander;

    static WindowsTitleBar()
    {
    }

    public WindowsTitleBar()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            IsVisible = false;
        }
        else
        {
            closeButton = e.NameScope.Find<Button>("PART_CloseButton");
            //defaultMenuBar = e.NameScope.Find<NativeMenuBar>("PART_DefaultMenuBar");
            maximizeButton = e.NameScope.Find<Button>("PART_MaximizeButton");
            maximizeIcon = e.NameScope.Find<PathIcon>("PART_MaximizeIcon");
            maximizeToolTip = e.NameScope.Find<ToolTip>("PART_MaximizeToolTip");
            minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");
            //seamlessMenuBar = e.NameScope.Find<NativeMenuBar>("SeamlessMenuBar");
            systemChromeTitle = e.NameScope.Find<TextBlock>("PART_SystemChromeTitle");
            titleBar = e.NameScope.Find<Grid>("PART_TitleBar");
            titleBarBackground = e.NameScope.Find<StackPanel>("PART_TitleBarBackground");
            windowIcon = e.NameScope.Find<Image>("PART_WindowIcon");

            minimizeButton.Click += MinimizeWindow;
            maximizeButton.Click += MaximizeWindow;
            closeButton.Click += CloseWindow;
            windowIcon.DoubleTapped += CloseWindow;

            SubscribeToWindowState();
        }
    }

    #region MinMaxCloseEvent

    private void CloseWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)VisualRoot;
        hostWindow.Close();
    }

    private void MaximizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)VisualRoot;

        if (hostWindow.WindowState == WindowState.Normal)
        {
            hostWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            hostWindow.WindowState = WindowState.Normal;
        }
    }

    private void MinimizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Window hostWindow = (Window)VisualRoot;
        hostWindow.WindowState = WindowState.Minimized;
    }

    #endregion MinMaxCloseEvent

    private async void SubscribeToWindowState()
    {
        Window hostWindow = (Window)VisualRoot;

        while (hostWindow == null)
        {
            hostWindow = (Window)VisualRoot;
            await Task.Delay(50);
        }
        //System.Reactive 有封装的方法

        hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
        {
            if (s != WindowState.Maximized)
            {
                maximizeIcon.Data = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                hostWindow.Padding = new Thickness(0, 0, 0, 0);
                maximizeToolTip.Content = "Maximize";
            }
            if (s == WindowState.Maximized)
            {
                maximizeIcon.Data = Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                hostWindow.Padding = new Thickness(7, 7, 7, 7);
                maximizeToolTip.Content = "Restore Down";
            }
        });
    }
}