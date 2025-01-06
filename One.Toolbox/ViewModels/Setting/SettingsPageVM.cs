﻿using ExCSS;

using Microsoft.Extensions.DependencyInjection;

using One.Base.Helpers;
using One.Base.Helpers.HttpHelper;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Enums;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Dashboard;

using System.Net.Http;
using System.Threading.Tasks;

using Ursa.Controls;

namespace One.Toolbox.ViewModels.Setting;

public partial class SettingsPageVM : BasePageVM
{
    [ObservableProperty]
    private bool autoUpdate = true;

    private SettingService SettingService { get; set; }

    public SettingsPageVM()
    {
        SettingService = App.Current!.Services.GetService<Services.SettingService>()!;
        LoadSetting();
    }

    public override void OnNavigatedLeave()
    {
        base.OnNavigatedLeave();

        SaveSetting();
    }

    public override void OnNavigatedEnter()
    {
        base.OnNavigatedEnter();
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.Setting);
    }

    public override void InitializeViewModel()
    {
        Task.Run(async () =>
        {
            var a = await GetLatestInfo();

            if (a.NeedUpdate)
            {
                //待定
            }
        });

        base.InitializeViewModel();
    }

    private HttpClient httpClient = new HttpClient();

    private async Task<GithubReleaseFilterInfo> GetLatestInfo()
    {
        //https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#get-the-latest-release

        httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");

        var result = await httpClient.GetStringAsync("https://api.github.com/repos/KleinPan/One/releases/latest");

        var githubReleaseInfoM = System.Text.Json.JsonSerializer.Deserialize(result, SourceGenerationContext.Default.GithubReleaseInfoM);

        var localVersion = AssemblyHelper.Instance.ProductVersion;
        Version gitVersion = Version.Parse(githubReleaseInfoM.tag_name.Replace("v", ""));

        GithubReleaseFilterInfo githubReleaseFilterInfo = new GithubReleaseFilterInfo();

        if (gitVersion > localVersion)
        {
            githubReleaseFilterInfo.NeedUpdate = true;
            githubReleaseFilterInfo.Version = gitVersion.ToString();
            githubReleaseFilterInfo.DownloadURL = githubReleaseInfoM.assets[0].browser_download_url;
        }

        return githubReleaseFilterInfo;
    }

    private void LoadSetting()
    {
        AutoUpdate = SettingService.AllConfig.Setting.SutoUpdate;
    }

    private void SaveSetting()
    {
        SettingService.AllConfig.Setting.SutoUpdate = AutoUpdate;

        SettingService.Save();
    }
}

public class SettingModel
{
    public bool SutoUpdate;
    public bool ShowStickOnStart;

    public SettingModel()
    {
    }
}