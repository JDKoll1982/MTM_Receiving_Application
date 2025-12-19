using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class ReviewGridView : UserControl
    {
        public ReviewGridViewModel ViewModel { get; }

        public ReviewGridView()
        {
            ViewModel = App.GetService<ReviewGridViewModel>();
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
