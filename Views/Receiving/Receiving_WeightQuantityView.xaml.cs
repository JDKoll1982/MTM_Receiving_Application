using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_WeightQuantityView : UserControl
    {
        public Receiving_WeightQuantityViewModel ViewModel { get; }

        public Receiving_WeightQuantityView()
        {
            ViewModel = App.GetService<Receiving_WeightQuantityViewModel>();
            this.InitializeComponent();

            this.Loaded += WeightQuantityView_Loaded;
        }

        private async void WeightQuantityView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
