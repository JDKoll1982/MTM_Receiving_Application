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

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.Review)
        {
            LoadSessionLoads();
        }
    }

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_DunnageLoad> _sessionLoads = new();

    [ObservableProperty]
    private Model_DunnageLoad? _currentLoad;

    [ObservableProperty]
    private int _currentEntryIndex = 1;

    [ObservableProperty]
    private int _loadCount;

    [ObservableProperty]
    private bool _canSave = true;

    [ObservableProperty]
    private bool _isSuccessMessageVisible = false;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _isSingleView = true;

    [ObservableProperty]
    private bool _isTableView = false;

    [ObservableProperty]
    private bool _canGoBack = false;

    [ObservableProperty]
    private bool _canGoNext = false;

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

            // Set up first entry for single view
            if (LoadCount > 0)
            {
                CurrentEntryIndex = 1;
                CurrentLoad = SessionLoads[0];
                UpdateNavigationButtons();
            }

            _logger.LogInfo($"Loaded {LoadCount} loads for review", "Review");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading session loads: {ex.Message}", ex, "Review");
        }
    }

    private void UpdateNavigationButtons()
    {
        CanGoBack = CurrentEntryIndex > 1;
        CanGoNext = CurrentEntryIndex < LoadCount;
    }

    #endregion

    #region View Navigation Commands

    [RelayCommand]
    private void SwitchToSingleView()
    {
        IsSingleView = true;
        IsTableView = false;
        _logger.LogInfo("Switched to Single View", "Review");
    }

    [RelayCommand]
    private void SwitchToTableView()
    {
        IsSingleView = false;
        IsTableView = true;
        _logger.LogInfo("Switched to Table View", "Review");
    }

    [RelayCommand]
    private void PreviousEntry()
    {
        if (CurrentEntryIndex > 1)
        {
            CurrentEntryIndex--;
            CurrentLoad = SessionLoads[CurrentEntryIndex - 1];
            UpdateNavigationButtons();
        }
    }

    [RelayCommand]
    private void NextEntry()
    {
        if (CurrentEntryIndex < LoadCount)
        {
            CurrentEntryIndex++;
            CurrentLoad = SessionLoads[CurrentEntryIndex - 1];
            UpdateNavigationButtons();
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
        if (!CanSave || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            CanSave = false;
            StatusMessage = "Saving loads...";

            await _logger.LogInfoAsync($"Starting SaveAllAsync: {LoadCount} loads to save");

            // Save loads to database
            var saveResult = await _dunnageService.SaveLoadsAsync(SessionLoads.ToList());

            if (!saveResult.Success)
            {
                await _logger.LogErrorAsync($"Failed to save {LoadCount} loads: {saveResult.ErrorMessage}");
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            await _logger.LogInfoAsync($"Successfully saved {LoadCount} loads to database");
            StatusMessage = "Exporting to CSV...";

            // Export to CSV
            var csvResult = await _csvWriter.WriteToCSVAsync(SessionLoads.ToList());

            if (!csvResult.LocalSuccess)
            {
                await _logger.LogWarningAsync($"CSV export failed for {LoadCount} loads: {csvResult.ErrorMessage}");
                await _errorHandler.HandleErrorAsync(
                    csvResult.ErrorMessage ?? "CSV export failed",
                    Enum_ErrorSeverity.Warning,
                    null,
                    true);
                // Continue anyway - database save was successful
            }
            else
            {
                await _logger.LogInfoAsync($"Successfully exported {LoadCount} loads to CSV");
            }

            // Show success message
            SuccessMessage = $"✓ Successfully saved {LoadCount} load(s) to database and exported to CSV";
            IsSuccessMessageVisible = true;

            await _logger.LogInfoAsync($"Completed SaveAllAsync: {LoadCount} loads processed successfully");

            // Wait a moment for user to see success message
            await Task.Delay(2000);

            // Clear session and return to Mode Selection
            _workflowService.ClearSession();
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Exception in SaveAllAsync: {ex.Message}");
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
