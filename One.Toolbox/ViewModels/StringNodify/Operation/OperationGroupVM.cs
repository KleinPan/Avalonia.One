using Avalonia;

using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class OperationGroupVM : OperationVM
{
    [ObservableProperty]
    private Size groupSize;
}