using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Triggers;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.Views;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using InfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Workflow
        : ViewModel_Shared_Base,
            IViewModel_HeaderTitleProvider
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;
        private readonly IService_LabelViewLauncher _labelViewLauncher;
        private readonly IService_ReceivingSettings _receivingSettings;
        private readonly IService_ViewModelRegistry _viewModelRegistry;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]
        private string _currentStepTitle = "Receiving - Mode Selection";

        public string CurrentHeaderTitle => CurrentStepTitle;

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
        private bool _isModeSelectionVisible;

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
        private bool _isReconciliationReviewVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowBackButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNextButton))]
        [NotifyPropertyChangedFor(nameof(ShowWorkflowNavigationButtons))]
        private bool _isPOEntryVisible;

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

        [ObservableProperty]
        private string _saveProgressMessage = "Initializing...";

        private bool _isNextButtonEnabled = true;

        /// <summary>
        /// Gets a value indicating whether the workflow Next button is currently available.
        /// </summary>
        public bool IsNextButtonEnabled
        {
            get => _isNextButtonEnabled;
            private set => SetProperty(ref _isNextButtonEnabled, value);
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

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _workflowHelpText = "Help";

        [ObservableProperty]
        private string _workflowBackText = "Back";

        [ObservableProperty]
        private string _workflowNextText = "Next";

        [ObservableProperty]
        private string _workflowModeSelectionText = "Mode Selection";

        [ObservableProperty]
        private string _workflowResetLabelDataText = "Clear Label Data";

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
                WorkflowModeSelectionText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WorkflowModeSelection
                );
                WorkflowResetLabelDataText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.UiText.WorkflowResetLabelData
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
            IsModeSelectionVisible = false;
            IsManualEntryVisible = false;
            IsEditModeVisible = false;
            IsReconciliationReviewVisible = false;
            IsPOEntryVisible = false;
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
                case Enum_ReceivingWorkflowStep.ModeSelection:
                    IsModeSelectionVisible = true;
                    CurrentStepTitle = "Receiving - Mode Selection";
                    break;
                case Enum_ReceivingWorkflowStep.ManualEntry:
                    IsManualEntryVisible = true;
                    CurrentStepTitle = "Receiving - Manual Entry";
                    break;
                case Enum_ReceivingWorkflowStep.EditMode:
                    IsEditModeVisible = true;
                    CurrentStepTitle = "Receiving - Edit Mode";
                    break;
                case Enum_ReceivingWorkflowStep.ReconciliationReview:
                    IsReconciliationReviewVisible = true;
                    CurrentStepTitle = "Receiving - Reconcile Saved Locations";
                    break;
                case Enum_ReceivingWorkflowStep.POEntry:
                    IsPOEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter PO Number";
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
                default:
                    IsModeSelectionVisible = true;
                    CurrentStepTitle = "Receiving - Mode Selection";
                    break;
            }

            // Update help content based on step
            HelpContent = Helper_WorkflowHelpContentGenerator.GenerateHelpContent(
                _workflowService.CurrentStep
            );

            _logger.LogInfo(
                $"Visibility updated. Current Step: {_workflowService.CurrentStep}, Title: {CurrentStepTitle}"
            );

            UpdateNextButtonEnabled();
            _ = RefreshClearLabelDataAvailabilityAsync();
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
                    $"Failed to refresh receiving label-data availability: {ex.Message}",
                    ex
                );
                CanClearLabelData = false;
            }
        }

        private void UpdateNextButtonEnabled()
        {
            IsNextButtonEnabled = !IsPOEntryVisible || _workflowService.CurrentPart is not null;
        }

        public void RefreshNextButtonEnabled()
        {
            UpdateNextButtonEnabled();
        }

        [RelayCommand]
        private async Task NextStepAsync()
        {
            try
            {
                _logger.LogInfo("NextStepAsync command triggered.");

                if (!await EnsureReceivingNonPoReferenceAsync())
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
                LastSaveResult = await _workflowService.SaveSessionAsync(
                    messageProgress,
                    percentProgress
                );
                _logger.LogInfo($"SaveSessionAsync returned. Success: {LastSaveResult.Success}");

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

        [RelayCommand]
        private async Task OpenReceivingLabelAsync()
        {
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

        [RelayCommand]
        private async Task ResetLabelDataAsync()
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
                var deleteResult = await _workflowService.ResetLabelDataAsync();
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
                        await _receivingSettings.GetStringAsync(
                            ReceivingSettingsKeys.Workflow.StatusLabelDataClearedFailed
                        ),
                        InfoBarSeverity.Warning
                    );
                }
            }
        }

        [RelayCommand]
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
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_LabelPaths),
                    $"{labelName} path is missing or invalid. Opening Receiving label settings."
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

        [RelayCommand]
        private async Task ReturnToModeSelectionAsync()
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
                    ReceivingSettingsKeys.Dialogs.ConfirmChangeModeTitle
                ),
                Content = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Dialogs.ConfirmChangeModeContent
                ),
                PrimaryButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Dialogs.ConfirmChangeModeConfirm
                ),
                CloseButtonText = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.Dialogs.ConfirmChangeModeCancel
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
                // Reset workflow and return to mode selection
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
                ShowStatus(
                    await _receivingSettings.GetStringAsync(
                        ReceivingSettingsKeys.Workflow.StatusWorkflowCleared
                    ),
                    InfoBarSeverity.Informational
                );
            }
        }

        /// <summary>
        /// Shows contextual help for current workflow step
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.Workflow");
        }

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
                case Enum_ReceivingWorkflowStep.ReconciliationReview:
                case Enum_ReceivingWorkflowStep.POEntry:
                case Enum_ReceivingWorkflowStep.PartSelection:
                case Enum_ReceivingWorkflowStep.LoadEntry:
                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
                case Enum_ReceivingWorkflowStep.HeatLotEntry:
                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
                case Enum_ReceivingWorkflowStep.Review:
                    _startNewEntryStep = Enum_ReceivingWorkflowStep.POEntry;
                    break;
            }
        }

        #endregion
    }
}
