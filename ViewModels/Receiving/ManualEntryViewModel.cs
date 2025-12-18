using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class ManualEntryViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads;

        [ObservableProperty]
        private Model_ReceivingLoad? _selectedLoad;

        public ManualEntryViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _loads = new ObservableCollection<Model_ReceivingLoad>(_workflowService.CurrentSession.Loads);
        }

        [RelayCommand]
        private void AddRow()
        {
            var newLoad = new Model_ReceivingLoad
            {
                LoadID = System.Guid.NewGuid(),
                ReceivedDate = System.DateTime.Now,
                LoadNumber = Loads.Count + 1
            };
            Loads.Add(newLoad);
            _workflowService.CurrentSession.Loads.Add(newLoad);
        }

        [RelayCommand]
        private void RemoveRow()
        {
            if (SelectedLoad != null)
            {
                _workflowService.CurrentSession.Loads.Remove(SelectedLoad);
                Loads.Remove(SelectedLoad);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // In manual mode, we skip step-by-step validation and go straight to saving
            // The workflow service will handle the actual save logic
            await _workflowService.AdvanceToNextStepAsync();
        }
    }
}
