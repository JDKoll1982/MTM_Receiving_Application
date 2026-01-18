using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Type Management
/// Handles CRUD operations for dunnage_types table with impact analysis
/// </summary>
public partial class ViewModel_Dunnage_AdminTypes : ViewModel_Shared_Base
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_DunnageAdminWorkflow _adminWorkflow;
    private readonly IService_Window _windowService;
    private readonly IService_Help _helpService;

    public ViewModel_Dunnage_AdminTypes(
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageAdminWorkflow adminWorkflow,
        IService_Window windowService,
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _dunnageService = dunnageService;
        _adminWorkflow = adminWorkflow;
        _windowService = windowService;
        _helpService = helpService;
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnageType> _types = new();

    [ObservableProperty]
    private Model_DunnageType? _selectedType;

    [ObservableProperty]
    private bool _canEdit = false;

    [ObservableProperty]
    private bool _canDelete = false;

    #endregion

    #region Load Data

    /// <summary>
    /// Load all dunnage types from database
    /// </summary>
    [RelayCommand]
    private async Task LoadTypesAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading types...";

            var result = await _dunnageService.GetAllTypesAsync();

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "LoadTypesAsync", true);
                return;
            }

            Types.Clear();
            if (result.Data != null)
            {
                foreach (var type in result.Data)
                {
                    Types.Add(type);
                }
            }

            StatusMessage = $"Loaded {Types.Count} types";
            await _logger.LogInfoAsync($"Loaded {Types.Count} dunnage types", "TypeManagement");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error loading types",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Add Type

    /// <summary>
    /// Show Add New Type dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowAddTypeAsync()
    {
        try
        {
            var dialog = new Module_Dunnage.Views.View_Dunnage_Dialog_Dunnage_AddTypeDialog
            {
                XamlRoot = _windowService.GetXamlRoot()
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                StatusMessage = "Type added successfully";
                await _logger.LogInfoAsync("New dunnage type added via Add Type Dialog", "TypeManagement");

                // Reload types to show the new type
                await LoadTypesAsync();
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error showing Add Type dialog",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
    }

    #endregion

    #region Edit Type

    /// <summary>
    /// Show edit dialog for selected type
    /// </summary>
    [RelayCommand]
    private async Task ShowEditTypeAsync()
    {
        if (SelectedType == null)
        {
            return;
        }

        try
        {
            var editedType = new Model_DunnageType
            {
                Id = SelectedType.Id,
                DunnageType = SelectedType.DunnageType,
                Icon = SelectedType.Icon,
                DateAdded = SelectedType.DateAdded,
                AddedBy = SelectedType.AddedBy,
                LastModified = SelectedType.LastModified,
                ModifiedBy = SelectedType.ModifiedBy
            };

            var dialog = new ContentDialog
            {
                Title = "Edit Dunnage Type",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _windowService.GetXamlRoot()
            };

            var stackPanel = new StackPanel { Spacing = 12 };

            var typeNameBox = new TextBox
            {
                Header = "Type Name",
                Text = editedType.DunnageType,
                PlaceholderText = "Enter type name"
            };

            // Icon selector button
            var iconSelectorButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12, 16, 12)
            };

            var iconButtonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12
            };

            var iconDisplay = new Material.Icons.WinUI3.MaterialIcon
            {
                Kind = editedType.IconKind,
                Width = 32,
                Height = 32,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["AccentFillColorDefaultBrush"]
            };

            var iconTextPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconLabel = new TextBlock
            {
                Text = "Click to select icon",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            var iconName = new TextBlock
            {
                Text = editedType.Icon,
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["TextFillColorSecondaryBrush"]
            };

            iconTextPanel.Children.Add(iconLabel);
            iconTextPanel.Children.Add(iconName);
            iconButtonPanel.Children.Add(iconDisplay);
            iconButtonPanel.Children.Add(iconTextPanel);
            iconSelectorButton.Content = iconButtonPanel;

            // Handle icon selection
            iconSelectorButton.Click += async (_, __) =>
            {
                var iconSelector = new MTM_Receiving_Application.Module_Shared.Views.View_Shared_IconSelectorWindow();
                iconSelector.SetInitialSelection(editedType.IconKind);
                iconSelector.Activate();

                var selectedIcon = await iconSelector.WaitForSelectionAsync();
                if (selectedIcon.HasValue)
                {
                    editedType.Icon = selectedIcon.Value.ToString();
                    iconDisplay.Kind = selectedIcon.Value;
                    iconName.Text = selectedIcon.Value.ToString();
                }
            };

            var iconHeader = new TextBlock
            {
                Text = "Icon",
                Margin = new Thickness(0, 8, 0, 0),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            stackPanel.Children.Add(typeNameBox);
            stackPanel.Children.Add(iconHeader);
            stackPanel.Children.Add(iconSelectorButton);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                editedType.DunnageType = typeNameBox.Text.Trim();
                // Icon is already set by the button click handler

                if (string.IsNullOrWhiteSpace(editedType.DunnageType))
                {
                    await _errorHandler.ShowUserErrorAsync("Type name is required", "Validation Error", "ShowEditTypeAsync");
                    return;
                }

                IsBusy = true;
                StatusMessage = "Saving changes...";

                var updateResult = await _dunnageService.UpdateTypeAsync(editedType);

                if (!updateResult.Success)
                {
                    await _errorHandler.HandleDaoErrorAsync(updateResult, "UpdateTypeAsync", true);
                    return;
                }

                StatusMessage = "Type updated successfully";
                await _logger.LogInfoAsync($"Updated type: {editedType.DunnageType}", "TypeManagement");

                // Reload types
                await LoadTypesAsync();
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error editing type",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Delete Type

    /// <summary>
    /// Show delete confirmation dialog with impact analysis
    /// </summary>
    [RelayCommand]
    private async Task ShowDeleteConfirmationAsync()
    {
        if (SelectedType == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Checking impact...";

            // Get impact counts
            var partCountResult = await _dunnageService.GetPartCountByTypeAsync(SelectedType.Id);
            var transactionCountResult = await _dunnageService.GetTransactionCountByTypeAsync(SelectedType.Id);

            if (!partCountResult.Success || !transactionCountResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(
                    partCountResult.Success ? transactionCountResult : partCountResult,
                    "GetImpactCounts",
                    true
                );
                return;
            }

            int partCount = partCountResult.Data;
            int transactionCount = transactionCountResult.Data;

            IsBusy = false;

            // Build impact message
            string impactMessage = $"Type: {SelectedType.DunnageType}\n\n";
            impactMessage += $"Parts using this type: {partCount}\n";
            impactMessage += $"Transactions using this type: {transactionCount}\n\n";

            if (partCount > 0 || transactionCount > 0)
            {
                impactMessage += "WARNING: This type is in use and cannot be deleted.";

                var warningDialog = new ContentDialog
                {
                    Title = "Cannot Delete Type",
                    Content = impactMessage,
                    CloseButtonText = "OK",
                    XamlRoot = _windowService.GetXamlRoot()
                };

                await warningDialog.ShowAsync();
                return;
            }

            // Require "DELETE" confirmation
            impactMessage += "This type is not in use and can be deleted.\n\n";
            impactMessage += "Type DELETE to confirm:";

            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Delete",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = _windowService.GetXamlRoot()
            };

            var stackPanel = new StackPanel { Spacing = 12 };
            stackPanel.Children.Add(new TextBlock { Text = impactMessage, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });

            var confirmBox = new TextBox
            {
                PlaceholderText = "Type DELETE to confirm"
            };
            stackPanel.Children.Add(confirmBox);

            confirmDialog.Content = stackPanel;

            var result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && confirmBox.Text == "DELETE")
            {
                await DeleteTypeAsync();
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error checking delete impact",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Delete the selected type (after confirmation)
    /// </summary>
    private async Task DeleteTypeAsync()
    {
        if (SelectedType == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Deleting type...";

            var result = await _dunnageService.DeleteTypeAsync(SelectedType.Id);

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "DeleteTypeAsync", true);
                return;
            }

            StatusMessage = "Type deleted successfully";
            await _logger.LogInfoAsync($"Deleted type: {SelectedType.DunnageType}", "TypeManagement");

            // Reload types
            await LoadTypesAsync();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error deleting type",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Return to Admin Main Hub
    /// </summary>
    [RelayCommand]
    private async Task ReturnToAdminHubAsync()
    {
        try
        {
            _logger.LogInfo("Returning to Admin Hub from Admin Types");
            await _adminWorkflow.NavigateToHubAsync();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error navigating back",
                Enum_ErrorSeverity.Low,
                ex,
                false
            );
        }
    }

    #endregion

    #region Property Changed

    partial void OnSelectedTypeChanged(Model_DunnageType? value)
    {
        CanEdit = value != null;
        CanDelete = value != null;
    }

    #endregion

    #region Help Content Helpers

    /// <summary>
    /// Gets a tooltip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    #endregion
}

