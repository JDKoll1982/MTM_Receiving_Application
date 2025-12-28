using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_PackageTypeView : UserControl
    {
        public Receiving_PackageTypeViewModel ViewModel { get; }

        public Receiving_PackageTypeView()
        {
            ViewModel = App.GetService<Receiving_PackageTypeViewModel>();
            this.InitializeComponent();

            this.Loaded += PackageTypeView_Loaded;
        }

        private async void PackageTypeView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
