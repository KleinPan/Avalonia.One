using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.RegularTester;

public partial class PatternVM : BaseVM
{
    [ObservableProperty]
    private string pattern;

    [ObservableProperty]
    private string description;
}