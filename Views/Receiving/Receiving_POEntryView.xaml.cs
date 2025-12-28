using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_POEntryView : UserControl
    {
        public Receiving_POEntryViewModel ViewModel { get; }

        public Receiving_POEntryView()
        {
            ViewModel = App.GetService<Receiving_POEntryViewModel>();
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
