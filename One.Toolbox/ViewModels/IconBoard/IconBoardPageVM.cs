using Avalonia;
using Avalonia.Controls;

using One.Base.Collections;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ViewModels.Base;

using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.IconBoard;

public partial class IconBoardPageVM : BasePageVM
{
    public IconBoardPageVM()
    {
        _dataList = new List<IconItemVM>();
    }

    public override void InitializeViewModel()
    {
        base.InitializeViewModel();

        InitData();
    }

    [ObservableProperty]
    private string searchText;

    [ObservableProperty]
    private IconItemVM selectItem;

    public ManualObservableCollection<IconItemVM> IconItems { get; set; } = new ManualObservableCollection<IconItemVM>();

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.Icon);
    }

    async void InitData()
    {
        await LoadAssets();

       
        FilterItems("");
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterItems(value);
    }

    private readonly List<IconItemVM> _dataList;

    private void FilterItems(string key)
    {
        IconItems.CanNotify = false;
        IconItems.Clear();

        foreach (IconItemVM data in _dataList)
        {
            if (data.Name.ToLower().Contains(key.ToLower()))
            {
                IconItems.Add(data);
            }
        }

        IconItems.CanNotify = true;
    }

    Task LoadAssets()
    {
        IResourceDictionary allRes = Application.Current!.Resources;
        ResourceDictionary? target = allRes.MergedDictionaries[0] as ResourceDictionary;

        foreach (KeyValuePair<object, object?> item in target)
        {
            _dataList.Add(new IconItemVM() { Name = item.Key.ToString(), Icon = target[item.Key.ToString()] });
        }
       
        return Task.CompletedTask;
    }
}