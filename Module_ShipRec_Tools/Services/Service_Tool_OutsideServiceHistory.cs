using System;
using System.Collections.Generic;
using System.Linq;
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
        IService_LoggingUtility logger
    )
    {
        ArgumentNullException.ThrowIfNull(inforVisual);
        ArgumentNullException.ThrowIfNull(logger);
        _inforVisual = inforVisual;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByPartAsync(
        string partNumber
    )
    {
        _logger.LogInfo($"Querying outside service history for part: {partNumber}");
        return await _inforVisual.GetOutsideServiceHistoryByPartAsync(partNumber);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByVendorAsync(
        string vendorId
    )
    {
        _logger.LogInfo($"Querying outside service history for vendor: {vendorId}");
        return await _inforVisual.GetOutsideServiceHistoryByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(
        string term
    )
    {
        _logger.LogInfo($"Fuzzy searching parts for term: '{term}'");
        return await _inforVisual.FuzzySearchPartsAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchVendorsAsync(
        string term
    )
    {
        _logger.LogInfo($"Fuzzy searching vendors for term: '{term}'");
        return await _inforVisual.FuzzySearchVendorsAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(
        string vendorId
    )
    {
        _logger.LogInfo($"Querying parts serviced by vendor: {vendorId}");
        return await _inforVisual.GetPartsByVendorAsync(vendorId);
    }

    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorsAsync(
        IReadOnlyList<string> vendorIds
    )
    {
        var distinctVendorIds = vendorIds
            .Where(static vendorId => string.IsNullOrWhiteSpace(vendorId) is false)
            .Select(static vendorId => vendorId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctVendorIds.Count == 0)
        {
            return Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>());
        }

        var historyRows = new List<Model_OutsideServiceHistory>();
        foreach (var vendorId in distinctVendorIds)
        {
            var historyResult = await _inforVisual.GetOutsideServiceHistoryByVendorAsync(vendorId);
            if (!historyResult.IsSuccess || historyResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>(
                    historyResult.ErrorMessage,
                    historyResult.Exception
                );
            }

            historyRows.AddRange(historyResult.Data);
        }

        var results = historyRows
            .Where(static row => string.IsNullOrWhiteSpace(row.PartNumber) is false)
            .GroupBy(static row => row.PartNumber!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var latestDispatch = group.Max(static row => row.DispatchDate);
                var dispatchCount = group.Count();
                var detail = latestDispatch.HasValue
                    ? $"{dispatchCount} dispatch(es) — last {latestDispatch.Value:MM/dd/yyyy}"
                    : $"{dispatchCount} dispatch(es)";

                return new Model_FuzzySearchResult
                {
                    Key = group.Key,
                    Label = group.Key,
                    Detail = detail,
                };
            })
            .OrderBy(static result => result.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Model_Dao_Result_Factory.Success(results);
    }

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetHistoryByVendorAndPartAsync(string vendorId, string partNumber)
    {
        _logger.LogInfo(
            $"Querying outside service history for vendor {vendorId}, part {partNumber}"
        );
        return await _inforVisual.GetOutsideServiceHistoryByVendorAndPartAsync(
            vendorId,
            partNumber
        );
    }

    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceHistory>>
    > GetHistoryByVendorsAndPartAsync(IReadOnlyList<string> vendorIds, string partNumber)
    {
        var distinctVendorIds = vendorIds
            .Where(static vendorId => string.IsNullOrWhiteSpace(vendorId) is false)
            .Select(static vendorId => vendorId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctVendorIds.Count == 0)
        {
            return Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>());
        }

        var results = new List<Model_OutsideServiceHistory>();
        foreach (var vendorId in distinctVendorIds)
        {
            var historyResult = await _inforVisual.GetOutsideServiceHistoryByVendorAndPartAsync(
                vendorId,
                partNumber
            );
            if (!historyResult.IsSuccess || historyResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                    historyResult.ErrorMessage,
                    historyResult.Exception
                );
            }

            results.AddRange(historyResult.Data);
        }

        var orderedResults = results
            .OrderByDescending(static row => row.DispatchDate)
            .ThenBy(static row => row.VendorName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static row => row.VendorID, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Model_Dao_Result_Factory.Success(orderedResults);
    }
}
