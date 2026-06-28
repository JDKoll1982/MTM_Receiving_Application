using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_VendorVariables : Page
{
    public ViewModel_Settings_Receiving_VendorVariables ViewModel { get; }

    public View_Settings_Receiving_VendorVariables(
        ViewModel_Settings_Receiving_VendorVariables viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void VendorTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var vendorText = VendorTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(vendorText))
        {
            return;
        }

        await TriggerVendorSearchAsync(vendorText);
    }

    private async void SearchVendorButton_Click(object sender, RoutedEventArgs e)
    {
        var vendorText = VendorTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(vendorText))
        {
            vendorText = "A";
        }

        await TriggerVendorSearchAsync(vendorText);
    }

    private async Task TriggerVendorSearchAsync(string searchTerm)
    {
        try
        {
            var matches = await ViewModel.SearchVendorsAsync(searchTerm);
            if (matches is null || !matches.Any())
            {
                ViewModel.ShowStatus($"No vendors found matching '{searchTerm}'");
                return;
            }

            var dialog = new Dialog_FuzzySearchPicker(
                matches,
                "Select Vendor",
                $"Matching vendors for '{searchTerm}':"
            )
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.SelectedResult is not null)
            {
                await ViewModel.LoadMappingForVendorAsync(dialog.SelectedResult.Label);
            }
        }
        catch (Exception ex)
        {
            ViewModel.ShowStatus($"Error searching vendors: {ex.Message}");
        }
    }
}
