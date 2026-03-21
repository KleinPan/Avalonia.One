using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Enums;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.Setting;

public partial class CloudSettingsVM : BaseVM
{
    #region UI

    [ObservableProperty]
    private WebDAVTypeEnum selectedWebDAVTypeEnum;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isUploading;

    [ObservableProperty]
    private bool isDownloading;

    [ObservableProperty]
    private bool useProxy = true;

    [ObservableProperty]
    private string proxyAddress = "socks5://localhost:10808";

    #endregion UI

    private readonly SettingService settingService;
    private readonly CloudSettingSyncService cloudSettingSyncService;

    public CloudSettingsVM()
    {
        settingService = App.Current!.Services.GetService<SettingService>()!;
        cloudSettingSyncService = App.Current!.Services.GetService<CloudSettingSyncService>()!;
    }

    [RelayCommand]
    private async Task Upload()
    {
        if (IsUploading)
        {
            return;
        }

        IsUploading = true;
        try
        {
            var error = await cloudSettingSyncService.UploadSettingAsync(
                SelectedWebDAVTypeEnum,
                UserName,
                Password,
                UseProxy,
                ProxyAddress,
                SettingService.LocalConfig);

            if (!string.IsNullOrWhiteSpace(error))
            {
                App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(error);
            }
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
        }
        finally
        {
            IsUploading = false;
        }
    }

    [RelayCommand]
    private async Task Download()
    {
        if (IsDownloading)
        {
            return;
        }

        IsDownloading = true;
        try
        {
            var error = await cloudSettingSyncService.DownloadSettingAsync(
                SelectedWebDAVTypeEnum,
                UserName,
                Password,
                UseProxy,
                ProxyAddress,
                SettingService.LocalConfig);

            if (string.IsNullOrWhiteSpace(error))
            {
                settingService.LoadTargetCommonSetting(SettingService.LocalConfig);
                return;
            }

            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(error);
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
