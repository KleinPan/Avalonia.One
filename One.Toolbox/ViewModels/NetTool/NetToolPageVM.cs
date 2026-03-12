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
    private string socketMode = "TCP Client";

    public ObservableCollection<string> SocketModes { get; } = ["TCP Client", "TCP Server", "UDP"];

    [ObservableProperty]
    private TextDocument logDocument = new();

    public bool IsTcpClientMode => SocketMode == "TCP Client";

    public bool IsTcpServerMode => SocketMode == "TCP Server";

    public bool IsUdpMode => SocketMode == "UDP";

    public bool ShowRemoteEndpoint => IsTcpClientMode || IsUdpMode;

    public bool ShowLocalPort => IsTcpServerMode || IsUdpMode;

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
            usages.Add(new PortUsageItemVM("TCP Listener", item.Address.ToString(), item.Port, "Listening"));
        }

        foreach (var item in ip.GetActiveUdpListeners())
        {
            usages.Add(new PortUsageItemVM("UDP Listener", item.Address.ToString(), item.Port, "Listening"));
        }

        foreach (var item in ip.GetActiveTcpConnections())
        {
            usages.Add(new PortUsageItemVM(
                "TCP Connection",
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
                case "TCP Client":
                    await StartTcpClient();
                    break;

                case "TCP Server":
                    StartTcpServer();
                    break;

                case "UDP":
                    StartUdp();
                    break;
            }

            SocketStarted = true;
            AppendLog($"[SYS] Socket已启动: {SocketMode}");
        }
        catch (Exception ex)
        {
            AppendLog($"[ERR] 启动失败: {ex.Message}");
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
            throw new Exception("TCP客户端启动失败");
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
            throw new Exception("TCP服务端启动失败");
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
            throw new Exception("UDP启动失败");
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
            AppendLog($"[ERR] 停止异常: {ex.Message}");
        }
        finally
        {
            tcpClient = null;
            tcpServer = null;
            udpClient = null;
            SocketStarted = false;
            AppendLog("[SYS] Socket已停止");
        }
    }

    #endregion Start&Stop

    [RelayCommand]
    private void SendSocketData()
    {
        if (!SocketStarted)
        {
            AppendLog("[SYS] 请先启动Socket");
            return;
        }

        try
        {
            var payload = BuildPayload(SocketSendText, SocketHexSend);
            switch (SocketMode)
            {
                case "TCP Client":
                    tcpClient?.SendData(payload);
                    break;

                case "TCP Server":
                    tcpServer?.SendDataToAll(payload);
                    break;

                case "UDP":
                    udpClient?.SendData(new IPEndPoint(IPAddress.Parse(SocketRemoteHost), SocketRemotePort), payload);
                    break;
            }
        }
        catch (Exception ex)
        {
            AppendLog($"[ERR] 发送失败: {ex.Message}");
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