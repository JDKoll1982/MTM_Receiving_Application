using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class POEntryView : UserControl
    {
        public POEntryViewModel ViewModel { get; }

        public POEntryView()
        {
            ViewModel = App.GetService<POEntryViewModel>();
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto-focus on first textbox when view loads
            FirstTextBox?.Focus(FocusState.Programmatic);
        }

        private void POTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Trigger auto-correction command
            ViewModel.PoTextBoxLostFocusCommand.Execute(null);
        }
    }
}
