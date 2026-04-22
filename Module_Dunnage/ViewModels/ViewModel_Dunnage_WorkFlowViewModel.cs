using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
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
    private readonly IService_Window _windowService;

    public ViewModel_Dunnage_WorkFlowViewModel(
        IService_DunnageWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _windowService = windowService;
        _workflowService.StepChanged += OnWorkflowStepChanged;
        _workflowService.NavigationLockChanged += OnNavigationLockChanged;

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
    private string _currentStepTitle = "Dunnage - Mode Selection";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNavigate))]
    private bool _isNavigationLocked;

    public string CurrentHeaderTitle => CurrentStepTitle;
    public bool CanNavigate => !IsNavigationLocked;

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
    }

    private void OnNavigationLockChanged(object? sender, EventArgs e)
    {
        IsNavigationLocked = _workflowService.IsNavigationLocked;
    }

    [RelayCommand]
    private async Task ClearLabelDataAsync()
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
                var clearResult = await _workflowService.ClearLabelDataAsync();
                StatusMessage = clearResult.IsSuccess
                    ? $"Label data cleared — {clearResult.Data} row(s) archived to history."
                    : $"Clear Label Data failed: {clearResult.ErrorMessage}";
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

    #endregion
}
