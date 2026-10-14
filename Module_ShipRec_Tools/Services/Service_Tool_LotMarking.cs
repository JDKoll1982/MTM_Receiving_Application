using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Service for the Heat / Lot Management tool. Normalizes a heat number, resolves a lot's marking
/// through the palette and the ledger, and keeps the enabled-part list.
/// </summary>
/// <remarks>
/// The marking is chosen once and then read back forever. Nothing here recomputes a marking that
/// already exists, so a lot keeps the shape and color a handler already saw.
/// </remarks>
public class Service_Tool_LotMarking : IService_Tool_LotMarking
{
    private readonly Dao_Tool_LotMarking _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_LotMarking(Dao_Tool_LotMarking dao, IService_LoggingUtility logger)
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetAllMarkingsAsync()
    {
        var result = await _dao.GetAllMarkingsAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to load markings. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> SearchMarkingsByHeatAsync(
        string? searchText
    )
    {
        var result = await _dao.SearchMarkingsByHeatAsync(searchText);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to search markings. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveMarkingAsync(
        string? heatNumber,
        string? supplierKey,
        Enum_LotMarkingCollisionPolicy policy
    )
    {
        // A line with no heat number gets no marking. This is the normal case, not a failure,
        // and it must never stop a receipt from being saved.
        if (Helper_LotNumberNormalizer.IsBlank(heatNumber))
        {
            return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(null);
        }

        var normalizedHeat = Helper_LotNumberNormalizer.Normalize(heatNumber);
        var supplier = Helper_LotMarkingResolver.NormalizeSupplier(supplierKey);

        var stored = await _dao.ResolveAsync(normalizedHeat, supplier);
        if (!stored.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to read the marking for '{normalizedHeat}'. {stored.ErrorMessage}",
                stored.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarking?>(
                stored.ErrorMessage,
                stored.Exception
            );
        }

        if (stored.Data is { Count: > 0 })
        {
            return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(stored.Data[0]);
        }

        var live = await _dao.GetAllMarkingsAsync();
        var liveMarkings = live.IsSuccess ? live.Data : new List<Model_Tool_LotMarking>();

        if (
            !Helper_LotMarkingResolver.TryResolve(
                heatNumber,
                supplier,
                liveMarkings,
                policy,
                out var shapeGlyph,
                out var colorCode
            )
        )
        {
            return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(null);
        }

        var inserted = await _dao.InsertAsync(
            heatNumber!.Trim(),
            normalizedHeat,
            supplier,
            supplierKey,
            shapeGlyph,
            colorCode,
            nameof(Enum_LotMarkingSource.Auto),
            null,
            null
        );

        if (!inserted.IsSuccess)
        {
            // Either the insert genuinely failed, or another caller recorded this lot first.
            // If a row now exists the ledger wins, because an existing marking is never replaced.
            var raced = await _dao.ResolveAsync(normalizedHeat, supplier);
            if (raced.IsSuccess && raced.Data is { Count: > 0 })
            {
                return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(raced.Data[0]);
            }

            _logger.LogError(
                $"LotMarking: failed to record a marking for '{normalizedHeat}'. {inserted.ErrorMessage}",
                inserted.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarking?>(
                inserted.ErrorMessage,
                inserted.Exception
            );
        }

        var created = await _dao.ResolveAsync(normalizedHeat, supplier);
        if (created.IsSuccess && created.Data is { Count: > 0 })
        {
            return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(created.Data[0]);
        }

        return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarking?>(
            "The marking was saved but could not be read back."
        );
    }

    /// <inheritdoc />
    /// <remarks>
    /// The lock is deliberately not consulted here. It stops the resolver from recomputing a lot,
    /// not the operator from correcting one by hand, so a locked marking stays overridable and
    /// stays locked afterwards.
    /// </remarks>
    public async Task<Model_Dao_Result> OverrideMarkingAsync(
        int id,
        string shapeGlyph,
        string colorCode,
        string reason,
        string actingUser
    )
    {
        if (id <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Lot marking not found");
        }

        if (Model_LotMarkingPalette.FindShape(shapeGlyph) is null)
        {
            return Model_Dao_Result_Factory.Failure("That shape is not part of the palette.");
        }

        if (Model_LotMarkingPalette.FindColor(colorCode) is null)
        {
            return Model_Dao_Result_Factory.Failure("That color is not part of the palette.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Model_Dao_Result_Factory.Failure(
                "A reason is required when a marking is changed by hand."
            );
        }

        var result = await _dao.UpdateMarkingAsync(
            id,
            shapeGlyph,
            colorCode,
            nameof(Enum_LotMarkingSource.Manual),
            reason.Trim(),
            actingUser
        );

        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to override marking {id}. {result.ErrorMessage}",
                result.Exception
            );
            return Model_Dao_Result_Factory.Failure(result.ErrorMessage, result.Exception);
        }

        return Model_Dao_Result_Factory.Success();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Locking changes nothing about the codes a lot already carries. It only stops the resolver
    /// from recomputing the lot, and it records the acting user so a frozen marking can be traced.
    /// </remarks>
    public async Task<Model_Dao_Result> SetMarkingLockedAsync(
        int id,
        bool isLocked,
        string actingUser
    )
    {
        if (id <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Lot marking not found");
        }

        var result = await _dao.SetLockedAsync(id, isLocked, actingUser);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to set the lock on marking {id}. {result.ErrorMessage}",
                result.Exception
            );
            return Model_Dao_Result_Factory.Failure(result.ErrorMessage, result.Exception);
        }

        return Model_Dao_Result_Factory.Success();
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> GetEnabledPartsAsync()
    {
        var result = await _dao.GetEnabledPartsAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to load the enabled-part list. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> SearchPartsAsync(
        string? searchText
    )
    {
        var result = await _dao.SearchPartsAsync(searchText);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to search the enabled-part list. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<int>> AddPartAsync(string partId, string actingUser)
    {
        if (string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure<int>("A part number is required.");
        }

        var result = await _dao.UpsertPartAsync(partId.Trim(), actingUser);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to add part '{partId}'. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This is both the tool's Remove and its Re-enable action. It deactivates or reactivates the
    /// row and never deletes it, so every marking already recorded against the part keeps its
    /// shape and color.
    /// </remarks>
    public async Task<Model_Dao_Result> SetPartEnabledAsync(int id, bool isEnabled)
    {
        if (id <= 0)
        {
            return Model_Dao_Result_Factory.Failure(
                "Select a part that is already on the list before removing or re-enabling it."
            );
        }

        var result = await _dao.SetPartEnabledAsync(id, isEnabled);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to change the enabled state of list row {id}. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Only used to clear a row added by mistake. Ordinary removal disables the row instead, so the
    /// markings already recorded against the part are not disturbed.
    /// </remarks>
    public async Task<Model_Dao_Result> RemovePartAsync(int id)
    {
        if (id <= 0)
        {
            return Model_Dao_Result_Factory.Failure("A part on the list must be selected first.");
        }

        var result = await _dao.DeletePartAsync(id);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to delete list row {id}. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> ApplyMarkingToQueueRowAsync(
        string loadId,
        string? shapeGlyph,
        string? colorCode
    )
    {
        if (string.IsNullOrWhiteSpace(loadId))
        {
            return Model_Dao_Result_Factory.Failure("A load id is required to mark a queue row.");
        }

        var result = await _dao.SetQueueRowMarkingAsync(loadId, shapeGlyph, colorCode);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to mark queue row '{loadId}'. {result.ErrorMessage}",
                result.Exception
            );
        }

        return result;
    }
}
