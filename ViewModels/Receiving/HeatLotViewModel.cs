using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for Heat/Lot Entry step.
    /// Allows user to enter heat/lot numbers for all loads with quick-fill checkboxes.
    /// </summary>
    public partial class HeatLotViewModel : BaseStepViewModel<HeatLotData>
    {
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
            : base(workflowService, errorHandler, logger)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.HeatLotEntry;

        /// <summary>
        /// Called when this step becomes active. Load loads from session and set up heat number tracking.
        /// </summary>
        protected override Task OnNavigatedToAsync()
        {
            Loads.Clear();
            UniqueHeatNumbers.Clear();
            StepData.Loads.Clear();
            StepData.UniqueHeatNumbers.Clear();

            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                    StepData.Loads.Add(load);
                    load.PropertyChanged -= Load_PropertyChanged;
                    load.PropertyChanged += Load_PropertyChanged;
                }
            }
            
            UpdateUniqueHeatNumbers();
            return base.OnNavigatedToAsync();
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
                    StepData.UniqueHeatNumbers.RemoveAt(i);
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
                    StepData.UniqueHeatNumbers.Add(newItem);
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

        /// <summary>
        /// Validates all heat/lot numbers before advancing.
        /// </summary>
        protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
        {
            foreach (var load in StepData.Loads)
            {
                var result = _validationService.ValidateHeatLotNumber(load.HeatLotNumber);
                if (!result.IsValid)
                {
                    return Task.FromResult((false, $"Load {load.LoadNumber}: {result.Message}"));
                }
            }

            return Task.FromResult((true, string.Empty));
        }
    }
}
