using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace One.Toolbox.Views;

public partial class WindowsTitleBar : UserControl
{
    private Button minimizeButton;
    private Button maximizeButton;
    private PathIcon maximizeIcon;
    private ToolTip maximizeToolTip;
    private Button closeButton;
    private Image windowIcon;

    private DockPanel titleBar;
    private DockPanel titleBarBackground;
    private TextBlock systemChromeTitle;
    private NativeMenuBar seamlessMenuBar;
    private NativeMenuBar defaultMenuBar;

    #region AvaloniaProperty

    public static readonly StyledProperty<bool> IsSeamlessProperty = AvaloniaProperty.Register<WindowsTitleBar, bool>(nameof(IsSeamless));

    public bool IsSeamless
    {
        get { return GetValue(IsSeamlessProperty); }
        set
        {
            SetValue(IsSeamlessProperty, value);
            if (titleBarBackground != null &&
                systemChromeTitle != null &&
                seamlessMenuBar != null &&
                defaultMenuBar != null)
            {
                titleBarBackground.IsVisible = IsSeamless ? false : true;
                systemChromeTitle.IsVisible = IsSeamless ? false : true;
                seamlessMenuBar.IsVisible = IsSeamless ? true : false;
                defaultMenuBar.IsVisible = IsSeamless ? false : true;

                if (IsSeamless == false)
                {
                    titleBar.Resources["SystemControlForegroundBaseHighBrush"] = new SolidColorBrush { Color = new Color(255, 0, 0, 0) };
                }
            }
        }
    }

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

    #endregion AvaloniaProperty

    /// <summary> ´°¿ÚÒÆ¶¯ </summary>
    public event EventHandler<PointerPressedEventArgs> OnPointerMouseHander;

    public WindowsTitleBar()
    {
        InitializeComponent();

        DataContext = this;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            IsVisible = false;
        }
        else
        {
            closeButton = this.FindControl<Button>("CloseButton");
            defaultMenuBar = this.FindControl<NativeMenuBar>("DefaultMenuBar");
            maximizeButton = this.FindControl<Button>("MaximizeButton");
            maximizeIcon = this.FindControl<PathIcon>("MaximizeIcon");
            maximizeToolTip = this.FindControl<ToolTip>("MaximizeToolTip");
            minimizeButton = this.FindControl<Button>("MinimizeButton");
            seamlessMenuBar = this.FindControl<NativeMenuBar>("SeamlessMenuBar");
            systemChromeTitle = this.FindControl<TextBlock>("SystemChromeTitle");
            titleBar = this.FindControl<DockPanel>("TitleBar");
            titleBarBackground = this.FindControl<DockPanel>("TitleBarBackground");
            windowIcon = this.FindControl<Image>("WindowIcon");

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

                // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                /*hostWindow.Padding = new Thickness(
                        hostWindow.OffScreenMargin.Left,
                        hostWindow.OffScreenMargin.Top,
                        hostWindow.OffScreenMargin.Right,
                        hostWindow.OffScreenMargin.Bottom);*/
            }
        });
    }
}