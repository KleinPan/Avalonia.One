using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels;

public partial class ACopyVM : BaseVM
{
    public ACopyVM()
    {
    }

    public override void OnNavigatedEnter()
    {
        base.OnNavigatedEnter();
        InitData();
    }

    void InitData()
    {
    }
}