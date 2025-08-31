using Avalonia;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class OperationGraphVM : CalculatorOperationVM
{
    [ObservableProperty]
    private Size desiredSize;

    private Size _prevSize;

    private bool _isExpanded = true;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
            {
                if (_isExpanded)
                {
                    DesiredSize = _prevSize;
                }
                else
                {
                    _prevSize = Size;
                    // Fit content
                    DesiredSize = new Size(0, 0);
                }
            }
        }
    }

    public OperationGraphVM()
    {
        InnerCalculator.Operations[0].Location = new Point(50, 50);
        InnerCalculator.Operations[1].Location = new Point(200, 50);
    }
}