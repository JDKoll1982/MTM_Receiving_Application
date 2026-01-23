using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_ManualEntry : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_MySQL_Receiving _mysqlService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Window _windowService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingSettings _receivingSettings;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _manualEntryAddRowText = "Add Row";

        [ObservableProperty]
        private string _manualEntryAddMultipleText = "Add Multiple";

        [ObservableProperty]
        private string _manualEntryRemoveRowText = "Remove Row";

        [ObservableProperty]
        private string _manualEntryAutoFillText = "Auto-Fill";

        [ObservableProperty]
        private string _manualEntrySaveAndFinishText = "Save & Finish";

        [ObservableProperty]
        private string _manualEntryColumnLoadNumberText = "Load #";

        [ObservableProperty]
        private string _manualEntryColumnPartIdText = "Part ID";

        [ObservableProperty]
        private string _manualEntryColumnWeightQtyText = "Weight/Qty";

        [ObservableProperty]
        private string _manualEntryColumnHeatLotText = "Heat/Lot";

        [ObservableProperty]
        private string _manualEntryColumnPkgTypeText = "Pkg Type";

        [ObservableProperty]
        private string _manualEntryColumnPkgsPerLoadText = "Pkgs/Load";

        [ObservableProperty]
        private string _manualEntryColumnWtPerPkgText = "Wt/Pkg";

        public ObservableCollection<Enum_PackageType> PackageTypes { get; } = new(Enum.GetValues<Enum_PackageType>());

        public ViewModel_Receiving_ManualEntry(
            IService_ReceivingWorkflow workflowService,
            IService_MySQL_Receiving mysqlService,
            IService_ReceivingValidation validationService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Window windowService,
            IService_Help helpService,
            IService_ReceivingSettings receivingSettings,
            IService_Notification notificationService)
            : base(errorHandler, logger, notificationService)
        {
            _windowService = windowService;
            _workflowService = workflowService;
            _mysqlService = mysqlService;
            _validationService = validationService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;
            _loads = new ObservableCollection<Model_ReceivingLoad>(_workflowService.CurrentSession.Loads);

            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                ManualEntryAddRowText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryAddRow);
                ManualEntryAddMultipleText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryAddMultiple);
                ManualEntryRemoveRowText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryRemoveRow);
                ManualEntryAutoFillText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryAutoFill);
                ManualEntrySaveAndFinishText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntrySaveAndFinish);

                ManualEntryColumnLoadNumberText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnLoadNumber);
                ManualEntryColumnPartIdText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnPartId);
                ManualEntryColumnWeightQtyText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnWeightQty);
                ManualEntryColumnHeatLotText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnHeatLot);
                ManualEntryColumnPkgTypeText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnPkgType);
                ManualEntryColumnPkgsPerLoadText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnPkgsPerLoad);
                ManualEntryColumnWtPerPkgText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.ManualEntryColumnWtPerPkg);

                _logger.LogInfo("Manual Entry UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading Manual Entry UI text from settings: {ex.Message}", ex);
            }
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
            if (xamlRoot == null)
            {
                return;
            }

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
                Title = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleTitle),
                Content = inputTextBox,
                PrimaryButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleAdd),
                CloseButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleCancel),
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
            if (IsBusy)
            {
                return; // Prevent re-entry
            }

            try
            {
                IsBusy = true;
                _logger.LogInfo($"Saving {Loads.Count} loads from manual entry");

                // Set default Heat/Lot if empty
                foreach (var load in Loads)
                {
                    if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                    {
                        load.HeatLotNumber = "Nothing Entered";
                    }

                    // Check for quality hold requirement on each load
                    if (!string.IsNullOrWhiteSpace(load.PartID))
                    {
                        var (isRestricted, restrictionType) = await _validationService.IsRestrictedPartAsync(load.PartID);
                        if (isRestricted)
                        {
                            load.IsQualityHoldRequired = true;
                            load.QualityHoldRestrictionType = restrictionType;
                        }
                    }
                }

                // Check if any loads have quality holds that haven't been acknowledged yet
                var loadsWithUnacknowledgedHolds = Loads
                    .Where(l => l.IsQualityHoldRequired && !l.IsQualityHoldAcknowledged)
                    .ToList();
                
                if (loadsWithUnacknowledgedHolds.Count > 0)
                {
                    // Show confirmation dialog for quality hold acknowledgment
                    var acknowledged = await ShowQualityHoldConfirmationAsync(loadsWithUnacknowledgedHolds);
                    if (!acknowledged)
                    {
                        _logger.LogInfo("User cancelled save due to unacknowledged quality holds");
                        return; // Block save if user doesn't acknowledge
                    }
                    
                    // Mark all as acknowledged
                    foreach (var load in loadsWithUnacknowledgedHolds)
                    {
                        load.IsQualityHoldAcknowledged = true;
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
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Shows a confirmation dialog for quality hold acknowledgment before allowing save.
        /// </summary>
        /// <param name="loadsWithHolds">List of loads requiring quality acknowledgment</param>
        /// <returns>True if user acknowledges; false if cancelled</returns>
        private async Task<bool> ShowQualityHoldConfirmationAsync(System.Collections.Generic.List<Model_ReceivingLoad> loadsWithHolds)
        {
            ArgumentNullException.ThrowIfNull(loadsWithHolds);
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show quality hold dialog: XamlRoot is null");
                await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
                return false;
            }

            // Build list of restricted parts
            var restrictedPartsList = string.Join(
                "\n",
                loadsWithHolds.Select(l => $"  • {l.PartID} ({l.QualityHoldRestrictionType})")
            );

            var content = $"The following parts require quality hold acknowledgment:\n\n{restrictedPartsList}\n\n" +
                         "You must contact quality immediately and quality MUST accept the load before any paperwork is signed.\n\n" +
                         "Has quality accepted these loads?";

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Quality Hold Acknowledgment Required",
                Content = content,
                PrimaryButtonText = "Yes - Quality Accepted",
                CloseButtonText = "Cancel - Do Not Save",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync().AsTask();
            return result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
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
                Title = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeTitle),
                Content = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeContent),
                PrimaryButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeConfirm),
                CloseButtonText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Dialogs.ConfirmChangeModeCancel),
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
                    _workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
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

        /// <summary>
        /// Shows contextual help for manual entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.ManualEntry");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}


