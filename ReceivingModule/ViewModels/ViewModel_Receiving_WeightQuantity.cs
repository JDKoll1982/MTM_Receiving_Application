using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ReceivingModule.ViewModels
{
    public partial class ViewModel_Receiving_WeightQuantity : Shared_BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_Help _helpService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private string _warningMessage = string.Empty;

        [ObservableProperty]
        private bool _hasWarning;

        [ObservableProperty]
        private string _poQuantityInfo = string.Empty;

        public ViewModel_Receiving_WeightQuantity(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_InforVisual inforVisualService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _inforVisualService = inforVisualService;
            _helpService = helpService;

            _workflowService.StepChanged += OnStepChanged;
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

            UpdatePOQuantityInfo();
            await CheckSameDayReceivingAsync();
        }

        private void UpdatePOQuantityInfo()
        {
            if (_workflowService.CurrentSession.IsNonPO)
            {
                PoQuantityInfo = "Non-PO Item";
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
                    WarningMessage = $"Warning: {result.Data:N2} of this part has already been received today on this PO.";
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
                    WarningMessage = $"Warning: Total quantity ({totalWeight:N2}) exceeds PO ordered amount ({_workflowService.CurrentPart.QtyOrdered:N2}).";
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
