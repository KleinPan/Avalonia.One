using One.Base.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.UnixTimeConverter;

public partial class UnixTimeConverterVM : BasePageVM
{
    public UnixTimeConverterVM()
    {
        Now();
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.TimeConvert);
    }

    [ObservableProperty]
    private string input;

    [ObservableProperty]
    private DateTime localTme;

    [ObservableProperty]
    private DateTime uTCTime;

    [RelayCommand]
    void Clear()
    {
        Input = "";
    }

    [RelayCommand]
    void Now()
    {
        Input = TimeHelper.GetUnixTimestamp();
    }

    partial void OnInputChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        try
        {
            var temp = TimeHelper.GetUnixTime(value, 10);
            LocalTme = temp;
            UTCTime = localTme.ToUniversalTime();
        }
        catch (Exception ex)
        {
            //App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.ToString());
        }
    }
}