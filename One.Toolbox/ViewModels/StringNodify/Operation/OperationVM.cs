using Avalonia;

using One.Toolbox.ViewModels.Base;

using System.ComponentModel;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class OperationVM : BaseVM
{
    [ObservableProperty]
    private Point _location;

    [ObservableProperty]
    private Size _size;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private bool _isSelected;

    public bool IsReadOnly { get; set; }

    [ObservableProperty]
    private IOperation? _operation;

    [ObservableProperty]
    private ConnectorVM? _output;

    partial void OnOutputChanged(ConnectorVM? value)
    {
        if (_output != null) { _output.Operation = this; }
    }

    public NodifyObservableCollection<ConnectorVM> Input { get; } = new();

    public OperationVM()
    {
        Input.WhenAdded(x =>
        {
            x.Operation = this;
            x.IsInput = true;
            x.PropertyChanged += OnInputValueChanged;
        }).WhenRemoved(x =>
        {
            x.PropertyChanged -= OnInputValueChanged;
        });
    }

    private void OnInputValueChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectorVM.Value))
        {
            OnInputValueChanged();
        }
    }

    protected virtual void OnInputValueChanged()
    {
        if (Output != null && Operation != null)
        {
            try
            {
                var input = Input.Select(i => i.Value).ToArray();
                Output.Value = Operation?.Execute(input) ?? 0;
            }
            catch
            {
            }
        }
    }
}