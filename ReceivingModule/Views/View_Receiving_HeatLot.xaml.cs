using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_HeatLot : UserControl
    {
        public ViewModel_Receiving_HeatLot ViewModel { get; }

        public View_Receiving_HeatLot()
        {
            ViewModel = App.GetService<ViewModel_Receiving_HeatLot>();
            this.InitializeComponent();

            this.Loaded += HeatLotView_Loaded;
        }

        private async void HeatLotView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
