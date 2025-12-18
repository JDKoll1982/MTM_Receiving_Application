using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Mode Selection step.
    /// Allows user to choose between Guided and Manual workflow modes.
    /// </summary>
    public partial class ModeSelectionViewModel : BaseStepViewModel<ModeSelectionData>
    {
        public ModeSelectionViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(workflowService, errorHandler, logger)
        {
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.ModeSelection;

        [RelayCommand]
        private void SelectGuidedMode()
        {
            _logger.LogInfo("User selected Guided Mode.");
            StepData.SelectedMode = WorkflowMode.Guided;
            _workflowService.GoToStep(WorkflowStep.POEntry);
        }

        [RelayCommand]
        private void SelectManualMode()
        {
            _logger.LogInfo("User selected Manual Mode.");
            StepData.SelectedMode = WorkflowMode.Manual;
            _workflowService.GoToStep(WorkflowStep.ManualEntry);
        }
    }
}
