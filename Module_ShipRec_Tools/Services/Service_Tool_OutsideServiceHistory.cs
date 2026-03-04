using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Business logic layer for the Outside Service Provider History tool.
/// Delegates to <see cref="IService_InforVisual"/> for read-only Infor Visual SQL Server access.
/// ⚠️ READ-ONLY — no writes to Infor Visual.
/// </summary>
public class Service_Tool_OutsideServiceHistory : IService_Tool_OutsideServiceHistory
{
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_OutsideServiceHistory(
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(inforVisual);
        ArgumentNullException.ThrowIfNull(logger);
        _inforVisual = inforVisual;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByPartAsync(string partNumber)
    {
        _logger.LogInfo($"Querying outside service history for part: {partNumber}");
        return await _inforVisual.GetOutsideServiceHistoryByPartAsync(partNumber);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByVendorAsync(string vendorId)
    {
        _logger.LogInfo($"Querying outside service history for vendor: {vendorId}");
        return await _inforVisual.GetOutsideServiceHistoryByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(string term)
    {
        _logger.LogInfo($"Fuzzy searching parts for term: '{term}'");
        return await _inforVisual.FuzzySearchPartsAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchVendorsAsync(string term)
    {
        _logger.LogInfo($"Fuzzy searching vendors for term: '{term}'");
        return await _inforVisual.FuzzySearchVendorsAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(string vendorId)
    {
        _logger.LogInfo($"Querying parts serviced by vendor: {vendorId}");
        return await _inforVisual.GetPartsByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByVendorAndPartAsync(
        string vendorId,
        string partNumber)
    {
        _logger.LogInfo($"Querying outside service history for vendor {vendorId}, part {partNumber}");
        return await _inforVisual.GetOutsideServiceHistoryByVendorAndPartAsync(vendorId, partNumber);
    }
}
