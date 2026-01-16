using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_HeatLot : ViewModel_Shared_Base
    {
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_ReceivingValidation _validationService;
        private readonly IService_Help _helpService;

        [ObservableProperty]
        private ObservableCollection<Model_ReceivingLoad> _loads = new();

        public ViewModel_Receiving_HeatLot(
            IService_ReceivingWorkflow workflowService,
            IService_ReceivingValidation validationService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _workflowService = workflowService;
            _validationService = validationService;
            _helpService = helpService;

            _workflowService.StepChanged += OnStepChanged;
        }

        private void OnStepChanged(object? sender, System.EventArgs e)
        {
            if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.HeatLotEntry)
            {
                _ = OnNavigatedToAsync();
            }
        }

        public Task OnNavigatedToAsync()
        {
            Loads.Clear();

            if (_workflowService.CurrentSession?.Loads != null)
            {
                foreach (var load in _workflowService.CurrentSession.Loads)
                {
                    Loads.Add(load);
                }
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private void AutoFill()
        {
            // Fill down logic:
            // Iterate through loads. If a load has a blank HeatLotNumber,
            // copy it from the previous load (if available).
            for (int i = 1; i < Loads.Count; i++)
            {
                var currentLoad = Loads[i];
                var prevLoad = Loads[i - 1];

                if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(prevLoad.HeatLotNumber))
                {
                    currentLoad.HeatLotNumber = prevLoad.HeatLotNumber;
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
        /// Ensures all heat/lot fields have a value. Sets "Nothing Entered" for blank fields.
        /// </summary>
        private void PrepareHeatLotFields()
        {
            foreach (var load in Loads)
            {
                if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                {
                    load.HeatLotNumber = "Nothing Entered";
                }
            }
        }

        /// <summary>
        /// Shows contextual help for heat/lot entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.HeatLot");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

