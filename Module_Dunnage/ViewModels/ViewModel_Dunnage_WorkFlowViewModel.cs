using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Label entry page - Orchestrates workflow step visibility
/// </summary>
public partial class ViewModel_Dunnage_WorkFlowViewModel
    : ViewModel_Shared_Base,
        IViewModel_HeaderTitleProvider
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_DunnageSettings _dunnageSettings;
    private readonly IService_LabelViewLauncher _labelViewLauncher;
    private readonly IService_Window _windowService;

    public ViewModel_Dunnage_WorkFlowViewModel(
        IService_DunnageWorkflow workflowService,
        IService_DunnageSettings dunnageSettings,
        IService_LabelViewLauncher labelViewLauncher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dunnageSettings = dunnageSettings;
        _labelViewLauncher = labelViewLauncher;
        _windowService = windowService;
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _workflowService.NavigationLockChanged += OnNavigationLockChanged;
        _workflowService.CurrentSession.PropertyChanged += OnCurrentSessionPropertyChanged;

        DunnageLines = new ObservableCollection<Model_DunnageLine>();
        _currentLine = new Model_DunnageLine();
        IsNavigationLocked = _workflowService.IsNavigationLocked;

        // Start the workflow - it will check for default mode and navigate accordingly
        _ = InitializeWorkflowAsync();
    }

    /// <summary>
    /// Initialize the workflow - checks for default mode and navigates to appropriate step
    /// </summary>
    private async Task InitializeWorkflowAsync()
    {
        try
        {
            await _workflowService.StartWorkflowAsync();
            await RefreshClearLabelDataAvailabilityAsync();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(InitializeWorkflowAsync),
                nameof(ViewModel_Dunnage_WorkFlowViewModel)
            );
        }
    }

    public ObservableCollection<Model_DunnageLine> DunnageLines { get; }

    [ObservableProperty]
    private Model_DunnageLine _currentLine;

    #region Step Visibility Properties

    [ObservableProperty]
    private bool _isModeSelectionVisible;

    [ObservableProperty]
    private bool _isTypeSelectionVisible;

    [ObservableProperty]
    private bool _isPartSelectionVisible;

    [ObservableProperty]
    private bool _isQuantityEntryVisible;

    [ObservableProperty]
    private bool _isDetailsEntryVisible;

    [ObservableProperty]
    private bool _isReviewVisible;

    [ObservableProperty]
    private bool _isManualEntryVisible;

    [ObservableProperty]
    private bool _isEditModeVisible;

    [ObservableProperty]
    private bool _isImagePartSearchVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderContextTitle))]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderContextSubtitle))]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderContextImageSource))]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderContextIconKind))]
    private string _currentStepTitle = "Dunnage - Mode Selection";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNavigate))]
    private bool _isNavigationLocked;

    [ObservableProperty]
    private bool _canClearLabelData;

    public string CurrentHeaderTitle => CurrentStepTitle;
    public string? CurrentHeaderContextTitle => GetHeaderContextTitle();

    public string? CurrentHeaderContextSubtitle => GetHeaderContextSubtitle();

    public ImageSource? CurrentHeaderContextImageSource =>
        !IsPartSelectionVisible
            ? null
            : Helper_DunnageImagePaths.CreateImageSource(
                _workflowService.CurrentSession.SelectedType?.ImagePath
            );

    public MaterialIconKind? CurrentHeaderContextIconKind
    {
        get
        {
            if (!IsPartSelectionVisible)
            {
                return null;
            }

            var selectedIcon = _workflowService.CurrentSession.SelectedType?.Icon;
            if (
                !string.IsNullOrWhiteSpace(selectedIcon)
                && Enum.TryParse(selectedIcon, true, out MaterialIconKind kind)
            )
            {
                return kind;
            }

            return MaterialIconKind.PackageVariantClosed;
        }
    }

    public bool CanNavigate => !IsNavigationLocked;

    private string? GetHeaderContextTitle()
    {
        if (IsPartSelectionVisible)
        {
            return string.IsNullOrWhiteSpace(_workflowService.CurrentSession.SelectedTypeName)
                ? "Select Part"
                : $"Select Part - {_workflowService.CurrentSession.SelectedTypeName}";
        }

        if (IsQuantityEntryVisible)
        {
            return "Enter Loads";
        }

        if (IsDetailsEntryVisible)
        {
            return "Enter Details";
        }

        return null;
    }

    private string? GetHeaderContextSubtitle()
    {
        if (IsPartSelectionVisible)
        {
            return "Use the search function to quickly find parts. Parts with a green badge are tracked in inventory.";
        }

        if (IsQuantityEntryVisible)
        {
            return "Set the number of loads and enter the quantity for each load.";
        }

        if (IsDetailsEntryVisible)
        {
            return "Enter the PO number, confirm the location, and review per-load details before saving.";
        }

        return null;
    }

    #endregion

    #region Event Handlers

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        // Hide all steps
        IsModeSelectionVisible = false;
        IsTypeSelectionVisible = false;
        IsPartSelectionVisible = false;
        IsQuantityEntryVisible = false;
        IsDetailsEntryVisible = false;
        IsReviewVisible = false;
        IsManualEntryVisible = false;
        IsEditModeVisible = false;
        IsImagePartSearchVisible = false;

        // Show current step
        switch (_workflowService.CurrentStep)
        {
            case Enum_DunnageWorkflowStep.ModeSelection:
                IsModeSelectionVisible = true;
                CurrentStepTitle = "Dunnage - Mode Selection";
                break;
            case Enum_DunnageWorkflowStep.TypeSelection:
                IsTypeSelectionVisible = true;
                CurrentStepTitle = "Dunnage - Select Type";
                break;
            case Enum_DunnageWorkflowStep.PartSelection:
                IsPartSelectionVisible = true;
                CurrentStepTitle = "Dunnage - Select Part";
                break;
            case Enum_DunnageWorkflowStep.QuantityEntry:
                IsQuantityEntryVisible = true;
                CurrentStepTitle = "Dunnage - Enter Loads";
                break;
            case Enum_DunnageWorkflowStep.DetailsEntry:
                IsDetailsEntryVisible = true;
                CurrentStepTitle = "Dunnage - Enter Details";
                break;
            case Enum_DunnageWorkflowStep.Review:
                IsReviewVisible = true;
                CurrentStepTitle = "Dunnage - Review & Save";
                break;
            case Enum_DunnageWorkflowStep.ManualEntry:
                IsManualEntryVisible = true;
                CurrentStepTitle = "Dunnage - Manual Entry";
                break;
            case Enum_DunnageWorkflowStep.EditMode:
                IsEditModeVisible = true;
                CurrentStepTitle = "Dunnage - Edit Mode";
                break;
            case Enum_DunnageWorkflowStep.ImagePartSearch:
                IsImagePartSearchVisible = true;
                CurrentStepTitle = "Dunnage - Search Parts by Image";
                break;
        }

        NotifyHeaderContextChanged();

        _ = RefreshClearLabelDataAvailabilityAsync();
    }

    private void OnCurrentSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!IsPartSelectionVisible)
        {
            return;
        }

        if (
            string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName == nameof(Model_DunnageSession.SelectedType)
            || e.PropertyName == nameof(Model_DunnageSession.SelectedTypeName)
        )
        {
            NotifyHeaderContextChanged();
        }
    }

    private void NotifyHeaderContextChanged()
    {
        OnPropertyChanged(nameof(CurrentHeaderContextTitle));
        OnPropertyChanged(nameof(CurrentHeaderContextSubtitle));
        OnPropertyChanged(nameof(CurrentHeaderContextImageSource));
        OnPropertyChanged(nameof(CurrentHeaderContextIconKind));
    }

    private async Task RefreshClearLabelDataAvailabilityAsync()
    {
        try
        {
            CanClearLabelData = await _workflowService.HasActiveLabelDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to refresh dunnage label-data availability: {ex.Message}",
                ex
            );
            CanClearLabelData = false;
        }
    }

    private void OnNavigationLockChanged(object? sender, EventArgs e)
    {
        IsNavigationLocked = _workflowService.IsNavigationLocked;
    }

    [RelayCommand]
    private async Task ClearLabelDataAsync(bool clearAllRows = false)
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show dialog: XamlRoot is null");
            await _errorHandler.HandleErrorAsync(
                "Unable to display dialog",
                Enum_ErrorSeverity.Error
            );
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Clear Label Data",
            Content =
                "Are you sure you want to archive and clear the active label queue? All queued rows will be moved to history. This action cannot be undone.",
            PrimaryButtonText = "Clear Label Data",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            IsBusy = true;
            try
            {
                var clearResult = await _workflowService.ClearLabelDataAsync(clearAllRows);
                StatusMessage = clearResult.IsSuccess
                    ? $"Label data cleared — {clearResult.Data} row(s) archived to history."
                    : $"Clear Label Data failed: {clearResult.ErrorMessage}";
                await RefreshClearLabelDataAvailabilityAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
    #endregion

    #region Commands

    [RelayCommand]
    private async Task OpenDunnageLabelAsync()
    {
        await OpenLabelAsync(DunnageSettingsKeys.Labels.DunnageLabelPath, "Dunnage Label");
    }

    [RelayCommand]
    private async Task ReturnToModeSelectionAsync()
    {
        if (_workflowService.IsNavigationLocked)
        {
            return;
        }

        if (_workflowService.HasUnsavedData())
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot is not null)
            {
                var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Change Mode?",
                    Content =
                        "Changing mode will clear the current Dunnage entry. This cannot be undone. Continue?",
                    PrimaryButtonText = "Change Mode",
                    CloseButtonText = "Stay Here",
                    DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                    XamlRoot = xamlRoot,
                };

                MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                    dialog,
                    xamlRoot
                );

                var result = await dialog.ShowAsync();
                if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    return;
                }
            }
        }

        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
    }

    private async Task OpenLabelAsync(string settingsKey, string labelName)
    {
        var executablePath = await _labelViewLauncher.ResolveExecutablePathAsync();
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            await RedirectToSettingsPageAsync(
                typeof(Module_Settings.Core.Views.View_Settings_LabelViewExecutable),
                "LabelView is not configured. Opening LabelView settings."
            );
            return;
        }

        var labelPath = await _dunnageSettings.GetStringAsync(settingsKey);
        if (!_labelViewLauncher.IsLabelFilePathValid(labelPath))
        {
            await RedirectToSettingsPageAsync(
                typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_LabelPaths),
                $"{labelName} path is missing or invalid. Opening Dunnage label settings."
            );
            return;
        }

        var launchResult = await _labelViewLauncher.LaunchLabelAsync(labelPath);
        if (!launchResult.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(launchResult, nameof(OpenLabelAsync));
            return;
        }

        StatusMessage = $"{labelName} opened in LabelView.";
    }

    private async Task RedirectToSettingsPageAsync(Type pageType, string statusMessage)
    {
        var navigationSucceeded = await _windowService.NavigateToSettingsPageAsync(pageType);
        StatusMessage = navigationSucceeded
            ? statusMessage
            : $"{statusMessage} Unable to navigate automatically.";
    }

    #endregion
}
