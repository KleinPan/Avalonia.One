using Avalonia;

using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class OperationsMenuVM : BaseVM
{
    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private Point _location;

    public event Action? Closed;

    public NodifyObservableCollection<OperationInfoVM> AvailableOperations { get; }

    private readonly CalculatorVM _calculator;

    public OperationsMenuVM(CalculatorVM calculator)
    {
        _calculator = calculator;

        List<OperationInfoVM> operations = new List<OperationInfoVM>()
        {
            new OperationInfoVM()
            {
                Title = "ParseInput",
                Type = OperationType.Input,
                MinInput = 1,
                MaxInput = 10,
               
               
            },

            new OperationInfoVM()
            {
                Title = "ParseOutput",
                Type = OperationType.Output,
                MinInput = 1,
                MaxInput = 1,
              
            }
        };

        operations.AddRange(OperationFactory.GetOperationsInfo(typeof(OperationsContainer)));

        AvailableOperations = new NodifyObservableCollection<OperationInfoVM>(operations);
    }

    public void OpenAt(Point targetLocation)
    {
        Close();
        Location = targetLocation;
        IsVisible = true;
    }

    public void Close()
    {
        IsVisible = false;
    }

    [RelayCommand]
    private void CreateOperation(OperationInfoVM operationInfo)
    {
        OperationVM op = OperationFactory.GetOperation(operationInfo);
        op.Location = Location;

        _calculator.Nodes.Add(op);

        var pending = _calculator.PendingConnection;
        if (pending.IsVisible)
        {
            var connector = pending.Source.IsInput ? op.Output : op.Input.FirstOrDefault();
            if (connector != null && _calculator.CanPendingConnectionCompleted(pending.Source, connector))
            {
                _calculator.PendingConnectionCompleted(pending.Source, connector);
            }
        }
        Close();
    }
}