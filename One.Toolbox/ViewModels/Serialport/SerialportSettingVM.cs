using One.Toolbox.ViewModels.Base;

using System.IO.Ports;

namespace One.Toolbox.ViewModels.Serialport;

public partial class SerialportSettingVM : ObservableObject
{
    [ObservableProperty]
    private List<int> databitList = new() { 5, 6, 7, 8 };

    [ObservableProperty]
    private List<StopBits> stopBitsList = Enum.GetValues(typeof(StopBits)).Cast<StopBits>().ToList();

    [ObservableProperty]
    private List<Parity> parityList = Enum.GetValues(typeof(Parity)).Cast<Parity>().ToList();

    [ObservableProperty]
    private SendAndReceiveSettingVM sendAndReceiveSettingVM;

    public SerialportParams SerialportParams { get; set; } = new SerialportParams();
    public List<QuickSendVM> QuickSendList { get; set; } = new List<QuickSendVM>();

    public SerialportSettingVM()
    {
    }

    public SerialportSettingM ToModel()
    {
        SerialportSettingM model = new SerialportSettingM();
        model.SendAndReceiveSettingModel = SendAndReceiveSettingVM.ToModel();
        model.QuickSendList = QuickSendList.Select(x => x.ToM()).ToList();
        model.SerialportParams = SerialportParams;
        return model;
    }
}

public partial class SerialportSettingM
{
    public SendAndReceiveSettingModel SendAndReceiveSettingModel { get; set; } = new SendAndReceiveSettingModel();
    public SerialportParams SerialportParams { get; set; } = new SerialportParams();
    public List<QuickSendModel> QuickSendList { get; set; } = new List<QuickSendModel>();

    public SerialportSettingM()
    {
    }

    public SerialportSettingVM ToVM()
    {
        SerialportSettingVM vm = new SerialportSettingVM();
        vm.SerialportParams = SerialportParams;
        vm.SendAndReceiveSettingVM = SendAndReceiveSettingModel.ToVM();
        vm.QuickSendList = QuickSendList.Select(x => x.ToVM()).ToList();

        return vm;
    }
}