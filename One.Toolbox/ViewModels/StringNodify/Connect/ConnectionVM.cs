using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>连接线</summary>
public partial class ConnectionVM : BaseVM
{
    /// <summary>目标</summary>
    [ObservableProperty]
    private ConnectorVM _input = default!;

    /// <summary>源</summary>
    [ObservableProperty]
    private ConnectorVM _output = default!;
}