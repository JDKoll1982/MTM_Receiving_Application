using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_HeatLotView : UserControl
    {
        public Receiving_HeatLotViewModel ViewModel { get; }

        public Receiving_HeatLotView()
        {
            ViewModel = App.GetService<Receiving_HeatLotViewModel>();
            this.InitializeComponent();

            this.Loaded += HeatLotView_Loaded;
        }

        private async void HeatLotView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
