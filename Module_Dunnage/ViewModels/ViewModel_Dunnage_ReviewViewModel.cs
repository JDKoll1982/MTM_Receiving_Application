using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Review &amp; Save
/// </summary>
public partial class ViewModel_Dunnage_Review : ViewModel_Shared_Base, IResettableViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_Help _helpService;
    private readonly IService_Window _windowService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;

    public ViewModel_Dunnage_Review(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_Help helpService,
        IService_Window windowService,
        IService_ViewModelRegistry viewModelRegistry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dunnageService = dunnageService;
        _helpService = helpService;
        _windowService = windowService;
        _viewModelRegistry = viewModelRegistry;

        // Subscribe to workflow step changes to re-initialize when this step is reached
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _viewModelRegistry.Register(this);
    }

    public void ResetToDefaults()
    {
        SessionLoads = new ObservableCollection<Model_DunnageLoad>();
        CurrentLoad = null;
        CurrentEntryIndex = 1;
        LoadCount = 0;
        CanSave = false;
        IsSuccessMessageVisible = false;
        SuccessMessage = string.Empty;
        IsSingleView = true;
        IsTableView = false;
        CanGoBack = false;
        CanGoNext = false;
        StatusMessage = string.Empty;
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

    public bool HasCurrentVisual => CurrentVisualSource is not null;

    public ImageSource? CurrentVisualSource =>
        CurrentLoad?.PartImageSource ?? CurrentLoad?.TypeImageSource;

    #endregion

    #region Initialization

    /// <summary>
    /// Load session loads for review
    /// </summary>
    public void LoadSessionLoads()
    {
        try
        {
            IsBusy = true;
            var loads = _workflowService.CurrentSession.Loads;
            SessionLoads = new ObservableCollection<Model_DunnageLoad>(loads);

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
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateNavigationButtons()
    {
        CanGoBack = CurrentEntryIndex > 1;
        CanGoNext = CurrentEntryIndex < LoadCount;
        OnPropertyChanged(nameof(CurrentVisualSource));
        OnPropertyChanged(nameof(HasCurrentVisual));
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

            _logger.LogInfo(
                "Navigated to Type Selection for new load, workflow data cleared",
                "Review"
            );
        }
        catch (Exception ex)
        {
            IsBusy = false;
            _logger.LogError($"Error in AddAnotherAsync: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to prepare for new load entry",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
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
                Content =
                    "Current form data will be cleared to start a new entry. Your reviewed loads are preserved. Continue?",
                PrimaryButtonText = "Continue",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot,
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
                session.NumberOfLoads = 1;
                session.Quantity = 0;
                session.LoadQuantities.Clear();
                session.PONumber = string.Empty;
                session.Location = string.Empty;
                // Preserve Loads collection - these are already reviewed
            }

            // Clear UI inputs in connected ViewModels
            ClearUIInputsForNewEntry();

            _logger.LogInfo(
                "Transient workflow data and UI inputs cleared for new entry",
                "Review"
            );
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
        // Transient ViewModels state reset is handled by the transient nature or session clearing.
        // Direct property manipulation via Service Locator is removed.
        _logger.LogInfo("UI inputs cleared via session reset (loads preserved).");
    }

    [RelayCommand]
    private async Task SaveAllAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            _workflowService.SetNavigationLock(true);
            CanSave = false;
            StatusMessage = "Saving loads...";

            await _logger.LogInfoAsync($"Starting SaveAllAsync: {LoadCount} loads to save");

            // Save loads to database
            var saveResult = await _dunnageService.SaveLoadsAsync(SessionLoads.ToList());

            if (!saveResult.Success)
            {
                await _logger.LogErrorAsync(
                    $"Failed to save {LoadCount} loads: {saveResult.ErrorMessage}"
                );
                await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
                return;
            }

            await _logger.LogInfoAsync($"Successfully saved {LoadCount} loads to database");

            SuccessMessage = $"Successfully saved {LoadCount} load(s) to database";
            IsSuccessMessageVisible = true;
            StatusMessage = string.Empty;

            await _logger.LogInfoAsync(
                $"Completed SaveAllAsync: {LoadCount} loads processed successfully"
            );
            await ShowSaveCompleteDialogAsync();
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
            _workflowService.SetNavigationLock(false);
            IsBusy = false;
            CanSave = LoadCount > 0;
        }
    }

    [RelayCommand]
    private void StartNewEntry()
    {
        // Clear session and return to Type Selection for another guided entry.
        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
    }

    [RelayCommand]
    private void ReturnToModeSelectionAfterSave()
    {
        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
    }

    /// <summary>
    /// Shows contextual help for review
    /// </summary>
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.Review");
    }

    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInfo("Cancelling review, clearing session", "Review");

        // Clear session and return to Mode Selection
        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
    }

    private async Task ShowSaveCompleteDialogAsync()
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            _logger.LogWarning("Review success dialog skipped because XamlRoot is null", "Review");
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Dunnage Saved",
            Content = $"Successfully saved {LoadCount} load(s). What would you like to do next?",
            PrimaryButtonText = "Start New Entry",
            SecondaryButtonText = "Mode Selection",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            StartNewEntry();
            return;
        }

        ReturnToModeSelectionAfterSave();
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
