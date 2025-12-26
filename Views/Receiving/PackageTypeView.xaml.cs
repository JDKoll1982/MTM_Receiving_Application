using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class PackageTypeView : UserControl
    {
        public PackageTypeViewModel ViewModel { get; }

        public PackageTypeView()
        {
            ViewModel = App.GetService<PackageTypeViewModel>();
            this.InitializeComponent();
            
            this.Loaded += PackageTypeView_Loaded;
        }

        private async void PackageTypeView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
