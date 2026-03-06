using Avalonia.Threading;

using One.Base.Helpers.HttpHelper;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Setting;

using System.Text.Json;

namespace One.Toolbox.ViewModels.Dashboard;

public partial class DashboardCardVM : BaseVM
{
    [ObservableProperty] private string title;
    [ObservableProperty] private string iconData; // 存放 PathIcon 的 Data
    [ObservableProperty] private string accentColor; // 磁贴的主题色

    // 这里可以放一个通用的 Content，或者根据类型在 View 中选择不同的 Template
    [ObservableProperty] private object cardContent;
}

// 内容模型：每日一言
public partial class QuoteDataVM : ObservableObject
{
    [ObservableProperty] private string text;
    [ObservableProperty] private string author;


    public QuoteDataVM()
    {
        Task.Run(async () =>
        {
            // var a = await GetEveryDayYiyan();
            var jsonString = await HTTPClientHelper.GetStringAsync("https://v1.hitokoto.cn/");

            var json = JsonSerializer.Deserialize(jsonString, SourceGenerationContext.Default.YiyanAPIM);
            Text = json.hitokoto;
            Author = "--" + json.from;
        });
    }
}

 