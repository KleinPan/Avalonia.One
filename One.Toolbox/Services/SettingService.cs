using CommunityToolkit.Mvvm.Messaging;

using One.Base.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.ViewModels.Setting;

using System.Globalization;
using System.IO;
using System.Text.Json;

using PathHelper = One.Toolbox.Helpers.PathHelper;

namespace One.Toolbox.Services
{
    public class SettingService
    {
        #region Config

        public static string LocalConfig = PathHelper.ConfigPath + "Setting.json";
        public static string CloudConfig = PathHelper.ConfigPath + "CloudSetting.json";
        public AllConfigModel AllConfig { get; set; } = new AllConfigModel();

        #endregion Config

        public SettingService()
        {
            LoadLocalDefaultSetting();
        }

        #region LoadSave

        public void Save()
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
            LoadTargetSetting(LocalConfig);
        }

        public void LoadTargetSetting(string fullPath)
        {
            try
            {
                //AllConfig = IOHelper.Instance.ReadContentFromLocal<AllConfigModel>(fullPath);

                 //var content = System.IO.File.ReadAllText(fullPath);

                //AllConfig = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.AllConfigModel);

                AllConfig = IOHelper.Instance.ReadContentFromLocalSourceGeneration (fullPath, SourceGenerationContext.Default.AllConfigModel);
            }
            catch (Exception)
            {
                InitDefaultData();
            }
        }

        void InitDefaultData()
        {
            AllConfig = new AllConfigModel();
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
}