using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Manual Entry mode
/// </summary>
public partial class Dunnage_ManualEntryViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_DunnageCSVWriter _csvWriter;
    private readonly IService_Window _windowService;

    public Dunnage_ManualEntryViewModel(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageCSVWriter csvWriter,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _csvWriter = csvWriter;
        _windowService = windowService;
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
                AvailableTypes.Clear();
                foreach (var type in typesResult.Data)
                {
                    AvailableTypes.Add(type);
                }
            }

            // Load available parts
            var partsResult = await _dunnageService.GetAllPartsAsync();
            if (partsResult.Success && partsResult.Data != null)
            {
                AvailableParts.Clear();
                foreach (var part in partsResult.Data)
                {
                    AvailableParts.Add(part);
                }
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
            CreatedBy = Environment.UserName
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
                await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
                return;
            }

            var dialog = new Views.Dunnage.Dialogs.AddMultipleRowsDialog
            {
                XamlRoot = xamlRoot
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
                        CreatedBy = Environment.UserName
                    };
                    Loads.Add(newLoad);
                }

                UpdateCanSave();
                StatusMessage = $"Added {count} rows (Total: {Loads.Count})";
                _logger.LogInfo($"Added {count} rows via dialog, total: {Loads.Count}", "ManualEntry");
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
            if (string.IsNullOrWhiteSpace(load.PoNumber) && !string.IsNullOrWhiteSpace(lastLoad.PoNumber))
            {
                load.PoNumber = lastLoad.PoNumber;
            }

            // Copy Location
            if (string.IsNullOrWhiteSpace(load.Location) && !string.IsNullOrWhiteSpace(lastLoad.Location))
            {
                load.Location = lastLoad.Location;
            }

            // Copy specs if they exist
            if (load.SpecValues == null && lastLoad.SpecValues != null)
            {
                load.SpecValues = new Dictionary<string, object>(lastLoad.SpecValues);
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

        Loads.Clear();
        foreach (var load in sorted)
        {
            Loads.Add(load);
        }

        StatusMessage = "Sorted for printing (Part ID → PO → Type)";
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
                    true);
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

            // Auto-fill Spec Values from part master data
            if (part.SpecValuesDict?.Count > 0)
            {
                SelectedLoad.SpecValues = new Dictionary<string, object>(part.SpecValuesDict);
                _logger.LogInfo($"Auto-filled {part.SpecValuesDict.Count} spec values for Part ID: {SelectedLoad.PartId}", "ManualEntry");
            }

            // Set default values
            if (SelectedLoad.Quantity <= 0)
            {
                SelectedLoad.Quantity = 1;
            }

            if (string.IsNullOrWhiteSpace(SelectedLoad.InventoryMethod))
            {
                SelectedLoad.InventoryMethod = string.IsNullOrWhiteSpace(SelectedLoad.PoNumber) ? "Adjust In" : "Receive In";
            }

            StatusMessage = $"Auto-filled data for Part ID: {SelectedLoad.PartId}";
            _logger.LogInfo($"Auto-fill completed for Part ID: {SelectedLoad.PartId}", "ManualEntry");
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
            IsBusy = false;
            CanSave = true;
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
            CanSave = false;
            StatusMessage = "Saving loads...";

            // Validate all loads
            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
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

            // Export to CSV
            var csvResult = await _csvWriter.WriteToCSVAsync(Loads.ToList());

            if (!csvResult.LocalSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    csvResult.ErrorMessage ?? "CSV export failed",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true);
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
            IsBusy = false;
            CanSave = true;
        }
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
            await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Change Mode?",
            Content = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
            PrimaryButtonText = "Yes, Change Mode",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync().AsTask();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            try
            {
                _logger.LogInfo("User confirmed return to mode selection, clearing loads", "ManualEntry");
                Loads.Clear();
                _workflowService.ClearSession();
                _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to return to mode selection: {ex.Message}", ex, "ManualEntry");
                await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex, true);
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
            var specKeys = await _dunnageService.GetAllSpecKeysAsync();
            SpecColumnHeaders = specKeys;
            _logger.LogInfo($"Loaded {SpecColumnHeaders.Count} dynamic spec columns", "ManualEntry");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load spec columns: {ex.Message}", ex, "ManualEntry");
        }
    }

    /// <summary>
    /// Handles PartID change to trigger auto-fill
    /// </summary>
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

                // Auto-fill specs from part master
                if (part.SpecValuesDict?.Count > 0)
                {
                    load.SpecValues = new Dictionary<string, object>(part.SpecValuesDict);
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
        CanSave = Loads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
    }

    #endregion
}
