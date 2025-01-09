using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels;

public partial class ACopyVM : BasePageVM
{
    public ACopyVM()
    {
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