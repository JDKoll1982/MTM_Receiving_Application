using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_HeatLot : UserControl
    {
        public ViewModel_Receiving_HeatLot ViewModel { get; }
        private readonly IService_Focus _focusService;

        public View_Receiving_HeatLot()
        {
            ViewModel = App.GetService<ViewModel_Receiving_HeatLot>();
            _focusService = App.GetService<IService_Focus>();
            this.InitializeComponent();

            this.Loaded += HeatLotView_Loaded;
            _focusService.AttachFocusOnVisibility(this);
        }

        private async void HeatLotView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
