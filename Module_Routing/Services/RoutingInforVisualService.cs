using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;
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
    private readonly ILoggingService _logger;

    public RoutingInforVisualService(
        Dao_InforVisualPO daoInforVisualPO,
        ILoggingService logger)
    {
        _daoInforVisualPO = daoInforVisualPO ?? throw new ArgumentNullException(nameof(daoInforVisualPO));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Validating PO number: {poNumber}");
            
            // Check connection first (graceful degradation)
            var connectionResult = await _daoInforVisualPO.CheckConnectionAsync();
            if (!connectionResult.IsSuccess || !connectionResult.Data)
            {
                await _logger.LogWarningAsync($"Infor Visual unavailable: {connectionResult.ErrorMessage}");
                // Graceful degradation: return success but log the issue
                return Model_Dao_Result<bool>.Success(
                    true,
                    "Infor Visual unavailable - proceeding with OTHER workflow",
                    0
                );
            }

            return await _daoInforVisualPO.ValidatePOAsync(poNumber);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error validating PO {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result<bool>.Failure($"Error validating PO: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Getting PO lines for: {poNumber}");
            return await _daoInforVisualPO.GetLinesAsync(poNumber);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting PO lines for {poNumber}: {ex.Message}", ex);
            return Model_Dao_Result<List<Model_InforVisualPOLine>>.Failure($"Error getting PO lines: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<Model_InforVisualPOLine>> GetPoLineAsync(string poNumber, string lineNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Getting PO line {poNumber}-{lineNumber}");
            
            if (!int.TryParse(lineNumber, out int lineNum))
            {
                return Model_Dao_Result<Model_InforVisualPOLine>.Failure("Invalid line number format");
            }
            
            return await _daoInforVisualPO.GetLineAsync(poNumber, lineNum);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting PO line {poNumber}-{lineNumber}: {ex.Message}", ex);
            return Model_Dao_Result<Model_InforVisualPOLine>.Failure($"Error getting PO line: {ex.Message}", ex);
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
            return Model_Dao_Result<bool>.Success(false, $"Connection check failed: {ex.Message}", 0);
        }
    }
}
