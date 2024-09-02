using Avalonia.Controls;

namespace One.Toolbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += MainWindow_Closing; // 注册Closing事件处理程序
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true; // 取消默认的关闭行为
        this.Hide(); // 隐藏窗口而不是关闭
    }
}