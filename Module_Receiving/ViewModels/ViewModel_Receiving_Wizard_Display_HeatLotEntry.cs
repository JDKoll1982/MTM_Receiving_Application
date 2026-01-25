using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Settings;
using System;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Wizard_Display_HeatLotEntry : ViewModel_Shared_Base
    {
        private readonly IService_Receiving_Infrastructure_Workflow _workflowService;
        private readonly IService_Receiving_Infrastructure_Validation _validationService;
        private readonly IService_Help _helpService;
        private readonly IService_Receiving_Infrastructure_Settings _receivingSettings;

        [ObservableProperty]
        private ObservableCollection<Model_Receiving_Entity_ReceivingLoad> _loads = new();

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _heatLotHeaderText = "Load Entries";

        [ObservableProperty]
        private string _heatLotAutoFillText = "Auto-Fill";

        [ObservableProperty]
        private string _heatLotAutoFillTooltipText = "Fill blank heat numbers from rows above";

        [ObservableProperty]
        private string _heatLotLoadPrefixText = "Load #{0}";

        [ObservableProperty]
        private string _heatLotFieldHeaderText = "Heat/Lot Number (Optional)";

        [ObservableProperty]
        private string _heatLotFieldPlaceholderText = "Enter heat/lot number or leave blank";

        // Accessibility Properties
        [ObservableProperty]
        private string _heatLotAccessibilityName = "Heat Lot Number";

        public ViewModel_Receiving_Wizard_Display_HeatLotEntry(
            IService_Receiving_Infrastructure_Workflow workflowService,
            IService_Receiving_Infrastructure_Validation validationService,
            IService_Help helpService,
            IService_Receiving_Infrastructure_Settings receivingSettings,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService) : base(errorHandler, logger, notificationService)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;

            _workflowService.StepChanged += OnStepChanged;

            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                HeatLotHeaderText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotHeader);
                HeatLotAutoFillText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotAutoFill);
                HeatLotAutoFillTooltipText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotAutoFillTooltip);
                HeatLotLoadPrefixText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotLoadPrefix);
                HeatLotFieldHeaderText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotFieldHeader);
                HeatLotFieldPlaceholderText = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotFieldPlaceholder);
                HeatLotAccessibilityName = await _receivingSettings.GetStringAsync(Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.HeatLotNumber);

                _logger.LogInfo("Heat/Lot UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading Heat/Lot UI text from settings: {ex.Message}", ex);
            }
        }

        public string FormatHeatLotLoadLabel(int loadNumber)
        {
            return string.Format(HeatLotLoadPrefixText, loadNumber);
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.HeatLotEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public Task OnNavigatedToAsync()
        {
            Loads.Clear();

            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                }
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private void AutoFill()
        {
            // Fill down logic:
            // Iterate through loads. If a load has a blank HeatLotNumber,
            // copy it from the previous load (if available).
            for (int i = 1; i < Loads.Count; i++)
            {
                var currentLoad = Loads[i];
                var prevLoad = Loads[i - 1];

                if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(prevLoad.HeatLotNumber))
                {
                    currentLoad.HeatLotNumber = prevLoad.HeatLotNumber;
                }
            }
        }

        [RelayCommand]
        private Task ValidateAndContinueAsync()
        {
            // Set "Not Entered" for any blank heat/lot fields before advancing
            PrepareHeatLotFields();

            // Validation logic is handled by Service_ReceivingWorkflow when advancing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ensures all heat/lot fields have a value. Sets "Nothing Entered" for blank fields.
        /// </summary>
        private void PrepareHeatLotFields()
        {
            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                {
                    load.HeatLotNumber = "Nothing Entered";
                }
            }
        }

        /// <summary>
        /// Shows contextual help for heat/lot entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.HeatLot");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

