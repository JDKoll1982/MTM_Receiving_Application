using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Review step.
    /// Allows user to review and edit all loads before final submission.
    /// </summary>
    public partial class ReviewGridViewModel : BaseStepViewModel<ReviewData>
    {
        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        public ReviewGridViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(workflowService, errorHandler, logger)
        {
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.Review;

        /// <summary>
        /// Called when this step becomes active. Load all loads for final review.
        /// </summary>
        protected override Task OnNavigatedToAsync()
        {
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
            
            return base.OnNavigatedToAsync();
        }

        [RelayCommand]
        private async Task AddAnotherPartAsync()
        {
            // Add current loads to session and reset for next part entry
            await _workflowService.AddCurrentPartToSessionAsync();
            
            // Navigate back to PO Entry for next part
            _workflowService.GoToStep(WorkflowStep.POEntry);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Advance to Saving step
            await _workflowService.AdvanceToNextStepAsync();
        }

        /// <summary>
        /// Handles cascading updates when a load property changes in the review grid.
        /// Updates all other loads with the same value for PO# and Part# fields.
        /// </summary>
        public void HandleCascadingUpdate(Model_ReceivingLoad changedLoad, string propertyName)
        {
            if (changedLoad == null) return;

            var index = Loads.IndexOf(changedLoad);
            if (index < 0) return;

            // Cascade PO Number changes to all loads
            if (propertyName == nameof(Model_ReceivingLoad.PoNumber))
            {
                foreach (var load in Loads)
                {
                    if (load != changedLoad)
                    {
                        load.PoNumber = changedLoad.PoNumber;
                    }
                }
            }
            // Cascade Part ID changes to all loads
            else if (propertyName == nameof(Model_ReceivingLoad.PartID))
            {
                foreach (var load in Loads)
                {
                    if (load != changedLoad)
                    {
                        load.PartID = changedLoad.PartID;
                    }
                }
            }
        }
    }
}
