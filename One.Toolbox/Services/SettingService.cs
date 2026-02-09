using CommunityToolkit.Mvvm.Messaging;

using One.Base.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.ViewModels.Serialport;
using One.Toolbox.ViewModels.Setting;

using System.Globalization;

using PathHelper = One.Toolbox.Helpers.PathHelper;

namespace One.Toolbox.Services;

public class SettingService
{
    #region Config

    public static string LocalConfig = PathHelper.ConfigPath + "Setting.json";
    public static string CloudConfig = PathHelper.ConfigPath + "CloudSetting.json";
    public static string SerialportConfig = PathHelper.ConfigPath + "Serialport.json";

    public AllConfigModel AllConfig { get; set; } = new AllConfigModel();

    public SerialportSettingM SerialportSetting { get; set; } = new SerialportSettingM();

    #endregion Config

    public SettingService()
    {
    }

    #region LoadSave

    public void SaveCommonSetting()
    {
        //IOHelper.Instance.WriteContentTolocal(AllConfig, LocalConfig);

        //string newpath = System.IO.Path.GetDirectoryName(LocalConfig);

        //if (!Directory.Exists(newpath))
        //{
        //    Directory.CreateDirectory(newpath);
        //}

        //var json = JsonSerializer.Serialize(AllConfig, SourceGenerationContext.Default.AllConfigModel);

        //System.IO.File.WriteAllText(LocalConfig, json);

        IOHelper.Instance.WriteContentTolocalSourceGeneration(AllConfig, LocalConfig, SourceGenerationContext.Default.AllConfigModel);
    }

    public void LoadLocalDefaultSetting()
    {
        LoadTargetCommonSetting(LocalConfig);
    }

    public void LoadTargetCommonSetting(string fullPath)
    {
        try
        {
            //AllConfig = IOHelper.Instance.ReadContentFromLocal<AllConfigModel>(fullPath);

            //var content = System.IO.File.ReadAllText(fullPath);

            //AllConfig = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.AllConfigModel);

            AllConfig = IOHelper.Instance.ReadContentFromLocalSourceGeneration(fullPath, SourceGenerationContext.Default.AllConfigModel);
        }
        catch (Exception)
        {
            InitDefaultCommonSetting();
        }
    }

    void InitDefaultCommonSetting()
    {
        AllConfig = new AllConfigModel();
    }

    public void SaveSerialportSetting()
    {
        IOHelper.Instance.WriteContentTolocalSourceGeneration(SerialportSetting, SerialportConfig, SourceGenerationContext.Default.SerialportSettingM);
    }

    public void LoadLocalSerialportSetting()
    {
        LoadTargetSerialportSetting(SerialportConfig);
    }

    public void LoadTargetSerialportSetting(string fullPath)
    {
        try
        {
            //AllConfig = IOHelper.Instance.ReadContentFromLocal<AllConfigModel>(fullPath);

            //var content = System.IO.File.ReadAllText(fullPath);

            //AllConfig = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.AllConfigModel);

            SerialportSetting = IOHelper.Instance.ReadContentFromLocalSourceGeneration(fullPath, SourceGenerationContext.Default.SerialportSettingM);
        }
        catch (Exception)
        {
            InitDefaultCommonSetting();
        }
    }

    #endregion LoadSave

    #region Language

    public void ChangeLanguage(string language)
    {
        try
        {
            //var file = $"avares://One.Toolbox/Assets/Languages/{language}.axaml";
            //var data = new ResourceInclude(new Uri(file, UriKind.Absolute));
            //data.Source = new Uri(file, UriKind.Absolute);
            //Avalonia.Application.Current!.Resources.MergedDictionaries[0] = data;

            //Assets.Languages.Resources.Culture = new CultureInfo(language);

            var culture = new CultureInfo(language);
            I18nManager.Instance.Culture = culture;

            WeakReferenceMessenger.Default.Send(new string(language));
        }
        catch (Exception ex)
        {
        }
    }

    #endregion Language
}