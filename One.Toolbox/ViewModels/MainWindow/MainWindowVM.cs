using Microsoft.Extensions.DependencyInjection;

using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.MainWindow
{
    public partial class MainWindowVM : BaseVM
    {
        [ObservableProperty]
        private string a;

        [ObservableProperty]
        private MainViewVM mainViewVM;

        public MainWindowVM()
        {
            //MainViewVM = new MainViewVM();
            //MainViewVM.InitializeViewModel();
        }

        [RelayCommand]
        private void SetLanguage(object obj)
        {
        }

        public override void InitializeViewModel()
        {
            MainViewVM = App.Current!.Services.GetService<MainViewVM>()!;
            base.InitializeViewModel();

            MainViewVM.InitializeViewModel();
        }
    }
}