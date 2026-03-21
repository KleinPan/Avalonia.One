using Microsoft.Extensions.DependencyInjection;

using One.Base.Helpers.DataProcessHelpers;
using One.Toolbox.Helpers;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.Serialport;

public partial class QuickSendVM : BaseVM
{
    [ObservableProperty]
    private int id;

    /// <summary>发送内容</summary>
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private bool hex;

    /// <summary>按钮内容</summary>
    [ObservableProperty]
    private string commit = string.Empty;

    [RelayCommand]
    private void SendData(object obj)
    {
        var vm = App.Current.Services.GetService<SerialportPageVM>();
        if (vm is null || !vm.serialPortHelper.IsOpen())
        {
            return;
        }

        var data = System.Text.Encoding.UTF8.GetBytes(Text);

        try
        {
            byte[] dataConvert;
            if (Hex)
            {
                var temp = System.Text.Encoding.UTF8.GetString(data.ToArray());
                var pureHex = temp.Replace(" ", "").Replace("\r\n", "");
                dataConvert = StringHelper.HexStringToBytes(pureHex);
            }
            else
            {
                dataConvert = data;
            }

            if (vm.SerialportUISetting.SendAndReceiveSettingVM.WithExtraEnter)
            {
                var temp = dataConvert.ToList();
                temp.Add(0x0d);
                temp.Add(0x0a);
                dataConvert = temp.ToArray();
            }

            vm.serialPortHelper.SendData(dataConvert);
        }
        catch (Exception ex)
        {
            NotifyHelper.ShowErrorMessage($"{ResourceHelper.FindStringResource("ErrorSendFail")}\r\n{ex}");
        }
    }

    public QuickSendModel ToM()
    {
        return new QuickSendModel
        {
            Id = Id,
            Text = Text,
            Hex = Hex,
            Commit = Commit,
        };
    }
}

public class QuickSendModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Hex { get; set; }
    public string Commit { get; set; } = string.Empty;

    public QuickSendVM ToVM()
    {
        return new QuickSendVM
        {
            Id = Id,
            Text = Text,
            Hex = Hex,
            Commit = Commit,
        };
    }
}
