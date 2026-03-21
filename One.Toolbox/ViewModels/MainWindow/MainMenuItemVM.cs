using Avalonia.Controls;

using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.MainWindow;

public partial class MainMenuItemVM : BaseVM
{
    [ObservableProperty]
    private UserControl? content;

    public Func<UserControl>? ContentFactory { get; set; }

    [ObservableProperty]
    private string header;

    [ObservableProperty]
    private object icon;

    [ObservableProperty]
    public Dock dock;

    public MainMenuItemVM()
    {
        Dock = Dock.Top;
    }

    public UserControl EnsureContent()
    {
        if (Content == null)
        {
            if (ContentFactory is null)
            {
                throw new InvalidOperationException($"Menu item '{Header}' has no content factory.");
            }

            Content = ContentFactory();
        }

        return Content;
    }

    public override string ToString()
    {
        return Header.ToString();
    }
}
