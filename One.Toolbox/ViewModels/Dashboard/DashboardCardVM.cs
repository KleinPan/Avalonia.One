using Avalonia.Threading;

using One.Base.Helpers.HttpHelper;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Setting;

using System.Text.Json;

namespace One.Toolbox.ViewModels.Dashboard;

public partial class DashboardCardVM : BaseVM
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string iconData = string.Empty; // 存放 PathIcon 的 Data
    [ObservableProperty] private string accentColor = string.Empty; // 磁贴的主题色

    // 这里可以放一个通用的 Content，或者根据类型在 View 中选择不同的 Template
    [ObservableProperty] private object? cardContent;
}

// 内容模型：每日一言
public partial class QuoteDataVM : ObservableObject
{
    [ObservableProperty] private string text = string.Empty;
    [ObservableProperty] private string author = string.Empty;


    public QuoteDataVM()
    {
        _ = LoadQuoteAsync();
    }

    private async Task LoadQuoteAsync()
    {
        try
        {
            var jsonString = await HTTPClientHelper.GetStringAsync("https://v1.hitokoto.cn/");
            var json = JsonSerializer.Deserialize(jsonString, SourceGenerationContext.Default.YiyanAPIM);
            if (json == null)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Text = json.hitokoto ?? string.Empty;
                Author = "--" + (json.from ?? string.Empty);
            });
        }
        catch
        {
            // Ignore network failures on dashboard quote.
        }
    }
}

 
