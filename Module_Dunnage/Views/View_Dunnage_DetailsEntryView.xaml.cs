using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_DetailsEntryView : UserControl
{
    public ViewModel_Dunnage_DetailsEntry ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_DetailsEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_DetailsEntry>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSpecsForSelectedPartAsync();
    }

    public void CommitPendingInputs()
    {
        ViewModel.PoNumber = PoNumberTextBox.Text;
        ViewModel.Location = LocationTextBox.Text.Trim();
    }

    private void PoNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        ViewModel.PoNumber = textBox.Text;
        ViewModel.PoTextBoxLostFocusCommand.Execute(null);

        if (string.Equals(textBox.Text, ViewModel.PoNumber, System.StringComparison.Ordinal))
        {
            return;
        }

        textBox.Text = ViewModel.PoNumber;
    }

    private async void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        ViewModel.Location = textBox.Text.Trim();

        var validation = await ViewModel.ValidateLocationAsync();
        if (!validation.IsValid)
        {
            ViewModel.ShowStatus(
                validation.Message,
                Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            return;
        }

        textBox.Text = ViewModel.Location;
    }
}
