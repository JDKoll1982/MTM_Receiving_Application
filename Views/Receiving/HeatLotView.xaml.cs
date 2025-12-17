using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class HeatLotView : UserControl
    {
        public HeatLotViewModel ViewModel { get; }

        public HeatLotView()
        {
            ViewModel = App.GetService<HeatLotViewModel>();
            this.InitializeComponent();
            
            this.Loaded += HeatLotView_Loaded;
        }

        private async void HeatLotView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
