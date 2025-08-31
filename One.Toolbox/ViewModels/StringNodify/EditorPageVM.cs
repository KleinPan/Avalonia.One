using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.StringNodify;

public partial class EditorPageVM : BasePageVM
{
    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private CalculatorVM _calculator = default!;

    public EditorPageVM()
    {
        _calculator=new CalculatorVM();
    }
}