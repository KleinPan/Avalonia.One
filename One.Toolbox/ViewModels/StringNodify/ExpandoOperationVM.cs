namespace One.Toolbox.ViewModels.StringNodify;

public partial class ExpandoOperationVM : OperationVM
{
    public ExpandoOperationVM()
    {
        //不知道作用
        //Input.WhenAdded(_ => AddInputCommand.RaiseCanExecuteChanged());
        //Input.WhenRemoved(_ => AddInputCommand.RaiseCanExecuteChanged());
        //Input.WhenAdded(_ => RemoveInputCommand.RaiseCanExecuteChanged());
        //Input.WhenRemoved(_ => RemoveInputCommand.RaiseCanExecuteChanged());
    }

    bool CanAddInput() => Input.Count < MaxInput;

    [RelayCommand(CanExecute = nameof(CanAddInput))]
    private void AddInput()
    {
        Input.Add(new ConnectorVM());
    }

    bool CanRemoveInput() => Input.Count > MinInput;

    [RelayCommand(CanExecute = nameof(CanRemoveInput))]
    private void RemoveInput()
    {
        Input.RemoveAt(Input.Count - 1);
    }

    [ObservableProperty]
    private uint _minInput = 0;

    [ObservableProperty]
    private uint _maxInput = uint.MaxValue;
}