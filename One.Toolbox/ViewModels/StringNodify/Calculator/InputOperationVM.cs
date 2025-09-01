namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>纯输入算子，输入在输出显示</summary>
public partial class InputOperationVM : OperationVM
{
    public new NodifyObservableCollection<ConnectorVM> Output { get; set; } = new();

    public InputOperationVM()
    {
        Title = "Input Parameters";

        // 监听集合变化，当集合项增减时触发命令状态更新
        Output.CollectionChanged += (s, e) =>
        {
            // 通知 RemoveOutput 命令重新评估 CanExecute
            RemoveOutputCommand.NotifyCanExecuteChanged();
            // 同时更新 AddOutput 命令的状态（可选，视需求而定）
            AddOutputCommand.NotifyCanExecuteChanged();
        };

        Output.Add(new ConnectorVM
        {
            Title = "In 0",
            Value = "00",
            IsInput = true,
        });
    }

    bool CanAddOutput() => Output.Count < 10;

    [RelayCommand(CanExecute = nameof(CanAddOutput))]
    private void AddOutput()
    {
        Output.Add(new ConnectorVM
        {
            Title = $"In {Output.Count}",
            Value = "0" + Output.Count,
            IsInput = true,
        });
    }

    bool CanRemoveOutput() => Output.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRemoveOutput))]
    private void RemoveOutput()
    {
        Output.RemoveAt(Output.Count - 1);
    }
}