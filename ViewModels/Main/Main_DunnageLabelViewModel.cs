using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Main;

/// <summary>
/// ViewModel for Dunnage Label entry page - Orchestrates workflow step visibility
/// </summary>
public partial class Main_DunnageLabelViewModel : Shared_BaseViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;

    public Main_DunnageLabelViewModel(
        IService_DunnageWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _workflowService = workflowService;
        _workflowService.StepChanged += OnWorkflowStepChanged;

        DunnageLines = new ObservableCollection<Model_DunnageLine>();
        _currentLine = new Model_DunnageLine();

        // Initialize to Mode Selection
        IsModeSelectionVisible = true;
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
    private string _currentStepTitle = "Mode Selection";

    #endregion

    #region Status Properties

    [ObservableProperty]
    private bool _isStatusOpen;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private Enum_ErrorSeverity _statusSeverity = Enum_ErrorSeverity.Info;

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
                CurrentStepTitle = "Mode Selection";
                break;
            case Enum_DunnageWorkflowStep.TypeSelection:
                IsTypeSelectionVisible = true;
                CurrentStepTitle = "Select Type";
                break;
            case Enum_DunnageWorkflowStep.PartSelection:
                IsPartSelectionVisible = true;
                CurrentStepTitle = "Select Part";
                break;
            case Enum_DunnageWorkflowStep.QuantityEntry:
                IsQuantityEntryVisible = true;
                CurrentStepTitle = "Enter Quantity";
                break;
            case Enum_DunnageWorkflowStep.DetailsEntry:
                IsDetailsEntryVisible = true;
                CurrentStepTitle = "Enter Details";
                break;
            case Enum_DunnageWorkflowStep.Review:
                IsReviewVisible = true;
                CurrentStepTitle = "Review & Save";
                break;
            case Enum_DunnageWorkflowStep.ManualEntry:
                IsManualEntryVisible = true;
                CurrentStepTitle = "Manual Entry";
                break;
            case Enum_DunnageWorkflowStep.EditMode:
                IsEditModeVisible = true;
                CurrentStepTitle = "Edit Mode";
                break;
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
