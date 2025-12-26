using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for validating receiving data and enforcing business rules.
    /// </summary>
    public interface IService_ReceivingValidation
    {
        /// <summary>
        /// Validates a PO number format (6 digits, numeric).
        /// </summary>
        /// <param name="poNumber">PO number to validate</param>
        /// <returns>Validation result with success flag and error message if invalid</returns>
        ReceivingValidationResult ValidatePONumber(string poNumber);

        /// <summary>
        /// Validates a Part ID format and basic requirements.
        /// </summary>
        /// <param name="partID">Part ID to validate</param>
        /// <returns>Validation result</returns>
        ReceivingValidationResult ValidatePartID(string partID);

        /// <summary>
        /// Validates number of loads (1-99).
        /// </summary>
        /// <param name="numLoads">Number of loads</param>
        /// <returns>Validation result</returns>
        ReceivingValidationResult ValidateNumberOfLoads(int numLoads);

        /// <summary>
        /// Validates weight/quantity value (must be > 0).
        /// </summary>
        /// <param name="weightQuantity">Weight quantity</param>
        /// <returns>Validation result</returns>
        ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity);

        /// <summary>
        /// Validates package count (must be > 0).
        /// </summary>
        /// <param name="packagesPerLoad">Package count</param>
        /// <returns>Validation result</returns>
        ReceivingValidationResult ValidatePackageCount(int packagesPerLoad);

        /// <summary>
        /// Validates heat/lot number (required, max length).
        /// </summary>
        /// <param name="heatLotNumber">Heat/lot number</param>
        /// <returns>Validation result</returns>
        ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber);

        /// <summary>
        /// Validates total quantity against PO ordered quantity (for PO items only).
        /// Returns warning result if exceeded, allowing user to override.
        /// </summary>
        /// <param name="totalQuantity">Sum of all load quantities</param>
        /// <param name="orderedQuantity">PO ordered quantity</param>
        /// <param name="partID">Part identifier for error message</param>
        /// <returns>Validation result with warning severity if exceeded</returns>
        Task<ReceivingValidationResult> ValidateAgainstPOQuantityAsync(
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
        Task<ReceivingValidationResult> CheckSameDayReceivingAsync(
            string poNumber, 
            string partID, 
            decimal userEnteredQuantity);

        /// <summary>
        /// Validates a complete receiving load before save.
        /// </summary>
        /// <param name="load">Load to validate</param>
        /// <returns>Validation result</returns>
        ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load);

        /// <summary>
        /// Validates all loads in a session before save.
        /// </summary>
        /// <param name="loads">List of loads</param>
        /// <returns>Validation result with all errors aggregated</returns>
        ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Validates a part ID exists in Infor Visual (for edit scenarios).
        /// </summary>
        /// <param name="partID">Part ID to check</param>
        /// <returns>Validation result</returns>
        Task<ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID);
    }

    /// <summary>
    /// Result of a receiving validation operation.
    /// </summary>
    public class ReceivingValidationResult
    {
        public bool IsValid { get; set; }
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static ReceivingValidationResult Success() => new() { IsValid = true };
        public static ReceivingValidationResult Error(string message) => new() 
        { 
            IsValid = false, 
            Severity = ValidationSeverity.Error,
            Message = message,
            Errors = new List<string> { message }
        };
        public static ReceivingValidationResult Warning(string message) => new() 
        { 
            IsValid = true, // Warnings don't block
            Severity = ValidationSeverity.Warning,
            Message = message
        };
    }

    /// <summary>
    /// Severity level for validation results.
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}
