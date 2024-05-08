using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.IconBoard
{
    public partial class IconItemVM : BaseVM
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private object icon;
    }
}
