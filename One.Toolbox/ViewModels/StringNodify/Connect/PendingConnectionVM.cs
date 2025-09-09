using Avalonia;

using One.Toolbox.ViewModels.Base;

using System.Diagnostics;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class PendingConnectionVM : BaseVM
{
    [ObservableProperty]
    private ConnectorVM _source = default!;

    [ObservableProperty]
    private ConnectorVM? _target;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private Point _targetLocation;

    partial void OnTargetLocationChanged(Point value)
    {
       // Debug.WriteLine("PendingConnectionVM" + value);
    }

    
}