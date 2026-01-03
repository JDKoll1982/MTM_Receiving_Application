using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.DunnageModule.Models;
using MTM_Receiving_Application.DunnageModule.Enums;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.DunnageModule.ViewModels;

/// <summary>
/// ViewModel for Add New Type Dialog with real-time validation and custom field preview
/// </summary>
public partial class ViewModel_Dunnage_AddTypeDialog : Shared_BaseViewModel
{
    #region Dependencies
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _validationTimer;
    #endregion

    #region Observable Properties - Basic Information
    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private MaterialIconKind _selectedIcon = MaterialIconKind.PackageVariantClosed; // Default icon

    [ObservableProperty]
    private string _typeNameError = string.Empty;

    [ObservableProperty]
    private bool _showDuplicateWarning;

    [ObservableProperty]
    private string _duplicateTypeId = string.Empty;
    #endregion

    #region Observable Properties - Custom Fields
    [ObservableProperty]
    private ObservableCollection<Model_CustomFieldDefinition> _customFields = new();

    [ObservableProperty]
    private string _fieldName = string.Empty;

    [ObservableProperty]
    private string _fieldType = "Text";

    [ObservableProperty]
    private bool _isFieldRequired;

    [ObservableProperty]
    private string _fieldNameError = string.Empty;

    [ObservableProperty]
    private int _fieldCharacterCount;

    [ObservableProperty]
    private bool _showFieldLimitWarning;

    [ObservableProperty]
    private bool _canAddField = true;

    [ObservableProperty]
    private Model_CustomFieldDefinition? _editingField;
    #endregion

    #region Observable Properties - Validation
    [ObservableProperty]
    private bool _canSave;
    #endregion

    #region Observable Properties - Icon Picker
    [ObservableProperty]
    private ObservableCollection<Model_IconDefinition> _recentlyUsedIcons = new();
    #endregion

    #region Constructor
    public ViewModel_Dunnage_AddTypeDialog(
        IService_MySQL_Dunnage dunnageService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _dunnageService = dunnageService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Initialize validation timer
        _validationTimer = _dispatcherQueue.CreateTimer();
        _validationTimer.Interval = TimeSpan.FromMilliseconds(300);
        _validationTimer.Tick += OnValidationTimerTick;

        // Load recently used icons
        _ = LoadRecentlyUsedIconsAsync();
    }
    #endregion

    #region Commands - Save Type
    [RelayCommand]
    private async Task SaveTypeAsync()
    {
        if (IsBusy || !CanSave)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Saving type...";

            // Save type to database
            var typeResult = await _dunnageService.InsertTypeAsync(TypeName, SelectedIcon.ToString());
            if (!typeResult.IsSuccess)
            {
                await _errorHandler.ShowUserErrorAsync(typeResult.ErrorMessage, "Save Failed", nameof(SaveTypeAsync));
                return;
            }

            // Save custom fields
            int typeId = typeResult.Data;
            foreach (var field in CustomFields)
            {
                var fieldResult = await _dunnageService.InsertCustomFieldAsync(typeId, field);
                if (!fieldResult.IsSuccess)
                {
                    await _errorHandler.ShowUserErrorAsync($"Failed to save field '{field.FieldName}': {fieldResult.ErrorMessage}",
                        "Save Failed", nameof(SaveTypeAsync));
                    return;
                }
            }

            // Update recently used icons
            await _dunnageService.UpsertUserPreferenceAsync($"RecentIcon_{SelectedIcon}", DateTime.Now.ToString("O"));

            StatusMessage = "Type saved successfully";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
                nameof(SaveTypeAsync), nameof(ViewModel_Dunnage_AddTypeDialog));
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Commands - Custom Fields
    [RelayCommand]
    private void AddField()
    {
        if (string.IsNullOrWhiteSpace(FieldName) || CustomFields.Count >= 25)
            return;

        if (EditingField != null)
        {
            // Update existing field
            EditingField.FieldName = FieldName;
            EditingField.FieldType = FieldType;
            EditingField.IsRequired = IsFieldRequired;
            EditingField = null;
        }
        else
        {
            // Add new field
            var field = new Model_CustomFieldDefinition
            {
                FieldName = FieldName,
                FieldType = FieldType,
                IsRequired = IsFieldRequired,
                DisplayOrder = CustomFields.Count + 1
            };
            CustomFields.Add(field);
        }

        // Clear form
        FieldName = string.Empty;
        FieldType = "Text";
        IsFieldRequired = false;

        // Update warnings and limits
        ShowFieldLimitWarning = CustomFields.Count >= 10;
        CanAddField = CustomFields.Count < 25;
        UpdateCanSave();
    }

    [RelayCommand]
    private void EditField(Model_CustomFieldDefinition field)
    {
        EditingField = field;
        FieldName = field.FieldName;
        FieldType = field.FieldType;
        IsFieldRequired = field.IsRequired;
    }

    [RelayCommand]
    private void DeleteField(Model_CustomFieldDefinition field)
    {
        CustomFields.Remove(field);

        // Update display order
        for (int i = 0; i < CustomFields.Count; i++)
        {
            CustomFields[i].DisplayOrder = i + 1;
        }

        ShowFieldLimitWarning = CustomFields.Count >= 10;
        CanAddField = CustomFields.Count < 25;
        UpdateCanSave();
    }
    #endregion

    #region Validation Methods
    partial void OnTypeNameChanged(string value)
    {
        FieldCharacterCount = value.Length;
        _validationTimer?.Stop();
        _validationTimer?.Start();
    }

    partial void OnFieldNameChanged(string value)
    {
        FieldCharacterCount = value.Length;
        _validationTimer?.Stop();
        _validationTimer?.Start();
    }

    private void OnValidationTimerTick(object? sender, object e)
    {
        _validationTimer?.Stop();
        ValidateTypeName();
        ValidateFieldName();
        UpdateCanSave();
    }

    private async void ValidateTypeName()
    {
        TypeNameError = string.Empty;
        ShowDuplicateWarning = false;

        if (string.IsNullOrWhiteSpace(TypeName))
        {
            TypeNameError = "Type name is required";
            return;
        }

        if (TypeName.Length > 100)
        {
            TypeNameError = "Type name cannot exceed 100 characters";
            return;
        }

        // Check for duplicate
        var result = await _dunnageService.CheckDuplicateTypeNameAsync(TypeName);
        if (result.IsSuccess && result.Data > 0)
        {
            ShowDuplicateWarning = true;
            DuplicateTypeId = result.Data.ToString();
        }
    }

    private void ValidateFieldName()
    {
        FieldNameError = string.Empty;

        if (string.IsNullOrWhiteSpace(FieldName))
            return; // Don't show error if empty (only on add)

        if (FieldName.Length > 100)
        {
            FieldNameError = "Field name cannot exceed 100 characters";
            return;
        }

        // Check for special characters
        if (FieldName.Any(c => "<>{}[]|\\".Contains(c)))
        {
            FieldNameError = "Field name cannot contain special characters: < > { } [ ] | \\";
            return;
        }

        // Check for duplicate field name
        if (CustomFields.Any(f => f.FieldName.Equals(FieldName, StringComparison.OrdinalIgnoreCase) && f != EditingField))
        {
            FieldNameError = "Field name must be unique";
            return;
        }
    }

    private void UpdateCanSave()
    {
        CanSave = !string.IsNullOrWhiteSpace(TypeName) &&
                  string.IsNullOrEmpty(TypeNameError) &&
                  TypeName.Length <= 100;
    }
    #endregion

    #region Helper Methods
    private async Task LoadRecentlyUsedIconsAsync()
    {
        try
        {
            var result = await _dunnageService.GetRecentlyUsedIconsAsync(6);
            if (result.IsSuccess && result.Data != null)
            {
                RecentlyUsedIcons.Clear();
                foreach (var icon in result.Data)
                {
                    RecentlyUsedIcons.Add(icon);
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Failed to load recently used icons: {ex.Message}");
        }
    }
    #endregion

    #region Drag-Drop Reordering
    public void OnDragItemsCompleted()
    {
        // Update DisplayOrder based on current position in collection
        for (int i = 0; i < CustomFields.Count; i++)
        {
            CustomFields[i].DisplayOrder = i + 1;
        }
    }
    #endregion
}
