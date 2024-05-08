using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using One.Toolbox.Helpers;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.IconBoard;

public partial class IconBoardPageVM : BaseVM
{
    public IconBoardPageVM() { }

    public override void OnNavigatedEnter()
    {
        base.OnNavigatedEnter();
    }

    public override void InitializeViewModel()
    {
        base.InitializeViewModel();
        InitData();
    }

    public ObservableCollection<IconItemVM> IconItems { get; set; } = new ObservableCollection<IconItemVM>();

    async void InitData()
    {
        await LoadAssets();
    }

    Task LoadAssets()
    {
        var allRes = Application.Current!.Resources;
        var target = allRes.MergedDictionaries[0] as ResourceDictionary;
     
        foreach (var item in target)
        {
            IconItems.Add(new IconItemVM() { Name = item.Key.ToString(), Icon = target[item.Key.ToString()] });
        }

        return Task.CompletedTask;
    }
}
