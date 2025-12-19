using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Shared
{
    /// <summary>
    /// Base class for all workflow step ViewModels providing common navigation,
    /// lifecycle management, and validation infrastructure.
    /// </summary>
    /// <typeparam name="TStepData">The data type associated with this step</typeparam>
    public abstract partial class BaseStepViewModel<TStepData> : BaseViewModel where TStepData : class, new()
    {
        protected readonly IService_ReceivingWorkflow _workflowService;

        [ObservableProperty]
        private TStepData _stepData = new();

        protected BaseStepViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            
            // Subscribe to workflow step changes
            _workflowService.StepChanged += OnWorkflowStepChanged;
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract WorkflowStep ThisStep { get; }

        /// <summary>
        /// Called when workflow step changes. Triggers OnNavigatedTo if this step is active.
        /// </summary>
        private void OnWorkflowStepChanged(object? sender, EventArgs e)
        {
            if (_workflowService.CurrentStep == ThisStep)
            {
                _ = OnNavigatedToAsync();
            }
            else if (ShouldCallNavigatedFrom())
            {
                _ = OnNavigatedFromAsync();
            }
        }

        /// <summary>
        /// Determines if OnNavigatedFromAsync should be called.
        /// Override to customize when navigation-from occurs.
        /// </summary>
        protected virtual bool ShouldCallNavigatedFrom()
        {
            // By default, don't call OnNavigatedFrom (can be overridden if needed)
            return false;
        }

        /// <summary>
        /// Called when this step becomes active in the workflow.
        /// Override to load data, refresh UI, etc.
        /// </summary>
        protected virtual Task OnNavigatedToAsync()
        {
            _logger.LogInfo($"{GetType().Name} navigated to.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when navigating away from this step.
        /// Override to persist data, cleanup, etc.
        /// </summary>
        protected virtual Task OnNavigatedFromAsync()
        {
            _logger.LogInfo($"{GetType().Name} navigated from.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates the current step data before advancing.
        /// Override to provide custom validation logic.
        /// </summary>
        protected virtual Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
        {
            // Default: no validation errors
            return Task.FromResult((true, string.Empty));
        }

        /// <summary>
        /// Command to advance to the next step (bound to Next button).
        /// Validates before advancing.
        /// </summary>
        [RelayCommand]
        protected virtual async Task NextAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Validating...";

                // Validate step data
                var (isValid, errorMessage) = await ValidateStepAsync();
                if (!isValid)
                {
                    await _errorHandler.HandleErrorAsync(errorMessage, Models.Enums.Enum_ErrorSeverity.Warning);
                    return;
                }

                // Persist any step-specific data to workflow service
                await OnBeforeAdvanceAsync();

                // Advance workflow
                StatusMessage = "Advancing...";
                var result = await _workflowService.AdvanceToNextStepAsync();

                if (!result.Success)
                {
                    var errors = string.Join("\n", result.ValidationErrors);
                    await _errorHandler.HandleErrorAsync(errors, Models.Enums.Enum_ErrorSeverity.Error);
                }
                else if (!string.IsNullOrEmpty(result.Message))
                {
                    StatusMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync(
                    "Failed to advance to next step",
                    Models.Enums.Enum_ErrorSeverity.Error,
                    ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Command to go back to the previous step (bound to Back button).
        /// </summary>
        [RelayCommand]
        protected virtual void Previous()
        {
            if (IsBusy) return;

            try
            {
                var result = _workflowService.GoToPreviousStep();
                if (!result.Success)
                {
                    var errors = string.Join("\n", result.ValidationErrors);
                    _errorHandler.HandleErrorAsync(errors, Models.Enums.Enum_ErrorSeverity.Warning).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleErrorAsync(
                    "Failed to go to previous step",
                    Models.Enums.Enum_ErrorSeverity.Error,
                    ex).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Command to cancel the current workflow (bound to Cancel button).
        /// </summary>
        [RelayCommand]
        protected virtual async Task CancelAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Canceling workflow...";

                await _workflowService.ResetWorkflowAsync();
                
                StatusMessage = "Workflow canceled.";
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync(
                    "Failed to cancel workflow",
                    Models.Enums.Enum_ErrorSeverity.Error,
                    ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Command to return to mode selection, clearing current workflow progress.
        /// Shows a confirmation dialog before resetting.
        /// </summary>
        [RelayCommand]
        protected virtual async Task ReturnToModeSelectionAsync()
        {
            if (IsBusy) return;

            if (App.MainWindow?.Content?.XamlRoot == null)
            {
                _logger.LogError("Cannot show dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync("Unable to display dialog", Models.Enums.Enum_ErrorSeverity.Error);
                return;
            }

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Return to Mode Selection?",
                Content = "This will clear all current workflow progress. Do you want to continue?",
                PrimaryButtonText = "Yes, Clear Workflow",
                CloseButtonText = "No, Continue",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                try
                {
                    IsBusy = true;
                    StatusMessage = "Returning to mode selection...";
                    
                    await _workflowService.ResetWorkflowAsync();
                    
                    StatusMessage = "Workflow cleared. Please select a mode.";
                }
                catch (Exception ex)
                {
                    await _errorHandler.HandleErrorAsync(
                        "Failed to return to mode selection",
                        Models.Enums.Enum_ErrorSeverity.Error,
                        ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Called before advancing to next step. Override to persist step-specific data.
        /// </summary>
        protected virtual Task OnBeforeAdvanceAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Cleanup method to unsubscribe from events.
        /// </summary>
        public virtual void Dispose()
        {
            _workflowService.StepChanged -= OnWorkflowStepChanged;
        }
    }
}
