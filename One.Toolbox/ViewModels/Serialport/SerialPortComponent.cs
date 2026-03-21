using One.Toolbox.ViewModels.Base;

using System.IO.Ports;

namespace One.Toolbox.ViewModels.Serialport;

internal sealed class SerialPortComponent : BaseVM, IDisposable
{
    private readonly object serialLock = new();
    private readonly AutoResetEvent waitUartReceive = new(false);
    private readonly CancellationTokenSource receiveLoopCts = new();
    private readonly Task receiveLoopTask;
    private readonly Action<string> innerlogaction;

    private Stream? lastPortBaseStream;

    private SerialportSettingM serialportSetting = new();
    private SerialPort serialPort = new();

    public event EventHandler? UartDataRecived;
    public event EventHandler? UartDataSent;

    public SerialPortComponent(Action<string> logAction)
    {
        innerlogaction = logAction;
        serialPort.DataReceived += Serial_DataReceived;
        receiveLoopTask = Task.Run(() => ReadDataLoop(receiveLoopCts.Token));
    }

    public string GetName()
    {
        lock (serialLock)
        {
            return serialPort.PortName;
        }
    }

    public void SetName(string name)
    {
        lock (serialLock)
        {
            serialPort.PortName = name;
        }
    }

    public bool IsOpen()
    {
        lock (serialLock)
        {
            return serialPort.IsOpen;
        }
    }

    public void Open(SerialportSettingM setting)
    {
        lock (serialLock)
        {
            serialportSetting = setting;

            var portName = serialPort.PortName;
            RefreshSerialDevice();
            serialPort.PortName = portName;

            serialPort.BaudRate = serialportSetting.SerialportParams.BaudRate;
            serialPort.Parity = (Parity)serialportSetting.SerialportParams.Parity;
            serialPort.DataBits = serialportSetting.SerialportParams.DataBits;
            serialPort.StopBits = (StopBits)serialportSetting.SerialportParams.StopBits;
            serialPort.RtsEnable = serialportSetting.SerialportParams.RtsEnable;
            serialPort.DtrEnable = serialportSetting.SerialportParams.DtrEnable;

            WriteTraceLog("[UartOpen]open");
            serialPort.Open();
            lastPortBaseStream = serialPort.BaseStream;
            WriteTraceLog("[UartOpen]done");
        }
    }

    public void Close()
    {
        lock (serialLock)
        {
            WriteTraceLog("[UartClose]start");
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            finally
            {
                RefreshSerialDevice();
            }

            WriteTraceLog("[UartClose]done");
        }
    }

    public void SendData(byte[] data)
    {
        if (data.Length == 0)
        {
            return;
        }

        lock (serialLock)
        {
            if (!serialPort.IsOpen)
            {
                return;
            }

            serialPort.Write(data, 0, data.Length);
        }

        UartDataSent?.Invoke(data, EventArgs.Empty);
    }

    private void WriteTraceLog(string msg)
    {
        innerlogaction?.Invoke(msg);
    }

    private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        waitUartReceive.Set();
    }

    private void ReadDataLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            waitUartReceive.WaitOne(100);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            SerialportSettingM snapshot;
            lock (serialLock)
            {
                snapshot = serialportSetting;
            }

            if (snapshot.SendAndReceiveSettingModel.Timeout > 0)
            {
                Thread.Sleep(snapshot.SendAndReceiveSettingModel.Timeout);
            }

            var result = new List<byte>();
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[]? rev = null;

                lock (serialLock)
                {
                    if (!serialPort.IsOpen)
                    {
                        break;
                    }

                    try
                    {
                        var length = serialPort.BytesToRead;
                        if (length == 0)
                        {
                            break;
                        }

                        rev = new byte[length];
                        serialPort.Read(rev, 0, length);
                    }
                    catch
                    {
                        break;
                    }
                }

                if (rev == null || rev.Length == 0)
                {
                    break;
                }

                result.AddRange(rev);
                if (result.Count > snapshot.SendAndReceiveSettingModel.MaxLength)
                {
                    break;
                }
            }

            if (result.Count > 0)
            {
                UartDataRecived?.Invoke(result.ToArray(), EventArgs.Empty);
            }
        }
    }

    private void RefreshSerialDevice()
    {
        WriteTraceLog("[refreshSerialDevice]start");

        try
        {
            lastPortBaseStream?.Dispose();
        }
        catch
        {
        }

        try
        {
            serialPort.BaseStream?.Dispose();
        }
        catch
        {
        }

        try
        {
            serialPort.DataReceived -= Serial_DataReceived;
            serialPort.Dispose();
        }
        catch
        {
        }

        serialPort = new SerialPort();
        serialPort.DataReceived += Serial_DataReceived;

        WriteTraceLog("[refreshSerialDevice]done");
    }

    public void Dispose()
    {
        receiveLoopCts.Cancel();
        waitUartReceive.Set();

        try
        {
            receiveLoopTask.Wait(TimeSpan.FromSeconds(1));
        }
        catch
        {
        }

        lock (serialLock)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch
            {
            }

            try
            {
                serialPort.DataReceived -= Serial_DataReceived;
                serialPort.Dispose();
            }
            catch
            {
            }
        }

        waitUartReceive.Dispose();
        receiveLoopCts.Dispose();
    }
}
