using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ReceivingModule.Models;
using MTM_Receiving_Application.ViewModels.Shared;
using MTM_Receiving_Application.Helpers.UI;
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ReceivingModule.ViewModels
{
    public partial class ViewModel_Receiving_Workflow : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;

        [ObservableProperty]
        private string _currentStepTitle = "📥 Receiving - Mode Selection";

        [ObservableProperty]
        private bool _isModeSelectionVisible;

        [ObservableProperty]
        private bool _isManualEntryVisible;

        [ObservableProperty]
        private bool _isEditModeVisible;

        [ObservableProperty]
        private bool _isPOEntryVisible;

        [ObservableProperty]
        private bool _isPartSelectionVisible;

        [ObservableProperty]
        private bool _isLoadEntryVisible;

        [ObservableProperty]
        private bool _isWeightQuantityEntryVisible;

        [ObservableProperty]
        private bool _isHeatLotEntryVisible;

        [ObservableProperty]
        private bool _isPackageTypeEntryVisible;

        [ObservableProperty]
        private bool _isReviewVisible;

        [ObservableProperty]
        private bool _isSavingVisible;

        [ObservableProperty]
        private bool _isCompleteVisible;

        [ObservableProperty]
        private Model_SaveResult? _lastSaveResult;

        [ObservableProperty]
        private string _saveProgressMessage = "Initializing...";

        [ObservableProperty]
        private double _saveProgressValue = 0;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusOpen;

        [ObservableProperty]
        private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

        [ObservableProperty]
        private Microsoft.UI.Xaml.UIElement? _helpContent;

        private bool _isSaving = false;
        private readonly IService_Dispatcher _dispatcherService;
        private readonly IService_Window _windowService;

        public ViewModel_Receiving_Workflow(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Dispatcher dispatcherService,
            IService_Window windowService,
            IService_Help helpService)
            : base(errorHandler, logger)
        {
            _dispatcherService = dispatcherService;
            _windowService = windowService;
            _workflowService = workflowService;
            _helpService = helpService;
            _workflowService.StepChanged += (s, e) =>
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
                UpdateStepVisibility();
                _logger.LogInfo("Visibility updated.");
            };
            _workflowService.StatusMessageRaised += (_, message) => ShowStatus(message);

            // Initialize visibility based on current step
            UpdateStepVisibility();
        }
        public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            StatusMessage = message;
            StatusSeverity = severity;
            IsStatusOpen = true;

            // Auto-dismiss after 5 seconds if informational or success
            if (severity == InfoBarSeverity.Informational || severity == InfoBarSeverity.Success)
            {
                Task.Delay(5000).ContinueWith(_ =>
                {
                    _dispatcherService.TryEnqueue(() =>
                    {
                        IsStatusOpen = false;
                    });
                });
            }
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

                if (result.Success)
                {
                    UpdateStepVisibility();
                    // PerformSaveAsync is now triggered by the StepChanged event handler
                }
                else
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
                SaveProgressMessage = "Saving to local and network CSV...";
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
                UpdateStepVisibility();
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
            UpdateStepVisibility();
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

            var dialog = new ContentDialog
            {
                Title = "Reset CSV Files",
                Content = "Are you sure you want to delete the local and network CSV files? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var deleteResult = await _workflowService.ResetCSVFilesAsync();
                if (deleteResult.LocalDeleted || deleteResult.NetworkDeleted)
                {
                    ShowStatus("CSV files deleted successfully.", InfoBarSeverity.Success);
                }
                else
                {
                    ShowStatus("Failed to delete CSV files or files not found.", InfoBarSeverity.Warning);
                }
            }
        }

        [RelayCommand]
        private void PreviousStep()
        {
            var result = _workflowService.GoToPreviousStep();
            if (result.Success)
            {
                UpdateStepVisibility();
            }
        }

        [RelayCommand]
        private async Task ReturnToModeSelectionAsync()
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Change Mode?",
                Content = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
                PrimaryButtonText = "Yes, Change Mode",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Reset workflow and return to mode selection
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
                UpdateStepVisibility();
                ShowStatus("Workflow cleared. Please select a mode.", InfoBarSeverity.Informational);
            }
        }

        private void UpdateStepVisibility()
        {
            var step = _workflowService.CurrentStep;

            IsModeSelectionVisible = step == Enum_ReceivingWorkflowStep.ModeSelection;
            IsManualEntryVisible = step == Enum_ReceivingWorkflowStep.ManualEntry;
            IsEditModeVisible = step == Enum_ReceivingWorkflowStep.EditMode;
            IsPOEntryVisible = step == Enum_ReceivingWorkflowStep.POEntry;
            IsPartSelectionVisible = step == Enum_ReceivingWorkflowStep.PartSelection;
            IsLoadEntryVisible = step == Enum_ReceivingWorkflowStep.LoadEntry;
            IsWeightQuantityEntryVisible = step == Enum_ReceivingWorkflowStep.WeightQuantityEntry;
            IsHeatLotEntryVisible = step == Enum_ReceivingWorkflowStep.HeatLotEntry;
            IsPackageTypeEntryVisible = step == Enum_ReceivingWorkflowStep.PackageTypeEntry;
            IsReviewVisible = step == Enum_ReceivingWorkflowStep.Review;
            IsSavingVisible = step == Enum_ReceivingWorkflowStep.Saving;
            IsCompleteVisible = step == Enum_ReceivingWorkflowStep.Complete;

            // Update title based on step
            CurrentStepTitle = step switch
            {
                Enum_ReceivingWorkflowStep.ModeSelection => "📥 Receiving - Mode Selection",
                Enum_ReceivingWorkflowStep.ManualEntry => "Manual Entry",
                Enum_ReceivingWorkflowStep.EditMode => "Edit Mode",
                Enum_ReceivingWorkflowStep.POEntry => "Enter PO Number",
                Enum_ReceivingWorkflowStep.PartSelection => "Select Part",
                Enum_ReceivingWorkflowStep.LoadEntry => "Enter Number of Loads",
                Enum_ReceivingWorkflowStep.WeightQuantityEntry => "Enter Weight/Quantity",
                Enum_ReceivingWorkflowStep.HeatLotEntry => "Enter Heat/Lot Numbers",
                Enum_ReceivingWorkflowStep.PackageTypeEntry => "Select Package Type",
                Enum_ReceivingWorkflowStep.Review => "Review & Save",
                Enum_ReceivingWorkflowStep.Saving => "Saving...",
                Enum_ReceivingWorkflowStep.Complete => "Complete",
                _ => "📥 Receiving - Mode Selection"
            };

            // Update help content based on step
            HelpContent = Helper_WorkflowHelpContentGenerator.GenerateHelpContent(step);
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
