using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_WeightQuantity : UserControl
    {
        public ViewModel_Receiving_WeightQuantity ViewModel { get; }
        private readonly IService_Focus _focusService;

        public View_Receiving_WeightQuantity()
        {
            ViewModel = App.GetService<ViewModel_Receiving_WeightQuantity>();
            _focusService = App.GetService<IService_Focus>();
            this.InitializeComponent();

            this.Loaded += WeightQuantityView_Loaded;
            _focusService.AttachFocusOnVisibility(this);
        }

        private async void WeightQuantityView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
