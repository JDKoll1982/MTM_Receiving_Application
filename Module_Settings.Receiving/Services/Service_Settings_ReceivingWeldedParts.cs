using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Receiving.Data;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Services;

/// <summary>
/// Service for the welded part numbers list on the Receiving settings page.
/// </summary>
/// <remarks>
/// Normalizes part numbers, enforces the column length, and checks the part against the Infor Visual
/// part master, which is read-only. The list itself lives in <c>settings_weldedcoils</c> and is shared
/// with the Extra Tools Welded Coils page.
/// </remarks>
public class Service_Settings_ReceivingWeldedParts : IService_Settings_ReceivingWeldedParts
{
    private const int MaxPartIdLength = 11;

    private readonly Dao_Settings_ReceivingWeldedParts _dao;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Settings_ReceivingWeldedParts(
        Dao_Settings_ReceivingWeldedParts dao,
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<Model_Dao_Result<List<Model_Settings_ReceivingWeldedPart>>> GetAllAsync() =>
        _dao.GetAllAsync();

    /// <inheritdoc />
    public async Task<Model_Dao_Result<int>> AddAsync(string partId)
    {
        var normalized = NormalizePartId(partId);
        if (normalized is null)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Enter a part number of {MaxPartIdLength} characters or fewer."
            );
        }

        var partError = await ValidatePartExistsAsync(normalized);
        if (partError is not null)
        {
            return Model_Dao_Result_Factory.Failure<int>(partError);
        }

        var result = await _dao.InsertAsync(normalized);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingWeldedParts: failed to add '{normalized}'. {result.ErrorMessage}",
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
                $"ReceivingWeldedParts: failed to delete row {id}. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    private static string? NormalizePartId(string partId)
    {
        var normalized = (partId ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > MaxPartIdLength)
        {
            return null;
        }

        return normalized;
    }

    private async Task<string?> ValidatePartExistsAsync(string partId)
    {
        var exists = await _inforVisual.PartExistsAsync(partId);
        if (!exists.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingWeldedParts: part-master check failed for '{partId}'. {exists.ErrorMessage}",
                exists.Exception
            );
            return $"Could not verify part '{partId}' in the part master. {exists.ErrorMessage}";
        }

        return exists.Data ? null : $"Part '{partId}' does not exist in the part master.";
    }
}
