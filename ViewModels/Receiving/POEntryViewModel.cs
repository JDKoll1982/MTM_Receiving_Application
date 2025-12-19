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
    public partial class POEntryViewModel : BaseViewModel
    {
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_ReceivingWorkflow _workflowService;

        [ObservableProperty]
        private string _poNumber = string.Empty;

        [ObservableProperty]
        private string _partID = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isNonPOItem;

        [ObservableProperty]
        private bool _isLoadPOEnabled = false;

        [ObservableProperty]
        private string _poValidationMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Model_InforVisualPart> _parts = new();

        [ObservableProperty]
        private Model_InforVisualPart? _selectedPart;

        public POEntryViewModel(
            IService_InforVisual inforVisualService,
            IService_ReceivingWorkflow workflowService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger)
            : base(errorHandler, logger)
        {
            _inforVisualService = inforVisualService;
            _workflowService = workflowService;
        }

        [RelayCommand]
        private async Task LoadPOAsync()
        {
            if (string.IsNullOrWhiteSpace(PoNumber))
            {
                await _errorHandler.HandleErrorAsync("Please enter a PO number.", Enum_ErrorSeverity.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _inforVisualService.GetPOWithPartsAsync(PoNumber);
                if (result.IsSuccess && result.Data != null)
                {
                    Parts.Clear();
                    foreach (var part in result.Data.Parts)
                    {
                        Parts.Add(part);
                    }
                    _workflowService.RaiseStatusMessage($"PO {PoNumber} loaded with {Parts.Count} parts.");
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
            IsNonPOItem = !IsNonPOItem;
            Parts.Clear();
            SelectedPart = null;
            PoNumber = string.Empty;
            PartID = string.Empty;
            _workflowService.IsNonPOItem = IsNonPOItem;
        }

        [RelayCommand]
        private async Task LookupPartAsync()
        {
            if (string.IsNullOrWhiteSpace(PartID))
            {
                await _errorHandler.HandleErrorAsync("Please enter a Part ID.", Enum_ErrorSeverity.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _inforVisualService.GetPartByIDAsync(PartID);
                if (result.IsSuccess && result.Data != null)
                {
                    SelectedPart = result.Data;
                    // For Non-PO items, we might want to show it in the list or just set SelectedPart directly.
                    // Setting SelectedPart directly is enough for the workflow, but the UI might want to show it.
                    Parts.Clear();
                    Parts.Add(result.Data);
                    _workflowService.RaiseStatusMessage($"Part {PartID} found.");
                }
                else
                {
                    await _errorHandler.HandleErrorAsync(result.ErrorMessage ?? "Part not found.", Enum_ErrorSeverity.Error);
                    SelectedPart = null;
                    Parts.Clear();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPoNumberChanged(string value)
        {
            // Auto-correct PO number format: PO-NNNNNN (6 digits)
            // Accept: 66868 -> PO-066868, 066868 -> PO-066868, po-066868 -> PO-066868
            // Reject invalid formats
            
            if (string.IsNullOrWhiteSpace(value))
            {
                _workflowService.CurrentPONumber = string.Empty;
                IsLoadPOEnabled = false;
                PoValidationMessage = string.Empty;
                return;
            }

            string correctedPO = string.Empty;
            bool isValid = false;

            // Remove any whitespace
            value = value.Trim();

            // Check if value starts with "po-" or "PO-" (case insensitive)
            if (value.StartsWith("po-", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the number part after "PO-"
                string numberPart = value.Substring(3);
                
                // Check if it's all digits and validate length
                if (numberPart.All(char.IsDigit))
                {
                    if (numberPart.Length <= 6)
                    {
                        // Pad with leading zeros to make it 6 digits
                        correctedPO = $"PO-{numberPart.PadLeft(6, '0')}";
                        isValid = true;
                    }
                    else
                    {
                        // Too many digits
                        PoValidationMessage = "PO number must be 6 digits or less";
                        IsLoadPOEnabled = false;
                        return;
                    }
                }
                else
                {
                    // Contains non-numeric characters after PO-
                    PoValidationMessage = "Invalid PO format. Use: PO-NNNNNN (6 digits)";
                    IsLoadPOEnabled = false;
                    return;
                }
            }
            else if (value.All(char.IsDigit))
            {
                // Just numbers, no prefix
                if (value.Length <= 6)
                {
                    correctedPO = $"PO-{value.PadLeft(6, '0')}";
                    isValid = true;
                }
                else
                {
                    PoValidationMessage = "PO number must be 6 digits or less";
                    IsLoadPOEnabled = false;
                    return;
                }
            }
            else
            {
                // Invalid format (contains letters or special chars in wrong places)
                PoValidationMessage = "Invalid PO format. Enter: 66868 or PO-066868";
                IsLoadPOEnabled = false;
                return;
            }

            // If valid, update the field with corrected format
            if (isValid && correctedPO != value)
            {
                PoNumber = correctedPO;
            }
            
            _workflowService.CurrentPONumber = correctedPO;
            IsLoadPOEnabled = isValid;
            PoValidationMessage = isValid ? string.Empty : "Invalid PO number format";
        }

        partial void OnSelectedPartChanged(Model_InforVisualPart? value)
        {
            _workflowService.CurrentPart = value;
        }
    }
}
