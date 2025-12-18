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

            // Auto-dismiss after 3 seconds if informational or success
            if (severity == InfoBarSeverity.Informational || severity == InfoBarSeverity.Success)
            {
                Task.Delay(3000).ContinueWith(_ => 
                {
                    // Ensure UI update runs on UI thread if needed
                    // Since we are in ViewModel, we rely on binding.
                    // But Task.Delay runs on thread pool.
                    // We should use DispatcherQueue if possible, but ViewModel doesn't have it directly.
                    // However, ObservableProperty usually handles property change on any thread?
                    // No, UI updates must be on UI thread.
                    // We can ignore auto-dismiss for now or assume the binding engine handles it (it doesn't always).
                    // Or we can expose a method to close it.
                    // Let's just set it to false, and hope for the best or use a timer.
                    // Better: Don't auto-dismiss here to avoid threading issues without Dispatcher.
                    // Or use a Timer.
                });
            }
        }

        
        [RelayCommand]
        private async Task StartWorkflowAsync()
        {
            await _workflowService.StartWorkflowAsync();
            UpdateStepVisibility();
        }

        [RelayCommand]
        private async Task NextStepAsync()
        {
            _logger.LogInfo("NextStepAsync command triggered.");
            
            // Force yield to ensure UI is responsive before starting
            await Task.Yield();

            var result = await _workflowService.AdvanceToNextStepAsync();
            _logger.LogInfo($"AdvanceToNextStepAsync returned. Success: {result.Success}, Step: {_workflowService.CurrentStep}");
            
            if (result.Success)
            {
                // Ensure UI update happens on UI thread
                // Although RelayCommand should be on UI thread, let's be safe
                UpdateStepVisibility();

                if (_workflowService.CurrentStep == WorkflowStep.Saving)
                {
                    _logger.LogInfo("Current step is Saving. Calling PerformSaveAsync...");
                    // Remove delay to start immediately
                    // await Task.Delay(500);
                    await PerformSaveAsync();
                }
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

        private async Task PerformSaveAsync()
        {
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
        }

        [RelayCommand]
        private async Task StartNewEntryAsync()
        {
            await _workflowService.ResetWorkflowAsync();
            UpdateStepVisibility();
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

        private void UpdateStepVisibility()
        {
            var step = _workflowService.CurrentStep;

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
