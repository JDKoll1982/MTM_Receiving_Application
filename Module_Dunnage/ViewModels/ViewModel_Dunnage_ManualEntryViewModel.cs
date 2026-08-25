using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Manual Entry mode
/// </summary>
public partial class ViewModel_Dunnage_ManualEntry : ViewModel_Shared_Base, IResettableViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Window _windowService;
    private readonly IService_Help _helpService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;

    public ViewModel_Dunnage_ManualEntry(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_ViewModelRegistry viewModelRegistry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_Help helpService,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _viewModelRegistry = viewModelRegistry;
        _windowService = windowService;
        _helpService = helpService;
        _viewModelRegistry.Register(this);
    }

    public bool HasUnsavedData =>
        Loads.Any(load =>
            string.IsNullOrWhiteSpace(load.TypeName) is false
            || string.IsNullOrWhiteSpace(load.PartId) is false
            || string.IsNullOrWhiteSpace(load.PoNumber) is false
            || string.IsNullOrWhiteSpace(load.Location) is false
            || string.IsNullOrWhiteSpace(load.HomeLocation) is false
            || Enumerable.Range(1, 10).Any(slot =>
                !string.IsNullOrWhiteSpace(load.GetUdcValue(slot))
            )
        );

    public void ResetToDefaults()
    {
        ReplaceLoads(Array.Empty<Model_DunnageLoad>());
        SelectedLoad = null;
        CanSave = false;
        StatusMessage = string.Empty;
        AddRow();
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _loads = new();

    [ObservableProperty]
    private Model_DunnageLoad? _selectedLoad;

    [ObservableProperty]
    private bool _canSave = false;

    [ObservableProperty]
    private ObservableCollection<Model_DunnageType> _availableTypes = new();

    [ObservableProperty]
    private ObservableCollection<Model_DunnagePart> _availableParts = new();

    [ObservableProperty]
    private List<string> _specColumnHeaders = new();

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading types and parts...";

            // Load available types
            var typesResult = await _dunnageService.GetAllTypesAsync();
            if (typesResult.Success && typesResult.Data != null)
            {
                AvailableTypes = new ObservableCollection<Model_DunnageType>(typesResult.Data);
            }

            // Load available parts
            var partsResult = await _dunnageService.GetAllPartsAsync();
            if (partsResult.Success && partsResult.Data != null)
            {
                AvailableParts = new ObservableCollection<Model_DunnagePart>(partsResult.Data);
            }

            // Load dynamic spec columns
            await LoadSpecColumnsAsync();

            // Add initial empty row
            AddRow();

            _logger.LogInfo("Manual Entry initialized", "ManualEntry");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error initializing manual entry",
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

    #region Row Management Commands

    [RelayCommand]
    private void AddRow()
    {
        var newLoad = new Model_DunnageLoad
        {
            Quantity = 1,
            CreatedDate = DateTime.Now,
            CreatedBy = Environment.UserName,
        };

        Loads.Add(newLoad);
        SelectedLoad = newLoad;
        UpdateCanSave();

        _logger.LogInfo($"Added new row, total: {Loads.Count}", "ManualEntry");
    }

    [RelayCommand]
    private async Task AddMultipleRowsAsync()
    {
        try
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null", null, "ManualEntry");
                await _errorHandler.HandleErrorAsync(
                    "Unable to display dialog",
                    Enum_ErrorSeverity.Error,
                    null,
                    true
                );
                return;
            }

            var dialog = new Module_Dunnage.Views.View_Dunnage_Dialog_AddMultipleRowsDialog
            {
                XamlRoot = xamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                int count = dialog.RowCount;

                for (int i = 0; i < count; i++)
                {
                    var newLoad = new Model_DunnageLoad
                    {
                        Quantity = 1,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Environment.UserName,
                    };
                    Loads.Add(newLoad);
                }

                UpdateCanSave();
                StatusMessage = $"Added {count} rows (Total: {Loads.Count})";
                _logger.LogInfo(
                    $"Added {count} rows via dialog, total: {Loads.Count}",
                    "ManualEntry"
                );
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error adding multiple rows",
                Enum_ErrorSeverity.Warning,
                ex,
                true
            );
        }
    }

    [RelayCommand]
    private void RemoveRow()
    {
        if (SelectedLoad == null)
        {
            return;
        }

        Loads.Remove(SelectedLoad);
        SelectedLoad = Loads.LastOrDefault();
        UpdateCanSave();

        _logger.LogInfo($"Removed row, total: {Loads.Count}", "ManualEntry");
    }

    #endregion

    #region Utility Commands

    [RelayCommand]
    private void FillBlankSpaces()
    {
        if (Loads.Count < 2)
        {
            return;
        }

        var lastLoad = Loads.LastOrDefault();
        if (lastLoad == null)
        {
            return;
        }

        foreach (var load in Loads)
        {
            // Copy PO Number
            if (
                string.IsNullOrWhiteSpace(load.PoNumber)
                && !string.IsNullOrWhiteSpace(lastLoad.PoNumber)
            )
            {
                load.PoNumber = lastLoad.PoNumber;
            }

            // Copy Location
            if (
                string.IsNullOrWhiteSpace(load.Location)
                && !string.IsNullOrWhiteSpace(lastLoad.Location)
            )
            {
                load.Location = lastLoad.Location;
            }

            // Copy udc values if they exist
            for (var slot = 1; slot <= 10; slot++)
            {
                if (
                    string.IsNullOrWhiteSpace(load.GetUdcValue(slot))
                    && lastLoad.GetUdcValue(slot) is not null
                )
                {
                    load.SetUdcValue(slot, lastLoad.GetUdcValue(slot));
                }
            }
        }

        StatusMessage = "Filled blank spaces from last row";
        _logger.LogInfo("Fill blank spaces executed", "ManualEntry");
    }

    [RelayCommand]
    private void SortForPrinting()
    {
        var sorted = Loads
            .OrderBy(l => l.PartId)
            .ThenBy(l => l.PoNumber)
            .ThenBy(l => l.TypeName)
            .ToList();

        ReplaceLoads(sorted);

        StatusMessage = "Sorted (Part ID → PO → Type)";
        _logger.LogInfo("Sort for printing executed", "ManualEntry");
    }

    [RelayCommand]
    private async Task AutoFillAsync()
    {
        if (SelectedLoad == null)
        {
            StatusMessage = "Please select a row first";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedLoad.PartId))
        {
            StatusMessage = "Please enter a Part ID first";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Auto-filling from part master data...";

            // Get part by ID
            var partResult = await _dunnageService.GetPartByIdAsync(SelectedLoad.PartId);
            if (!partResult.Success || partResult.Data == null)
            {
                await _errorHandler.HandleErrorAsync(
                    $"Part ID '{SelectedLoad.PartId}' not found in master data",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true
                );
                StatusMessage = "Part not found - please check Part ID";
                return;
            }

            var part = partResult.Data;

            // Auto-fill Type
            var type = AvailableTypes.FirstOrDefault(t => t.Id == part.TypeId);
            if (type != null)
            {
                SelectedLoad.TypeName = type.TypeName;
                SelectedLoad.DunnageType = type.TypeName;
            }

            // Auto-fill udc values from part master data
            var partUdc = Helper_Dunnage_PartSpecs.ExtractUdc(part);
            for (var slot = 1; slot <= 10; slot++)
            {
                var value = Helper_Dunnage_PartSpecs.GetValueForSlot(partUdc, slot);
                if (string.IsNullOrWhiteSpace(value) is false)
                {
                    SelectedLoad.SetUdcValue(slot, value);
                }
            }

            // Set default values
            if (SelectedLoad.Quantity <= 0)
            {
                SelectedLoad.Quantity = 1;
            }

            if (string.IsNullOrWhiteSpace(SelectedLoad.InventoryMethod))
            {
                SelectedLoad.InventoryMethod = string.IsNullOrWhiteSpace(SelectedLoad.PoNumber)
                    ? "Adjust In"
                    : "Receive In";
            }

            StatusMessage = $"Auto-filled data for Part ID: {SelectedLoad.PartId}";
            _logger.LogInfo(
                $"Auto-fill completed for Part ID: {SelectedLoad.PartId}",
                "ManualEntry"
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error during auto-fill",
                Enum_ErrorSeverity.Warning,
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

    #region Save Commands

    [RelayCommand]
    private async Task SaveToHistoryAsync()
    {
        if (Loads.Count == 0)
        {
            return;
        }

        try
        {
            IsBusy = true;
            _workflowService.SetNavigationLock(true);
            CanSave = false;
            StatusMessage = "Saving to history...";

            // Use same save method - history is just another save
            var result = await _dunnageService.SaveLoadsAsync(Loads.ToList());

            if (result.Success)
            {
                StatusMessage = $"Saved {Loads.Count} loads to history";
                _logger.LogInfo($"Saved {Loads.Count} loads to history", "ManualEntry");
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(result, "SaveToHistoryAsync", true);
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving to history",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            _workflowService.SetNavigationLock(false);
            IsBusy = false;
            UpdateCanSave();
        }
    }

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (Loads.Count == 0)
        {
            return;
        }

        try
        {
            IsBusy = true;
            _workflowService.SetNavigationLock(true);
            CanSave = false;
            StatusMessage = "Saving loads...";

            // Validate all loads
            foreach (var load in Loads)
            {
                if (
                    string.IsNullOrWhiteSpace(load.TypeName)
                    || string.IsNullOrWhiteSpace(load.PartId)
                )
                {
                    StatusMessage = "All loads must have Type and Part ID";
                    return;
                }
            }

            // Save to database
            var saveResult = await _dunnageService.SaveLoadsAsync(Loads.ToList());

            if (!saveResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            StatusMessage = $"Successfully saved {Loads.Count} loads";
            _logger.LogInfo($"Saved {Loads.Count} loads", "ManualEntry");

            // Clear loads
            Loads.Clear();
            AddRow(); // Add new empty row
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving loads",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            _workflowService.SetNavigationLock(false);
            IsBusy = false;
            UpdateCanSave();
        }
    }

    /// <summary>
    /// Shows contextual help for manual entry.
    /// </summary>
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.ManualEntry");
    }

    private void ReplaceLoads(IEnumerable<Model_DunnageLoad> loads)
    {
        var selectedLoadId = SelectedLoad?.LoadUuid;
        Loads = new ObservableCollection<Model_DunnageLoad>(loads);
        SelectedLoad = selectedLoadId is null
            ? Loads.LastOrDefault()
            : Loads.FirstOrDefault(load => load.LoadUuid == selectedLoadId.Value)
                ?? Loads.LastOrDefault();
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private async Task ReturnToModeSelectionAsync()
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show dialog: XamlRoot is null", null, "ManualEntry");
            await _errorHandler.HandleErrorAsync(
                "Unable to display dialog",
                Enum_ErrorSeverity.Error,
                null,
                true
            );
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Change Mode?",
            Content =
                "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
            PrimaryButtonText = "Yes, Change Mode",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        var result = await dialog.ShowAsync().AsTask();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            try
            {
                _logger.LogInfo(
                    "User confirmed return to mode selection, clearing loads",
                    "ManualEntry"
                );
                Loads.Clear();
                _workflowService.ClearSession();
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Failed to return to mode selection: {ex.Message}",
                    ex,
                    "ManualEntry"
                );
                await _errorHandler.HandleErrorAsync(
                    "Failed to return to mode selection",
                    Enum_ErrorSeverity.Error,
                    ex,
                    true
                );
            }
        }
        else
        {
            _logger.LogInfo("User cancelled return to mode selection", "ManualEntry");
        }
    }

    #endregion

    #region Helper Methods

    private async Task LoadSpecColumnsAsync()
    {
        try
        {
            var columnNames = new List<string>();
            var typesResult = await _dunnageService.GetAllTypesAsync();
            if (typesResult.IsSuccess && typesResult.Data != null)
            {
                foreach (var type in typesResult.Data)
                {
                    var fieldsResult = await _dunnageService.GetCustomFieldsByTypeAsync(type.Id);
                    if (fieldsResult.IsSuccess && fieldsResult.Data != null)
                    {
                        foreach (var field in fieldsResult.Data)
                        {
                            if (
                                columnNames.Contains(
                                    field.FieldName,
                                    StringComparer.OrdinalIgnoreCase
                                ) is false
                            )
                            {
                                columnNames.Add(field.FieldName);
                            }
                        }
                    }
                }
            }

            SpecColumnHeaders = columnNames;
            _logger.LogInfo(
                $"Loaded {SpecColumnHeaders.Count} custom-field columns",
                "ManualEntry"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load spec columns: {ex.Message}", ex, "ManualEntry");
        }
    }

    /// <summary>
    /// Handles PartID change to trigger auto-fill
    /// </summary>
    /// <param name="load"></param>
    public async Task OnPartIdChangedAsync(Model_DunnageLoad load)
    {
        if (load == null || string.IsNullOrWhiteSpace(load.PartId))
        {
            return;
        }

        try
        {
            // Auto-populate type and specs when PartID is entered
            var partResult = await _dunnageService.GetPartByIdAsync(load.PartId);
            if (partResult.Success && partResult.Data != null)
            {
                var part = partResult.Data;
                var type = AvailableTypes.FirstOrDefault(t => t.Id == part.TypeId);
                if (type != null)
                {
                    load.TypeName = type.TypeName;
                    load.DunnageType = type.TypeName;
                }

                // Auto-fill udc values from part master
                var partUdc = Helper_Dunnage_PartSpecs.ExtractUdc(part);
                for (var slot = 1; slot <= 10; slot++)
                {
                    var value = Helper_Dunnage_PartSpecs.GetValueForSlot(partUdc, slot);
                    if (string.IsNullOrWhiteSpace(value) is false)
                    {
                        load.SetUdcValue(slot, value);
                    }
                }

                _logger.LogInfo($"Auto-populated data for Part ID: {load.PartId}", "ManualEntry");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error auto-populating part data: {ex.Message}", ex, "ManualEntry");
        }
    }

    private void UpdateCanSave()
    {
        CanSave = Loads.Any(l =>
            !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId)
        );
    }

    #endregion

    #region Help Content Helpers

    /// <summary>
    /// Gets a tooltip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    /// <summary>
    /// Gets a placeholder by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

    /// <summary>
    /// Gets a tip by key from the help service
    /// </summary>
    /// <param name="key"></param>
    public string GetTip(string key) => _helpService.GetTip(key);

    #endregion
}
