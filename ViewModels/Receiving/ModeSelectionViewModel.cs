using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class ModeSelectionViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;

        public ModeSelectionViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
        }

        [RelayCommand]
        private void SelectGuidedMode()
        {
            _logger.LogInfo("User selected Guided Mode.");
            _workflowService.GoToStep(WorkflowStep.POEntry);
        }

        [RelayCommand]
        private void SelectManualMode()
        {
            _logger.LogInfo("User selected Manual Mode.");
            _workflowService.GoToStep(WorkflowStep.ManualEntry);
        }
    }
}
