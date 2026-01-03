using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Review & Save
/// </summary>
public partial class ViewModel_Dunnage_Review : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService; private readonly IService_Help _helpService; private readonly IService_DunnageCSVWriter _csvWriter;
    private readonly IService_Window _windowService;

    public ViewModel_Dunnage_Review(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageCSVWriter csvWriter,
        IService_Help helpService,
        IService_Window windowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _csvWriter = csvWriter;
        _helpService = helpService;
        _windowService = windowService;

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
    private async Task AddAnotherAsync()
    {
        _logger.LogInfo("User requested to add another load", "Review");

        try
        {
            // Show confirmation dialog to prevent accidental data loss
            if (!await ConfirmAddAnotherAsync())
            {
                _logger.LogInfo("User cancelled add another load", "Review");
                return;
            }

            // Clear transient workflow data to prepare for new entry
            ClearTransientWorkflowData();

            // Navigate back to Type Selection without clearing session loads
            _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);

            _logger.LogInfo("Navigated to Type Selection for new load, workflow data cleared", "Review");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in AddAnotherAsync: {ex.Message}", ex, "Review");
            await _errorHandler.HandleErrorAsync(
                "Failed to prepare for new load entry",
                Enum_ErrorSeverity.Medium,
                ex,
                true);
        }
    }

    /// <summary>
    /// Shows confirmation dialog before clearing data for new entry
    /// </summary>
    private async Task<bool> ConfirmAddAnotherAsync()
    {
        try
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogWarning("XamlRoot is null, proceeding without confirmation", "Review");
                return true;
            }

            var dialog = new ContentDialog
            {
                Title = "Add Another Load",
                Content = "Current form data will be cleared to start a new entry. Your reviewed loads are preserved. Continue?",
                PrimaryButtonText = "Continue",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex, "Review");
            return true; // Proceed if dialog fails
        }
    }

    /// <summary>
    /// Clears transient workflow data from intermediate steps (not the session loads)
    /// This prevents data duplication when adding another load
    /// </summary>
    private void ClearTransientWorkflowData()
    {
        try
        {
            // Clear the current session properties that hold form data
            // but preserve the Loads collection (already reviewed loads)
            var session = _workflowService.CurrentSession;
            if (session != null)
            {
                // Clear selection properties to start fresh
                session.SelectedTypeId = 0;
                session.SelectedTypeName = string.Empty;
                session.SelectedPart = null;
                session.Quantity = 0;
                session.PONumber = string.Empty;
                session.Location = string.Empty;
                // Preserve Loads collection - these are already reviewed
            }

            // Clear UI inputs in connected ViewModels
            ClearUIInputsForNewEntry();

            _logger.LogInfo("Transient workflow data and UI inputs cleared for new entry", "Review");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex, "Review");
        }
    }

    /// <summary>
    /// Clears UI input properties in ViewModels to prepare for new entry
    /// while preserving reviewed loads
    /// </summary>
    private void ClearUIInputsForNewEntry()
    {
        try
        {
            // Clear TypeSelection ViewModel
            var typeSelectionVM = App.GetService<ViewModel_Dunnage_TypeSelection>();
            if (typeSelectionVM != null)
            {
                typeSelectionVM.SelectedType = null;
            }

            // Clear PartSelection ViewModel
            var partSelectionVM = App.GetService<ViewModel_Dunnage_PartSelection>();
            if (partSelectionVM != null)
            {
                partSelectionVM.SelectedPart = null;
            }

            // Clear DetailsEntry ViewModel
            var detailsEntryVM = App.GetService<ViewModel_Dunnage_DetailsEntry>();
            if (detailsEntryVM != null)
            {
                detailsEntryVM.PoNumber = string.Empty;
                detailsEntryVM.Location = string.Empty;
                detailsEntryVM.SpecInputs?.Clear();
            }

            // Clear QuantityEntry ViewModel
            var quantityEntryVM = App.GetService<ViewModel_Dunnage_QuantityEntry>();
            if (quantityEntryVM != null)
            {
                quantityEntryVM.Quantity = 1;
            }

            _logger.LogInfo("UI inputs cleared for new entry (loads preserved)", "Review");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing UI inputs: {ex.Message}", ex, "Review");
        }
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

    /// <summary>\n    /// Shows contextual help for review\n    /// </summary>\n    [RelayCommand]\n    private async Task ShowHelpAsync()\n    {\n        await _helpService.ShowHelpAsync(\"Dunnage.Review\");\n    }

    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInfo("Cancelling review, clearing session", "Review");

        // Clear session and return to Mode Selection
        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
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
