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

    public Dunnage_ManualEntryViewModel(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageCSVWriter csvWriter,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _csvWriter = csvWriter;
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
            if (typesResult.Success)
            {
                AvailableTypes.Clear();
                foreach (var type in typesResult.Data)
                {
                    AvailableTypes.Add(type);
                }
            }

            // Load available parts
            var partsResult = await _dunnageService.GetAllPartsAsync();
            if (partsResult.Success)
            {
                AvailableParts.Clear();
                foreach (var part in partsResult.Data)
                {
                    AvailableParts.Add(part);
                }
            }

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
    private void AddMultiple(string countStr)
    {
        if (!int.TryParse(countStr, out int count) || count <= 0 || count > 100)
        {
            StatusMessage = "Please enter a number between 1 and 100";
            return;
        }

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
        _logger.LogInfo($"Added {count} rows, total: {Loads.Count}", "ManualEntry");
    }

    [RelayCommand]
    private void RemoveRow()
    {
        if (SelectedLoad == null) return;

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
        if (Loads.Count < 2) return;

        var lastLoad = Loads.LastOrDefault();
        if (lastLoad == null) return;

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
        if (Loads.Count == 0) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Auto-filling data...";

            // Auto-fill logic: for each load, populate missing fields intelligently
            foreach (var load in Loads)
            {
                // If part ID is set but type name is missing, look up the type
                if (!string.IsNullOrWhiteSpace(load.PartId) && string.IsNullOrWhiteSpace(load.TypeName))
                {
                    var part = AvailableParts.FirstOrDefault(p => p.PartId == load.PartId);
                    if (part != null)
                    {
                        var type = AvailableTypes.FirstOrDefault(t => t.Id == part.TypeId);
                        if (type != null)
                        {
                            load.TypeName = type.TypeName;
                        }
                    }
                }

                // Set default quantity if missing
                if (load.Quantity <= 0)
                {
                    load.Quantity = 1;
                }

                // Set default inventory method
                if (string.IsNullOrWhiteSpace(load.InventoryMethod))
                {
                    load.InventoryMethod = string.IsNullOrWhiteSpace(load.PoNumber) ? "Adjust In" : "Receive In";
                }
            }

            StatusMessage = "Auto-fill completed";
            _logger.LogInfo("Auto-fill executed", "ManualEntry");
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
        if (Loads.Count == 0) return;

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
        if (Loads.Count == 0) return;

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

    #region Helper Methods

    private void UpdateCanSave()
    {
        CanSave = Loads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
    }

    #endregion
}
