using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_WeightQuantity : UserControl
    {
        public ViewModel_Receiving_WeightQuantity ViewModel { get; }

        public View_Receiving_WeightQuantity()
        {
            ViewModel = App.GetService<ViewModel_Receiving_WeightQuantity>();
            this.InitializeComponent();

            this.Loaded += WeightQuantityView_Loaded;
        }

        private async void WeightQuantityView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
