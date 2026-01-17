using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for managing the Inventoried Parts List
/// Enables adding, editing, and removing parts that require Visual ERP inventory tracking
/// </summary>
public partial class ViewModel_Dunnage_AdminInventory : ViewModel_Shared_Base
{
    private readonly Dao_InventoriedDunnage _daoInventory;
    private readonly Dao_DunnagePart _daoPart;
    private readonly IService_DunnageAdminWorkflow _adminWorkflow;
    private readonly IService_Window _windowService;

    [ObservableProperty]
    private ObservableCollection<Model_InventoriedDunnage> _inventoriedParts;

    [ObservableProperty]
    private Model_InventoriedDunnage? _selectedInventoriedPart;

    /// <summary>
    /// Gets whether a part is currently selected
    /// </summary>
    public bool HasSelection => SelectedInventoriedPart != null;

    public ViewModel_Dunnage_AdminInventory(
        Dao_InventoriedDunnage daoInventory,
        Dao_DunnagePart daoPart,
        IService_DunnageAdminWorkflow adminWorkflow,
        IService_Window windowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _daoInventory = daoInventory ?? throw new ArgumentNullException(nameof(daoInventory));
        _daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
        _adminWorkflow = adminWorkflow ?? throw new ArgumentNullException(nameof(adminWorkflow));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));

        _inventoriedParts = new ObservableCollection<Model_InventoriedDunnage>();
    }

    /// <summary>
    /// Initialize the view model - loads inventoried parts
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadInventoriedPartsAsync();
    }

    /// <summary>
    /// Load all parts from the inventoried list
    /// </summary>
    [RelayCommand]
    private async Task LoadInventoriedPartsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading inventoried parts...";

            var result = await _daoInventory.GetAllAsync();

            if (result.IsSuccess && result.Data != null)
            {
                InventoriedParts.Clear();
                foreach (var part in result.Data)
                {
                    InventoriedParts.Add(part);
                }
                StatusMessage = $"Loaded {InventoriedParts.Count} inventoried parts";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load inventoried parts",
                    "Load Error",
                    nameof(LoadInventoriedPartsAsync));
                StatusMessage = "Failed to load inventoried parts";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadInventoriedPartsAsync),
                nameof(ViewModel_Dunnage_AdminInventory));
            StatusMessage = "Error loading inventoried parts";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Show dialog to add a new part to the inventoried list
    /// </summary>
    [RelayCommand]
    private async Task ShowAddToListAsync()
    {
        try
        {
            var dialog = App.GetService<Module_Dunnage.Views.View_Dunnage_Dialog_AddToInventoriedListDialog>();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Dialog handles the insert, just reload the list
                await LoadInventoriedPartsAsync();
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ShowAddToListAsync),
                nameof(ViewModel_Dunnage_AdminInventory));
        }
    }

    /// <summary>
    /// Show dialog to edit an existing inventoried entry
    /// </summary>
    [RelayCommand]
    private async Task ShowEditEntryAsync()
    {
        if (SelectedInventoriedPart == null)
        {
            await _errorHandler.ShowUserErrorAsync(
                "Please select a part to edit",
                "No Selection",
                nameof(ShowEditEntryAsync));
            return;
        }

        try
        {
            var dialog = new ContentDialog
            {
                Title = $"Edit Inventoried Part: {SelectedInventoriedPart.PartId}",
                PrimaryButtonText = "Save Changes",
                CloseButtonText = "Cancel",
                XamlRoot = _windowService.GetXamlRoot()
            };

            // Create the edit form
            var stackPanel = new StackPanel { Spacing = 12 };

            // Part ID (readonly)
            stackPanel.Children.Add(new TextBlock { Text = "Part ID (read-only)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var partIdBox = new TextBox
            {
                Text = SelectedInventoriedPart.PartId,
                IsReadOnly = true,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 12)
            };
            stackPanel.Children.Add(partIdBox);

            // Inventory Method
            stackPanel.Children.Add(new TextBlock { Text = "Inventory Method", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var methodCombo = new ComboBox
            {
                ItemsSource = new[] { "Adjust In", "Receive In", "Both" },
                SelectedItem = SelectedInventoriedPart.InventoryMethod ?? "Both",
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 12)
            };
            stackPanel.Children.Add(methodCombo);

            // Notes
            stackPanel.Children.Add(new TextBlock { Text = "Notes (optional)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var notesBox = new TextBox
            {
                Text = SelectedInventoriedPart.Notes ?? string.Empty,
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Height = 80,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 12)
            };
            stackPanel.Children.Add(notesBox);

            dialog.Content = stackPanel;

            var dialogResult = await dialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary)
            {
                // Save changes
                var updateResult = await _daoInventory.UpdateAsync(
                    SelectedInventoriedPart.Id,
                    methodCombo.SelectedItem?.ToString() ?? "Both",
                    notesBox.Text,
                    Environment.UserName);

                if (updateResult.IsSuccess)
                {
                    await LoadInventoriedPartsAsync();
                    StatusMessage = "Part updated successfully";
                }
                else
                {
                    await _errorHandler.ShowUserErrorAsync(
                        updateResult.ErrorMessage ?? "Failed to update part",
                        "Update Error",
                        nameof(ShowEditEntryAsync));
                }
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ShowEditEntryAsync),
                nameof(ViewModel_Dunnage_AdminInventory));
        }
    }

    /// <summary>
    /// Show confirmation dialog before removing a part from the inventoried list
    /// </summary>
    [RelayCommand]
    private async Task ShowRemoveConfirmationAsync()
    {
        if (SelectedInventoriedPart == null)
        {
            await _errorHandler.ShowUserErrorAsync(
                "Please select a part to remove",
                "No Selection",
                nameof(ShowRemoveConfirmationAsync));
            return;
        }

        try
        {
            var dialog = new ContentDialog
            {
                Title = "Remove from Inventoried List",
                Content = $"Are you sure you want to remove '{SelectedInventoriedPart.PartId}' from the inventoried list?\n\n" +
                         "This will not delete the part from the system, but it will no longer trigger inventory tracking notifications during data entry.",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = _windowService.GetXamlRoot()
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await RemoveFromListAsync();
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ShowRemoveConfirmationAsync),
                nameof(ViewModel_Dunnage_AdminInventory));
        }
    }

    /// <summary>
    /// Remove the selected part from the inventoried list
    /// </summary>
    private async Task RemoveFromListAsync()
    {
        if (SelectedInventoriedPart == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Removing part from inventoried list...";

            var result = await _daoInventory.DeleteAsync(SelectedInventoriedPart.Id);

            if (result.IsSuccess)
            {
                await LoadInventoriedPartsAsync();
                StatusMessage = "Part removed from inventoried list";
                SelectedInventoriedPart = null;
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to remove part from list",
                    "Remove Error",
                    nameof(RemoveFromListAsync));
                StatusMessage = "Failed to remove part";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RemoveFromListAsync),
                nameof(ViewModel_Dunnage_AdminInventory));
            StatusMessage = "Error removing part";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Return to main admin navigation hub
    /// </summary>
    [RelayCommand]
    private async Task BackToHubAsync()
    {
        _logger.LogInfo("Returning to Settings Mode Selection from Admin Inventory");
        var settingsWorkflow = App.GetService<IService_SettingsWorkflow>();
        settingsWorkflow.GoBack();
    }

    /// <summary>
    /// Notifies dependent properties when selection changes
    /// </summary>
    partial void OnSelectedInventoriedPartChanged(Model_InventoriedDunnage? value)
    {
        OnPropertyChanged(nameof(HasSelection));
    }
}

