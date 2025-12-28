using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Dunnage;

/// <summary>
/// ViewModel for Dunnage Review & Save
/// </summary>
public partial class Dunnage_ReviewViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_DunnageCSVWriter _csvWriter;

    public Dunnage_ReviewViewModel(
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
    private ObservableCollection<Model_DunnageLoad> _sessionLoads = new();

    [ObservableProperty]
    private int _loadCount;

    [ObservableProperty]
    private bool _canSave = true;

    [ObservableProperty]
    private bool _isSuccessMessageVisible = false;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    #endregion

    #region Initialization

    /// <summary>
    /// Load session loads for review
    /// </summary>
    public void LoadSessionLoads()
    {
        try
        {
            SessionLoads.Clear();

            var loads = _workflowService.CurrentSession.Loads;
            foreach (var load in loads)
            {
                SessionLoads.Add(load);
            }

            LoadCount = SessionLoads.Count;
            CanSave = LoadCount > 0;

            _logger.LogInfo($"Loaded {LoadCount} loads for review", "Review");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading session loads: {ex.Message}", ex, "Review");
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void AddAnother()
    {
        _logger.LogInfo("Adding another load, preserving session", "Review");
        
        // Navigate back to Type Selection without clearing session
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
    }

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (!CanSave || IsBusy) return;

        try
        {
            IsBusy = true;
            CanSave = false;
            StatusMessage = "Saving loads...";

            // Save loads to database
            var saveResult = await _dunnageService.SaveLoadsAsync(SessionLoads.ToList());

            if (!saveResult.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            StatusMessage = "Exporting to CSV...";

            // Export to CSV
            var csvResult = await _csvWriter.WriteToCSVAsync(SessionLoads.ToList());

            if (!csvResult.LocalSuccess)
            {
                await _errorHandler.HandleErrorAsync(
                    csvResult.ErrorMessage ?? "CSV export failed",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true);
                // Continue anyway - database save was successful
            }

            // Show success message
            SuccessMessage = $"✓ Successfully saved {LoadCount} load(s) to database and exported to CSV";
            IsSuccessMessageVisible = true;

            _logger.LogInfo($"Saved {LoadCount} loads successfully", "Review");

            // Wait a moment for user to see success message
            await Task.Delay(2000);

            // Clear session and return to Mode Selection
            _workflowService.ClearSession();
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
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

    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInfo("Cancelling review, clearing session", "Review");
        
        // Clear session and return to Mode Selection
        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
    }

    #endregion
}
