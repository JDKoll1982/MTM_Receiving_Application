using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;
using Windows.Foundation;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class ManualEntryViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_Receiving _mysqlService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        public ObservableCollection<Enum_PackageType> PackageTypes { get; } = new(Enum.GetValues<Enum_PackageType>());

        public ManualEntryViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_Receiving mysqlService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _mysqlService = mysqlService;
            _loads = new ObservableCollection<Model_ReceivingLoad>(_workflowService.CurrentSession.Loads);
        }

        [RelayCommand]
        private async Task AutoFillAsync()
        {
            if (SelectedLoad == null)
            {
                _logger.LogWarning("AutoFill attempted with no selected load");
                await _errorHandler.HandleErrorAsync("Please select a row to auto-fill.", Enum_ErrorSeverity.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedLoad.PartID))
            {
                _logger.LogWarning($"AutoFill attempted with empty PartID for LoadNumber {SelectedLoad.LoadNumber}");
                await _errorHandler.HandleErrorAsync("Please enter a Part ID first.", Enum_ErrorSeverity.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Searching history...";
                _logger.LogInfo($"Auto-filling data for Part ID: {SelectedLoad.PartID}");

                // Look back 1 year for history
                var endDate = DateTime.Now;
                var startDate = endDate.AddYears(-1);

                var history = await _mysqlService.GetReceivingHistoryAsync(SelectedLoad.PartID, startDate, endDate);

                if (history != null && history.Count > 0)
                {
                    // Get the most recent entry (assuming list is sorted or we sort it)
                    // The service contract doesn't specify sort order, so let's sort by date descending
                    var lastEntry = history.OrderByDescending(h => h.ReceivedDate).First();

                    // Auto-fill fields
                    SelectedLoad.PoNumber = lastEntry.PoNumber;
                    SelectedLoad.HeatLotNumber = lastEntry.HeatLotNumber;
                    SelectedLoad.WeightQuantity = lastEntry.WeightQuantity;
                    SelectedLoad.PackagesPerLoad = lastEntry.PackagesPerLoad;
                    SelectedLoad.PackageTypeName = lastEntry.PackageTypeName;
                    SelectedLoad.PartType = lastEntry.PartType;
                    
                    // If it was a non-PO item before, assume it still is? 
                    // Or check if PO exists? Let's just copy the flag if we have it, 
                    // but Model_ReceivingLoad has IsNonPOItem.
                    SelectedLoad.IsNonPOItem = lastEntry.IsNonPOItem;

                    StatusMessage = "Auto-filled from last entry.";
                    _logger.LogInfo($"Successfully auto-filled Part ID {SelectedLoad.PartID} from history dated {lastEntry.ReceivedDate}");
                }
                else
                {
                    _logger.LogWarning($"No receiving history found for Part ID: {SelectedLoad.PartID}");
                    await _errorHandler.HandleErrorAsync($"No history found for Part ID: {SelectedLoad.PartID}", Enum_ErrorSeverity.Info);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auto-fill failed for Part ID {SelectedLoad?.PartID}: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Auto-fill failed", Enum_ErrorSeverity.Error, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void AddRow()
        {
            AddNewLoad();
        }

        [RelayCommand]
        private async Task AddMultipleRowsAsync()
        {
            // Prompt for number of rows
            if (App.MainWindow?.Content?.XamlRoot == null) return;

            var inputTextBox = new Microsoft.UI.Xaml.Controls.TextBox
            {
                PlaceholderText = "Enter number of rows (1-50)",
                InputScope = new Microsoft.UI.Xaml.Input.InputScope
                {
                    Names = { new Microsoft.UI.Xaml.Input.InputScopeName(Microsoft.UI.Xaml.Input.InputScopeNameValue.Number) }
                }
            };

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Add Multiple Rows",
                Content = inputTextBox,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                if (int.TryParse(inputTextBox.Text, out int count) && count > 0 && count <= 50)
                {
                    _logger.LogInfo($"Adding {count} new rows to manual entry grid");
                    for (int i = 0; i < count; i++)
                    {
                        AddNewLoad();
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid row count entered: {inputTextBox.Text}");
                    await _errorHandler.HandleErrorAsync("Please enter a valid number between 1 and 50.", Enum_ErrorSeverity.Warning);
                }
            }
        }

        public void AddNewLoad()
        {
            var newLoad = new Model_ReceivingLoad
            {
                LoadID = System.Guid.NewGuid(),
                ReceivedDate = System.DateTime.Now,
                LoadNumber = Loads.Count + 1
            };
            Loads.Add(newLoad);
            _workflowService.CurrentSession.Loads.Add(newLoad);
            
            // Select the new load
            SelectedLoad = newLoad;
        }

        [RelayCommand]
        private void RemoveRow()
        {
            if (SelectedLoad != null)
            {
                _logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
                _workflowService.CurrentSession.Loads.Remove(SelectedLoad);
                Loads.Remove(SelectedLoad);
            }
            else
            {
                _logger.LogWarning("RemoveRow called with no selected load");
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                _logger.LogInfo($"Saving {Loads.Count} loads from manual entry");
                // In manual mode, we skip step-by-step validation and go straight to saving
                // The workflow service will handle the actual save logic
                await _workflowService.AdvanceToNextStepAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save manual entry data: {ex.Message}");
                await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
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
                try
                {
                    _logger.LogInfo("User confirmed return to mode selection, resetting workflow");
                    // Reset workflow and return to mode selection
                    await _workflowService.ResetWorkflowAsync();
                    _workflowService.GoToStep(WorkflowStep.ModeSelection);
                    // The ReceivingWorkflowViewModel will handle the visibility update through StepChanged event
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to reset workflow: {ex.Message}");
                    await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
                }
            }
            else
            {
                _logger.LogInfo("User cancelled return to mode selection");
            }
        }
    }
}
