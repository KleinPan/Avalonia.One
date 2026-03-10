using Avalonia.Controls;
using Avalonia.Threading;

using One.Base.Helpers;
using One.Base.Helpers.HttpHelper;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Setting;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace One.Toolbox.ViewModels.Dashboard;

public partial class DashboardPageVM : BasePageVM
{
    public DashboardPageVM()
    {
        Register();

        InitializeCards();

        // 初始网络信息
        LocalIp = GetLocalIpAddress();
    }

    public override void OnNavigatedEnter(UserControl userControl)
    {
        base.OnNavigatedEnter(userControl);
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.Dashboard);
    }

    private void OpenUrl(string urlObj)
    {
        var url = urlObj as string;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //https://stackoverflow.com/a/2796367/241446
            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
            proc.Start();

            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("x-www-browser", url);
            return;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) throw new ArgumentException("invalid url: " + url);
        Process.Start("open", url);
        return;
    }

    private void Register()
    {
        //https://blog.cool2645.com/posts/csruanjianjiaxul/
        //https://m.xp.cn/b.php/92230.html
        var regTime = RegistryHelper.ReadSetting("Toolbox", "FirstRun", "");
        if (string.IsNullOrEmpty(regTime))
        {
            var first = DateTime.Now.ToString("u");
            RegistryHelper.WriteKey("Toolbox", "FirstRun", first);
        }
        else
        {
            DateTime firstInfo = DateTime.ParseExact(regTime, "u", CultureInfo.InvariantCulture);

            var sub = DateTime.Now - firstInfo;

            if (sub > TimeSpan.FromDays(7))
            {
                // App .Current!.Services.GetService<INotifyService>()!.ShowErrorMessage("试用期到期！");
            }
        }
    }

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private string author;

    [RelayCommand]
    private async void Test()
    {
        //var s = AssemblyHelper.Instance.FileVersionInfo;
        //var ab = new AssemblyHelper(Assembly.GetExecutingAssembly());

        //List<InputInfoVM> inputInfoVMs = new List<InputInfoVM>()
        //{
        //    new InputInfoVM("aa","bb"),
        //     new InputInfoVM("cc","dd"),
        //     new InputInfoVM("dd","ee"),
        //};
        //var res = await DialogHelper.Instance.ShowInputDialog("test", inputInfoVMs);
    }

    // 动态卡片集合
    public ObservableCollection<DashboardCardVM> DashboardCards { get; } = new();

    // 实时监控数据（供卡片内部绑定）
    [ObservableProperty] private double cpuUsage;

    [ObservableProperty] private double memoryPercent;
    [ObservableProperty] private string localIp;
    [ObservableProperty] private string upTime;

    private void InitializeCards()
    {
        DashboardCards.Add(new DashboardCardVM
        {
            Title = "每日一言",
            AccentColor = "#FF9800", // 暖色调图标
            IconData = "M14,17H17L19,13V7H13V13H16M6,17H9L11,13V7H5V13H8L6,17Z",
            // 这里的 CardContent 我们传一个特殊的标识对象或者直接传数据
            CardContent = new QuoteDataVM()
        });

      

        // 2. 网络快照卡片
        DashboardCards.Add(new DashboardCardVM
        {
            Title = "网络信息",
            AccentColor = "#E81123",
            IconData = "M12,2C6.48,2 2,6.48 2,12C2,17.52 6.48,22 12,22C17.52,22 22,17.52 22,12C22,6.48 17.52,2 12,2M11,19.93C7.05,19.44 4,16.08 4,12C4,11.38 4.08,10.78 4.21,10.21L5.67,11.67C5.86,11.86 6.11,11.97 6.38,11.97H8.38C8.93,11.97 9.38,11.52 9.38,10.97V8.97C9.38,8.42 8.93,7.97 8.38,7.97H7.38V5.97C7.38,5.42 7.83,4.97 8.38,4.97H11.38C11.93,4.97 12.38,5.42 12.38,5.97V7.97H14.38C14.93,7.97 15.38,8.42 15.38,8.97V11.97C15.38,12.52 14.93,12.97 14.38,12.97H13.38V15.97C13.38,16.52 13.83,16.97 14.38,16.97H15.38V18.97C15.38,19.52 14.93,19.97 14.38,19.97H11V19.93Z",
            CardContent = $"IP: {GetLocalIpAddress()}"
        });
    }

    private string GetLocalIpAddress()
    {
        try
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
        }
        catch { return "N/A"; }
    }
}