using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts
{
    /// <summary>
    /// Service for validating receiving data and enforcing business rules.
    /// </summary>
    public interface IService_Receiving_Infrastructure_Validation
    {
        /// <summary>
        /// Validates a PO number format (6 digits, numeric).
        /// </summary>
        /// <param name="poNumber">PO number to validate</param>
        /// <returns>Validation result with success flag and error message if invalid</returns>
        public Model_ReceivingValidationResult ValidatePONumber(string poNumber);

        /// <summary>
        /// Validates a Part ID format and basic requirements.
        /// </summary>
        /// <param name="partID">Part ID to validate</param>
        /// <returns>Validation result</returns>
        public Model_ReceivingValidationResult ValidatePartID(string partID);

        /// <summary>
        /// Validates number of loads (1-99).
        /// </summary>
        /// <param name="numLoads">Number of loads</param>
        /// <returns>Validation result</returns>
        public Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads);

        /// <summary>
        /// Validates weight/quantity value (must be > 0).
        /// </summary>
        /// <param name="weightQuantity">Weight quantity</param>
        /// <returns>Validation result</returns>
        public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity);

        /// <summary>
        /// Validates package count (must be > 0).
        /// </summary>
        /// <param name="packagesPerLoad">Package count</param>
        /// <returns>Validation result</returns>
        public Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad);

        /// <summary>
        /// Validates heat/lot number (required, max length).
        /// </summary>
        /// <param name="heatLotNumber">Heat/lot number</param>
        /// <returns>Validation result</returns>
        public Model_ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber);

        /// <summary>
        /// Validates total quantity against PO ordered quantity (for PO items only).
        /// Returns warning result if exceeded, allowing user to override.
        /// </summary>
        /// <param name="totalQuantity">Sum of all load quantities</param>
        /// <param name="orderedQuantity">PO ordered quantity</param>
        /// <param name="partID">Part identifier for error message</param>
        /// <returns>Validation result with warning severity if exceeded</returns>
        public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(
            decimal totalQuantity,
            decimal orderedQuantity,
            string partID);

        /// <summary>
        /// Checks for same-day receiving and returns warning if found.
        /// For PO items only - queries Infor Visual for today's receipts.
        /// </summary>
        /// <param name="poNumber">PO number</param>
        /// <param name="partID">Part identifier</param>
        /// <param name="userEnteredQuantity">User's total entered quantity</param>
        /// <returns>Validation result with warning if same-day receiving exists</returns>
        public Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(
            string poNumber,
            string partID,
            decimal userEnteredQuantity);

        /// <summary>
        /// Validates a complete receiving load before save.
        /// </summary>
        /// <param name="load">Load to validate</param>
        /// <returns>Validation result</returns>
        public Model_ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load);

        /// <summary>
        /// Validates all loads in a session before save.
        /// </summary>
        /// <param name="loads">List of loads</param>
        /// <returns>Validation result with all errors aggregated</returns>
        public Model_ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Validates a part ID exists in Infor Visual (for edit scenarios).
        /// </summary>
        /// <param name="partID">Part ID to check</param>
        /// <returns>Validation result</returns>
        public Task<Model_ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID);

        /// <summary>
        /// Checks if a part ID is restricted and requires quality hold acknowledgment.
        /// Restricted parts: MMFSR (Sheet - Quality Required), MMCSR (Coil - Quality Required).
        /// </summary>
        /// <param name="partID">Part ID to check</param>
        /// <returns>Tuple of (IsRestricted, RestrictionType)</returns>
        public Task<(bool IsRestricted, string RestrictionType)> IsRestrictedPartAsync(string partID);
    }
}

