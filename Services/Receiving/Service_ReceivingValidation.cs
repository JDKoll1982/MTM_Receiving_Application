using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReceivingValidationResult = MTM_Receiving_Application.Contracts.Services.ReceivingValidationResult;

namespace MTM_Receiving_Application.Services.Receiving
{
    /// <summary>
    /// Service for validating receiving data and enforcing business rules.
    /// </summary>
    public class Service_ReceivingValidation : IService_ReceivingValidation
    {
        private readonly IService_InforVisual _inforVisualService;

        public Service_ReceivingValidation(IService_InforVisual inforVisualService)
        {
            _inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        }

        public ReceivingValidationResult ValidatePONumber(string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return ReceivingValidationResult.Error("PO number is required");

            // Allow optional "PO-" prefix followed by 1-6 digits
            if (!Regex.IsMatch(poNumber, @"^(PO-)?\d{1,6}$", RegexOptions.IgnoreCase))
                return ReceivingValidationResult.Error("PO number must be numeric (up to 6 digits) or in PO-###### format");

            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidatePartID(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
                return ReceivingValidationResult.Error("Part ID is required");

            if (partID.Length > 50)
                return ReceivingValidationResult.Error("Part ID cannot exceed 50 characters");

            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidateNumberOfLoads(int numLoads)
        {
            if (numLoads < 1)
                return ReceivingValidationResult.Error("Number of loads must be at least 1");

            if (numLoads > 99)
                return ReceivingValidationResult.Error("Number of loads cannot exceed 99");

            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity)
        {
            if (weightQuantity <= 0)
                return ReceivingValidationResult.Error("Weight/Quantity must be greater than 0");

            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidatePackageCount(int packagesPerLoad)
        {
            if (packagesPerLoad <= 0)
                return ReceivingValidationResult.Error("Package count must be greater than 0");

            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber)
        {
            if (string.IsNullOrWhiteSpace(heatLotNumber))
                return ReceivingValidationResult.Error("Heat/Lot number is required");

            if (heatLotNumber.Length > 50)
                return ReceivingValidationResult.Error("Heat/Lot number cannot exceed 50 characters");

            return ReceivingValidationResult.Success();
        }

        public Task<ReceivingValidationResult> ValidateAgainstPOQuantityAsync(decimal totalQuantity, decimal orderedQuantity, string partID)
        {
            if (totalQuantity > orderedQuantity)
            {
                return Task.FromResult(ReceivingValidationResult.Warning(
                    $"Total quantity ({totalQuantity:F2}) exceeds PO ordered quantity ({orderedQuantity:F2}) for part {partID}. Do you want to continue?"));
            }

            return Task.FromResult(ReceivingValidationResult.Success());
        }

        public async Task<ReceivingValidationResult> CheckSameDayReceivingAsync(string poNumber, string partID, decimal userEnteredQuantity)
        {
            var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partID, DateTime.Today);

            if (result.Success)
            {
                var sameDayQty = result.Data;
                if (sameDayQty > 0)
                {
                    return ReceivingValidationResult.Warning(
                        $"Part {partID} was already received today. Visual shows {sameDayQty:F2} received. Your entry totals {userEnteredQuantity:F2}. Please verify.");
                }
            }

            // If check fails or returns 0, don't block - just skip the warning
            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(load.PartID))
                errors.Add("Part ID is required");

            if (string.IsNullOrWhiteSpace(load.PartType))
                errors.Add("Part Type is required");

            if (load.LoadNumber < 1)
                errors.Add("Load number must be at least 1");

            if (load.WeightQuantity <= 0)
                errors.Add($"Load {load.LoadNumber}: Weight/Quantity must be greater than 0");

            if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
                errors.Add($"Load {load.LoadNumber}: Heat/Lot number is required");

            if (load.PackagesPerLoad <= 0)
                errors.Add($"Load {load.LoadNumber}: Package count must be greater than 0");

            if (string.IsNullOrWhiteSpace(load.PackageTypeName))
                errors.Add($"Load {load.LoadNumber}: Package type is required");

            if (errors.Any())
            {
                var result = ReceivingValidationResult.Error(string.Join("; ", errors));
                result.Errors = errors;
                return result;
            }

            return ReceivingValidationResult.Success();
        }

        public ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads)
        {
            if (loads == null || loads.Count == 0)
                return ReceivingValidationResult.Error("Session must contain at least one load");

            var allErrors = new List<string>();

            foreach (var load in loads)
            {
                var loadValidation = ValidateReceivingLoad(load);
                if (!loadValidation.IsValid)
                {
                    allErrors.AddRange(loadValidation.Errors);
                }
            }

            if (allErrors.Any())
            {
                var result = ReceivingValidationResult.Error($"{allErrors.Count} validation error(s) found");
                result.Errors = allErrors;
                return result;
            }

            return ReceivingValidationResult.Success();
        }

        public async Task<ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID)
        {
            var result = await _inforVisualService.GetPartByIDAsync(partID);

            if (!result.Success)
            {
                return ReceivingValidationResult.Error($"Error validating part: {result.ErrorMessage}");
            }

            if (result.Data == null)
            {
                return ReceivingValidationResult.Error($"Part ID {partID} not found in Infor Visual database");
            }

            return ReceivingValidationResult.Success();
        }
    }
}
