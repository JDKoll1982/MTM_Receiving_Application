using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Label entry page - Orchestrates workflow step visibility
/// </summary>
public partial class ViewModel_Dunnage_WorkFlowViewModel : ViewModel_Shared_Base
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Window _windowService;

    public ViewModel_Dunnage_WorkFlowViewModel(
        IService_DunnageWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Window windowService,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _windowService = windowService;
        _workflowService.StepChanged += OnWorkflowStepChanged;

        DunnageLines = new ObservableCollection<Model_DunnageLine>();
        _currentLine = new Model_DunnageLine();

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
                nameof(ViewModel_Dunnage_WorkFlowViewModel));
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
    private string _currentStepTitle = "Dunnage - Mode Selection";

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
                CurrentStepTitle = "Dunnage - Enter Quantity";
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
        }
    }

    [RelayCommand]
    private async Task ResetCSVAsync()
    {
        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show dialog: XamlRoot is null");
            await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
            return;
        }

        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Reset CSV Files",
            Content = "Are you sure you want to delete the local and network CSV files? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            // Try to save to DB first
            var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
            if (!saveResult.IsSuccess)
            {
                var warnDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Database Save Failed",
                    Content = $"Failed to save to database: {saveResult.ErrorMessage}\n\nDo you want to proceed with deleting CSV files anyway?",
                    PrimaryButtonText = "Delete Anyway",
                    CloseButtonText = "Cancel",
                    DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                    XamlRoot = xamlRoot
                };

                var warnResult = await warnDialog.ShowAsync();
                if (warnResult != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
                {
                    return;
                }
            }

            var deleteResult = await _workflowService.ResetCSVFilesAsync();
            if (deleteResult.LocalDeleted || deleteResult.NetworkDeleted)
            {
                StatusMessage = "CSV files deleted successfully.";
            }
            else
            {
                StatusMessage = "Failed to delete CSV files or files not found.";
            }
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ReturnToModeSelection()
    {
        _workflowService.ClearSession();
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
    }

    [RelayCommand]
    private async Task AddLineAsync()
    {
        // TODO: Implement when ready
        await Task.CompletedTask;
    }

    #endregion
}

