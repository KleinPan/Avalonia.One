﻿using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Enums;
using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.IO;
using System.Net;
using System.Threading.Tasks;

using WebDav;

namespace One.Toolbox.ViewModels.Setting;

public partial class CloudSettingsVM : BaseVM
{
    //https://github.com/skazantsev/WebDavClient/tree/main
    public static IWebDavClient _client = new WebDavClient();

    #region UI

    [ObservableProperty]
    private WebDAVTypeEnum selectedWebDAVTypeEnum;

    [ObservableProperty]
    private string userName;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isUploading;

    [ObservableProperty]
    private bool isDownloading;

    [ObservableProperty]
    private bool useProxy = true;

    [ObservableProperty]
    private string proxyAddress = "socks5://localhost:10808";

    #endregion UI

    private const string targetFile = targetDir + "/Setting.json";
    private const string targetDir = "One.Toolbox";//"One.Toolbox/Setting"//yandex 不支持 嵌套文件夹
    private SettingService settingService;

    public CloudSettingsVM()
    {
        settingService = App.Current.Services.GetService<Services.SettingService>();
    }

    

    [RelayCommand]
    private async void Upload()
    {
        IsUploading = true;
        await UploadCloudSettingAsync();
        IsUploading = false;
    }

    [RelayCommand]
    private void Download()
    {
        IsDownloading = true;
        DownloadCloudSettingAsync();
        IsDownloading = false;
    }

    private WebDavClientParams InitWebDavParam()
    {
        string addr = SelectedWebDAVTypeEnum switch
        {
            WebDAVTypeEnum.坚果云 => "https://dav.jianguoyun.com/dav/",
            WebDAVTypeEnum.Yandex => "https://webdav.yandex.com/",
            _ => throw new Exception("Unsupport WebDAV type!"),
        };

        var clientParams = new WebDavClientParams
        {
            BaseAddress = new Uri(addr),
            Credentials = new NetworkCredential(UserName, Password)
        };
        if (useProxy)
        {
            WebProxy webProxy = new WebProxy(ProxyAddress);//"socks5://localhost:10808"
            clientParams.Proxy = webProxy;
        }

        return clientParams;
    }

    public async Task DownloadCloudSettingAsync()
    {
        try
        {
            var param = InitWebDavParam();
            using (var client = new WebDavClient(param))
            {
                // create a setting directory
                var resMK = await client.Mkcol(targetDir);

                if (!resMK.IsSuccessful)
                {
                    App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage("Connect failed!");
                    return;
                }
                var res = await client.Propfind(targetFile);
                if (res.IsSuccessful)
                {
                    if (res.Resources.Count > 0)
                    {
                        var rsp = await client.GetRawFile(targetFile);

                        using (var fileStream = File.Create(SettingService.LocalConfig))
                        {
                            rsp.Stream.CopyTo(fileStream);
                        }

                        settingService.LoadTargetSetting(SettingService.LocalConfig);
                    }

                    App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage("Not find cloud setting!");
                }
                else
                {
                    App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage("Not find cloud setting!");
                }
            }
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
        }
    }

    public async Task UploadCloudSettingAsync()
    {
        try
        {
            var param = InitWebDavParam();
            //简化using
            using var client = new WebDavClient(param);
            // create a setting directory
            var resMK = await client.Mkcol(targetDir);
            if (!resMK.IsSuccessful)
            {
                App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage($"Error {resMK.Description};");
                return;
            }
            await client.PutFile(targetFile, File.OpenRead(SettingService.LocalConfig)); // upload a resource
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.Message);
        }
    }
}