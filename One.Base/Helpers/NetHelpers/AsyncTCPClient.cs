using System.Net;
using System.Net.Sockets;
using System.Text;

namespace One.Base.Helpers.NetHelpers;

public class AsyncTCPClient : BaseHelper
{
    private Socket? socket;

    // 保留你原有的业务回调
    public Action<byte[]> ReceiveAction;

    public Action<byte[]> SendAction;
    public Action<byte[]> OnConnected;
    public Action<byte[]> OnDisConnected;

    public CancellationToken cancellationToken = default;

    public AsyncTCPClient(Action<string> logAction) : base(logAction)
    {
    }

    /// <summary>初始化并连接。现在它返回 <see cref="Task"/>，调用方可以用 await 准确知道是否连接成功。</summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public async Task<bool> InitClient(IPAddress ip, int port)
    {
        try
        {
            // 如果已有连接，先释放
            ReleaseClient();

            IPEndPoint ipEndPoint = new(ip, port);
            socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 1. 尝试连接（真正异步等待结果）
            await socket.ConnectAsync(ipEndPoint, cancellationToken);

            // 2. 连接成功后的处理
            var a = $"{socket.LocalEndPoint} connected!";
            WriteLog(a);

            // 触发你定义的回调
            OnConnected?.Invoke(Encoding.UTF8.GetBytes(a));

            // 3. 启动后台接收循环（不阻塞当前线程）
            _ = ReceiveLoop();

            return true;
        }
        catch (Exception ex)
        {
            WriteLog($"连接失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>循环接收数据</summary>
    private async Task ReceiveLoop()
    {
        // 缓冲区放在循环外重用，减少 GC 压力
        byte[] buffer = new byte[1024];
        try
        {
            while (socket != null && socket.Connected)
            {
                // 使用内存片段接收，直接 await 结果
                int bytesReceived = await socket.ReceiveAsync(buffer.AsMemory(), SocketFlags.None, cancellationToken);

                if (bytesReceived <= 0)
                {
                    WriteLog("服务器主动断开连接");
                    break;
                }

                // 提取实际数据并触发你的回调
                var receivedData = buffer.Take(bytesReceived).ToArray();
                ReceiveAction?.Invoke(receivedData);
            }
        }
        catch (OperationCanceledException) { /* 正常退出 */ }
        catch (Exception ex)
        {
            WriteLog($"接收异常: {ex.Message}");
        }
        finally
        {
            // 如果循环跳出，说明连接断了，清理资源
            ReleaseClient();
        }
    }

    /// <summary>发送数据</summary>
    public async void SendData(byte[] data)
    {
        if (socket == null || !socket.Connected)
        {
            WriteLog("发送失败：Socket 未连接");
            return;
        }

        try
        {
            // 循环发送直到发送完毕
            int totalSent = 0;
            while (totalSent < data.Length)
            {
                int sent = await socket.SendAsync(data.AsMemory(totalSent), SocketFlags.None, cancellationToken);
                if (sent <= 0) break;
                totalSent += sent;
            }

            // 触发你定义的发送回调
            SendAction?.Invoke(data);
        }
        catch (Exception ex)
        {
            WriteLog($"发送数据异常: {ex.Message}");
        }
    }

    /// <summary>释放连接</summary>
    public void ReleaseClient()
    {
        try
        {
            if (socket == null) return;

            if (socket.Connected)
            {
                var a = $"{socket.LocalEndPoint} disconnected!";
                OnDisConnected?.Invoke(Encoding.UTF8.GetBytes(a));

                socket.Shutdown(SocketShutdown.Both);
            }

            socket.Close();
            socket.Dispose();
            socket = null;
            WriteLog("Socket 资源已释放");
        }
        catch (Exception ex)
        {
            WriteLog($"释放资源错误: {ex.Message}");
        }
    }
}