using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class Receiving_ManualEntryViewModel : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_Receiving _mysqlService;
        private readonly IService_Window _windowService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        public ObservableCollection<Enum_PackageType> PackageTypes { get; } = new(Enum.GetValues<Enum_PackageType>());

        public Receiving_ManualEntryViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_Receiving mysqlService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Window windowService)
            : base(errorHandler, logger)
        {
            _windowService = windowService;
            _workflowService = workflowService;
            _mysqlService = mysqlService;
            _loads = new ObservableCollection<Model_ReceivingLoad>(_workflowService.CurrentSession.Loads);
        }

        [RelayCommand]
        private async Task AutoFillAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Filling blank spaces...";
                _logger.LogInfo("Starting Auto-Fill Blank Spaces operation");

                int filledCount = 0;

                // Iterate through all loads in the grid
                for (int i = 0; i < Loads.Count; i++)
                {
                    var currentLoad = Loads[i];

                    // 1. Part ID (Material ID) - Fill Down Logic
                    // If blank, copy from immediate predecessor
                    if (string.IsNullOrWhiteSpace(currentLoad.PartID))
                    {
                        if (i > 0)
                        {
                            var prev = Loads[i - 1];
                            if (!string.IsNullOrWhiteSpace(prev.PartID))
                            {
                                currentLoad.PartID = prev.PartID;
                                filledCount++;
                            }
                        }
                    }

                    // 2. Other Fields - Copy from nearest previous row with SAME PartID
                    if (!string.IsNullOrWhiteSpace(currentLoad.PartID))
                    {
                        // Find source load (nearest previous row with same PartID)
                        Model_ReceivingLoad? sourceLoad = null;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (Loads[j].PartID == currentLoad.PartID)
                            {
                                sourceLoad = Loads[j];
                                break;
                            }
                        }

                        if (sourceLoad != null)
                        {
                            // PO Number: If blank, use PO from previous row with same Material ID
                            if (string.IsNullOrWhiteSpace(currentLoad.PoNumber) && !string.IsNullOrWhiteSpace(sourceLoad.PoNumber))
                            {
                                currentLoad.PoNumber = sourceLoad.PoNumber;
                                filledCount++;
                            }

                            // Quantity: If blank (0), use Quantity from previous row with same Material ID
                            if (currentLoad.WeightQuantity <= 0 && sourceLoad.WeightQuantity > 0)
                            {
                                currentLoad.WeightQuantity = sourceLoad.WeightQuantity;
                                filledCount++;
                            }

                            // Heat: If blank, use Heat from previous row with same Material ID
                            if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(sourceLoad.HeatLotNumber))
                            {
                                currentLoad.HeatLotNumber = sourceLoad.HeatLotNumber;
                                filledCount++;
                            }

                            // Also copy other useful fields if blank, consistent with "same material" logic
                            if (currentLoad.PackagesPerLoad <= 0 && sourceLoad.PackagesPerLoad > 0)
                            {
                                currentLoad.PackagesPerLoad = sourceLoad.PackagesPerLoad;
                            }

                            if (string.IsNullOrWhiteSpace(currentLoad.PackageTypeName) && !string.IsNullOrWhiteSpace(sourceLoad.PackageTypeName))
                            {
                                currentLoad.PackageTypeName = sourceLoad.PackageTypeName;
                            }
                        }
                    }

                    // Date: If blank (default/min value), use current date
                    // Note: Model_ReceivingLoad initializes ReceivedDate to DateTime.Now usually, but let's ensure it.
                    if (currentLoad.ReceivedDate == default || currentLoad.ReceivedDate == DateTime.MinValue)
                    {
                        currentLoad.ReceivedDate = DateTime.Now;
                    }

                    // Employee: If blank, use employee from first row
                    // Assuming "Employee" maps to UserId or similar. 
                    // Model_ReceivingLoad has UserId.
                    if (string.IsNullOrWhiteSpace(currentLoad.UserId))
                    {
                        // Try previous row first (propagation)
                        if (i > 0 && !string.IsNullOrWhiteSpace(Loads[i - 1].UserId))
                        {
                            currentLoad.UserId = Loads[i - 1].UserId;
                        }
                        else if (Loads.Count > 0 && !string.IsNullOrWhiteSpace(Loads[0].UserId))
                        {
                            currentLoad.UserId = Loads[0].UserId;
                        }
                        // If first row is also blank, maybe use current session user?
                        else if (_workflowService.CurrentSession.User != null)
                        {
                            currentLoad.UserId = _workflowService.CurrentSession.User.WindowsUsername;
                        }
                    }
                }

                StatusMessage = $"Auto-fill complete. Updated {filledCount} fields.";
                _logger.LogInfo($"Auto-Fill Blank Spaces completed. Updated {filledCount} fields across {Loads.Count} rows.");

                // Force UI update if needed (ObservableCollection updates properties automatically if INPC is fired)
                // Model_ReceivingLoad implements ObservableObject so it should be fine.
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auto-fill failed: {ex.Message}");
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
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null) return;

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
                XamlRoot = xamlRoot
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

                // Set default Heat/Lot if empty
                foreach (var load in Loads)
                {
                    if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                    {
                        load.HeatLotNumber = "Nothing Entered";
                    }
                }

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
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
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
                XamlRoot = xamlRoot
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
