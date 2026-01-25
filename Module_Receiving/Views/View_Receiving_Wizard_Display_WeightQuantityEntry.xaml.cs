using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_WeightQuantity : UserControl
    {
        public ViewModel_Receiving_Wizard_Display_WeightQuantityEntry ViewModel
        {
            get => (ViewModel_Receiving_Wizard_Display_WeightQuantityEntry)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModel_Receiving_Wizard_Display_WeightQuantityEntry),
                typeof(View_Receiving_WeightQuantity),
                new PropertyMetadata(null));
        private readonly IService_Focus _focusService;

        public View_Receiving_WeightQuantity()
        {
            ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_WeightQuantityEntry>();
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
