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

    public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting PO lines for: {poNumber}");
            return await _daoInforVisualPO.GetLinesAsync(poNumber);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting PO lines for {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPOLine>>($"Error getting PO lines: {ex.Message}", ex);
        }
    }

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
