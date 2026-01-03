using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_PackageType : UserControl
    {
        public ViewModel_Receiving_PackageType ViewModel { get; }

        public View_Receiving_PackageType()
        {
            ViewModel = App.GetService<ViewModel_Receiving_PackageType>();
            this.InitializeComponent();

            this.Loaded += PackageTypeView_Loaded;
        }

        private async void PackageTypeView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
