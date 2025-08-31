namespace One.Toolbox.ViewModels.StringNodify;

public partial class CalculatorInputOperationVM : OperationVM
{
    public new NodifyObservableCollection<ConnectorVM> Output { get; set; } = new();

    public CalculatorInputOperationVM()
    {
        Output.Add(new ConnectorVM
        {
            Title = $"In {Output.Count}"
        });
    }

    bool CanAddOutput() => Output.Count < 10;

    [RelayCommand(CanExecute = nameof(CanAddOutput))]
    private void AddOutput()
    {
        Output.Add(new ConnectorVM
        {
            Title = $"In {Output.Count}"
        });
    }

    bool CanRemoveOutput() => Output.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRemoveOutput))]
    private void RemoveOutput()
    {
        Output.RemoveAt(Output.Count - 1);
    }
}