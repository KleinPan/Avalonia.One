using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;

using AvaloniaEdit.Document;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;

using One.Base.ExtensionMethods;
using One.Base.Helpers.DataProcessHelpers;
using One.Control.Markup.I18n;
using One.SimpleLog;
using One.SimpleLog.Extensions;
using One.SimpleLog.Loggers;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Messenger;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

namespace One.Toolbox.ViewModels.Serialport;

public partial class SerialportPageVM : BasePageVM
{
    #region SerialPortSetting

    public ObservableCollection<string> PortNameList { get; set; } = new ObservableCollection<string>();

    [ObservableProperty]
    private string selectedPortName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> baudRateList = new ObservableCollection<string>();

    [ObservableProperty]
    private string selectedBaudRate = "115200";

    [ObservableProperty]
    private SerialportSettingVM serialportUISetting = new SerialportSettingM().ToVM();

    #endregion SerialPortSetting

    [ObservableProperty]
    private string dataToSend = "";

    internal SerialPortComponent serialPortHelper { get; set; }
    private CancellationTokenSource? timedSendCts;
    private CancellationTokenSource? refreshCts;

    /// <summary>快捷发送列表</summary>
    public ObservableCollection<QuickSendVM> QuickSendList { get; set; } = new ObservableCollection<QuickSendVM>();

    #region 界面显示

    [ObservableProperty]
    private string openCloseButtonContent = I18nManager.GetString(Language.Open)!;

    [ObservableProperty]
    private string statusTextBlockContent = I18nManager.GetString(Language.Close)!;

    [ObservableProperty]
    private int sentCount;

    [ObservableProperty]
    private int receivedCount;

    [ObservableProperty]
    private bool isOpen = false;

    [ObservableProperty]
    private string logTip = "右键打开Log目录";

    #region 定时发送

    /// <summary>定时发送</summary>
    [ObservableProperty]
    private bool timedSend;

    [ObservableProperty]
    private int timedCount = 1000;

    #endregion 定时发送

    #endregion 界面显示

    public static LoggerWrapper logger = LogManager.GetLogger();

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.SerialportDebugTool)!;
    }

    public override void OnNavigatedLeave()
    {
        base.OnNavigatedLeave();

        timedSendCts?.Cancel();
        refreshCts?.Cancel();
        SaveSetting();
    }

    public SerialportPageVM()
    {
        RandomHeader = "Default";
    }

    public override void OnNavigatedEnter(UserControl userControl)
    {
        base.OnNavigatedEnter(userControl);
        LoadSetting();
    }

    public override void InitializeViewModel()
    {
        if (isInitialized)
            return;

        WeakReferenceMessenger.Default.Register<CloseMessage>(this, (r, m) =>
        {
            // Handle the message here, with r being the recipient and m being the input message.
            // Using the recipient passed as input makes it so that the lambda expression doesn't
            // capture "this", improving performance.

            SaveSetting();
        });

        RefreshPortList();

        BaudRateList.AddRange(new List<string>() { "330", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "56000", "115200", "128000", "230400" });

        SelectedBaudRate = "115200";

        //Dtr = true;
        serialPortHelper = new SerialPortComponent(WriteInfoLog);
        serialPortHelper.UartDataSent += SerialPortHelper_UartDataSent;
        serialPortHelper.UartDataRecived += SerialPortHelper_UartDataRecived;

        base.InitializeViewModel();
    }

    private void SerialPortHelper_UartDataRecived(object? sender, EventArgs e)
    {
        if (sender is not byte[] data)
        {
            return;
        }

        _ = Dispatcher.UIThread.InvokeAsync(() =>
        {
            ReceivedCount += data.Length;
            ShowData("", data, false, SerialportUISetting.SendAndReceiveSettingVM.HexShow, true);
        });
    }

    private void SerialPortHelper_UartDataSent(object? sender, EventArgs e)
    {
        if (sender is not byte[] data)
        {
            return;
        }

        _ = Dispatcher.UIThread.InvokeAsync(() =>
        {
            SentCount += data.Length;
            ShowData("", data: data, send: true, SerialportUISetting.SendAndReceiveSettingVM.HexSend, alreadyOnUIThread: true);
        });
    }

    private bool refreshLock = false;

    /// <summary>刷新设备列表</summary>
    [RelayCommand]
    private async Task RefreshPortList(string? lastPort = null)
    {
        if (refreshLock)
            return;
        refreshLock = true;
        refreshCts?.Cancel();
        refreshCts = new CancellationTokenSource();
        var token = refreshCts.Token;

        try
        {
            var ports = await Task.Run(() => GetPortDisplayNames(token), token);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PortNameList.Clear();
                foreach (var item in ports)
                {
                    PortNameList.Add(item);
                }

                if (PortNameList.Count > 0)
                {
                    SelectedPortName = PortNameList[0];
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Ignore canceled refresh.
        }
        finally
        {
            refreshLock = false;
        }
    }

    private static List<string> GetPortDisplayNames(CancellationToken cancellationToken)
    {
        List<string> names = new();

        if (OperatingSystem.IsWindows())
        {
            names.AddRange(ReadPortsFromWmi(cancellationToken));
        }

        foreach (var p in SerialPort.GetPortNames())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pp = p;
            if (p.IndexOf('\0') > 0)
            {
                pp = p.Substring(0, p.IndexOf('\0'));
            }

            if (!names.Any(n => n.Contains($"({pp})", StringComparison.OrdinalIgnoreCase)))
            {
                names.Add($"Serial Port {pp} ({pp})");
            }
        }

        return names;
    }

    [SupportedOSPlatform("windows")]
    private static List<string> ReadPortsFromWmi(CancellationToken cancellationToken)
    {
        List<string> result = new();
        var regExp = new Regex("\\(COM\\d+\\)");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var caption = queryObj["Caption"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(caption) && regExp.IsMatch(caption))
                    {
                        result.Add(caption);
                    }
                }

                break;
            }
            catch
            {
                Task.Delay(500, cancellationToken).Wait(cancellationToken);
            }
        }

        return result;
    }

    /// <summary>是否正在打开端口</summary>
    private bool isOpeningPort = false;

    #region OpenClose

    [RelayCommand]
    private async Task OpenClosePort()
    {
        if (!serialPortHelper.IsOpen())//打开串口逻辑
        {
            await OpenPort();
        }
        else//关闭串口逻辑
        {
            try
            {
                serialPortHelper.Close();
            }
            catch
            {
                //串口关闭失败！

                NotifyHelper.ShowErrorMessage(I18nManager.GetString(Language.ErrorClosePort)!);
            }

            OpenCloseButtonContent = I18nManager.GetString(Language.Open)!;

            IsOpen = false;

            StatusTextBlockContent = I18nManager.GetString(Language.Close)!;

            //refreshPortList(lastPort);
        }
    }

    private async Task OpenPort()
    {
        if (isOpeningPort)
            return;
        if (SelectedPortName != null)
        {
            string[] ports;//获取所有串口列表
            try
            {
                WriteInfoLog($"GetPortNames");
                ports = SerialPort.GetPortNames();
                WriteInfoLog($"GetPortNames{ports.Length}");
            }
            catch (Exception e)
            {
                ports = new string[0];

                NotifyHelper.ShowErrorMessage($"[openPort]GetPortNames Exception:{e.Message}");
            }
            string port = "";//最终串口名
            foreach (string p in ports)//循环查找符合名称串口
            {
                //有些人遇到了微软库的bug，所以需要手动从0x00截断
                var pp = p;
                if (p.IndexOf("\0") > 0)
                    pp = p.Substring(0, p.IndexOf("\0"));
                if ((SelectedPortName as string).Contains($"({pp})"))//如果和选中项目匹配
                {
                    port = pp;
                    break;
                }
            }

            if (port != "")
            {
                await Task.Run(() =>
                {
                    isOpeningPort = true;
                    try
                    {
                        WriteInfoLog($"SetName");
                        serialPortHelper.SetName(port);

                        SerialportUISetting.SerialportParams.BaudRate = int.Parse(SelectedBaudRate);
                        WriteInfoLog($"Open");

                        serialPortHelper.Open(SerialportUISetting.ToModel());

                        Dispatcher.UIThread.Post(() =>
                        {
                            IsOpen = true;
                            OpenCloseButtonContent = I18nManager.GetString(Language.Close)!;
                            StatusTextBlockContent = I18nManager.GetString(Language.Open)!;
                        });
                    }
                    catch (Exception e)
                    {
                        //串口打开失败！
                        Dispatcher.UIThread.Post(() =>
                        {
                            NotifyHelper.ShowErrorMessage(I18nManager.GetString(Language.ErrorOpenPort)! + e.Message);
                        });
                    }
                    isOpeningPort = false;
                });
            }
        }
    }

    #endregion OpenClose

    #region SendData

    [RelayCommand]
    private void SendData()
    {
        //string temp;
        //if (SerialportUISetting.SendAndReceiveSettingVM.DeleteWhiteSpace)
        //{
        //    temp = DataToSend.Replace(" ", "");
        //}
        //else
        //{
        //    temp = DataToSend;
        //}
        //var data = System.Text.Encoding.UTF8.GetBytes(temp);

        SendUartData(DataToSend);
    }

    /// <summary>定时发送</summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    partial void OnTimedSendChanged(bool value)
    {
        timedSendCts?.Cancel();

        if (value)
        {
            timedSendCts = new CancellationTokenSource();
            _ = StartTimedSendAsync(timedSendCts.Token);
        }
    }

    private async Task StartTimedSendAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (TimedSend && !cancellationToken.IsCancellationRequested)
            {
                SendUartData(DataToSend);
                await Task.Delay(TimedCount, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore canceled timed send.
        }
    }

    /// <summary>发串口数据</summary>
    /// <param name="data"></param>
    private void SendUartData(string dataToSend)
    {
        if (!serialPortHelper.IsOpen()) return;

        byte[] dataConvert;

        try
        {
            string tempData = DataToSend.Replace("\r\n", "");

            if (SerialportUISetting.SendAndReceiveSettingVM.DeleteWhiteSpace)
            {
                tempData = tempData.Replace(" ", "");
            }

            if (SerialportUISetting.SendAndReceiveSettingVM.HexSend)
            {
                dataConvert = StringHelper.HexStringToBytes(tempData);//转换成对应Hex发送
            }
            else
            {
                dataConvert = System.Text.Encoding.UTF8.GetBytes(tempData);//直接转换成byte发送
            }

            if (SerialportUISetting.SendAndReceiveSettingVM.WithExtraEnter)
            {
                var temp = dataConvert.ToList();
                temp.Add(0x0d);
                temp.Add(0x0a);
                dataConvert = temp.ToArray();
            }
            serialPortHelper.SendData(dataConvert);
        }
        catch (Exception ex)
        {
            NotifyHelper.ShowErrorMessage($"{I18nManager.GetString(Language.ErrorSendFail)}\r\n" + ex.ToString());
            return;
        }
    }

    #endregion SendData

    #region QuickSendList

    [RelayCommand]
    private void AddQuickSendItem()
    {
        QuickSendList.Add(new QuickSendVM() { Id = QuickSendList.Count + 1, Text = "", Hex = false, Commit = I18nManager.GetString("QuickSendButton") ?? "Send" });
    }

    [RelayCommand]
    private void DeleteQuickSendItem(QuickSendVM item)
    {
        if (item != null && QuickSendList.Contains(item))
        {
            QuickSendList.Remove(item);
            // 重新排序 ID (可选)
            for (int i = 0; i < QuickSendList.Count; i++)
            {
                QuickSendList[i].Id = (i + 1);
            }
        }
        // NotifyHelper.ShowErrorMessage(ex.ToString());
    }

    #endregion QuickSendList

    #region Setting

    [RelayCommand]
    private void ShowMoreSerialportSetting(object obj)
    {
        Debug.WriteLine("ShowMoreSerialportSetting");
        LoadSetting();
    }

    [RelayCommand]
    private void SaveMoreSerialportSetting(object obj)
    {
        Debug.WriteLine("SaveMoreSerialportSetting");
        SaveSetting();
    }

    public void SaveSetting()
    {
        var service = App.Current.Services.GetService<SettingService>()!;

        SerialportUISetting.QuickSendList = QuickSendList.ToList();

        SerialportUISetting.SerialportParams.BaudRate = int.TryParse(SelectedBaudRate, out var baudRate) ? baudRate : 115200;
        service.SerialportSetting = SerialportUISetting.ToModel();

        service.SaveSerialportSetting();
    }

    public void LoadSetting()
    {
        var service = App.Current.Services.GetService<SettingService>()!;

        service.LoadLocalSerialportSetting();
        SerialportUISetting = service.SerialportSetting.ToVM();

        SelectedBaudRate = SerialportUISetting.SerialportParams.BaudRate.ToString();

        QuickSendList.Clear();
        QuickSendList.AddRange(SerialportUISetting.QuickSendList);
    }

    #endregion Setting

    #region Log

    [RelayCommand]
    private void SaveLog()
    {
        GenerateRandomHeader();
    }

    [RelayCommand]
    private void ClearLog(object obj)
    {
        LogDocument = new TextDocument();
    }

    public string RandomHeader { get; set; } = "Default";

    private void GenerateRandomHeader()
    {
        Random random = new Random();
        RandomHeader = random.Next(0, 100000).ToString();

        LogTip = "右键打开Log目录,前缀-> " + RandomHeader;
    }

    public override void WriteInfoLog(string msg)
    {
        logger.WithPatternProperty("Random", RandomHeader).Info(msg);
    }

    #endregion Log

    [ObservableProperty]
    private TextDocument logDocument = new TextDocument();

    #region ShowLog

    /// <summary>显示消息的方法</summary>
    /// <param name="title">只支持字符串</param>
    /// <param name="data"></param>
    /// <param name="send"></param>
    /// <param name="hexMode"></param>
    public virtual void ShowData(string title = "", byte[]? data = null, bool send = false, bool hexMode = false, bool alreadyOnUIThread = false)
    {
        string realData = "";

        if (data != null)
        {
            realData = hexMode
                ? StringHelper.BytesToHexString(data)
                : Encoding.UTF8.GetString(data);
        }

        var prefix = data == null ? "" : (send ? " << " : " >> ");

        string line =
            $"{DateTime.Now:HH:mm:ss.fff}{prefix}{realData}";

        WriteInfoLog(line);

        Action insertAction = () =>
        {
            LogDocument.Insert(
                LogDocument.TextLength,
                line + Environment.NewLine);
        };

        if (alreadyOnUIThread)
        {
            insertAction();
            return;
        }

        Dispatcher.UIThread.Post(insertAction);
    }

    #endregion ShowLog
}
