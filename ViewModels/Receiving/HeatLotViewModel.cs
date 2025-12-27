using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    public partial class HeatLotViewModel : BaseViewModel
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        [ObservableProperty]
        private ObservableCollection<Model_HeatCheckboxItem> _uniqueHeatNumbers = new();

        public HeatLotViewModel(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;

            _workflowService.StepChanged += OnStepChanged;
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == WorkflowStep.HeatLotEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public Task OnNavigatedToAsync()
        {
            Loads.Clear();
            UniqueHeatNumbers.Clear();

            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                    load.PropertyChanged -= Load_PropertyChanged; // Unsubscribe first to be safe
                    load.PropertyChanged += Load_PropertyChanged;
                }
            }

            UpdateUniqueHeatNumbers();
            return Task.CompletedTask;
        }

        private void Load_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model_ReceivingLoad.HeatLotNumber))
            {
                UpdateUniqueHeatNumbers();
            }
        }

        private void UpdateUniqueHeatNumbers()
        {
            var currentHeats = Loads
                .Where(l => !string.IsNullOrWhiteSpace(l.HeatLotNumber))
                .GroupBy(l => l.HeatLotNumber)
                .Select(g => new { Heat = g.Key, FirstLoad = g.Min(l => l.LoadNumber) })
                .ToList();

            // Remove items no longer present
            for (int i = UniqueHeatNumbers.Count - 1; i >= 0; i--)
            {
                var item = UniqueHeatNumbers[i];
                if (!currentHeats.Any(h => h.Heat == item.HeatLotNumber))
                {
                    item.PropertyChanged -= HeatCheckboxItem_PropertyChanged;
                    UniqueHeatNumbers.RemoveAt(i);
                }
            }

            // Add new items
            foreach (var heat in currentHeats)
            {
                if (!UniqueHeatNumbers.Any(i => i.HeatLotNumber == heat.Heat))
                {
                    var newItem = new Model_HeatCheckboxItem
                    {
                        HeatLotNumber = heat.Heat,
                        FirstLoadNumber = heat.FirstLoad,
                        IsChecked = false
                    };
                    newItem.PropertyChanged += HeatCheckboxItem_PropertyChanged;
                    UniqueHeatNumbers.Add(newItem);
                }
            }
        }

        private void HeatCheckboxItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model_HeatCheckboxItem.IsChecked))
            {
                if (sender is Model_HeatCheckboxItem item && item.IsChecked)
                {
                    ApplyHeatToEmptyLoads(item.HeatLotNumber);
                    // Optional: Uncheck after applying? 
                    // Or keep checked to indicate "this is the active heat"?
                    // Let's keep it checked for now.
                }
            }
        }

        private void ApplyHeatToEmptyLoads(string heatNumber)
        {
            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                {
                    load.HeatLotNumber = heatNumber;
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
        /// Ensures all heat/lot fields have a value. Sets "Not Entered" for blank fields.
        /// </summary>
        private void PrepareHeatLotFields()
        {
            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                {
                    load.HeatLotNumber = "Not Entered";
                }
            }
        }
    }
}
