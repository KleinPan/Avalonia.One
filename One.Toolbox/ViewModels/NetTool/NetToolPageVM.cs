using Avalonia.Controls;
using Avalonia.Threading;

using AvaloniaEdit.Document;

using One.Base.Helpers.DataProcessHelpers;
using One.Base.Helpers.NetHelpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace One.Toolbox.ViewModels.NetTool;

public partial class NetToolPageVM : BasePageVM
{
    private AsyncTCPClient? tcpClient;
    private AsyncTCPServer? tcpServer;
    private AsyncUDPClient? udpClient;

    [ObservableProperty]
    private ObservableCollection<PortUsageItemVM> portUsageItems = [];

    [ObservableProperty]
    private int localPort = 9000;

    [ObservableProperty]
    private bool socketHexSend;

    [ObservableProperty]
    private bool socketHexShow;

    [ObservableProperty]
    private string socketSendText = "";

    [ObservableProperty]
    private string socketRemoteHost = "127.0.0.1";

    [ObservableProperty]
    private int socketRemotePort = 9000;

    [ObservableProperty]
    private bool socketStarted;

    [ObservableProperty]
    private string socketMode = ModeTcpClient;

    public ObservableCollection<string> SocketModes { get; } = [ModeTcpClient, ModeTcpServer, ModeUdp];

    [ObservableProperty]
    private TextDocument logDocument = new();

    public bool IsTcpClientMode => SocketMode == "TCP Client";

    public bool IsTcpServerMode => SocketMode == "TCP Server";

    public bool IsUdpMode => SocketMode == "UDP";

    public bool ShowRemoteEndpoint => IsTcpClientMode || IsUdpMode;

    public bool ShowLocalPort => IsTcpServerMode || IsUdpMode;

    private const string ModeTcpClient = "TCP Client";
    private const string ModeTcpServer = "TCP Server";
    private const string ModeUdp = "UDP";

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.NetDebugTool)!;
    }

    [RelayCommand]
    private void RefreshPortUsage()
    {
        var usages = new List<PortUsageItemVM>();
        var ip = IPGlobalProperties.GetIPGlobalProperties();

        foreach (var item in ip.GetActiveTcpListeners())
        {
            usages.Add(new PortUsageItemVM(I18nManager.GetString(Language.NetTcpListener)!, item.Address.ToString(), item.Port, I18nManager.GetString(Language.NetListening)!));
        }

        foreach (var item in ip.GetActiveUdpListeners())
        {
            usages.Add(new PortUsageItemVM(I18nManager.GetString(Language.NetUdpListener)!, item.Address.ToString(), item.Port, I18nManager.GetString(Language.NetListening)!));
        }

        foreach (var item in ip.GetActiveTcpConnections())
        {
            usages.Add(new PortUsageItemVM(
                I18nManager.GetString(Language.NetTcpConnection)!,
                item.LocalEndPoint.ToString(),
                item.LocalEndPoint.Port,
                item.State.ToString()));
        }

        PortUsageItems = new ObservableCollection<PortUsageItemVM>(usages.OrderBy(x => x.Port).ThenBy(x => x.Type));
    }

    #region Start&Stop

    [RelayCommand]
    private async Task StartSocket()
    {
        if (SocketStarted)
            return;

        try
        {
            switch (SocketMode)
            {
                case ModeTcpClient:
                    await StartTcpClient();
                    break;

                case ModeTcpServer:
                    StartTcpServer();
                    break;

                case ModeUdp:
                    StartUdp();
                    break;
            }

            SocketStarted = true;
            AppendLog(string.Format(I18nManager.GetString(Language.NetSocketStartedLog)!, SocketMode));
        }
        catch (Exception ex)
        {
            AppendLog($"{I18nManager.GetString(Language.NetStartFailedLog)}: {ex.Message}");
        }
    }

    private async Task StartTcpClient()
    {
        tcpClient = new AsyncTCPClient(WriteInfoLog);
        tcpClient.ReceiveAction = data => ShowSocketData(data, send: false, SocketHexShow);
        tcpClient.SendAction = data => ShowSocketData(data, send: true, SocketHexSend);
        tcpClient.OnConnected = data => AppendLog($"[SYS] {Encoding.UTF8.GetString(data)}");
        tcpClient.OnDisConnected = data => AppendLog($"[SYS] {Encoding.UTF8.GetString(data)}");

        if (!await tcpClient.InitClient(IPAddress.Parse(SocketRemoteHost), SocketRemotePort))
        {
            throw new Exception(I18nManager.GetString(Language.NetTcpClientStartFailed)!);
        }
    }

    private void StartTcpServer()
    {
        tcpServer = new AsyncTCPServer(WriteInfoLog);
        tcpServer.ReceiveAction = (_, data) => ShowSocketData(data, send: false, SocketHexShow);
        tcpServer.SendAction = data => ShowSocketData(data, send: true, SocketHexSend);
        tcpServer.OnConnected = data => AppendLog($"[SYS] {Encoding.UTF8.GetString(data)}");
        tcpServer.OnDisConnected = data => AppendLog($"[SYS] {Encoding.UTF8.GetString(data)}");

        if (!tcpServer.InitAsServer(IPAddress.Any, LocalPort))
        {
            throw new Exception(I18nManager.GetString(Language.NetTcpServerStartFailed)!);
        }
    }

    private void StartUdp()
    {
        udpClient = new AsyncUDPClient(WriteInfoLog);
        udpClient.ReceiveAction = (remote, data) => ShowSocketData(data, send: false, SocketHexShow, remote);
        udpClient.SendAction = data => ShowSocketData(data, send: true, SocketHexSend);
        udpClient.OnConnected = data => AppendLog($"[SYS] {Encoding.UTF8.GetString(data)}");
        udpClient.OnDisConnected = data => AppendLog($"[SYS] {Encoding.UTF8.GetString(data)}");

        if (!udpClient.InitClient(IPAddress.Any, LocalPort))
        {
            throw new Exception(I18nManager.GetString(Language.NetUdpStartFailed)!);
        }
    }

    [RelayCommand]
    private void StopSocket()
    {
        try
        {
            tcpClient?.ReleaseClient();
            tcpServer?.ReleaseServer();
            udpClient?.ReleaseClient();
        }
        catch (Exception ex)
        {
            AppendLog($"{I18nManager.GetString(Language.NetStopFailedLog)}: {ex.Message}");
        }
        finally
        {
            tcpClient = null;
            tcpServer = null;
            udpClient = null;
            SocketStarted = false;
            AppendLog(I18nManager.GetString(Language.NetSocketStoppedLog)!);
        }
    }

    #endregion Start&Stop

    [RelayCommand]
    private void SendSocketData()
    {
        if (!SocketStarted)
        {
            AppendLog(I18nManager.GetString(Language.NetStartSocketFirstLog)!);
            return;
        }

        try
        {
            var payload = BuildPayload(SocketSendText, SocketHexSend);
            switch (SocketMode)
            {
                case ModeTcpClient:
                    tcpClient?.SendData(payload);
                    break;

                case ModeTcpServer:
                    tcpServer?.SendDataToAll(payload);
                    break;

                case ModeUdp:
                    udpClient?.SendData(new IPEndPoint(IPAddress.Parse(SocketRemoteHost), SocketRemotePort), payload);
                    break;
            }
        }
        catch (Exception ex)
        {
            AppendLog($"{I18nManager.GetString(Language.NetSendFailedLog)}: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        LogDocument = new TextDocument();
    }

    public override void OnNavigatedEnter(UserControl userControl)
    {
        base.OnNavigatedEnter(userControl);
        RefreshPortUsage();
    }

    partial void OnSocketModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsTcpClientMode));
        OnPropertyChanged(nameof(IsTcpServerMode));
        OnPropertyChanged(nameof(IsUdpMode));
        OnPropertyChanged(nameof(ShowRemoteEndpoint));
        OnPropertyChanged(nameof(ShowLocalPort));
    }

    private byte[] BuildPayload(string text, bool hex)
    {
        if (!hex)
        {
            return Encoding.UTF8.GetBytes(text ?? string.Empty);
        }

        return StringHelper.HexStringToBytes((text ?? string.Empty).Replace(" ", ""));
    }

    private string FormatBytes(byte[] data, bool hex)
    {
        if (hex)
        {
            return StringHelper.BytesToHexString(data);
        }

        return Encoding.UTF8.GetString(data);
    }

    private void ShowSocketData(byte[] data, bool send, bool hexMode, string? remote = null)
    {
        var prefix = send ? " << " : " >> ";
        var remotePart = string.IsNullOrWhiteSpace(remote) ? string.Empty : $"[{remote}] ";
        AppendLog($"{remotePart}{prefix}{FormatBytes(data, hexMode)}");
    }

    private void AppendLog(string line)
    {
        WriteInfoLog(line);
        Dispatcher.UIThread.Post(() =>
        {
            LogDocument.Insert(LogDocument.TextLength, $"{DateTime.Now:HH:mm:ss.fff}{line}{Environment.NewLine}");
        });
    }
}

public partial class PortUsageItemVM : ObservableObject
{
    public PortUsageItemVM(string type, string endpoint, int port, string state)
    {
        Type = type;
        Endpoint = endpoint;
        Port = port;
        State = state;
    }

    public string Type { get; }
    public string Endpoint { get; }
    public int Port { get; }
    public string State { get; }
}
