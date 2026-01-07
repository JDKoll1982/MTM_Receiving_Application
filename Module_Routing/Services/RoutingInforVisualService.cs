using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for Infor Visual ERP integration (READ ONLY)
/// Provides PO validation and line retrieval with graceful degradation
/// </summary>
public class RoutingInforVisualService : IRoutingInforVisualService
{
    private readonly Dao_InforVisualPO _daoInforVisualPO;
    private readonly IService_LoggingUtility _logger;

    public RoutingInforVisualService(
        Dao_InforVisualPO daoInforVisualPO,
        IService_LoggingUtility logger)
    {
        _daoInforVisualPO = daoInforVisualPO ?? throw new ArgumentNullException(nameof(daoInforVisualPO));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that a PO number exists in Infor Visual ERP
    /// </summary>
    /// <param name="poNumber">PO number to validate</param>
    /// <returns>Result with true if PO exists, false otherwise</returns>
    public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Validating PO number: {poNumber}");

            // Check connection first (graceful degradation)
            var connectionResult = await _daoInforVisualPO.CheckConnectionAsync();
            if (!connectionResult.IsSuccess || !connectionResult.Data)
            {
                await _logger.LogWarningAsync($"Infor Visual unavailable: {connectionResult.ErrorMessage}");
                // Graceful degradation: return success but log the issue
                return Model_Dao_Result_Factory.Success(true);
            }

            return await _daoInforVisualPO.ValidatePOAsync(poNumber);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error validating PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<bool>($"Error validating PO: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all line items for a purchase order from Infor Visual
    /// </summary>
    /// <param name="poNumber">PO number</param>
    /// <returns>Result with list of PO lines</returns>
    public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting PO lines for: {poNumber}");
            return await _daoInforVisualPO.GetLinesAsync(poNumber);
        }
        catch (Exception ex)
        {
            // Issue #11: Standardized error handling pattern
            await _logger.LogErrorAsync($"Error getting PO lines for {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPOLine>>($"Failed to retrieve PO lines: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a specific line item from a purchase order
    /// </summary>
    /// <param name="poNumber">PO number</param>
    /// <param name="lineNumber">Line number</param>
    /// <returns>Result with PO line data</returns>
    public async Task<Model_Dao_Result<Model_InforVisualPOLine>> GetPoLineAsync(string poNumber, string lineNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting PO line {poNumber}-{lineNumber}");

            if (!int.TryParse(lineNumber, out int lineNum))
            {
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPOLine>("Invalid line number format");
            }

            return await _daoInforVisualPO.GetLineAsync(poNumber, lineNum);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting PO line {poNumber}-{lineNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPOLine>($"Error getting PO line: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tests connection to Infor Visual database
    /// </summary>
    /// <returns>Result with true if connection successful</returns>
    public async Task<Model_Dao_Result<bool>> CheckConnectionAsync()
    {
        try
        {
            return await _daoInforVisualPO.CheckConnectionAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error checking Infor Visual connection: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Success(false);
        }
    }
}
