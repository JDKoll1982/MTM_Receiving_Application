using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Data;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_Dialog_AddToInventoriedListDialog : ContentDialog
{
    private readonly Dao_DunnagePart _daoPart;
    private readonly Dao_InventoriedDunnage _daoInventory;
    private readonly IService_Focus _focusService;

    public View_Dunnage_Dialog_AddToInventoriedListDialog()
    {
        _daoPart = App.GetService<Dao_DunnagePart>();
        _daoInventory = App.GetService<Dao_InventoriedDunnage>();
        _focusService = App.GetService<IService_Focus>();

        InitializeComponent();
        _focusService.AttachFocusOnVisibility(this);
        _ = LoadPartsAsync();
    }

    private async Task LoadPartsAsync()
    {
        try
        {
            var result = await _daoPart.GetAllAsync();

            if (result.IsSuccess && result.Data != null)
            {
                PartIdComboBox.ItemsSource = result.Data.ConvertAll(p => p.PartId);
            }
        }
        catch
        {
            // Silent fail - user can still type part ID
        }
    }

    private void PartIdComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        // Validate part ID exists when user types and presses enter
        if (!string.IsNullOrWhiteSpace(args.Text))
        {
            PartIdError.Visibility = Visibility.Collapsed;
        }
    }

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Get deferral to perform async validation
        var deferral = args.GetDeferral();

        try
        {
            // Validate Part ID
            var partId = PartIdComboBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(partId))
            {
                PartIdError.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            // Check if part already in inventoried list
            var checkResult = await _daoInventory.GetByPartAsync(partId);
            if (checkResult.IsSuccess && checkResult.Data != null)
            {
                // Show error - part already in list
                var errorDialog = new ContentDialog
                {
                    Title = "Part Already in List",
                    Content = $"Part '{partId}' is already in the inventoried list.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                args.Cancel = true;
                return;
            }

            // Get selected inventory method
            var methodItem = InventoryMethodComboBox.SelectedItem as ComboBoxItem;
            var inventoryMethod = methodItem?.Content?.ToString() ?? "Both";

            // Get notes
            var notes = NotesTextBox.Text?.Trim();

            // Insert into database
            var insertResult = await _daoInventory.InsertAsync(
                partId,
                inventoryMethod,
                notes ?? string.Empty,
                Environment.UserName);

            if (!insertResult.IsSuccess)
            {
                // Show error
                var errorDialog = new ContentDialog
                {
                    Title = "Insert Failed",
                    Content = insertResult.ErrorMessage ?? "Failed to add part to inventoried list",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                args.Cancel = true;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }
}
