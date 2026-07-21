using System;
using System.Collections.Generic;
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
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Review & Save
/// </summary>
public partial class ViewModel_Dunnage_Review : ViewModel_Shared_Base, IResettableViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_Help _helpService;
    private readonly IService_Window _windowService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;
    private readonly Dictionary<string, (string VendorName, string ErrorMessage)> _poVendorCache =
        new(StringComparer.OrdinalIgnoreCase);
    private int _vendorLookupVersion;

    public ViewModel_Dunnage_Review(
        IService_DunnageWorkflow workflowService,
        IService_MySQL_Dunnage dunnageService,
        IService_InforVisual inforVisualService,
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
        _inforVisualService = inforVisualService;
        _helpService = helpService;
        _windowService = windowService;
        _viewModelRegistry = viewModelRegistry;

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
        CurrentVendorName = string.Empty;
        CurrentVendorErrorMessage = string.Empty;
        StatusMessage = string.Empty;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.Review)
        {
            _ = LoadSessionLoadsAsync();
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
    private bool _isSuccessMessageVisible;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _isSingleView = true;

    [ObservableProperty]
    private bool _isTableView;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCurrentVendorState))]
    private string _currentVendorName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCurrentVendorState))]
    private string _currentVendorErrorMessage = string.Empty;

    public bool HasCurrentVisual => CurrentVisualSource is not null;

    public bool HasCurrentVendorState =>
        string.IsNullOrWhiteSpace(CurrentVendorName) is false
        || string.IsNullOrWhiteSpace(CurrentVendorErrorMessage) is false;

    public ImageSource? CurrentVisualSource =>
        CurrentLoad?.PartImageSource ?? CurrentLoad?.TypeImageSource;

    #endregion

    #region Initialization

    /// <summary>
    /// Load session loads for review
    /// </summary>
    public async Task LoadSessionLoadsAsync()
    {
        try
        {
            IsBusy = true;
            var loads = _workflowService.CurrentSession.Loads;
            SessionLoads = new ObservableCollection<Model_DunnageLoad>(loads);

            foreach (var load in SessionLoads)
            {
                var normalizedPoNumber = Helper_DunnagePoNumber.FormatForEntry(load.PoNumber);
                if (
                    string.Equals(load.PoNumber, normalizedPoNumber, StringComparison.Ordinal)
                    is false
                )
                {
                    load.PoNumber = normalizedPoNumber;
                }
            }

            LoadCount = SessionLoads.Count;
            CanSave = LoadCount > 0;

            if (LoadCount > 0)
            {
                CurrentEntryIndex = 1;
                CurrentLoad = SessionLoads[0];
                UpdateNavigationButtons();
                await RefreshCurrentVendorStateAsync(CurrentLoad);
            }
            else
            {
                ClearCurrentVendorState();
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
            _ = RefreshCurrentVendorStateAsync(CurrentLoad);
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
            _ = RefreshCurrentVendorStateAsync(CurrentLoad);
        }
    }

    #endregion

    private async Task RefreshCurrentVendorStateAsync(Model_DunnageLoad? load)
    {
        var requestVersion = ++_vendorLookupVersion;
        ClearCurrentVendorState();

        if (load is null)
        {
            return;
        }

        var normalizedPoNumber = Helper_DunnagePoNumber.FormatForEntry(load.PoNumber);
        if (string.Equals(load.PoNumber, normalizedPoNumber, StringComparison.Ordinal) is false)
        {
            load.PoNumber = normalizedPoNumber;
        }

        if (!Helper_DunnagePoNumber.IsValidLookupInput(normalizedPoNumber))
        {
            return;
        }

        if (_poVendorCache.TryGetValue(normalizedPoNumber, out var cachedVendorState))
        {
            ApplyVendorState(cachedVendorState.VendorName, cachedVendorState.ErrorMessage);
            return;
        }

        var poResult = await _inforVisualService.GetPOWithPartsAsync(normalizedPoNumber);
        if (requestVersion != _vendorLookupVersion)
        {
            return;
        }

        if (!poResult.IsSuccess)
        {
            _logger.LogWarning(
                $"Unable to load vendor information for Dunnage review PO {normalizedPoNumber}: {poResult.ErrorMessage}",
                "Review"
            );
            return;
        }

        if (poResult.Data is null)
        {
            var errorMessage = $"{normalizedPoNumber} was not found in Infor Visual.";
            _poVendorCache[normalizedPoNumber] = (string.Empty, errorMessage);
            ApplyVendorState(string.Empty, errorMessage);
            return;
        }

        var vendorName = poResult.Data.Vendor?.Trim() ?? string.Empty;
        _poVendorCache[normalizedPoNumber] = (vendorName, string.Empty);
        ApplyVendorState(vendorName, string.Empty);
    }

    private void ApplyVendorState(string vendorName, string errorMessage)
    {
        CurrentVendorName = vendorName;
        CurrentVendorErrorMessage = errorMessage;
    }

    private void ClearCurrentVendorState()
    {
        CurrentVendorName = string.Empty;
        CurrentVendorErrorMessage = string.Empty;
    }

    #region Commands

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

            var saveResult = await _dunnageService.SaveLoadsAsync(SessionLoads.ToList());

            if (!saveResult.Success)
            {
                await _logger.LogErrorAsync(
                    $"Failed to save {LoadCount} loads: {saveResult.ErrorMessage}"
                );
                await _errorHandler.HandleDaoErrorAsync(saveResult, nameof(SaveAllAsync), true);
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
    private async Task StartNewEntryAsync()
    {
        await _workflowService.StartWorkflowAsync();
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

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await StartNewEntryAsync();
            return;
        }

        ReturnToModeSelectionAfterSave();
    }

    #endregion

    #region Help Content Helpers

    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

    public string GetTip(string key) => _helpService.GetTip(key);

    #endregion
}
