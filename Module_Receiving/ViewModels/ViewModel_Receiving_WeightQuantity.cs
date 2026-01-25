using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Receiving.Settings;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Wizard_Display_WeightQuantityEntry : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_Help _helpService;
        private readonly IService_ReceivingSettings _receivingSettings;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private string _warningMessage = string.Empty;

        [ObservableProperty]
        private bool _hasWarning;

        [ObservableProperty]
        private string _poQuantityInfo = string.Empty;

        // UI Text Properties (Loaded from Settings)
        [ObservableProperty]
        private string _weightQuantityHeaderText = "Weight/Quantity";

        [ObservableProperty]
        private string _weightQuantityPlaceholderText = "Enter whole number";

        // Accessibility Properties
        [ObservableProperty]
        private string _weightQuantityAccessibilityName = "Weight Quantity";

        public ViewModel_Receiving_Wizard_Display_WeightQuantityEntry(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_InforVisual inforVisualService,
            IService_Help helpService,
            IService_ReceivingSettings receivingSettings,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService)
            : base(errorHandler, logger, notificationService)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _inforVisualService = inforVisualService;
            _helpService = helpService;
            _receivingSettings = receivingSettings;

            _workflowService.StepChanged += OnStepChanged;

            _ = LoadUITextAsync();
        }

        private async Task LoadUITextAsync()
        {
            try
            {
                WeightQuantityHeaderText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WeightQuantityHeader);
                WeightQuantityPlaceholderText = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.UiText.WeightQuantityPlaceholder);
                WeightQuantityAccessibilityName = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Accessibility.WeightQuantityInput);

                _logger.LogInfo("Weight/Quantity UI text loaded from settings successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading Weight/Quantity UI text from settings: {ex.Message}", ex);
            }
        }

        private void OnStepChanged(object? sender, EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.WeightQuantityEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public async Task OnNavigatedToAsync()
        {
            // Refresh loads from session
            Loads.Clear();
            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                }
            }

            await UpdatePOQuantityInfoAsync();
            await CheckSameDayReceivingAsync();
        }

        private async Task UpdatePOQuantityInfoAsync()
        {
            if (_workflowService.CurrentSession.IsNonPO)
            {
                PoQuantityInfo = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.Messages.InfoNonPoItem);
            }
            else if (_workflowService.CurrentPart != null)
            {
                PoQuantityInfo = $"Ordered: {_workflowService.CurrentPart.QtyOrdered:N2}";
            }
            else
            {
                PoQuantityInfo = string.Empty;
            }
        }

        private async Task CheckSameDayReceivingAsync()
        {
            HasWarning = false;
            WarningMessage = string.Empty;

            if (_workflowService.CurrentSession.IsNonPO)
            {
                return;
            }

            var poNumber = _workflowService.CurrentPONumber;
            var partId = _workflowService.CurrentPart?.PartID;

            if (string.IsNullOrEmpty(poNumber) || string.IsNullOrEmpty(partId))
            {
                return;
            }

            try
            {
                var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partId, DateTime.Today);
                if (result.IsSuccess && result.Data > 0)
                {
                    HasWarning = true;
                    WarningMessage = await _receivingSettings.FormatAsync(ReceivingSettingsKeys.Messages.WarningSameDayReceiving, result.Data);
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.LogErrorAsync("Failed to check same-day receiving", Enum_ErrorSeverity.Warning, ex);
            }
        }

        [RelayCommand]
        private async Task ValidateAndContinueAsync()
        {
            // Validate all loads have weight > 0
            foreach (var load in Loads)
            {
                var result = _validationService.ValidateWeightQuantity(load.WeightQuantity);
                if (!result.IsValid)
                {
                    await _errorHandler.HandleErrorAsync($"Load {load.LoadNumber}: {result.Message}", Enum_ErrorSeverity.Warning);
                    return;
                }
            }

            if (!_workflowService.CurrentSession.IsNonPO && _workflowService.CurrentPart != null)
            {
                var totalWeight = Loads.Sum(l => l.WeightQuantity);
                if (totalWeight > _workflowService.CurrentPart.QtyOrdered)
                {
                    WarningMessage = await _receivingSettings.FormatAsync(ReceivingSettingsKeys.Messages.WarningExceedsOrdered, totalWeight, _workflowService.CurrentPart.QtyOrdered);
                    HasWarning = true;
                }
            }
        }

        /// <summary>
        /// Shows contextual help for weight/quantity entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.WeightQuantity");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

