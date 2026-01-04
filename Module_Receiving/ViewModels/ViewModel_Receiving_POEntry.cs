using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base
    {
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;

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

        [ObservableProperty]
        private string _packageType = "Skids";  // Default package type

        [ObservableProperty]
        private string _poStatus = string.Empty;

        [ObservableProperty]
        private string _poStatusDescription = string.Empty;

        [ObservableProperty]
        private bool _isPOClosed = false;

        public ViewModel_Receiving_POEntry(
            IService_InforVisual inforVisualService,
            IService_ReceivingWorkflow workflowService,
            IService_Help helpService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _inforVisualService = inforVisualService;
            _workflowService = workflowService;
            _helpService = helpService;
        }

        [RelayCommand]
        private void PoTextBoxLostFocus()
        {
            // Auto-correction only on LostFocus (not TextChanged)
            if (string.IsNullOrWhiteSpace(PoNumber))
            {
                return;
            }

            // Trim and uppercase first
            string value = PoNumber.Trim().ToUpper();

            // Format the PO number
            if (value.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
            {
                // Extract number part and reformat
                string numberPart = value.Substring(3);
                if (numberPart.All(char.IsDigit) && numberPart.Length <= 6)
                {
                    PoNumber = $"PO-{numberPart.PadLeft(6, '0')}";
                }
            }
            else if (value.All(char.IsDigit) && value.Length <= 6)
            {
                // Just numbers - add PO- prefix and pad
                PoNumber = $"PO-{value.PadLeft(6, '0')}";
            }
            // If invalid format, leave as-is and let validation message show
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

                    // Set PO status
                    PoStatus = result.Data.Status;
                    PoStatusDescription = result.Data.StatusDescription;
                    IsPOClosed = result.Data.IsClosed;

                    // Load parts and populate remaining quantity for each
                    foreach (var part in result.Data.Parts)
                    {
                        // Get remaining quantity for this part
                        var remainingQtyResult = await _inforVisualService.GetRemainingQuantityAsync(PoNumber, part.PartID);
                        if (remainingQtyResult.IsSuccess)
                        {
                            part.RemainingQuantity = remainingQtyResult.Data;
                        }

                        Parts.Add(part);
                    }

                    _workflowService.RaiseStatusMessage($"Purchase Order {PoNumber} loaded with {Parts.Count} parts.");
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
            // Validate PO number format (validation only, no auto-correction)
            // Auto-correction happens only on LostFocus

            if (string.IsNullOrWhiteSpace(value))
            {
                _workflowService.CurrentPONumber = string.Empty;
                IsLoadPOEnabled = false;
                PoValidationMessage = string.Empty;
                return;
            }

            string validatedPO = value.Trim();
            bool isValid = false;

            // Check if value starts with "po-" or "PO-" (case insensitive)
            if (validatedPO.StartsWith("po-", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the number part after "PO-"
                string numberPart = validatedPO.Substring(3);

                // Check if it's all digits and validate length
                if (numberPart.All(char.IsDigit))
                {
                    if (numberPart.Length <= 6)
                    {
                        validatedPO = $"PO-{numberPart.PadLeft(6, '0')}";
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
                    PoValidationMessage = "Invalid PO format. Use: PO-NNNNNN (6 digits)";
                    IsLoadPOEnabled = false;
                    return;
                }
            }
            else if (validatedPO.All(char.IsDigit))
            {
                // Just numbers, no prefix
                if (validatedPO.Length <= 6)
                {
                    validatedPO = $"PO-{validatedPO.PadLeft(6, '0')}";
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
                PoValidationMessage = "Invalid PO format. Enter: 66868 or PO-066868";
                IsLoadPOEnabled = false;
                return;
            }

            _workflowService.CurrentPONumber = validatedPO;
            IsLoadPOEnabled = isValid;
            PoValidationMessage = isValid ? string.Empty : "Invalid PO number format";
        }

        partial void OnPartIDChanged(string value)
        {
            // Auto-detect package type based on Part ID prefix
            if (string.IsNullOrWhiteSpace(value))
            {
                PackageType = "Skids"; // Default
                return;
            }

            var upperPart = value.Trim().ToUpper();

            if (upperPart.StartsWith("MMC"))
                PackageType = "Coils";
            else if (upperPart.StartsWith("MMF"))
                PackageType = "Sheets";
            else
                PackageType = "Skids";
        }

        partial void OnSelectedPartChanged(Model_InforVisualPart? value)
        {
            _workflowService.CurrentPart = value;

            // Auto-detect package type when a part is selected from PO
            if (value != null && !string.IsNullOrWhiteSpace(value.PartID))
            {
                var upperPart = value.PartID.Trim().ToUpper();

                if (upperPart.StartsWith("MMC"))
                    PackageType = "Coils";
                else if (upperPart.StartsWith("MMF"))
                    PackageType = "Sheets";
                else
                    PackageType = "Skids";
            }
        }

        /// <summary>
        /// Shows contextual help for PO entry
        /// </summary>
        [RelayCommand]
        private async Task ShowHelpAsync()
        {
            await _helpService.ShowHelpAsync("Receiving.POEntry");
        }

        #region Help Content Helpers

        public string GetTooltip(string key) => _helpService.GetTooltip(key);
        public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
        public string GetTip(string key) => _helpService.GetTip(key);

        #endregion
    }
}

