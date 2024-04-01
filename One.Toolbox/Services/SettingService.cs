using Newtonsoft.Json;

using One.Toolbox.Helpers;
using One.Toolbox.ViewModels.Setting;

using System.Globalization;
using System.IO;

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
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(AllConfig, Formatting.Indented, jsonSerializerSettings);

            File.WriteAllText(LocalConfig, json);
        }

        public void LoadLocalDefaultSetting()
        {
            LoadTargetSetting(LocalConfig);
        }

        public void LoadTargetSetting(string fullPath)
        {
            try
            {
                var text = File.ReadAllText(fullPath);

                AllConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<AllConfigModel>(text);
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

                Assets.Languages.Resources.Culture = new CultureInfo(language);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion Language
    }
}