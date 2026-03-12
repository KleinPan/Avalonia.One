using Avalonia.Controls;
using Avalonia.Threading;

using AvaloniaEdit.Document;

using One.Base.Helpers.DataProcessHelpers;
using One.Base.Helpers.NetHelpers;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace One.Toolbox.ViewModels.PortTool;

public partial class PortToolPageVM : BasePageVM
{
    private AsyncTCPClient? tcpClient;
    private AsyncTCPServer? tcpServer;
    private AsyncUDPClient? udpClient;

    [ObservableProperty]
    private ObservableCollection<PortUsageItemVM> portUsageItems = [];

    [ObservableProperty]
    private string targetHost = "127.0.0.1";

    [ObservableProperty]
    private int targetPort = 80;

    [ObservableProperty]
    private int localPort = 9000;

    [ObservableProperty]
    private string testResult = "";

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

    public override void UpdateTitle()
    {
        Title = "端口工具";
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

    [RelayCommand]
    private async Task RunTcpTest()
    {
        try
        {
            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            await client.ConnectAsync(TargetHost, TargetPort, cts.Token);
            TestResult = $"TCP连接成功: {TargetHost}:{TargetPort}";
        }
        catch (Exception ex)
        {
            TestResult = $"TCP连接失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RunUdpTest()
    {
        try
        {
            using var client = new UdpClient(LocalPort);
            var endpoint = new IPEndPoint(IPAddress.Parse(TargetHost), TargetPort);
            var bytes = Encoding.UTF8.GetBytes("UDP TEST");
            await client.SendAsync(bytes, endpoint);
            TestResult = $"UDP发送成功: {endpoint}";
        }
        catch (Exception ex)
        {
            TestResult = $"UDP测试失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void StartSocket()
    {
        if (SocketStarted)
            return;

        try
        {
            switch (SocketMode)
            {
                case "TCP Client":
                    StartTcpClient();
                    break;

                case "TCP Server":
                    StartTcpServer();
                    break;

                case "UDP":
                    StartUdp();
                    break;
            }

            SocketStarted = true;
            AppendLog($"Socket已启动: {SocketMode}");
        }
        catch (Exception ex)
        {
            AppendLog($"启动失败: {ex.Message}");
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
            AppendLog($"停止异常: {ex.Message}");
        }
        finally
        {
            tcpClient = null;
            tcpServer = null;
            udpClient = null;
            SocketStarted = false;
            AppendLog("Socket已停止");
        }
    }

    [RelayCommand]
    private void SendSocketData()
    {
        if (!SocketStarted)
        {
            AppendLog("请先启动Socket");
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

            AppendLog($"[Send] {FormatBytes(payload, SocketHexSend)}");
        }
        catch (Exception ex)
        {
            AppendLog($"发送失败: {ex.Message}");
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

    private void StartTcpClient()
    {
        tcpClient = new AsyncTCPClient(WriteInfoLog);
        tcpClient.ReceiveAction = data => AppendLog($"[Recv] {FormatBytes(data, SocketHexShow)}");
        tcpClient.OnConnected = data => AppendLog($"[Conn] {Encoding.UTF8.GetString(data)}");
        tcpClient.OnDisConnected = data => AppendLog($"[Disc] {Encoding.UTF8.GetString(data)}");

        if (!tcpClient.InitClient(IPAddress.Parse(SocketRemoteHost), SocketRemotePort))
        {
            throw new Exception("TCP客户端启动失败");
        }
    }

    private void StartTcpServer()
    {
        tcpServer = new AsyncTCPServer(WriteInfoLog);
        tcpServer.ReceiveAction = (_, data) => AppendLog($"[Recv] {FormatBytes(data, SocketHexShow)}");
        tcpServer.OnConnected = data => AppendLog($"[Conn] {Encoding.UTF8.GetString(data)}");
        tcpServer.OnDisConnected = data => AppendLog($"[Disc] {Encoding.UTF8.GetString(data)}");

        if (!tcpServer.InitAsServer(IPAddress.Any, LocalPort))
        {
            throw new Exception("TCP服务端启动失败");
        }
    }

    private void StartUdp()
    {
        udpClient = new AsyncUDPClient(WriteInfoLog);
        udpClient.ReceiveAction = (remote, data) => AppendLog($"[Recv {remote}] {FormatBytes(data, SocketHexShow)}");
        udpClient.OnConnected = data => AppendLog($"[Conn] {Encoding.UTF8.GetString(data)}");
        udpClient.OnDisConnected = _ => AppendLog("[Disc] UDP Closed");

        if (!udpClient.InitClient(IPAddress.Any, LocalPort))
        {
            throw new Exception("UDP启动失败");
        }
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

    private void AppendLog(string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LogDocument.Insert(LogDocument.TextLength, $"{DateTime.Now:HH:mm:ss.fff} {line}{Environment.NewLine}");
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
