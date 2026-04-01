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
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
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
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
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
                XamlRoot = _windowService.GetXamlRoot(),
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                StatusMessage = "Type added successfully";
                await _logger.LogInfoAsync(
                    "New dunnage type added via Add Type Dialog",
                    "TypeManagement"
                );

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
            var customFieldsResult = await _dunnageService.GetCustomFieldsByTypeAsync(
                SelectedType.Id
            );
            if (!customFieldsResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(
                    customFieldsResult,
                    nameof(IService_MySQL_Dunnage.GetCustomFieldsByTypeAsync),
                    true
                );
                return;
            }

            var originalCustomFields =
                customFieldsResult.Data?.Select(CloneCustomField).ToList()
                ?? new List<Model_CustomFieldDefinition>();
            var editableCustomFields = new ObservableCollection<Model_CustomFieldDefinition>(
                originalCustomFields.Select(CloneCustomField)
            );

            var editedType = new Model_DunnageType
            {
                Id = SelectedType.Id,
                DunnageType = SelectedType.DunnageType,
                Icon = SelectedType.Icon,
                DateAdded = SelectedType.DateAdded,
                AddedBy = SelectedType.AddedBy,
                LastModified = SelectedType.LastModified,
                ModifiedBy = SelectedType.ModifiedBy,
            };

            var dialog = new ContentDialog
            {
                Title = "Edit Dunnage Type",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _windowService.GetXamlRoot(),
            };

            var stackPanel = new StackPanel { Spacing = 12 };

            var typeNameBox = new TextBox
            {
                Header = "Type Name",
                Text = editedType.DunnageType,
                PlaceholderText = "Enter type name",
            };

            // Icon selector button
            var iconSelectorButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12, 16, 12),
            };

            var iconButtonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
            };

            var iconDisplay = new Material.Icons.WinUI3.MaterialIcon
            {
                Kind = editedType.IconKind,
                Width = 32,
                Height = 32,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)
                    Microsoft.UI.Xaml.Application.Current.Resources["AccentFillColorDefaultBrush"],
            };

            var iconTextPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var iconLabel = new TextBlock
            {
                Text = "Click to select icon",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            };

            var iconName = new TextBlock
            {
                Text = editedType.Icon,
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)
                    Microsoft.UI.Xaml.Application.Current.Resources["TextFillColorSecondaryBrush"],
            };

            iconTextPanel.Children.Add(iconLabel);
            iconTextPanel.Children.Add(iconName);
            iconButtonPanel.Children.Add(iconDisplay);
            iconButtonPanel.Children.Add(iconTextPanel);
            iconSelectorButton.Content = iconButtonPanel;

            // Handle icon selection
            iconSelectorButton.Click += async (_, __) =>
            {
                var iconSelector =
                    new MTM_Receiving_Application.Module_Shared.Views.View_Shared_IconSelectorWindow();
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
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            };

            stackPanel.Children.Add(typeNameBox);
            stackPanel.Children.Add(iconHeader);
            stackPanel.Children.Add(iconSelectorButton);

            var customFieldsHeader = new TextBlock
            {
                Text = "Custom Fields",
                Margin = new Thickness(0, 12, 0, 0),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            };
            stackPanel.Children.Add(customFieldsHeader);

            var customFieldNameBox = new TextBox
            {
                Header = "Field Name",
                PlaceholderText = "Enter field name",
            };
            var customFieldTypeBox = new ComboBox
            {
                Header = "Field Type",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            customFieldTypeBox.Items.Add("Text");
            customFieldTypeBox.Items.Add("Number");
            customFieldTypeBox.Items.Add("Dropdown");
            customFieldTypeBox.Items.Add("Yes/No");
            customFieldTypeBox.SelectedItem = "Text";

            var customFieldRequiredBox = new CheckBox { Content = "Required field" };
            var customFieldErrorText = new TextBlock
            {
                Foreground = (Microsoft.UI.Xaml.Media.Brush)
                    Application.Current.Resources["SystemFillColorCautionBrush"],
                TextWrapping = TextWrapping.Wrap,
            };

            Model_CustomFieldDefinition? editingField = null;
            var addOrUpdateFieldButton = new Button
            {
                Content = "Add Field",
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            var cancelEditFieldButton = new Button
            {
                Content = "Cancel Field Edit",
                HorizontalAlignment = HorizontalAlignment.Right,
                Visibility = Visibility.Collapsed,
            };

            void ResetCustomFieldEditor()
            {
                editingField = null;
                customFieldNameBox.Text = string.Empty;
                customFieldTypeBox.SelectedItem = "Text";
                customFieldRequiredBox.IsChecked = false;
                customFieldErrorText.Text = string.Empty;
                addOrUpdateFieldButton.Content = "Add Field";
                cancelEditFieldButton.Visibility = Visibility.Collapsed;
            }

            var customFieldsPanel = new StackPanel { Spacing = 8 };

            void RenderCustomFields()
            {
                customFieldsPanel.Children.Clear();

                if (editableCustomFields.Count == 0)
                {
                    customFieldsPanel.Children.Add(
                        new TextBlock
                        {
                            Text = "No custom fields saved for this type.",
                            Foreground = (Microsoft.UI.Xaml.Media.Brush)
                                Application.Current.Resources[
                                    "SystemControlForegroundBaseMediumBrush"
                                ],
                        }
                    );
                    return;
                }

                foreach (var field in editableCustomFields)
                {
                    var border = new Border
                    {
                        BorderBrush = (Microsoft.UI.Xaml.Media.Brush)
                            Application.Current.Resources["CardStrokeColorDefaultBrush"],
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12),
                    };

                    var row = new Grid { ColumnSpacing = 8 };
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.ColumnDefinitions.Add(
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    );
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    row.Children.Add(
                        new FontIcon
                        {
                            Glyph = "&#xE76F;",
                            VerticalAlignment = VerticalAlignment.Center,
                        }
                    );

                    var details = new StackPanel { Spacing = 4 };
                    details.Children.Add(
                        new TextBlock
                        {
                            Text = field.FieldName,
                            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        }
                    );
                    details.Children.Add(
                        new TextBlock
                        {
                            Text = $"{field.GetSummary()} | Column: {field.DatabaseColumnName}",
                            Foreground = (Microsoft.UI.Xaml.Media.Brush)
                                Application.Current.Resources["TextFillColorSecondaryBrush"],
                            FontSize = 12,
                        }
                    );
                    Grid.SetColumn(details, 1);
                    row.Children.Add(details);

                    var actions = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 4,
                    };
                    var editButton = new Button { Content = "Edit" };
                    editButton.Click += (_, __) =>
                    {
                        editingField = field;
                        customFieldNameBox.Text = field.FieldName;
                        customFieldTypeBox.SelectedItem = field.FieldType;
                        customFieldRequiredBox.IsChecked = field.IsRequired;
                        customFieldErrorText.Text = string.Empty;
                        addOrUpdateFieldButton.Content = "Update Field";
                        cancelEditFieldButton.Visibility = Visibility.Visible;
                    };

                    var deleteButton = new Button { Content = "Delete" };
                    deleteButton.Click += (_, __) =>
                    {
                        editableCustomFields.Remove(field);
                        NormalizeAndReindexCustomFields(editableCustomFields);

                        if (editingField == field)
                        {
                            ResetCustomFieldEditor();
                        }

                        RenderCustomFields();
                    };

                    actions.Children.Add(editButton);
                    actions.Children.Add(deleteButton);
                    Grid.SetColumn(actions, 2);
                    row.Children.Add(actions);

                    border.Child = row;
                    customFieldsPanel.Children.Add(border);
                }
            }

            addOrUpdateFieldButton.Click += (_, __) =>
            {
                var validationMessage = ValidateCustomFieldInput(
                    editableCustomFields,
                    customFieldNameBox.Text,
                    editingField
                );
                if (!string.IsNullOrWhiteSpace(validationMessage))
                {
                    customFieldErrorText.Text = validationMessage;
                    return;
                }

                if (editingField != null)
                {
                    editingField.FieldName = customFieldNameBox.Text.Trim();
                    editingField.DatabaseColumnName =
                        Model_CustomFieldDefinition.BuildDatabaseColumnName(editingField.FieldName);
                    editingField.FieldType = customFieldTypeBox.SelectedItem?.ToString() ?? "Text";
                    editingField.IsRequired = customFieldRequiredBox.IsChecked ?? false;
                }
                else
                {
                    editableCustomFields.Add(
                        new Model_CustomFieldDefinition
                        {
                            FieldName = customFieldNameBox.Text.Trim(),
                            DatabaseColumnName =
                                Model_CustomFieldDefinition.BuildDatabaseColumnName(
                                    customFieldNameBox.Text
                                ),
                            FieldType = customFieldTypeBox.SelectedItem?.ToString() ?? "Text",
                            IsRequired = customFieldRequiredBox.IsChecked ?? false,
                        }
                    );
                }

                NormalizeAndReindexCustomFields(editableCustomFields);
                ResetCustomFieldEditor();
                RenderCustomFields();
            };

            cancelEditFieldButton.Click += (_, __) => ResetCustomFieldEditor();

            stackPanel.Children.Add(customFieldNameBox);
            stackPanel.Children.Add(customFieldTypeBox);
            stackPanel.Children.Add(customFieldRequiredBox);
            stackPanel.Children.Add(customFieldErrorText);

            var customFieldButtons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            customFieldButtons.Children.Add(cancelEditFieldButton);
            customFieldButtons.Children.Add(addOrUpdateFieldButton);
            stackPanel.Children.Add(customFieldButtons);
            stackPanel.Children.Add(
                new ScrollViewer { Content = customFieldsPanel, MaxHeight = 260 }
            );

            RenderCustomFields();

            dialog.Content = new ScrollViewer { Content = stackPanel, MaxHeight = 640 };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                editedType.DunnageType = typeNameBox.Text.Trim();
                // Icon is already set by the button click handler

                if (string.IsNullOrWhiteSpace(editedType.DunnageType))
                {
                    await _errorHandler.ShowUserErrorAsync(
                        "Type name is required",
                        "Validation Error",
                        "ShowEditTypeAsync"
                    );
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

                var syncFieldsResult = await SyncCustomFieldsAsync(
                    editedType.Id,
                    originalCustomFields,
                    editableCustomFields
                );
                if (!syncFieldsResult.Success)
                {
                    await _errorHandler.HandleDaoErrorAsync(
                        syncFieldsResult,
                        nameof(SyncCustomFieldsAsync),
                        true
                    );
                    return;
                }

                StatusMessage = "Type updated successfully";
                await _logger.LogInfoAsync(
                    $"Updated type: {editedType.DunnageType}",
                    "TypeManagement"
                );

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
            var transactionCountResult = await _dunnageService.GetTransactionCountByTypeAsync(
                SelectedType.Id
            );

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
                    XamlRoot = _windowService.GetXamlRoot(),
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
                XamlRoot = _windowService.GetXamlRoot(),
            };

            var stackPanel = new StackPanel { Spacing = 12 };
            stackPanel.Children.Add(
                new TextBlock
                {
                    Text = impactMessage,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                }
            );

            var confirmBox = new TextBox { PlaceholderText = "Type DELETE to confirm" };
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
            await _logger.LogInfoAsync(
                $"Deleted type: {SelectedType.DunnageType}",
                "TypeManagement"
            );

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

    #region Custom Field Helpers

    private async Task<Model_Dao_Result> SyncCustomFieldsAsync(
        int typeId,
        IReadOnlyCollection<Model_CustomFieldDefinition> originalFields,
        ObservableCollection<Model_CustomFieldDefinition> currentFields
    )
    {
        NormalizeAndReindexCustomFields(currentFields);

        var originalById = originalFields.ToDictionary(field => field.Id);
        var currentIds = currentFields
            .Where(field => field.Id > 0)
            .Select(field => field.Id)
            .ToHashSet();

        foreach (var deletedField in originalFields.Where(field => !currentIds.Contains(field.Id)))
        {
            var deleteResult = await _dunnageService.DeleteCustomFieldAsync(deletedField.Id);
            if (!deleteResult.Success)
            {
                return deleteResult;
            }
        }

        foreach (var field in currentFields)
        {
            if (field.Id <= 0)
            {
                var insertResult = await _dunnageService.InsertCustomFieldAsync(typeId, field);
                if (!insertResult.Success)
                {
                    return insertResult;
                }

                continue;
            }

            if (
                originalById.TryGetValue(field.Id, out var originalField)
                && HasCustomFieldChanges(originalField, field)
            )
            {
                var updateResult = await _dunnageService.UpdateCustomFieldAsync(field.Id, field);
                if (!updateResult.Success)
                {
                    return updateResult;
                }
            }
        }

        return Model_Dao_Result_Factory.Success();
    }

    private static Model_CustomFieldDefinition CloneCustomField(Model_CustomFieldDefinition field)
    {
        return new Model_CustomFieldDefinition
        {
            Id = field.Id,
            DunnageTypeId = field.DunnageTypeId,
            FieldName = field.FieldName,
            DatabaseColumnName = field.DatabaseColumnName,
            FieldType = field.FieldType,
            DisplayOrder = field.DisplayOrder,
            IsRequired = field.IsRequired,
            ValidationRules = field.ValidationRules,
            CreatedDate = field.CreatedDate,
            CreatedBy = field.CreatedBy,
        };
    }

    private static bool HasCustomFieldChanges(
        Model_CustomFieldDefinition originalField,
        Model_CustomFieldDefinition currentField
    )
    {
        return !string.Equals(
                originalField.FieldName,
                currentField.FieldName,
                StringComparison.Ordinal
            )
            || !string.Equals(
                originalField.DatabaseColumnName,
                currentField.DatabaseColumnName,
                StringComparison.Ordinal
            )
            || !string.Equals(
                originalField.FieldType,
                currentField.FieldType,
                StringComparison.Ordinal
            )
            || originalField.DisplayOrder != currentField.DisplayOrder
            || originalField.IsRequired != currentField.IsRequired
            || !string.Equals(
                originalField.ValidationRules,
                currentField.ValidationRules,
                StringComparison.Ordinal
            );
    }

    private static void NormalizeAndReindexCustomFields(
        ObservableCollection<Model_CustomFieldDefinition> customFields
    )
    {
        for (var index = 0; index < customFields.Count; index++)
        {
            customFields[index].DisplayOrder = index + 1;
            customFields[index].DatabaseColumnName =
                Model_CustomFieldDefinition.BuildDatabaseColumnName(customFields[index].FieldName);
        }
    }

    private static string ValidateCustomFieldInput(
        ObservableCollection<Model_CustomFieldDefinition> existingFields,
        string fieldName,
        Model_CustomFieldDefinition? editingField
    )
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return "Field name is required.";
        }

        if (fieldName.Trim().Length > 100)
        {
            return "Field name cannot exceed 100 characters.";
        }

        if (fieldName.Any(character => "<>{}[]|\\".Contains(character)))
        {
            return "Field name cannot contain special characters: < > { } [ ] | \\";
        }

        var trimmedFieldName = fieldName.Trim();
        if (
            existingFields.Any(field =>
                !ReferenceEquals(field, editingField)
                && string.Equals(
                    field.FieldName,
                    trimmedFieldName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            return "Field name must be unique for this type.";
        }

        return string.Empty;
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
