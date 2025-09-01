using Avalonia;

using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>一个操作的连接点</summary>
public partial class ConnectorVM : BaseVM
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private byte[] _value;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isInput;

    [ObservableProperty]
    private Point _anchor;

    [ObservableProperty]
    private OperationVM _operation = default!;

    public List<ConnectorVM> ValueObservers { get; } = new();
}