using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class ReceivingWorkflowViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;

        [ObservableProperty]
        private string _currentStepTitle = "Receiving Workflow";

        [ObservableProperty]
        private bool _isModeSelectionVisible;

        [ObservableProperty]
        private bool _isManualEntryVisible;

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
        private SaveResult? _lastSaveResult;

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

        private bool _isSaving = false;

        public ReceivingWorkflowViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _workflowService.StepChanged += (s, e) => 
            {
                _logger.LogInfo("StepChanged event received in ViewModel. Updating visibility.");

                if (_workflowService.CurrentStep == WorkflowStep.Saving)
                {
                    _logger.LogInfo("Step is Saving. Enqueuing PerformSaveAsync via Dispatcher.");
                    App.MainWindow?.DispatcherQueue?.TryEnqueue(async () => 
                    {
                        await PerformSaveAsync();
                    });
                }
                UpdateStepVisibility();
                _logger.LogInfo("Visibility updated.");
            };
            _workflowService.StatusMessageRaised += (s, message) => ShowStatus(message);
            
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
                    App.MainWindow?.DispatcherQueue?.TryEnqueue(() => 
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
            if (App.MainWindow?.Content?.XamlRoot == null)
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
                XamlRoot = App.MainWindow.Content.XamlRoot
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
            if (App.MainWindow?.Content?.XamlRoot == null)
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
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Reset workflow and return to mode selection
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(WorkflowStep.ModeSelection);
                UpdateStepVisibility();
                ShowStatus("Workflow cleared. Please select a mode.", InfoBarSeverity.Informational);
            }
        }

        private void UpdateStepVisibility()
        {
            var step = _workflowService.CurrentStep;

            IsModeSelectionVisible = step == WorkflowStep.ModeSelection;
            IsManualEntryVisible = step == WorkflowStep.ManualEntry;
            IsPOEntryVisible = step == WorkflowStep.POEntry;
            IsPartSelectionVisible = step == WorkflowStep.PartSelection;
            IsLoadEntryVisible = step == WorkflowStep.LoadEntry;
            IsWeightQuantityEntryVisible = step == WorkflowStep.WeightQuantityEntry;
            IsHeatLotEntryVisible = step == WorkflowStep.HeatLotEntry;
            IsPackageTypeEntryVisible = step == WorkflowStep.PackageTypeEntry;
            IsReviewVisible = step == WorkflowStep.Review;
            IsSavingVisible = step == WorkflowStep.Saving;
            IsCompleteVisible = step == WorkflowStep.Complete;

            // Update title based on step
            CurrentStepTitle = step switch
            {
                WorkflowStep.ModeSelection => "Select Mode",
                WorkflowStep.ManualEntry => "Manual Entry",
                WorkflowStep.POEntry => "Enter PO Number",
                WorkflowStep.PartSelection => "Select Part",
                WorkflowStep.LoadEntry => "Enter Number of Loads",
                WorkflowStep.WeightQuantityEntry => "Enter Weight/Quantity",
                WorkflowStep.HeatLotEntry => "Enter Heat/Lot Numbers",
                WorkflowStep.PackageTypeEntry => "Select Package Type",
                WorkflowStep.Review => "Review & Save",
                WorkflowStep.Saving => "Saving...",
                WorkflowStep.Complete => "Complete",
                _ => "Receiving Workflow"
            };
        }
    }
}
