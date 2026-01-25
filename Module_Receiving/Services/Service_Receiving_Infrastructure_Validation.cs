using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for validating receiving data and enforcing business rules.
    /// </summary>
    public class Service_ReceivingValidation : IService_Receiving_Infrastructure_Validation
    {
        private readonly IService_InforVisual _inforVisualService;
        private static readonly Regex _regex = new Regex(@"^(PO-)?\d{1,6}$", RegexOptions.IgnoreCase);
        private static readonly Regex _qualityHoldRegex = new Regex(@"(MMFSR|MMCSR)", RegexOptions.IgnoreCase);

        public Service_ReceivingValidation(IService_InforVisual inforVisualService)
        {
            _inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
        }

        public Model_ReceivingValidationResult ValidatePONumber(string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
            {
                return Model_ReceivingValidationResult.Error("PO number is required");
            }

            // Allow optional "PO-" prefix followed by 1-6 digits
            if (!_regex.IsMatch(poNumber))
            {
                return Model_ReceivingValidationResult.Error("PO number must be numeric (up to 6 digits) or in PO-###### format");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidatePartID(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
            {
                return Model_ReceivingValidationResult.Error("Part ID is required");
            }

            if (partID.Length > 50)
            {
                return Model_ReceivingValidationResult.Error("Part ID cannot exceed 50 characters");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads)
        {
            if (numLoads < 1)
            {
                return Model_ReceivingValidationResult.Error("Number of loads must be at least 1");
            }

            if (numLoads > 99)
            {
                return Model_ReceivingValidationResult.Error("Number of loads cannot exceed 99");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity)
        {
            if (weightQuantity <= 0)
            {
                return Model_ReceivingValidationResult.Error("Weight/Quantity must be greater than 0");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad)
        {
            if (packagesPerLoad <= 0)
            {
                return Model_ReceivingValidationResult.Error("Package count must be greater than 0");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber)
        {
            if (string.IsNullOrWhiteSpace(heatLotNumber))
            {
                return Model_ReceivingValidationResult.Error("Heat/Lot number is required");
            }

            if (heatLotNumber.Length > 50)
            {
                return Model_ReceivingValidationResult.Error("Heat/Lot number cannot exceed 50 characters");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(decimal totalQuantity, decimal orderedQuantity, string partID)
        {
            if (totalQuantity > orderedQuantity)
            {
                return Task.FromResult(Model_ReceivingValidationResult.Warning(
                    $"Total quantity ({totalQuantity:F2}) exceeds PO ordered quantity ({orderedQuantity:F2}) for part {partID}. Do you want to continue?"));
            }

            return Task.FromResult(Model_ReceivingValidationResult.Success());
        }

        public async Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(string poNumber, string partID, decimal userEnteredQuantity)
        {
            var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partID, DateTime.Today);

            if (result.Success)
            {
                var sameDayQty = result.Data;
                if (sameDayQty > 0)
                {
                    return Model_ReceivingValidationResult.Warning(
                        $"Part {partID} was already received today. Visual shows {sameDayQty:F2} received. Your entry totals {userEnteredQuantity:F2}. Please verify.");
                }
            }

            // If check fails or returns 0, don't block - just skip the warning
            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(load.PartID))
            {
                errors.Add("Part ID is required");
            }

            if (string.IsNullOrWhiteSpace(load.PartType))
            {
                errors.Add("Part Type is required");
            }

            if (load.LoadNumber < 1)
            {
                errors.Add("Load number must be at least 1");
            }

            if (load.WeightQuantity <= 0)
            {
                errors.Add($"Load {load.LoadNumber}: Weight/Quantity must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
            {
                errors.Add($"Load {load.LoadNumber}: Heat/Lot number is required");
            }

            if (load.PackagesPerLoad <= 0)
            {
                errors.Add($"Load {load.LoadNumber}: Package count must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(load.PackageTypeName))
            {
                errors.Add($"Load {load.LoadNumber}: Package type is required");
            }

            if (errors.Count > 0)
            {
                var result = Model_ReceivingValidationResult.Error(string.Join("; ", errors));
                result.Errors = errors;
                return result;
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads)
        {
            if (loads == null || loads.Count == 0)
            {
                return Model_ReceivingValidationResult.Error("Session must contain at least one load");
            }

            var allErrors = new List<string>();

            foreach (var load in loads)
            {
                var loadValidation = ValidateReceivingLoad(load);
                if (!loadValidation.IsValid)
                {
                    allErrors.AddRange(loadValidation.Errors);
                }
            }

            if (allErrors.Count > 0)
            {
                var result = Model_ReceivingValidationResult.Error($"{allErrors.Count} validation error(s) found");
                result.Errors = allErrors;
                return result;
            }

            return Model_ReceivingValidationResult.Success();
        }

        public async Task<Model_ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID)
        {
            var result = await _inforVisualService.GetPartByIDAsync(partID);

            if (!result.Success)
            {
                return Model_ReceivingValidationResult.Error($"Error validating part: {result.ErrorMessage}");
            }

            if (result.Data == null)
            {
                return Model_ReceivingValidationResult.Error($"Part ID {partID} not found in Infor Visual database");
            }

            return Model_ReceivingValidationResult.Success();
        }

        /// <summary>
        /// Checks if a part ID is restricted and requires quality hold acknowledgment.
        /// Restricted parts: MMFSR (Sheet - Quality Required), MMCSR (Coil - Quality Required).
        /// </summary>
        /// <param name="partID">Part ID to check</param>
        /// <returns>Tuple of (IsRestricted, RestrictionType)</returns>
        public Task<(bool IsRestricted, string RestrictionType)> IsRestrictedPartAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
            {
                return Task.FromResult((false, string.Empty));
            }

            var match = _qualityHoldRegex.Match(partID);
            if (match.Success)
            {
                var restrictionType = match.Groups[1].Value.ToUpperInvariant();
                return Task.FromResult((true, restrictionType));
            }

            return Task.FromResult((false, string.Empty));
        }
    }
}


