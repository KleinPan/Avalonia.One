using One.Base.Helpers;
using One.Toolbox.ViewModels.Setting;

using System.Globalization;

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
            IOHelper.Instance.WriteContentTolocal(AllConfig, LocalConfig);
        }

        public void LoadLocalDefaultSetting()
        {
            LoadTargetSetting(LocalConfig);
        }

        public void LoadTargetSetting(string fullPath)
        {
            try
            {
                AllConfig = IOHelper.Instance.ReadContentFromLocal<AllConfigModel>(fullPath);
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