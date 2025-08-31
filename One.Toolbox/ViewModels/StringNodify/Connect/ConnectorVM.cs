using Avalonia;

using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class ConnectorVM : BaseVM
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private double _value;

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