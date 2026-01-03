using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ReceivingModule.ViewModels;

namespace MTM_Receiving_Application.ReceivingModule.Views
{
    public sealed partial class View_Receiving_POEntry : UserControl
    {
        public ViewModel_Receiving_POEntry ViewModel { get; }

        public View_Receiving_POEntry()
        {
            ViewModel = App.GetService<ViewModel_Receiving_POEntry>();
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
