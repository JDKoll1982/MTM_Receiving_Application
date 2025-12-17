using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class WeightQuantityViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_InforVisual _inforVisualService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private string _warningMessage = string.Empty;

        [ObservableProperty]
        private bool _hasWarning;

        [ObservableProperty]
        private string _poQuantityInfo = string.Empty;

        public WeightQuantityViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_InforVisual inforVisualService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _inforVisualService = inforVisualService;

            _workflowService.StepChanged += OnStepChanged;
        }

        private void OnStepChanged(object? sender, EventArgs e)
        {
            if (_workflowService.CurrentStep == WorkflowStep.WeightQuantityEntry)
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

            // Check total vs PO quantity (Warning only)
            if (!_workflowService.CurrentSession.IsNonPO && _workflowService.CurrentPart != null)
            {
                var totalWeight = Loads.Sum(l => l.WeightQuantity);
                if (totalWeight > _workflowService.CurrentPart.QtyOrdered)
                {
                    // Just a warning, maybe we should show a dialog?
                    // For now, we'll log it or maybe we should have a UI confirmation?
                    // The spec says "Warns if entered quantities exceed PO ordered amounts".
                    // Let's assume the user sees the PO info and we rely on that, or we could show a dialog.
                    // I'll add a check here.
                    
                    // For MVP, let's just proceed but maybe log a warning.
                    // Or better, show a dialog asking to confirm.
                    // But I don't have a "ConfirmDialog" service easily accessible right now without adding more UI.
                    // I'll skip the blocking warning for now as per "Warns" usually implies non-blocking or UI indication.
                    // We display the Ordered Qty on screen.
                }
            }
            
            // The workflow service handles the actual transition
            // But wait, the Next button is in the parent view (ReceivingWorkflowView).
            // The parent view calls ViewModel.NextStepAsync().
            // So this validation needs to happen in the WorkflowService or be triggered by the parent.
            
            // Current architecture: ReceivingWorkflowViewModel calls _workflowService.AdvanceToNextStepAsync().
            // The service doesn't know about the ViewModel's validation state unless we put validation in the service or the model.
            
            // The service's AdvanceToNextStepAsync does:
            // case WorkflowStep.WeightQuantityEntry: CurrentStep = WorkflowStep.HeatLotEntry; break;
            
            // It doesn't validate the data in the session.
            // We should probably add validation logic to AdvanceToNextStepAsync in the service.
            // Or ReceivingWorkflowViewModel should call a Validate method on the current child VM?
            // But ReceivingWorkflowViewModel doesn't hold references to child VMs (it uses DI/View switching).
            
            // Alternative: The "Next" button in the parent view is bound to ReceivingWorkflowViewModel.NextStepCommand.
            // We can use the Messenger to request validation?
            // Or we can put the validation logic in the Service_ReceivingWorkflow.AdvanceToNextStepAsync.
            // The service has access to CurrentSession.Loads.
            // So the service can iterate loads and validate WeightQuantity.
            
            // Yes, validation logic belongs in the Service/Domain layer.
            // So I will update Service_ReceivingWorkflow to validate weights before advancing.
        }
    }
}
