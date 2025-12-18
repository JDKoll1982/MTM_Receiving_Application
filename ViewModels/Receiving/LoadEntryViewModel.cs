using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Load Entry step.
    /// Allows user to specify the number of loads/skids to create.
    /// </summary>
    public partial class LoadEntryViewModel : BaseStepViewModel<LoadEntryData>
    {
        private readonly IService_ReceivingValidation _validationService;

        public LoadEntryViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(workflowService, errorHandler, logger)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.LoadEntry;

        /// <summary>
        /// Called when this step becomes active. Load current state from workflow service.
        /// </summary>
        protected override Task OnNavigatedToAsync()
        {
            StepData.NumberOfLoads = _workflowService.NumberOfLoads;
            
            // Update part info if available
            if (_workflowService.CurrentPart != null)
            {
                StepData.SelectedPartInfo = $"{_workflowService.CurrentPart.PartID} - {_workflowService.CurrentPart.Description}";
            }
            
            return base.OnNavigatedToAsync();
        }

        /// <summary>
        /// Validates the number of loads before advancing.
        /// </summary>
        protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
        {
            var validationResult = _validationService.ValidateNumberOfLoads(StepData.NumberOfLoads);
            if (!validationResult.IsValid)
            {
                return Task.FromResult((false, validationResult.Message));
            }

            return Task.FromResult((true, string.Empty));
        }

        /// <summary>
        /// Persists the number of loads to the workflow service before advancing.
        /// </summary>
        protected override Task OnBeforeAdvanceAsync()
        {
            _workflowService.NumberOfLoads = StepData.NumberOfLoads;
            return Task.CompletedTask;
        }
    }
}
