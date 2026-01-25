using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_POEntry : UserControl
    {
        public ViewModel_Receiving_Wizard_Display_PONumberEntry ViewModel { get; }
        private readonly IService_Focus _focusService;

        public View_Receiving_POEntry()
        {
            ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_PONumberEntry>();
            _focusService = App.GetService<IService_Focus>();
            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
        }

        private void POTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Trigger auto-correction command
            ViewModel.PoTextBoxLostFocusCommand.Execute(null);
        }
    }
}
