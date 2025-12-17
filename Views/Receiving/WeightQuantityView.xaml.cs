using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class WeightQuantityView : UserControl
    {
        public WeightQuantityViewModel ViewModel { get; }

        public WeightQuantityView()
        {
            ViewModel = App.GetService<WeightQuantityViewModel>();
            this.InitializeComponent();
            
            this.Loaded += WeightQuantityView_Loaded;
        }

        private async void WeightQuantityView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
