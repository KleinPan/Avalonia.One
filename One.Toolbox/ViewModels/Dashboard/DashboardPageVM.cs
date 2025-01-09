using Avalonia.Controls;

using One.Base.Helpers;
using One.Base.Helpers.HttpHelper;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Setting;

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.Dashboard;

public partial class DashboardPageVM : BasePageVM
{
    public DashboardPageVM()
    {
    }

    public override void OnNavigatedEnter(UserControl userControl)
    {
        base.OnNavigatedEnter(userControl);

        InitData();
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.Test);
    }

    [RelayCommand]
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

    private void InitData()
    {
        Register();

        Task.Run(async () =>
        {
            // var a = await GetEveryDayYiyan();
            var jsonString = await HTTPClientHelper.GetStringAsync("https://v1.hitokoto.cn/");

            var json = JsonSerializer.Deserialize(jsonString, SourceGenerationContext.Default.YiyanAPIM);
            Text = json.hitokoto;
            Author = "--" + json.from;
        });
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
}