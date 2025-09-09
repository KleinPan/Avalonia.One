using Avalonia;

using One.Toolbox.ViewModels.Base;

using System.Diagnostics;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>一个操作的连接点</summary>
public partial class ConnectorVM : BaseVM,IDisposable
{
 
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _value;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isInput;

    [ObservableProperty]
    private Point _anchor;

    [ObservableProperty]
    private OperationVM _operation = default!;

    public List<ConnectorVM> ValueObservers { get; } = new();

    partial void OnValueChanged(string value)
    {
        ValueObservers.ForEach(o => o.Value = value);
    }

    partial void OnAnchorChanged(Point value)
    {
        Debug.WriteLine(Title + value);
    }

    public override string ToString()
    {
        return  "ConnectorVM: " + Title + " Value: " + Value + " IsInput: " + IsInput + " IsConnected: " + IsConnected;
    }
    private bool _disposed = false;

    // 释放资源的方法
    public void Dispose()
    {
        Dispose(true);
        // 告诉GC不需要再调用Finalize
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // 释放托管资源（如其他实现IDisposable的对象）
        }

        // 释放非托管资源（如文件、句柄等）
        Debug.WriteLine("对象资源已释放（可能即将被销毁）");
        _disposed = true;
    }
    // 析构函数（Finalizer）：当未主动调用Dispose时，GC会调用此方法
    ~ConnectorVM()
    {
        Dispose(false);
    }
}