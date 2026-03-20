using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for validating receiving data and enforcing business rules.
    /// </summary>
    public class Service_ReceivingValidation : IService_ReceivingValidation
    {
        private readonly IService_InforVisual _inforVisualService;
        private readonly IService_ReceivingSettings _receivingSettings;

        // Allows optional "PO-" prefix, 1-6 digits, and an optional B/b suffix for blanket POs (e.g. PO-064489B)
        private static readonly Regex _regex = new Regex(@"^(PO-)?\d{1,6}[Bb]?$", RegexOptions.IgnoreCase);
        private static readonly Regex _qualityHoldRegex = new Regex(@"(MMFSR|MMCSR)", RegexOptions.IgnoreCase);
        private static readonly IReadOnlyList<string> _presetLocations = new[]
        {
            "A-RECV-01",
            "B-RECV-02",
            "C-RECV-03",
            "DOCK-01",
            "QC-HOLD-01",
            "STAGE-01"
        };

        public bool UseMockLocationList { get; }

        public IReadOnlyList<string> PresetLocations => _presetLocations;

        public Service_ReceivingValidation(
            IService_InforVisual inforVisualService,
            IService_ReceivingSettings receivingSettings,
            IOptions<InforVisualSettings> inforVisualSettings)
        {
            _inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
            _receivingSettings = receivingSettings ?? throw new ArgumentNullException(nameof(receivingSettings));
            ArgumentNullException.ThrowIfNull(inforVisualSettings);
            UseMockLocationList = inforVisualSettings.Value.UseMockData;
        }

        private bool GetBoolSetting(string key, bool fallback)
        {
            try
            {
                return _receivingSettings.GetBoolAsync(key).GetAwaiter().GetResult();
            }
            catch
            {
                return fallback;
            }
        }

        private int GetIntSetting(string key, int fallback)
        {
            try
            {
                return _receivingSettings.GetIntAsync(key).GetAwaiter().GetResult();
            }
            catch
            {
                return fallback;
            }
        }

        public Model_ReceivingValidationResult ValidatePONumber(string poNumber)
        {
            if (!GetBoolSetting(ReceivingSettingsKeys.Validation.RequirePoNumber, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.RequirePoNumber])
                && string.IsNullOrWhiteSpace(poNumber))
            {
                return Model_ReceivingValidationResult.Success();
            }

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
            var minimumLoads = GetIntSetting(ReceivingSettingsKeys.Validation.MinLoadCount, 1);
            var maximumLoads = GetIntSetting(ReceivingSettingsKeys.Validation.MaxLoadCount, 99);

            if (numLoads < minimumLoads)
            {
                return Model_ReceivingValidationResult.Error($"Number of loads must be at least {minimumLoads}");
            }

            if (numLoads > maximumLoads)
            {
                return Model_ReceivingValidationResult.Error($"Number of loads cannot exceed {maximumLoads}");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity)
        {
            var allowNegativeQuantity = GetBoolSetting(ReceivingSettingsKeys.Validation.AllowNegativeQuantity, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.AllowNegativeQuantity]);
            var minimumQuantity = GetIntSetting(ReceivingSettingsKeys.Validation.MinQuantity, 0);
            var maximumQuantity = GetIntSetting(ReceivingSettingsKeys.Validation.MaxQuantity, 999999);

            if (allowNegativeQuantity)
            {
                if (weightQuantity == 0)
                {
                    return Model_ReceivingValidationResult.Error("Weight/Quantity cannot be 0");
                }
            }
            else if (weightQuantity <= 0)
            {
                return Model_ReceivingValidationResult.Error("Weight/Quantity must be greater than 0");
            }

            if (weightQuantity < minimumQuantity)
            {
                return Model_ReceivingValidationResult.Error($"Weight/Quantity cannot be less than {minimumQuantity}");
            }

            if (weightQuantity > maximumQuantity)
            {
                return Model_ReceivingValidationResult.Error($"Weight/Quantity cannot exceed {maximumQuantity}");
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
            if (GetBoolSetting(ReceivingSettingsKeys.Validation.RequireHeatLot, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.RequireHeatLot])
                && string.IsNullOrWhiteSpace(heatLotNumber))
            {
                return Model_ReceivingValidationResult.Error("Heat/Lot number is required");
            }

            // Heat/Lot number is optional - blank values are allowed and will be set to "Nothing Entered"
            if (string.IsNullOrWhiteSpace(heatLotNumber))
            {
                return Model_ReceivingValidationResult.Success();
            }

            if (heatLotNumber.Length > 50)
            {
                return Model_ReceivingValidationResult.Error("Heat/Lot number cannot exceed 50 characters");
            }

            return Model_ReceivingValidationResult.Success();
        }

        public async Task<Model_ReceivingValidationResult> ValidateLocationAsync(string? location, string warehouseCode = "002")
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return Model_ReceivingValidationResult.Success();
            }

            var normalizedLocation = location.Trim();
            if (normalizedLocation.Length > 15)
            {
                return Model_ReceivingValidationResult.Error("Location cannot exceed 15 characters");
            }

            if (UseMockLocationList)
            {
                return _presetLocations.Contains(normalizedLocation, StringComparer.OrdinalIgnoreCase)
                    ? Model_ReceivingValidationResult.Success()
                    : Model_ReceivingValidationResult.Error($"Location must match one of the preset mock locations: {string.Join(", ", _presetLocations)}");
            }

            var existsResult = await _inforVisualService.LocationExistsAsync(normalizedLocation, warehouseCode);
            if (!existsResult.IsSuccess)
            {
                return Model_ReceivingValidationResult.Error(existsResult.ErrorMessage ?? "Unable to validate location in Infor Visual");
            }

            return existsResult.Data
                ? Model_ReceivingValidationResult.Success()
                : Model_ReceivingValidationResult.Error($"Location '{normalizedLocation}' was not found in warehouse {warehouseCode}");
        }

        public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(decimal totalQuantity, decimal orderedQuantity, string partID)
        {
            if (!GetBoolSetting(ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo]))
            {
                return Task.FromResult(Model_ReceivingValidationResult.Success());
            }

            if (totalQuantity > orderedQuantity)
            {
                return Task.FromResult(Model_ReceivingValidationResult.Warning(
                    $"Total quantity ({totalQuantity:F2}) exceeds PO ordered quantity ({orderedQuantity:F2}) for part {partID}. Do you want to continue?"));
            }

            return Task.FromResult(Model_ReceivingValidationResult.Success());
        }

        public async Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(string poNumber, string partID, decimal userEnteredQuantity)
        {
            if (!GetBoolSetting(ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving, ReceivingSettingsDefaults.BoolDefaults[ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving]))
            {
                return Model_ReceivingValidationResult.Success();
            }

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

            var partValidation = ValidatePartID(load.PartID);
            if (!partValidation.IsValid)
            {
                errors.Add($"Load {load.LoadNumber}: {partValidation.Message}");
            }

            if (string.IsNullOrWhiteSpace(load.PartType))
            {
                errors.Add($"Load {load.LoadNumber}: Part Type is required");
            }

            if (load.LoadNumber < 1)
            {
                errors.Add("Load number must be at least 1");
            }

            var quantityValidation = ValidateWeightQuantity(load.WeightQuantity);
            if (!quantityValidation.IsValid)
            {
                errors.Add($"Load {load.LoadNumber}: {quantityValidation.Message}");
            }

            var heatLotValidation = ValidateHeatLotNumber(load.HeatLotNumber);
            if (!heatLotValidation.IsValid)
            {
                errors.Add($"Load {load.LoadNumber}: {heatLotValidation.Message}");
            }

            if (!string.IsNullOrWhiteSpace(load.InitialLocation) && load.InitialLocation.Length > 15)
            {
                errors.Add($"Load {load.LoadNumber}: Location cannot exceed 15 characters");
            }

            var packageCountValidation = ValidatePackageCount(load.PackagesPerLoad);
            if (!packageCountValidation.IsValid)
            {
                errors.Add($"Load {load.LoadNumber}: {packageCountValidation.Message}");
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


