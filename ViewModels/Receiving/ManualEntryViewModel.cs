using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using Windows.Foundation;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class ManualEntryViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        public ManualEntryViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _loads = new ObservableCollection<Model_ReceivingLoad>(_workflowService.CurrentSession.Loads);
        }

        [RelayCommand]
        private void AddRow()
        {
            var newLoad = new Model_ReceivingLoad
            {
                LoadID = System.Guid.NewGuid(),
                ReceivedDate = System.DateTime.Now,
                LoadNumber = Loads.Count + 1
            };
            Loads.Add(newLoad);
            _workflowService.CurrentSession.Loads.Add(newLoad);
        }

        [RelayCommand]
        private void RemoveRow()
        {
            if (SelectedLoad != null)
            {
                _workflowService.CurrentSession.Loads.Remove(SelectedLoad);
                Loads.Remove(SelectedLoad);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // In manual mode, we skip step-by-step validation and go straight to saving
            // The workflow service will handle the actual save logic
            await _workflowService.AdvanceToNextStepAsync();
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

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Change Mode?",
                Content = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
                PrimaryButtonText = "Yes, Change Mode",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync().AsTask();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                // Reset workflow and return to mode selection
                await _workflowService.ResetWorkflowAsync();
                _workflowService.GoToStep(WorkflowStep.ModeSelection);
                // The ReceivingWorkflowViewModel will handle the visibility update through StepChanged event
            }
        }
    }
}
