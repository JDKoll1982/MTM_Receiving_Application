using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Receiving.Settings;

using InfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingSettings _receivingSettings;

        [ObservableProperty]
        private string _currentStepTitle = "Receiving - Mode Selection";

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isModeSelectionVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isManualEntryVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isEditModeVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isPOEntryVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isPartSelectionVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isLoadEntryVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isWeightQuantityEntryVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isHeatLotEntryVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isPackageTypeEntryVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isReviewVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isSavingVisible;

        /// <summary>OBSOLETE: Use new consolidated 3-step workflow ViewModel instead.</summary>
        [Obsolete("Use new ViewModel_ReceivingWorkflow_Consolidation for the 3-step workflow.", DiagnosticId = "RECV001")]
        [ObservableProperty]
        private bool _isCompleteVisible;

        [ObservableProperty]
        private Model_SaveResult? _lastSaveResult;

        [ObservableProperty]
        private string _saveProgressMessage = "Initializing...";

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
        private string _workflowResetCsvText = "Reset CSV";

        [ObservableProperty]
        private string _completionSuccessTitleText = "Success!";

        [ObservableProperty]
        private string _completionFailureTitleText = "Save Failed";

        [ObservableProperty]
        private string _completionLoadsSavedSuffixText = " loads saved successfully.";

        [ObservableProperty]
        private string _completionSaveDetailsTitleText = "Save Details:";

        [ObservableProperty]
        private string _completionLocalCsvLabelText = "Local CSV:";

        [ObservableProperty]
        private string _completionNetworkCsvLabelText = "Network CSV:";

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
        private readonly System.Collections.Generic.Dictionary<Enum_ReceivingWorkflowStep, string> _stepTitles = new();

        public ViewModel_Receiving_Workflow(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Dispatcher dispatcherService,
            IService_Window windowService,
            IService_Help helpService,
            IService_ReceivingSettings receivingSettings,
            IService_Notification notificationService)
            : base(errorHandler, logger, notificationService)
        {
            _dispatcherService = dispatcherService;
            _windowService = windowService;
            _workflowService = workflowService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;
            _workflowService.StepChanged += OnWorkflowStepChanged;
            _workflowService.StatusMessageRaised += (_, message) => ShowStatus(message);

            _ = InitializeTextAsync();

            // Initialize visibility based on current step
            OnWorkflowStepChanged(this, EventArgs.Empty);
        }

        private async Task InitializeTextAsync()
        {
            try
            {
                WorkflowHelpText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WorkflowHelp);
                WorkflowBackText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WorkflowBack);
                WorkflowNextText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WorkflowNext);
                WorkflowModeSelectionText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WorkflowModeSelection);
                WorkflowResetCsvText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WorkflowResetCsv);

                CompletionSuccessTitleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionSuccessTitle);
                CompletionFailureTitleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionFailureTitle);
                CompletionLoadsSavedSuffixText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionLoadsSavedSuffix);
                CompletionSaveDetailsTitleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionSaveDetailsTitle);
                CompletionLocalCsvLabelText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionLocalCsvLabel);
                CompletionNetworkCsvLabelText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionNetworkCsvLabel);
                CompletionDatabaseLabelText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionDatabaseLabel);
                CompletionSavedText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionSaved);
                CompletionFailedText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionFailed);
                CompletionStartNewEntryText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.CompletionStartNewEntry);

                _stepTitles[Enum_ReceivingWorkflowStep.ModeSelection] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleModeSelection);
                _stepTitles[Enum_ReceivingWorkflowStep.ManualEntry] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleManualEntry);
                _stepTitles[Enum_ReceivingWorkflowStep.EditMode] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleEditMode);
                _stepTitles[Enum_ReceivingWorkflowStep.POEntry] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitlePoEntry);
                _stepTitles[Enum_ReceivingWorkflowStep.PartSelection] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitlePartSelection);
                _stepTitles[Enum_ReceivingWorkflowStep.LoadEntry] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleLoadEntry);
                _stepTitles[Enum_ReceivingWorkflowStep.WeightQuantityEntry] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleWeightQuantity);
                _stepTitles[Enum_ReceivingWorkflowStep.HeatLotEntry] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleHeatLot);
                _stepTitles[Enum_ReceivingWorkflowStep.PackageTypeEntry] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitlePackageType);
                _stepTitles[Enum_ReceivingWorkflowStep.Review] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleReviewAndSave);
                _stepTitles[Enum_ReceivingWorkflowStep.Saving] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleSaving);
                _stepTitles[Enum_ReceivingWorkflowStep.Complete] = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StepTitleComplete);

                SaveProgressMessage = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.SaveProgressInitializing);

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
            _logger.LogInfo("StepChanged event received in ViewModel. Updating visibility.");

            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Saving)
            {
                _logger.LogInfo("Step is Saving. Enqueuing PerformSaveAsync via Dispatcher.");
                _dispatcherService.TryEnqueue(async () =>
                {
                    await PerformSaveAsync();
                });
            }

            // Hide all steps
            IsModeSelectionVisible = false;
            IsManualEntryVisible = false;
            IsEditModeVisible = false;
            IsPOEntryVisible = false;
            IsPartSelectionVisible = false;
            IsLoadEntryVisible = false;
            IsWeightQuantityEntryVisible = false;
            IsHeatLotEntryVisible = false;
            IsPackageTypeEntryVisible = false;
            IsReviewVisible = false;
            IsSavingVisible = false;
            IsCompleteVisible = false;

            // Show current step and set title
            switch (_workflowService.CurrentStep)
            {
                case Enum_ReceivingWorkflowStep.ModeSelection:
                    IsModeSelectionVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.ModeSelection, out var modeTitle) ? modeTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.ManualEntry:
                    IsManualEntryVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.ManualEntry, out var manualTitle) ? manualTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.EditMode:
                    IsEditModeVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.EditMode, out var editTitle) ? editTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.POEntry:
                    IsPOEntryVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.POEntry, out var poTitle) ? poTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.PartSelection:
                    IsPartSelectionVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.PartSelection, out var partTitle) ? partTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.LoadEntry:
                    IsLoadEntryVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.LoadEntry, out var loadTitle) ? loadTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
                    IsWeightQuantityEntryVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.WeightQuantityEntry, out var weightTitle) ? weightTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.HeatLotEntry:
                    IsHeatLotEntryVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.HeatLotEntry, out var heatTitle) ? heatTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
                    IsPackageTypeEntryVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.PackageTypeEntry, out var pkgTitle) ? pkgTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.Review:
                    IsReviewVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.Review, out var reviewTitle) ? reviewTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.Saving:
                    IsSavingVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.Saving, out var savingTitle) ? savingTitle : CurrentStepTitle;
                    break;
                case Enum_ReceivingWorkflowStep.Complete:
                    IsCompleteVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.Complete, out var completeTitle) ? completeTitle : CurrentStepTitle;
                    break;
                default:
                    IsModeSelectionVisible = true;
                    CurrentStepTitle = _stepTitles.TryGetValue(Enum_ReceivingWorkflowStep.ModeSelection, out var defaultTitle) ? defaultTitle : CurrentStepTitle;
                    break;
            }

            // Update help content based on step
            HelpContent = Helper_WorkflowHelpContentGenerator.GenerateHelpContent(_workflowService.CurrentStep);

            _logger.LogInfo($"Visibility updated. Current Step: {_workflowService.CurrentStep}, Title: {CurrentStepTitle}");
        }

        [RelayCommand]
        private async Task NextStepAsync()
        {
            try
            {
                _logger.LogInfo("NextStepAsync command triggered.");

                // Removed Task.Yield() to avoid context switching issues
                // await Task.Yield();

                var result = await _workflowService.AdvanceToNextStepAsync();
                _logger.LogInfo($"AdvanceToNextStepAsync returned. Success: {result.Success}, Step: {_workflowService.CurrentStep}");

                if (!result.Success)
                {
                    if (result.ValidationErrors.Count > 0)
                    {
                        await _errorHandler.HandleErrorAsync(
                            string.Join("\n", result.ValidationErrors),
                            Enum_ErrorSeverity.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in NextStepAsync: {ex.Message}", ex);
                await _errorHandler.HandleErrorAsync($"An error occurred: {ex.Message}", Enum_ErrorSeverity.Error);
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
                SaveProgressMessage = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.SaveProgressSavingCsv);
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
                LastSaveResult = await _workflowService.SaveSessionAsync(messageProgress, percentProgress);
                _logger.LogInfo($"SaveSessionAsync returned. Success: {LastSaveResult.Success}");

                // Advance to Complete step
                await _workflowService.AdvanceToNextStepAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PerformSaveAsync: {ex.Message}", ex);
                await _errorHandler.HandleErrorAsync($"Save failed: {ex.Message}", Enum_ErrorSeverity.Error);
            }
            finally
            {
                _isSaving = false;
            }
        }

        [RelayCommand]
        private async Task StartNewEntryAsync()
        {
            await _workflowService.ResetWorkflowAsync();
        }

        [RelayCommand]
        private async Task ResetCSVAsync()
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync(await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Messages.ErrorUnableToDisplayDialog), Enum_ErrorSeverity.Error);
                return;
            }

            var dialog = new ContentDialog
            {
                Title = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.ResetCsvDialogTitle),
                Content = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.ResetCsvDialogContent),
                PrimaryButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.ResetCsvDialogDelete),
                CloseButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.ResetCsvDialogCancel),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Try to save to DB first
                var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
                if (!saveResult.Success)
                {
                    var warnDialog = new ContentDialog
                    {
                        Title = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.DbSaveFailedDialogTitle),
                        Content = $"Failed to save to database: {string.Join(", ", saveResult.Errors)}\n\nDo you want to proceed with deleting CSV files anyway?",
                        PrimaryButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.DbSaveFailedDialogDeleteAnyway),
                        CloseButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.DbSaveFailedDialogCancel),
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = xamlRoot
                    };

                    var warnResult = await warnDialog.ShowAsync();
                    if (warnResult != ContentDialogResult.Primary)
                    {
                        return;
                    }
                }

                var deleteResult = await _workflowService.ResetCSVFilesAsync();
                if (deleteResult.LocalDeleted || deleteResult.NetworkDeleted)
                {
                    ShowStatus(await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StatusCsvDeletedSuccess), InfoBarSeverity.Success);
                }
                else
                {
                    ShowStatus(await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StatusCsvDeletedFailed), InfoBarSeverity.Warning);
                }
            }
        }

        [RelayCommand]
        private void PreviousStep()
        {
            var result = _workflowService.GoToPreviousStep();
        }

        [RelayCommand]
        private async Task ReturnToModeSelectionAsync()
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync(await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Messages.ErrorUnableToDisplayDialog), Enum_ErrorSeverity.Error);
                return;
            }

            var dialog = new ContentDialog
            {
                Title = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeTitle),
                Content = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeContent),
                PrimaryButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeConfirm),
                CloseButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeCancel),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Reset workflow and return to mode selection
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
                ShowStatus(await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Workflow.StatusWorkflowCleared), InfoBarSeverity.Informational);
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

        #endregion
    }
}

