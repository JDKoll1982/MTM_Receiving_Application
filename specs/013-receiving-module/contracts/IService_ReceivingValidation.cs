using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

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
    Model_ReceivingValidationResult ValidatePONumber(string poNumber);

    /// <summary>
    /// Validates a Part ID format and basic requirements.
    /// </summary>
    /// <param name="partID">Part ID to validate</param>
    /// <returns>Validation result</returns>
    Model_ReceivingValidationResult ValidatePartID(string partID);

    /// <summary>
    /// Validates number of loads (1-99).
    /// </summary>
    /// <param name="numLoads">Number of loads</param>
    /// <returns>Validation result</returns>
    Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads);

    /// <summary>
    /// Validates weight/quantity value (must be > 0).
    /// </summary>
    /// <param name="weightQuantity">Weight quantity</param>
    /// <returns>Validation result</returns>
    Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity);

    /// <summary>
    /// Validates package count (must be > 0).
    /// </summary>
    /// <param name="packagesPerLoad">Package count</param>
    /// <returns>Validation result</returns>
    Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad);

    /// <summary>
    /// Validates heat/lot number (required, max length).
    /// </summary>
    /// <param name="heatLot">Heat/lot number</param>
    /// <returns>Validation result</returns>
    Model_ReceivingValidationResult ValidateHeatLot(string heatLot);

    /// <summary>
    /// Auto-formats PO number from user input.
    /// Converts "63150" to "PO-063150".
    /// </summary>
    /// <param name="poNumber">Raw PO number input</param>
    /// <returns>Formatted PO number</returns>
    string FormatPONumber(string poNumber);
}


