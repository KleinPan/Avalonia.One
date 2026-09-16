using System;

using Avalonia.Controls;

using One.Toolbox.Helpers;

namespace One.Toolbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StartupTracker.Mark("MainWindow XAML 实例化完成");

        Closing += MainWindow_Closing; // 注册Closing事件处理程序
        Opened += MainWindow_Opened; // 首帧窗口显示时间点
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        StartupTracker.Mark("MainWindow.Opened（首帧窗口显示）");
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true; // 取消默认的关闭行为
        this.Hide(); // 隐藏窗口而不是关闭
    }
}