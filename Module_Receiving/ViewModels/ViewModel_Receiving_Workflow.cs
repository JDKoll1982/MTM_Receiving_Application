using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using System;
using System.Threading.Tasks;

using InfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;

        [ObservableProperty]
        private string _currentStepTitle = "Receiving - Mode Selection";

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
            _workflowService.StepChanged += OnWorkflowStepChanged;
            _workflowService.StatusMessageRaised += (_, message) => ShowStatus(message);

            // Initialize visibility based on current step
            OnWorkflowStepChanged(this, EventArgs.Empty);
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
                    CurrentStepTitle = "Receiving - Enter Number of Loads";
                    break;
                case Enum_ReceivingWorkflowStep.WeightQuantityEntry:
                    IsWeightQuantityEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter Weight/Quantity";
                    break;
                case Enum_ReceivingWorkflowStep.HeatLotEntry:
                    IsHeatLotEntryVisible = true;
                    CurrentStepTitle = "Receiving - Enter Heat/Lot Numbers";
                    break;
                case Enum_ReceivingWorkflowStep.PackageTypeEntry:
                    IsPackageTypeEntryVisible = true;
                    CurrentStepTitle = "Receiving - Select Package Type";
                    break;
                case Enum_ReceivingWorkflowStep.Review:
                    IsReviewVisible = true;
                    CurrentStepTitle = "Receiving - Review & Save";
                    break;
                case Enum_ReceivingWorkflowStep.Saving:
                    IsSavingVisible = true;
                    CurrentStepTitle = "Receiving - Saving...";
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
                // Try to save to DB first
                var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
                if (!saveResult.Success)
                {
                     var warnDialog = new ContentDialog
                    {
                        Title = "Database Save Failed",
                        Content = $"Failed to save to database: {string.Join(", ", saveResult.Errors)}\n\nDo you want to proceed with deleting CSV files anyway?",
                        PrimaryButtonText = "Delete Anyway",
                        CloseButtonText = "Cancel",
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
                ShowStatus("Workflow cleared. Please select a mode.", InfoBarSeverity.Informational);
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

