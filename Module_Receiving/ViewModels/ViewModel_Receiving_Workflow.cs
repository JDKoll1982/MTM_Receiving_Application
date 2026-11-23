using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.UI;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.Views;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using InfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Workflow
        : ViewModel_Shared_Base,
            IViewModel_HeaderTitleProvider,
            IViewModel_HeaderStepBarProvider,
            IViewModel_ShellActionProvider
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;
        private readonly IService_LabelViewLauncher _labelViewLauncher;
        private readonly IService_ReceivingSettings _receivingSettings;
        private readonly IService_ViewModelRegistry _viewModelRegistry;
        private readonly IService_Tool_LotMarking _lotMarking;
        private readonly IService_UserPreferences _userPreferencesService;
        private readonly IService_UserSessionManager _sessionManager;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]
        [NotifyPropertyChangedFor(nameof(CurrentHeaderContextSubtitle))]
        private string _currentStepTitle = "Receiving";

        public string CurrentHeaderTitle => CurrentStepTitle;
        public string? CurrentHeaderContextSubtitle =>
            _workflowService.CurrentStep switch
            {
                Enum_ReceivingWorkflowStep.POEntry =>
                    "Enter a PO number or switch to the non-PO flow before loading parts.",
                Enum_ReceivingWorkflowStep.PartSelection =>
                    "Choose the receiving part that should be used for the loads you are about to create.",
                Enum_ReceivingWorkflowStep.LoadEntry =>
                    "Confirm the number of loads and review the selected part context before continuing.",
                Enum_ReceivingWorkflowStep.WeightQuantityEntry =>
                    "Enter the weight and quantity details for each generated load.",
                Enum_ReceivingWorkflowStep.HeatLotEntry =>
                    "Capture heat and lot information for each load before moving to package type.",
                Enum_ReceivingWorkflowStep.PackageTypeEntry =>
                    "Set the package type for each load so labels and saved records use the right packaging.",
                Enum_ReceivingWorkflowStep.Review =>
                    "Review every generated load before saving labels and database records.",
                Enum_ReceivingWorkflowStep.ManualEntry =>
                    "Enter or revise rows directly when the guided workflow is not the best fit.",
                Enum_ReceivingWorkflowStep.EditMode =>
                    "Inspect and update existing receiving rows directly from the edit grid.",
                Enum_ReceivingWorkflowStep.Saving =>
                    "Saving labels, queue files, and receiving records now.",
                Enum_ReceivingWorkflowStep.Complete =>
                    "Receiving save work is complete. Start a new entry when you are ready.",
                _ => null,
            };

        /// <summary>
        /// Called when CurrentStepTitle changes - ensures MainWindow header updates
        /// </summary>
        partial void OnCurrentStepTitleChanged(string value)
        {
            _logger.LogInfo($"CurrentStepTitle changed to: {value}");
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isManualEntryVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isEditModeVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isPOEntryVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isReceiverSelectionVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isPartSelectionVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isLoadEntryVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isWeightQuantityEntryVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isHeatLotEntryVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isPackageTypeEntryVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isReviewVisible;

        [ObservableProperty]
        private bool _isSavingVisible;

        [ObservableProperty]
        private bool _isCompleteVisible;

        [ObservableProperty]
        private Model_SaveResult? _lastSaveResult;

        /// <summary>
        /// The labels from the receipt that was just saved whose part number is on the list of parts that
        /// get the lot shape treatment, so the finish page can say which printed tags still need their
        /// colored symbol.
        /// </summary>
        public ObservableCollection<Model_ReceivingLotMarkingNeed> LotMarkingNeeds { get; } = [];

        /// <summary>True when the receipt that was just saved carries at least one label needing the overlay.</summary>
        public bool HasLotMarkingNeeds => LotMarkingNeeds.Count > 0;

        /// <summary>
        /// The labels needing the overlay, grouped by part and lot so the finish page reads at a glance.
        /// </summary>
        public string LotMarkingNeedsText =>
            Helper_ReceivingLotMarkingNeeds.Summarize(LotMarkingNeeds);

        /// <summary>How many labels the overlay has to be run for.</summary>
        public string LotMarkingNeedsCountText =>
            LotMarkingNeeds.Count == 1 ? "1 label" : $"{LotMarkingNeeds.Count} labels";

        [ObservableProperty]
        private string _saveProgressMessage = "Initializing...";

        private bool _isNextButtonEnabled = true;

        /// <summary>
        /// Gets a value indicating whether the workflow Next button is currently available.
        /// </summary>
        public bool IsNextButtonEnabled
        {
            get => _isNextButtonEnabled;
            private set
            {
                if (SetProperty(ref _isNextButtonEnabled, value))
                {
                    RefreshShellActions();
                }
            }
        }

        public bool ShowWorkflowBackButton =>
            IsPartSelectionVisible
            || IsLoadEntryVisible
            || IsWeightQuantityEntryVisible
            || IsHeatLotEntryVisible
            || IsPackageTypeEntryVisible
            || IsReviewVisible;

        public bool ShowWorkflowNextButton =>
            IsPOEntryVisible
            || IsPartSelectionVisible
            || IsLoadEntryVisible
            || IsWeightQuantityEntryVisible
            || IsHeatLotEntryVisible
            || IsPackageTypeEntryVisible;

        public bool ShowWorkflowNavigationButtons =>
            ShowWorkflowBackButton || ShowWorkflowNextButton;

        /// <summary>The order of the guided walk, which is also the order the header step bar draws.</summary>
        private static readonly (Enum_ReceivingWorkflowStep Step, string Label)[] GuidedTrail =
        [
            (Enum_ReceivingWorkflowStep.POEntry, "Enter PO"),
            (Enum_ReceivingWorkflowStep.PartSelection, "Part"),
            (Enum_ReceivingWorkflowStep.LoadEntry, "Loads"),
            (Enum_ReceivingWorkflowStep.WeightQuantityEntry, "Weight/Qty"),
            (Enum_ReceivingWorkflowStep.HeatLotEntry, "Heat/Lot"),
            (Enum_ReceivingWorkflowStep.PackageTypeEntry, "Package"),
            (Enum_ReceivingWorkflowStep.Review, "Review"),
        ];

        /// <summary>
        /// The order of the walk when it started from a receiver row. The receiver list replaces the
        /// PO and part doors, so the trail starts at Receivers and draws the same steps after it.
        /// </summary>
        private static readonly (Enum_ReceivingWorkflowStep Step, string Label)[] ByReceiverTrail =
        [
            (Enum_ReceivingWorkflowStep.ReceiverSelection, "Receivers"),
            (Enum_ReceivingWorkflowStep.LoadEntry, "Loads"),
            (Enum_ReceivingWorkflowStep.WeightQuantityEntry, "Weight/Qty"),
            (Enum_ReceivingWorkflowStep.HeatLotEntry, "Heat/Lot"),
            (Enum_ReceivingWorkflowStep.PackageTypeEntry, "Package"),
            (Enum_ReceivingWorkflowStep.Review, "Review"),
        ];

        /// <summary>True while the clerk is somewhere in the guided walk.</summary>
        public bool IsHeaderStepBarVisible => HeaderStepBarIndex >= 0;

        /// <summary>The trail the current walk draws, by-receiver or guided.</summary>
        private (Enum_ReceivingWorkflowStep Step, string Label)[] ActiveTrail =>
            _workflowService.IsByReceiverFlow ? ByReceiverTrail : GuidedTrail;

        /// <summary>The walk's steps, in order, for the main-window header step bar.</summary>
        public IReadOnlyList<string> HeaderStepBarSteps =>
            ActiveTrail.Select(entry => entry.Label).ToArray();

        /// <summary>
        /// The step the clerk is on, or -1 outside the walk so the header hides the bar rather than
        /// leaving a stale highlight behind.
        /// </summary>
        public int HeaderStepBarIndex =>
            Array.FindIndex(ActiveTrail, entry => entry.Step == _workflowService.CurrentStep);

        /// <summary>
        /// Re-announces the step bar state after a step change, so the header re-reads the index and
        /// the walk's visibility.
        /// </summary>
        private void RefreshHeaderStepBar()
        {
            OnPropertyChanged(nameof(HeaderStepBarIndex));
            OnPropertyChanged(nameof(HeaderStepBarSteps));
            OnPropertyChanged(nameof(IsHeaderStepBarVisible));
        }

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _workflowHelpText = "Help";

        [ObservableProperty]
        private string _workflowBackText = "Back";

        [ObservableProperty]
        private string _workflowNextText = "Next";

        [ObservableProperty]
        private string _workflowResetLabelDataText = "Clear Label Data";

        [ObservableProperty]
        private string _shellGuidedText = "Guided";

        [ObservableProperty]
        private string _shellByReceiverText = "By Receiver";

        [ObservableProperty]
        private string _shellManualText = "Manual";

        [ObservableProperty]
        private string _shellEditText = "Edit";

        [ObservableProperty]
        private string _shellOverlayText = "Overlay";

        [ObservableProperty]
        private string _shellGuidedDefaultTooltipText = "make Guided my default";

        [ObservableProperty]
        private string _shellByReceiverDefaultTooltipText = "make By Receiver my default";

        [ObservableProperty]
        private string _shellManualDefaultTooltipText = "make Manual my default";

        [ObservableProperty]
        private string _shellEditDefaultTooltipText = "make Edit my default";

        [ObservableProperty]
        private string _shellDefaultBadgeText = "Default";

        [ObservableProperty]
        private bool _canClearLabelData;

        [ObservableProperty]
        private string _completionSuccessTitleText = "Success!";

        [ObservableProperty]
        private string _completionFailureTitleText = "Save Failed";

        [ObservableProperty]
        private string _completionLoadsSavedSuffixText = " loads saved successfully.";

        [ObservableProperty]
        private string _completionSaveDetailsTitleText = "Save Details:";

        [ObservableProperty]
        private string _completionLabelQueueLabelText = "Label Queue:";

        [ObservableProperty]
        private string _completionArchiveQueueLabelText = "Archive Queue:";

        [ObservableProperty]
        private string _completionLabelDataLabelText = "Label Data:";

        [ObservableProperty]
        private string _completionDatabaseLabelText = "Database:";

        [ObservableProperty]
        private string _completionSavedText = "Saved";

        [ObservableProperty]
        private string _completionFailedText = "Failed";

        [ObservableProperty]
        private string _completionStartNewEntryText = "Start New Entry";

        [ObservableProperty]
        private double _saveProgressValue = 0;

        [ObservableProperty]
        private Microsoft.UI.Xaml.UIElement? _helpContent;

        private bool _isSaving = false;
        private readonly IService_Dispatcher _dispatcherService;
        private readonly IService_Window _windowService;
        private readonly System.Collections.Generic.Dictionary<
            Enum_ReceivingWorkflowStep,
            string
        > _stepTitles = new();
        private Enum_ReceivingWorkflowStep _startNewEntryStep = Enum_ReceivingWorkflowStep.POEntry;

        public ViewModel_Receiving_Workflow(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Dispatcher dispatcherService,
            IService_Window windowService,
            IService_Help helpService,
            IService_LabelViewLauncher labelViewLauncher,
            IService_ReceivingSettings receivingSettings,
            IService_ViewModelRegistry viewModelRegistry,
            IService_Tool_LotMarking lotMarking,
            IService_UserPreferences userPreferencesService,
            IService_UserSessionManager sessionManager,
            IService_Notification notificationService
        )
            : base(errorHandler, logger, notificationService)
        {
            _dispatcherService = dispatcherService;
            _windowService = windowService;
            _workflowService = workflowService;
            _helpService = helpService;
            _labelViewLauncher = labelViewLauncher;
            _receivingSettings = receivingSettings;
            _viewModelRegistry = viewModelRegistry;
            _lotMarking = lotMarking ?? throw new ArgumentNullException(nameof(lotMarking));
            _userPreferencesService =
                userPreferencesService
                ?? throw new ArgumentNullException(nameof(userPreferencesService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _viewModelRegistry.Register(this);
            _workflowService.StepChanged += OnWorkflowStepChanged;
            _workflowService.StatusMessageRaised += (_, message) => ShowStatus(message);

            _ = InitializeWorkflowAsync();
            _ = InitializeTextAsync();

            // Initialize visibility based on current step
            OnWorkflowStepChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Initialize the workflow to apply the default mode and clear any stale draft state.
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
                    nameof(ViewModel_Receiving_Workflow)
                );
            }
        }

        private async Task InitializeTextAsync()
        {
            try
            {
                WorkflowHelpText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WorkflowHelp
                );
                WorkflowBackText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WorkflowBack
                );
                WorkflowNextText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WorkflowNext
                );
                WorkflowResetLabelDataText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WorkflowResetLabelData
                );
                ShellGuidedText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellGuided
                );
                ShellByReceiverText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellByReceiver
                );
                ShellManualText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellManual
                );
                ShellEditText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellEdit
                );
                ShellOverlayText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellOverlay
                );
                ShellGuidedDefaultTooltipText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellGuidedDefaultTooltip
                );
                ShellByReceiverDefaultTooltipText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellByReceiverDefaultTooltip
                );
                ShellManualDefaultTooltipText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellManualDefaultTooltip
                );
                ShellEditDefaultTooltipText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellEditDefaultTooltip
                );
                ShellDefaultBadgeText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.ShellDefault
                );

                CompletionSuccessTitleText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionSuccessTitle
                );
                CompletionFailureTitleText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionFailureTitle
                );
                CompletionLoadsSavedSuffixText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionLoadsSavedSuffix
                );
                CompletionSaveDetailsTitleText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionSaveDetailsTitle
                );
                CompletionLabelQueueLabelText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionLabelQueueLabel
                );
                CompletionArchiveQueueLabelText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionArchiveQueueLabel
                );
                CompletionLabelDataLabelText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionLabelDataLabel
                );
                CompletionDatabaseLabelText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionDatabaseLabel
                );
                CompletionSavedText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionSaved
                );
                CompletionFailedText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionFailed
                );
                CompletionStartNewEntryText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.CompletionStartNewEntry
                );

                SaveProgressMessage = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Workflow.SaveProgressInitializing
                );

                _dispatcherService.TryEnqueue(() =>
                {
                    if (_stepTitles.TryGetValue(_workflowService.CurrentStep, out var title))
                    {
                        CurrentStepTitle = title;
                    }
                });

                RefreshShellActions();
            }
            catch
            {
                // fall back to existing hardcoded defaults already set in properties
            }
        }

        private void OnWorkflowStepChanged(object? sender, EventArgs e)
        {
            _logger.LogInfo(
                $"StepChanged event received in ViewModel. Current step: {_workflowService.CurrentStep}. Updating visibility."
            );

            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Saving && !_isSaving)
            {
                _logger.LogInfo(
                    $"Step is Saving and not currently saving. Enqueuing PerformSaveAsync via Dispatcher."
                );
                _dispatcherService.TryEnqueue(async () =>
                {
                    await PerformSaveAsync();
                });
            }
            else if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Saving && _isSaving)
            {
                _logger.LogWarning(
                    "Step is Saving but already in progress. Skipping duplicate save."
                );
            }

            UpdateStartNewEntryTarget(_workflowService.CurrentStep);

            // Hide all steps
            IsManualEntryVisible = false;
            IsEditModeVisible = false;
            IsPOEntryVisible = false;
            IsReceiverSelectionVisible = false;
            IsPartSelectionVisible = false;
            IsLoadEntryVisible = false;
            IsWeightQuantityEntryVisible = false;
            IsHeatLotEntryVisible = false;
            IsPackageTypeEntryVisible = false;
            IsReviewVisible = false;
            IsSavingVisible = false;
            IsCompleteVisible = false;

            // Show current step and set title (using hardcoded defaults like Dunnage does)
            switch (_workflowService.CurrentStep)
            {
                case Enum_ReceivingWorkflowStep.ManualEntry:
                    IsManualEntryVisible = true;
                    CurrentStepTitle = "Receiving - Manual Entry";
                    break;
                case Enum_ReceivingWorkflowStep.EditMode:
                    IsEditModeVisible = true;
                    CurrentStepTitle = "Receiving - Edit Mode";
                    break;
                case Enum_ReceivingWorkflowStep.POEntry:
                    IsPOEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter PO Number";
                    break;
                case Enum_ReceivingWorkflowStep.ReceiverSelection:
                    IsReceiverSelectionVisible = true;
                    CurrentStepTitle = "Receiving - By Receiver";
                    break;
                case Enum_ReceivingWorkflowStep.PartSelection:
                    IsPartSelectionVisible = true;
                    CurrentStepTitle = "Receiving - Select Part";
                    break;
                case Enum_ReceivingWorkflowStep.LoadEntry:
                    IsLoadEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter Load Information";
                    break;
                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
                    IsWeightQuantityEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter Weight & Quantity";
                    break;
                case Enum_ReceivingWorkflowStep.HeatLotEntry:
                    IsHeatLotEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter Heat & Lot";
                    break;
                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
                    IsPackageTypeEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter Package Type";
                    break;
                case Enum_ReceivingWorkflowStep.Review:
                    IsReviewVisible = true;
                    CurrentStepTitle = "Receiving - Review & Save";
                    break;
                case Enum_ReceivingWorkflowStep.Saving:
                    IsSavingVisible = true;
                    CurrentStepTitle = "Receiving - Saving";
                    break;
                case Enum_ReceivingWorkflowStep.Complete:
                    IsCompleteVisible = true;
                    CurrentStepTitle = "Receiving - Complete";
                    break;
            }

            // Help content relies on WinUI factories and can be unavailable in unit-test contexts.
            try
            {
                HelpContent = Helper_WorkflowHelpContentGenerator.GenerateHelpContent(
                    _workflowService.CurrentStep
                );
            }
            catch (COMException ex)
            {
                HelpContent = null;
                _logger.LogWarning(
                    $"Skipping workflow help generation for {_workflowService.CurrentStep}: {ex.Message}"
                );
            }

            _logger.LogInfo(
                $"Visibility updated. Current Step: {_workflowService.CurrentStep}, Title: {CurrentStepTitle}"
            );

            UpdateNextButtonEnabled();
            RefreshHeaderStepBar();
            RefreshShellActions();
            _ = RefreshClearLabelDataAvailabilityAsync();
        }

        public async Task RefreshClearLabelDataAvailabilityAsync()
        {
            try
            {
                CanClearLabelData = await _workflowService.HasActiveLabelDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Failed to refresh receiving label-data availability: {ex.Message}",
                    ex
                );
                CanClearLabelData = false;
            }
        }

        private void UpdateNextButtonEnabled()
        {
            // The part page lists the parts of the PO that was actually loaded, so the PO step can
            // only move on once that load produced a list - a typed-in number alone leaves the next
            // page empty. The part page is the one that cannot move on without a part.
            if (IsPartSelectionVisible)
            {
                IsNextButtonEnabled = _workflowService.CurrentPart is not null;
            }
            else if (IsPOEntryVisible)
            {
                IsNextButtonEnabled = _workflowService.IsNonPOItem
                    ? _workflowService.CurrentPart is not null
                    : _workflowService.IsPoPartListLoaded
                        && !string.IsNullOrWhiteSpace(_workflowService.CurrentPONumber);
            }
            else
            {
                IsNextButtonEnabled = true;
            }

            NotifyWorkflowCommandStates();
        }

        public void RefreshNextButtonEnabled()
        {
            UpdateNextButtonEnabled();
        }

        /// <summary>
        /// Mirrors the workflow Next button so the Next shortcut cannot advance a step that the
        /// button keeps disabled, such as PO entry without a selected part.
        /// </summary>
        private bool CanGoToNextStep() => ShowWorkflowNextButton && IsNextButtonEnabled;

        /// <summary>
        /// Mirrors the workflow Back button visibility so the Back shortcut cannot rewind from
        /// steps that do not offer the button, such as Saving or Complete.
        /// </summary>
        private bool CanGoToPreviousStep() => ShowWorkflowBackButton;

        private void NotifyWorkflowCommandStates()
        {
            NextStepCommand.NotifyCanExecuteChanged();
            PreviousStepCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextStep))]
        private async Task NextStepAsync()
        {
            try
            {
                _logger.LogInfo("NextStepAsync command triggered.");

                if (!await EnsureReceivingNonPoReferenceAsync())
                {
                    return;
                }

                if (!await ConfirmReceiverQuantityAsync())
                {
                    return;
                }

                // Removed Task.Yield() to avoid context switching issues
                // await Task.Yield();

                var result = await _workflowService.AdvanceToNextStepAsync();
                _logger.LogInfo(
                    $"AdvanceToNextStepAsync returned. Success: {result.Success}, Step: {_workflowService.CurrentStep}"
                );

                if (!result.Success)
                {
                    if (result.ValidationErrors.Count > 0)
                    {
                        await _errorHandler.HandleErrorAsync(
                            string.Join("\n", result.ValidationErrors),
                            Enum_ErrorSeverity.Error
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in NextStepAsync: {ex.Message}", ex);
                await _errorHandler.HandleErrorAsync(
                    $"An error occurred: {ex.Message}",
                    Enum_ErrorSeverity.Error
                );
            }
        }

        private async Task<bool> EnsureReceivingNonPoReferenceAsync()
        {
            if (_workflowService.CurrentStep != Enum_ReceivingWorkflowStep.PackageTypeEntry)
            {
                return true;
            }

            if (!_workflowService.IsNonPOItem)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(_workflowService.CurrentPONumber) is false)
            {
                return true;
            }

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                await _errorHandler.HandleErrorAsync(
                    "Unable to open the non-PO reference dialog.",
                    Enum_ErrorSeverity.Error
                );
                return false;
            }

            var nonPoDialog = new View_Receiving_Dialog_NonPOEntry(
                _workflowService.CurrentPart?.PartID
            )
            {
                XamlRoot = xamlRoot,
            };

            await nonPoDialog.ShowAsync();

            if (nonPoDialog.Result is null)
            {
                return false;
            }

            ApplyReceivingNonPoReference(nonPoDialog.Result);
            return true;
        }

        private void ApplyReceivingNonPoReference(string reference)
        {
            _workflowService.CurrentPONumber = reference;
            _workflowService.CurrentSession.PoNumber = reference;

            foreach (var load in _workflowService.CurrentSession.Loads)
            {
                load.PoNumber = reference;
            }
        }

        private async Task PerformSaveAsync()
        {
            if (_isSaving)
            {
                _logger.LogInfo("PerformSaveAsync called but already saving. Ignoring.");
                return;
            }
            _isSaving = true;

            try
            {
                _logger.LogInfo("PerformSaveAsync started.");

                // Update UI immediately
                SaveProgressMessage = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Workflow.SaveProgressSavingLabelData
                );
                SaveProgressValue = 30;

                // Perform save
                var messageProgress = new Progress<string>(msg =>
                {
                    _logger.LogInfo($"Save progress message: {msg}");
                    SaveProgressMessage = msg;
                });
                var percentProgress = new Progress<int>(pct =>
                {
                    _logger.LogInfo($"Save progress percent: {pct}");
                    SaveProgressValue = pct;
                });

                _logger.LogInfo("Calling _workflowService.SaveSessionAsync...");

                // Held before the save, because the workflow clears the session once the receipt is
                // written and the finish page still needs to name the labels that need the overlay.
                var savedLoads = _workflowService.CurrentSession?.Loads.ToList() ?? [];

                LastSaveResult = await _workflowService.SaveSessionAsync(
                    messageProgress,
                    percentProgress
                );
                _logger.LogInfo($"SaveSessionAsync returned. Success: {LastSaveResult.Success}");

                await RefreshLotMarkingNeedsAsync(savedLoads, LastSaveResult.Success);

                // Advance to Complete step
                await _workflowService.AdvanceToNextStepAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PerformSaveAsync: {ex.Message}", ex);
                await _errorHandler.HandleErrorAsync(
                    $"Save failed: {ex.Message}",
                    Enum_ErrorSeverity.Error
                );
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// Works out which of the labels that were just saved still need their colored symbol, so the
        /// finish page can offer the overlay for them. Only a saved receipt can be overlaid, so a failed
        /// save shows no reminder rather than sending the operator after tags that were never queued.
        /// </summary>
        private async Task RefreshLotMarkingNeedsAsync(
            IReadOnlyList<Model_ReceivingLoad> savedLoads,
            bool saveSucceeded
        )
        {
            LotMarkingNeeds.Clear();

            try
            {
                if (saveSucceeded)
                {
                    foreach (var need in await Helper_ReceivingLotMarkingNeeds.BuildAsync(
                        _lotMarking,
                        savedLoads
                    ))
                    {
                        LotMarkingNeeds.Add(need);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Receiving: the lot marking part list could not be read, so no overlay reminder is shown. {ex.Message}"
                );
                LotMarkingNeeds.Clear();
            }

            OnPropertyChanged(nameof(HasLotMarkingNeeds));
            OnPropertyChanged(nameof(LotMarkingNeedsText));
            OnPropertyChanged(nameof(LotMarkingNeedsCountText));
        }

        /// <summary>
        /// Set by the view's code-behind so the finish page can open the label overlay window without the
        /// ViewModel taking a dependency on UI types. The window is opened on the lots named here.
        /// </summary>
        public Func<IReadOnlyList<(string PartId, string HeatLotNumber)>, Task>? RequestOpenLabelOverlay { get; set; }

        /// <summary>
        /// Opens the label overlay on the labels this receipt left waiting for their colored symbol.
        /// </summary>
        /// <remarks>
        /// Nothing is saved here: this page only shows after a successful save, so the tags are already in
        /// the queue and the window can go straight to them.
        /// </remarks>
        [RelayCommand]
        private async Task OpenLotMarkingOverlayAsync()
        {
            if (LotMarkingNeeds.Count == 0 || RequestOpenLabelOverlay is null)
            {
                return;
            }

            var lots = LotMarkingNeeds.Select(need => (need.PartId, need.HeatLotNumber)).ToList();

            try
            {
                _logger.LogInfo(
                    $"Receiving: opening the label overlay for {lots.Count} saved label(s) that need the colored symbol."
                );

                await RequestOpenLabelOverlay(lots);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(
                    ex,
                    Enum_ErrorSeverity.Medium,
                    nameof(OpenLotMarkingOverlayAsync),
                    nameof(ViewModel_Receiving_Workflow)
                );
            }
        }

        [RelayCommand]
        private async Task OpenReceivingLabelAsync()        {
            await OpenLabelAsync(
                ReceivingSettingsKeys.Labels.ReceivingLabelPath,
                "Receiving Label"
            );
        }

        [RelayCommand]
        private async Task OpenMiniReceivingLabelAsync()
        {
            await OpenLabelAsync(
                ReceivingSettingsKeys.Labels.MiniReceivingLabelPath,
                "Mini-Receiving Label"
            );
        }

        [RelayCommand]
        private async Task StartNewEntryAsync()
        {
            await _workflowService.ResetWorkflowAsync();
            _workflowService.GoToStep(_startNewEntryStep);
        }

        // The strip binds this command with no parameter, and a command that takes a value type
        // reports CanExecute(null) as false, which would leave the button disabled forever. Keeping
        // a parameterless command is what lets the strip enable it from CanClearLabelData alone.
        [RelayCommand]
        private Task ResetLabelDataAsync() => ResetLabelDataCoreAsync(clearAllRows: false);

        /// <summary>Clears the active label data and the archive queue as well.</summary>
        [RelayCommand]
        private Task ResetLabelDataAllAsync() => ResetLabelDataCoreAsync(clearAllRows: true);

        private async Task ResetLabelDataCoreAsync(bool clearAllRows)
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync(
                    await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Messages.ErrorUnableToDisplayDialog
                    ),
                    Enum_ErrorSeverity.Error
                );
                return;
            }

            var dialog = new ContentDialog
            {
                Title = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Workflow.ResetLabelDataDialogTitle
                ),
                Content = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Workflow.ResetLabelDataDialogContent
                ),
                PrimaryButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Workflow.ResetLabelDataDialogDelete
                ),
                CloseButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Workflow.ResetLabelDataDialogCancel
                ),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot,
            };

            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                dialog,
                xamlRoot
            );

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Clear active label data by moving it to history.
                var deleteResult = await _workflowService.ResetLabelDataAsync(clearAllRows);
                if (deleteResult.LabelQueueCleared || deleteResult.ArchiveQueueCleared)
                {
                    foreach (
                        var editModeViewModel in _viewModelRegistry.GetViewModels<ViewModel_Receiving_EditMode>()
                    )
                    {
                        editModeViewModel.HandleCurrentLabelQueueCleared();
                    }

                    ShowStatus(
                        await _receivingSettings.GetStringAsync(
                            ReceivingSettingsKeys.Workflow.StatusLabelDataClearedSuccess
                        ),
                        InfoBarSeverity.Success
                    );
                    await RefreshClearLabelDataAvailabilityAsync();
                }
                else
                {
                    ShowStatus(
                        deleteResult.ArchiveQueueError
                            ?? deleteResult.LabelQueueError
                            ?? await _receivingSettings.GetStringAsync(
                                ReceivingSettingsKeys.Workflow.StatusLabelDataClearedFailed
                            ),
                        InfoBarSeverity.Warning
                    );
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousStep))]
        private void PreviousStep()
        {
            var result = _workflowService.GoToPreviousStep();
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

            var labelPath = await _receivingSettings.GetStringAsync(settingsKey);
            if (!_labelViewLauncher.IsLabelFilePathValid(labelPath))
            {
                await RedirectToSettingsPageAsync(
                    typeof(Module_Settings.Core.Views.View_Settings_LabelViewExecutable),
                    $"{labelName} path is missing or invalid. Opening LabelView settings."
                );
                return;
            }

            var launchResult = await _labelViewLauncher.LaunchLabelAsync(labelPath);
            if (!launchResult.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(launchResult, nameof(OpenLabelAsync));
                return;
            }

            ShowStatus($"{labelName} opened in LabelView.", InfoBarSeverity.Success);
        }

        private async Task RedirectToSettingsPageAsync(Type pageType, string statusMessage)
        {
            var navigationSucceeded = await _windowService.NavigateToSettingsPageAsync(pageType);
            if (navigationSucceeded)
            {
                ShowStatus(statusMessage, InfoBarSeverity.Warning);
                return;
            }

            await _errorHandler.HandleErrorAsync(
                "Unable to open the required settings page.",
                Enum_ErrorSeverity.Warning
            );
        }
        /// <summary>
        /// Shows contextual help for current workflow step
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            // Help topics are keyed per step (Receiving.POEntry, Receiving.Review, ...); there is no
            // single "Receiving.Workflow" topic, so ask for the step the operator is actually on.
            await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
        }

        #region Shell action bar

        private Model_ShellActionBarState _shellActions = Model_ShellActionBarState.Empty;
        private string _activeMode = "guided";
        private ViewModel_Receiving_ManualEntry? _manualEntryViewModel;
        private ViewModel_Receiving_EditMode? _editModeViewModel;
        private ViewModel_Receiving_Review? _reviewViewModel;
        private EventHandler? _editModeSaveCanExecuteChangedHandler;

        /// <summary>
        /// Gets the snapshot the shared shell action bar renders.
        /// </summary>
        public Model_ShellActionBarState ShellActions
        {
            get => _shellActions;
            private set
            {
                if (ReferenceEquals(_shellActions, value))
                {
                    return;
                }

                _shellActions = value;
                OnPropertyChanged(nameof(ShellActions));
            }
        }

        /// <summary>
        /// Set by the view's code-behind so the Overlay control can open the label overlay window
        /// without this view model taking a dependency on UI types.
        /// </summary>
        public Func<Task>? RequestOpenLabelOverlayWindow { get; set; }

        /// <summary>
        /// Hands the hosted mode view models over so their save verbs can occupy the shell's Back
        /// and Next slots, since only the page can see them.
        /// </summary>
        /// <param name="manualEntry">The hosted Manual Entry view model.</param>
        /// <param name="editMode">The hosted Edit Mode view model.</param>
        /// <param name="review">The hosted Review and Save view model.</param>
        public void AttachShellActionHosts(
            ViewModel_Receiving_ManualEntry manualEntry,
            ViewModel_Receiving_EditMode editMode,
            ViewModel_Receiving_Review review
        )
        {
            if (_editModeViewModel is not null && _editModeSaveCanExecuteChangedHandler is not null)
            {
                _editModeViewModel.SaveCommand.CanExecuteChanged -=
                    _editModeSaveCanExecuteChangedHandler;
            }

            _manualEntryViewModel = manualEntry;
            _editModeViewModel = editMode;
            _reviewViewModel = review;

            // Edit Mode only allows saving once its grid has rows, and it announces that change
            // long after the step arrived, so the Next slot has to be re-read when it does.
            _editModeSaveCanExecuteChangedHandler = (_, _) =>
                _dispatcherService.TryEnqueue(RefreshShellActions);
            _editModeViewModel.SaveCommand.CanExecuteChanged += _editModeSaveCanExecuteChangedHandler;

            RefreshShellActions();
        }

        /// <summary>
        /// Persists the signed-in user's default receiving mode. Ticking a default checkbox is the
        /// only caller, so this never changes the active page.
        /// </summary>
        /// <param name="storedModeValue">guided, manual, edit, or null to clear the default.</param>
        public void SetDefaultMode(string? storedModeValue) =>
            _ = SetDefaultModeAsync(storedModeValue);

        [RelayCommand]
        private async Task EnterGuidedModeAsync()
        {
            if (IsGuidedWalkStep(_workflowService.CurrentStep) && !_workflowService.IsByReceiverFlow)
            {
                // Already in the guided walk: pressing Guided must not restart it. A by-receiver
                // walk shares the guided steps, so switching back must still open the PO door.
                return;
            }

            if (await ConfirmModeChangeAsync())
            {
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
            }
        }

        [RelayCommand]
        private async Task EnterByReceiverModeAsync()
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.ReceiverSelection)
            {
                return;
            }

            if (await ConfirmModeChangeAsync())
            {
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ReceiverSelection);
            }
        }

        [RelayCommand]
        private async Task EnterManualModeAsync()
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.ManualEntry)
            {
                return;
            }

            if (await ConfirmModeChangeAsync())
            {
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
            }
        }

        [RelayCommand]
        private async Task EnterEditModeAsync()
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.EditMode)
            {
                return;
            }

            if (await ConfirmModeChangeAsync())
            {
                await _workflowService.ResetWorkflowAsync();
                _workflowService.RequestedEditDataSource = Enum_DataSourceType.CurrentLabels;
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.EditMode);
            }
        }

        private static bool IsGuidedWalkStep(Enum_ReceivingWorkflowStep step) =>
            Array.Exists(GuidedTrail, entry => entry.Step == step);

        [RelayCommand]
        private async Task OpenLabelOverlayWindowAsync()
        {
            if (RequestOpenLabelOverlayWindow is null)
            {
                return;
            }

            await RequestOpenLabelOverlayWindow();
        }

        partial void OnCanClearLabelDataChanged(bool value) => RefreshShellActions();

        private void RefreshShellActions() => ShellActions = BuildShellActions();

        private Model_ShellActionBarState BuildShellActions()
        {
            var step = _workflowService.CurrentStep;
            var showsActions = step != Enum_ReceivingWorkflowStep.Saving;
            var activeMode = ResolveActiveMode(step);

            return new Model_ShellActionBarState
            {
                IsVisible = showsActions,
                ShowModeGroup = showsActions,
                Guided = CreateModeAction(
                    "guided",
                    ShellGuidedText,
                    ShellGuidedDefaultTooltipText,
                    EnterGuidedModeCommand,
                    canBeDefault: true,
                    ShellDefaultBadgeText,
                    activeMode == "guided"
                ),
                ByReceiver = CreateModeAction(
                    "by-receiver",
                    ShellByReceiverText,
                    ShellByReceiverDefaultTooltipText,
                    EnterByReceiverModeCommand,
                    canBeDefault: true,
                    ShellDefaultBadgeText,
                    activeMode == "by-receiver"
                ),
                Manual = CreateModeAction(
                    "manual",
                    ShellManualText,
                    ShellManualDefaultTooltipText,
                    EnterManualModeCommand,
                    canBeDefault: true,
                    ShellDefaultBadgeText,
                    activeMode == "manual"
                ),
                Edit = CreateModeAction(
                    "edit",
                    ShellEditText,
                    ShellEditDefaultTooltipText,
                    EnterEditModeCommand,
                    canBeDefault: true,
                    ShellDefaultBadgeText,
                    activeMode == "edit"
                ),
                Overlay = CreateModeAction(
                    "overlay",
                    ShellOverlayText,
                    ShellOverlayText,
                    OpenLabelOverlayWindowCommand,
                    canBeDefault: false,
                    string.Empty,
                    isActive: false
                ),
                Back = BuildBackSlot(step),
                Next = BuildNextSlot(),
                ClearLabelData = CreateSlot(
                    showsActions,
                    CanClearLabelData,
                    WorkflowResetLabelDataText,
                    WorkflowResetLabelDataText,
                    ResetLabelDataCommand,
                    shiftCommand: ResetLabelDataAllCommand
                ),
                Help = CreateSlot(
                    showsActions,
                    true,
                    WorkflowHelpText,
                    WorkflowHelpText,
                    ShowHelpCommand
                ),
                IsGuidedDefault = CurrentDefaultMode == "guided",
                IsByReceiverDefault = CurrentDefaultMode == "by-receiver",
                IsManualDefault = CurrentDefaultMode == "manual",
                IsEditDefault = CurrentDefaultMode == "edit",
            };
        }

        private Model_ShellActionSlot BuildBackSlot(Enum_ReceivingWorkflowStep step)
        {
            if (step == Enum_ReceivingWorkflowStep.Complete)
            {
                return CreateSlot(
                    true,
                    true,
                    CompletionStartNewEntryText,
                    CompletionStartNewEntryText,
                    StartNewEntryCommand
                );
            }

            if (!ShowWorkflowBackButton)
            {
                return Model_ShellActionSlot.Hidden;
            }

            return CreateSlot(
                true,
                true,
                WorkflowBackText,
                WorkflowBackText,
                PreviousStepCommand,
                kind: Enum_ShellActionKind.Back
            );
        }

        private Model_ShellActionSlot BuildNextSlot()
        {
            if (ShowWorkflowNextButton)
            {
                return CreateSlot(
                    true,
                    IsNextButtonEnabled,
                    WorkflowNextText,
                    WorkflowNextText,
                    NextStepCommand
                );
            }

            return BuildHostedSaveSlot();
        }

        /// <summary>
        /// Puts a mode page's own save verb where Next normally sits, so the page stops drawing it.
        /// </summary>
        private Model_ShellActionSlot BuildHostedSaveSlot()
        {
            if (_manualEntryViewModel is not null && IsManualEntryVisible)
            {
                return CreateSlot(
                    true,
                    _manualEntryViewModel.SaveCommand.CanExecute(null),
                    _manualEntryViewModel.ManualEntrySaveAndFinishText,
                    _manualEntryViewModel.ManualEntrySaveAndFinishText,
                    _manualEntryViewModel.SaveCommand,
                    kind: Enum_ShellActionKind.Save
                );
            }

            if (_editModeViewModel is not null && IsEditModeVisible)
            {
                return CreateSlot(
                    true,
                    _editModeViewModel.SaveCommand.CanExecute(null),
                    _editModeViewModel.EditModeSaveAndFinishText,
                    _editModeViewModel.EditModeSaveAndFinishText,
                    _editModeViewModel.SaveCommand,
                    kind: Enum_ShellActionKind.Save
                );
            }

            if (_reviewViewModel is not null && IsReviewVisible)
            {
                return CreateSlot(
                    true,
                    _reviewViewModel.SaveCommand.CanExecute(null),
                    _reviewViewModel.ReviewSaveToDatabaseText,
                    _reviewViewModel.ReviewSaveToDatabaseText,
                    _reviewViewModel.SaveCommand,
                    kind: Enum_ShellActionKind.Save
                );
            }

            return Model_ShellActionSlot.Hidden;
        }

        private static Model_ShellActionSlot CreateSlot(
            bool isVisible,
            bool isEnabled,
            string label,
            string tooltip,
            System.Windows.Input.ICommand? command,
            bool showDividerAfter = false,
            Enum_ShellActionKind kind = Enum_ShellActionKind.Plain,
            System.Windows.Input.ICommand? shiftCommand = null
        ) =>
            new()
            {
                IsVisible = isVisible,
                IsEnabled = isEnabled,
                Label = label,
                Tooltip = tooltip,
                AccessibilityName = label,
                Command = command,
                ShiftCommand = shiftCommand,
                ShowDividerAfter = showDividerAfter,
                Kind = kind,
            };

        private static Model_ShellModeAction CreateModeAction(
            string mode,
            string label,
            string defaultTooltip,
            System.Windows.Input.ICommand? command,
            bool canBeDefault,
            string defaultBadge,
            bool isActive
        ) =>
            new()
            {
                Mode = mode,
                Label = label,
                Tooltip = label,
                AccessibilityName = label,
                Command = command,
                CanBeDefault = canBeDefault,
                DefaultBadge = canBeDefault ? defaultBadge : string.Empty,
                DefaultTooltip = defaultTooltip,
                ShowDividerAfter = canBeDefault,
                IsActive = isActive,
            };

        private string? CurrentDefaultMode =>
            _sessionManager.CurrentSession?.User?.DefaultReceivingMode;

        /// <summary>
        /// Works out which mode the operator is in right now, so the bar can mark it. Saving and the
        /// Result page keep the door they came through, because neither names a mode of its own.
        /// </summary>
        private string ResolveActiveMode(Enum_ReceivingWorkflowStep step)
        {
            if (_workflowService.IsByReceiverFlow)
            {
                _activeMode = "by-receiver";
            }
            else if (step == Enum_ReceivingWorkflowStep.ManualEntry)
            {
                _activeMode = "manual";
            }
            else if (step == Enum_ReceivingWorkflowStep.EditMode)
            {
                _activeMode = "edit";
            }
            else if (IsGuidedWalkStep(step))
            {
                _activeMode = "guided";
            }

            return _activeMode;
        }

        private async Task SetDefaultModeAsync(string? storedModeValue)
        {
            try
            {
                var currentUser = _sessionManager.CurrentSession?.User;
                if (currentUser is null)
                {
                    return;
                }

                var mode = NormalizeDefaultMode(storedModeValue);
                var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(
                    currentUser.WindowsUsername,
                    mode
                );

                if (result.IsSuccess)
                {
                    currentUser.DefaultReceivingMode = mode;
                    RefreshShellActions();
                }
                else
                {
                    await _errorHandler.ShowErrorDialogAsync(
                        "Save Error",
                        result.ErrorMessage,
                        Enum_ErrorSeverity.Error
                    );
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SetDefaultModeAsync),
                    nameof(ViewModel_Receiving_Workflow)
                );
            }
        }

        private static string? NormalizeDefaultMode(string? storedModeValue) =>
            storedModeValue?.Trim().ToLowerInvariant() switch
            {
                "guided" => "guided",
                "by-receiver" => "by-receiver",
                "manual" => "manual",
                "edit" => "edit",
                _ => null,
            };

        /// <summary>
        /// True when the loads entered so far do not add up to the quantity the chosen receiver
        /// recorded, which is what the Next warning asks the operator about.
        /// </summary>
        public bool HasReceiverQuantityMismatch(out decimal currentTotal, out decimal receiverQty)
        {
            currentTotal = 0m;
            receiverQty = 0m;

            var receiver = _workflowService.SelectedReceiver;
            var loads = _workflowService.CurrentSession?.Loads;

            if (receiver is null || loads is null || loads.Count == 0)
            {
                return false;
            }

            currentTotal = loads.Sum(load => load.WeightQuantity);
            receiverQty = receiver.QtyReceived;

            return currentTotal != receiverQty;
        }

        /// <summary>
        /// Warns when the loads do not total what the receiver recorded, and lets the operator go
        /// back to the quantities or continue as entered.
        /// </summary>
        private async Task<bool> ConfirmReceiverQuantityAsync()
        {
            if (_workflowService.CurrentStep != Enum_ReceivingWorkflowStep.WeightQuantityEntry)
            {
                return true;
            }

            if (!HasReceiverQuantityMismatch(out var currentTotal, out var receiverQty))
            {
                return true;
            }

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot is null)
            {
                _logger.LogWarning(
                    "XamlRoot is null, proceeding without the receiver quantity warning"
                );
                return true;
            }

            try
            {
                var dialog = new ContentDialog
                {
                    Title = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchTitle
                    ),
                    Content = await _receivingSettings.FormatAsync(
                        ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchContent,
                        currentTotal,
                        receiverQty
                    ),
                    PrimaryButtonText = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchContinue
                    ),
                    CloseButtonText = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchChangeValues
                    ),
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = xamlRoot,
                };

                MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                    dialog,
                    xamlRoot
                );

                return await dialog.ShowAsync() == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error showing the receiver quantity warning: {ex.Message}", ex);
                return true;
            }
        }

        /// <summary>
        /// Asks before a mode change discards data that has not been saved yet.
        /// </summary>
        private async Task<bool> ConfirmModeChangeAsync()
        {
            var shouldConfirm = await _receivingSettings.GetBoolAsync(
                ReceivingSettingsKeys.BusinessRules.ConfirmModeChange,
                _sessionManager.CurrentSession?.User?.EmployeeNumber
            );

            if (!shouldConfirm || !HasUnsavedData())
            {
                return true;
            }

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot is null)
            {
                _logger.LogWarning("XamlRoot is null, proceeding without confirmation");
                return true;
            }

            try
            {
                var dialog = new ContentDialog
                {
                    Title = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionTitle
                    ),
                    Content = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContent
                    ),
                    PrimaryButtonText = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContinue
                    ),
                    CloseButtonText = await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionCancel
                    ),
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = xamlRoot,
                };

                MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                    dialog,
                    xamlRoot
                );

                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error showing mode-change confirmation: {ex.Message}", ex);
                return true;
            }
        }

        private bool HasUnsavedData()
        {
            if (_workflowService.CurrentSession is null)
            {
                return false;
            }

            if (_workflowService.CurrentSession.Loads?.Count > 0)
            {
                return true;
            }

            return !string.IsNullOrEmpty(_workflowService.CurrentSession.PoNumber)
                || _workflowService.CurrentPart is not null;
        }

        #endregion

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);

        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

        public string GetTip(string key) => _helpService.GetTip(key);

        private void UpdateStartNewEntryTarget(Enum_ReceivingWorkflowStep currentStep)
        {
            switch (currentStep)
            {
                case Enum_ReceivingWorkflowStep.ManualEntry:
                    _startNewEntryStep = Enum_ReceivingWorkflowStep.ManualEntry;
                    break;
                case Enum_ReceivingWorkflowStep.EditMode:
                    _startNewEntryStep = Enum_ReceivingWorkflowStep.EditMode;
                    break;
                case Enum_ReceivingWorkflowStep.Review:
                    // Review keeps whatever door the clerk came in through, so a manual receipt
                    // starts over in the grid rather than in the guided walk.
                    break;

                case Enum_ReceivingWorkflowStep.POEntry:
                case Enum_ReceivingWorkflowStep.PartSelection:
                case Enum_ReceivingWorkflowStep.LoadEntry:
                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
                case Enum_ReceivingWorkflowStep.HeatLotEntry:
                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
                    _startNewEntryStep = Enum_ReceivingWorkflowStep.POEntry;
                    break;
            }
        }

        #endregion
    }
}
