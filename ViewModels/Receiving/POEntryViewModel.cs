using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.ViewModels.Shared;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ViewModels.Receiving
{
    /// <summary>
    /// ViewModel for PO Entry step.
    /// Allows user to enter PO number or mark as non-PO item, and select a part.
    /// </summary>
    public partial class POEntryViewModel : BaseStepViewModel<POEntryData>
    {
        private readonly IService_InforVisual _inforVisualService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<Model_InforVisualPart> _parts = new();

        public POEntryViewModel(
            IService_InforVisual inforVisualService,
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(workflowService, errorHandler, logger)
        {
            _inforVisualService = inforVisualService;
        }

        /// <summary>
        /// Gets the workflow step this ViewModel represents.
        /// </summary>
        protected override WorkflowStep ThisStep => WorkflowStep.POEntry;

        [RelayCommand]
        private async Task LoadPOAsync()
        {
            if (string.IsNullOrWhiteSpace(StepData.PONumber))
            {
                await _errorHandler.HandleErrorAsync("Please enter a PO number.", Enum_ErrorSeverity.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _inforVisualService.GetPOWithPartsAsync(StepData.PONumber);
                if (result.IsSuccess && result.Data != null)
                {
                    Parts.Clear();
                    foreach (var part in result.Data.Parts)
                    {
                        Parts.Add(part);
                    }
                    _workflowService.RaiseStatusMessage($"PO {StepData.PONumber} loaded with {Parts.Count} parts.");
                }
                else
                {
                    var errorMessage = !string.IsNullOrWhiteSpace(result.ErrorMessage) 
                        ? result.ErrorMessage 
                        : "PO not found or contains no parts.";
                    await _errorHandler.HandleErrorAsync(errorMessage, Enum_ErrorSeverity.Error);
                    Parts.Clear();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleNonPO()
        {
            StepData.IsNonPOItem = !StepData.IsNonPOItem;
            Parts.Clear();
            StepData.SelectedPart = null;
            StepData.PONumber = string.Empty;
            StepData.PartID = string.Empty;
        }

        [RelayCommand]
        private async Task LookupPartAsync()
        {
            if (string.IsNullOrWhiteSpace(StepData.PartID))
            {
                await _errorHandler.HandleErrorAsync("Please enter a Part ID.", Enum_ErrorSeverity.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _inforVisualService.GetPartByIDAsync(StepData.PartID);
                if (result.IsSuccess && result.Data != null)
                {
                    StepData.SelectedPart = result.Data;
                    Parts.Clear();
                    Parts.Add(result.Data);
                    _workflowService.RaiseStatusMessage($"Part {StepData.PartID} found.");
                }
                else
                {
                    await _errorHandler.HandleErrorAsync(result.ErrorMessage ?? "Part not found.", Enum_ErrorSeverity.Error);
                    StepData.SelectedPart = null;
                    Parts.Clear();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Validates PO entry and part selection before advancing.
        /// </summary>
        protected override Task<(bool IsValid, string ErrorMessage)> ValidateStepAsync()
        {
            if (string.IsNullOrEmpty(StepData.PONumber) && !StepData.IsNonPOItem)
            {
                return Task.FromResult((false, "PO Number is required."));
            }

            if (StepData.SelectedPart == null)
            {
                return Task.FromResult((false, "Part selection is required."));
            }

            return Task.FromResult((true, string.Empty));
        }

        /// <summary>
        /// Persists PO and part data to workflow service before advancing.
        /// </summary>
        protected override Task OnBeforeAdvanceAsync()
        {
            _workflowService.CurrentPONumber = StepData.PONumber;
            _workflowService.CurrentPart = StepData.SelectedPart;
            _workflowService.IsNonPOItem = StepData.IsNonPOItem;
            return Task.CompletedTask;
        }
    }
}
