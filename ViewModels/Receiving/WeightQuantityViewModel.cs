using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Weight/Quantity Entry step.
    /// Allows user to enter weight/quantity values for all loads.
    /// </summary>
    public partial class WeightQuantityViewModel : BaseStepViewModel<WeightQuantityData>
    {
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_InforVisual _inforVisualService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        public WeightQuantityViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_InforVisual inforVisualService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(workflowService, errorHandler, logger)
        {
            _validationService = validationService;
            _inforVisualService = inforVisualService;
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.WeightQuantityEntry;

        /// <summary>
        /// Called when this step becomes active. Load loads from session and check for warnings.
        /// </summary>
        protected override async Task OnNavigatedToAsync()
        {
            // Refresh loads from session
            Loads.Clear();
            StepData.Loads.Clear();
            
            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                    StepData.Loads.Add(load);
                }
            }

            UpdatePOQuantityInfo();
            await CheckSameDayReceivingAsync();
            
            await base.OnNavigatedToAsync();
        }

        private void UpdatePOQuantityInfo()
        {
            if (_workflowService.CurrentSession.IsNonPO)
            {
                StepData.POQuantityInfo = "Non-PO Item";
            }
            else if (_workflowService.CurrentPart != null)
            {
                StepData.POQuantityInfo = $"Ordered: {_workflowService.CurrentPart.QtyOrdered:N2}";
            }
            else
            {
                StepData.POQuantityInfo = string.Empty;
            }
        }

        private async Task CheckSameDayReceivingAsync()
        {
            StepData.HasWarning = false;
            StepData.WarningMessage = string.Empty;

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
                    StepData.HasWarning = true;
                    StepData.WarningMessage = $"Warning: {result.Data:N2} of this part has already been received today on this PO.";
                }
            }
            catch (Exception ex)
            {
                await _errorHandler.LogErrorAsync("Failed to check same-day receiving", Enum_ErrorSeverity.Warning, ex);
            }
        }

        /// <summary>
        /// Validates all load weights before advancing to next step.
        /// </summary>
        protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
        {
            foreach (var load in StepData.Loads)
            {
                var result = _validationService.ValidateWeightQuantity(load.WeightQuantity);
                if (!result.IsValid)
                {
                    return Task.FromResult((false, $"Load {load.LoadNumber}: {result.Message}"));
                }
            }

            return Task.FromResult((true, string.Empty));
        }
    }
}
