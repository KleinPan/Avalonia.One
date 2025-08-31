using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class ConnectionVM : BaseVM
{
    [ObservableProperty]
    private ConnectorVM _input = default!;

    [ObservableProperty]
    private ConnectorVM _output = default!;
}