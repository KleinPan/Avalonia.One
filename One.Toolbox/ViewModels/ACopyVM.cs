using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels;

public partial class ACopyVM : BasePageVM
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
        try
        {
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.ToString());
        }
    }
}