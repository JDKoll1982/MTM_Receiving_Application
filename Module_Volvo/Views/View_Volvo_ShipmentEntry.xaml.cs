using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Page for Volvo shipment entry
/// </summary>
public sealed partial class View_Volvo_ShipmentEntry : Page
{
    public ViewModel_Volvo_ShipmentEntry ViewModel { get; }

    public View_Volvo_ShipmentEntry()
    {
        ViewModel = App.GetService<ViewModel_Volvo_ShipmentEntry>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private void OnPartSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.UpdatePartSuggestions(sender.Text);
        }
    }

    private void OnPartSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Model_VolvoPart selectedPart)
        {
            ViewModel.OnPartSuggestionChosen(selectedPart);
        }
    }
}
