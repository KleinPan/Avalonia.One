﻿using Microsoft.Extensions.DependencyInjection;

using One.Base.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Enums;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.ViewModels.Dashboard;

using RestSharp;

using System.Threading.Tasks;

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

    private static async Task<GithubReleaseFilterInfo> GetLatestInfo()
    {
        //https://docs.github.com/en/rest/releases/releases?apiVersion=2022-11-28#get-the-latest-release

        var options = new RestClientOptions("https://api.github.com/repos/KleinPan/One/releases/latest")
        {
        };
        var client = new RestClient(options);
        client.AddDefaultHeader("Accept", "application/vnd.github+json");
        var request = new RestRequest("");

        // The cancellation token comes from the caller. You can still make a call without it.
        var timeline = await client.GetAsync(request);

        if (timeline.StatusCode == System.Net.HttpStatusCode.OK)
        {
            //var githubReleaseInfoM = JsonConvert.DeserializeObject<GithubReleaseInfoM>(timeline.Content);
            var githubReleaseInfoM = System.Text.Json.JsonSerializer.Deserialize<GithubReleaseInfoM>(timeline.Content);

            GithubReleaseFilterInfo githubReleaseFilterInfo = new GithubReleaseFilterInfo();

            //var localVersion = Assembly.GetExecutingAssembly().GetName().Version;

            //var localVersion =new AssemblyHelper(Assembly.GetExecutingAssembly()).ProductVersion;
            var localVersion = AssemblyHelper.Instance.ProductVersion;

            Version gitVersion = Version.Parse(githubReleaseInfoM.tag_name.Replace("v", ""));

            if (gitVersion > localVersion)
            {
                githubReleaseFilterInfo.NeedUpdate = true;
                githubReleaseFilterInfo.Version = gitVersion.ToString();
                githubReleaseFilterInfo.DownloadURL = githubReleaseInfoM.assets[0].browser_download_url;
            }
            return githubReleaseFilterInfo;
        }
        else
        {
            return new GithubReleaseFilterInfo()
            {
                NeedUpdate = false,
            };
        }
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