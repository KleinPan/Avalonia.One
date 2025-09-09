using One.Toolbox.ViewModels.StringNodify.Operation;

using System.Text;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>将多个输入合并为一个输入</summary>
public partial class InputOperationVM : OperationVM
{
    public new NodifyObservableCollection<ConnectorVM> Input { get; set; } = new();

    [ObservableProperty]
    private bool removeWhiteSpace=true;
    public InputOperationVM()
    {
        //监听集合变化，当集合项增减时触发命令状态更新
        Input.CollectionChanged += (s, e) =>
        {
            // 通知 RemoveOutput 命令重新评估 CanExecute
            RemoveInputCommand.NotifyCanExecuteChanged();
            // 同时更新 AddOutput 命令的状态（可选，视需求而定）
            AddInputCommand.NotifyCanExecuteChanged();

            OnInputValueChanged();
        };

        Input.WhenAdded(x =>
        {
            x.Operation = this;
            x.IsInput = true;
            x.PropertyChanged += X_PropertyChanged;
        }).WhenRemoved(x =>
        {
            x.PropertyChanged -= X_PropertyChanged;
        });

        Output = new ConnectorVM
        {
            Title = "Out",
            IsInput = false,
        };

        Operation = new CombineOperation(CombineInputs);

        Input.Add(new ConnectorVM
        {
            Title = "In 0",
            Value = "00",
            IsInput = true,
        });

        
    }

    private void X_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectorVM.Value))
        {
            OnInputValueChanged();
        }
    }

    protected override void OnInputValueChanged()
    {
        if (Output != null && Operation != null)
        {
            try
            {
                var input = Input.Select(i => i.Value.Replace(" ","")).ToArray();
                Output.Value = Operation?.Execute(input) ?? "";//The null-coalescing operator ?? returns the value of its left-hand operand if it isn't null;
            }
            catch
            {
            }
        }
    }

    bool CanAddInput() => Input.Count < 10;

    [RelayCommand(CanExecute = nameof(CanAddInput))]
    private void AddInput()
    {
        Input.Add(new ConnectorVM
        {
            Title = $"In {Input.Count}",
            Value = "0" + Input.Count,
            IsInput = true,
        });
    }

    bool CanRemoveInput() => Input.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRemoveInput))]
    private void RemoveInput()
    {
        Input.RemoveAt(Input.Count - 1);
    }

    string CombineInputs(string[] bytes)
    {
        string result2 = string.Join("", bytes);
        return result2;
    }
}