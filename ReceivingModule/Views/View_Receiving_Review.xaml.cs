using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_Review : UserControl
    {
        public ViewModel_Receiving_Review ViewModel { get; }

        public View_Receiving_Review()
        {
            ViewModel = App.GetService<ViewModel_Receiving_Review>();
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
