namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>纯输入算子，输入在输出显示</summary>
public partial class InputOperationVM : OperationVM
{
    public new NodifyObservableCollection<ConnectorVM> Output { get; set; } = new();

    public InputOperationVM()
    {
        Title = "Input Parameters";

        Output.Add(new ConnectorVM
        {
            Title = "In 0"
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