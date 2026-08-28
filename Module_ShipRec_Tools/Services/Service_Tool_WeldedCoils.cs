using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Service for the Welded Coils tool. Thin facade over Dao_Tool_WeldedCoil that
/// normalizes part numbers, validates them against the Infor Visual part master
/// (read-only), and maps DAO results into user-facing outcomes.
/// </summary>
public class Service_Tool_WeldedCoils : IService_Tool_WeldedCoils
{
    private const int MaxPartIdLength = 11;

    private readonly Dao_Tool_WeldedCoil _dao;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_WeldedCoils(
        Dao_Tool_WeldedCoil dao,
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_WeldedCoil>>> GetAllAsync()
    {
        var result = await _dao.GetAllAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"WeldedCoils: failed to load coils. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<int>> InsertAsync(string partId)
    {
        var normalized = NormalizePartId(partId);
        if (normalized is null)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                "Part number is required and must be 11 characters or fewer."
            );
        }

        var validation = await ValidatePartExistsAsync(normalized);
        if (validation is not null)
        {
            return Model_Dao_Result_Factory.Failure<int>(validation);
        }

        var result = await _dao.InsertAsync(normalized);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"WeldedCoils: failed to add part '{normalized}'. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> UpdateAsync(int id, string partId)
    {
        var normalized = NormalizePartId(partId);
        if (normalized is null)
        {
            return Model_Dao_Result_Factory.Failure(
                "Part number is required and must be 11 characters or fewer."
            );
        }

        var validation = await ValidatePartExistsAsync(normalized);
        if (validation is not null)
        {
            return Model_Dao_Result_Factory.Failure(validation);
        }

        var result = await _dao.UpdateAsync(id, normalized);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"WeldedCoils: failed to update row {id}. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> SetActiveAsync(int id, bool isActive)
    {
        var result = await _dao.SetActiveAsync(id, isActive);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"WeldedCoils: failed to set active={isActive} on row {id}. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var result = await _dao.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"WeldedCoils: failed to delete row {id}. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <summary>
    /// Trims and uppercases a part number, enforcing the VARCHAR(11) column limit.
    /// Returns null when the value is invalid so the caller can report a friendly error.
    /// </summary>
    private static string? NormalizePartId(string partId)
    {
        var normalized = partId.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > MaxPartIdLength)
        {
            return null;
        }

        return normalized;
    }

    /// <summary>
    /// Verifies the part exists in the Infor Visual part master (read-only).
    /// Returns an error message when the part is unknown or the check fails; null when valid.
    /// </summary>
    private async Task<string?> ValidatePartExistsAsync(string partId)
    {
        var existsResult = await _inforVisual.PartExistsAsync(partId);
        if (!existsResult.IsSuccess)
        {
            _logger.LogError(
                $"WeldedCoils: part-master check failed for '{partId}'. {existsResult.ErrorMessage}",
                existsResult.Exception
            );
            return $"Could not verify part '{partId}' in the part master. {existsResult.ErrorMessage}";
        }

        if (!existsResult.Data)
        {
            return $"Part '{partId}' does not exist in the part master.";
        }

        return null;
    }
}
