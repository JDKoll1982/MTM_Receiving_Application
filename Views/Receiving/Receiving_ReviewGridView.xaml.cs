using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_ReviewGridView : UserControl
    {
        public Receiving_ReviewGridViewModel ViewModel { get; }

        public Receiving_ReviewGridView()
        {
            ViewModel = App.GetService<Receiving_ReviewGridViewModel>();
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
