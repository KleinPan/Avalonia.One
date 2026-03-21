using Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;

using One.Base.ExtensionMethods;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;

namespace One.Toolbox.ViewModels.BingImage;

public partial class BingImagePageVM : BasePageVM
{
    [ObservableProperty]
    private ObservableCollection<UsefullImageInfoVM> obImageListInfo = new();

    private readonly BingImageService bingImageService;
    private CancellationTokenSource? loadCts;

    public BingImagePageVM()
    {
        bingImageService = App.Current!.Services.GetService<BingImageService>()!;
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.BingImage);
    }

    public override void OnNavigatedEnter(Avalonia.Controls.UserControl userControl)
    {
        base.OnNavigatedEnter(userControl);

        loadCts?.Cancel();
        loadCts = new CancellationTokenSource();
        _ = RefreshAsync(loadCts.Token);
    }

    public override void OnNavigatedLeave()
    {
        loadCts?.Cancel();
        base.OnNavigatedLeave();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        loadCts?.Cancel();
        loadCts = new CancellationTokenSource();
        await RefreshAsync(loadCts.Token);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            var list = await bingImageService.LoadLatestAsync(cancellationToken);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ObImageListInfo.Clear();
                ObImageListInfo.AddRange(list);
            });
        }
        catch (OperationCanceledException)
        {
            // Ignore cancelled refresh.
        }
    }
}
