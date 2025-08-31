using Avalonia.Controls;

namespace One.Toolbox.Views.Dashboard;

public partial class DashboardPage : UserControl
{
    // public DashboardVM ViewModel { get; }

    public DashboardPage()
    {
        // DataContext = ViewModel = App.Current.Services.GetService<DashboardVM>();
        InitializeComponent();
    }
}