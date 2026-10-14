using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IService_ShipRecToolsSettings _settings;

    public Service_Tool_LotMarking(
        Dao_Tool_LotMarking dao,
        IService_LoggingUtility logger,
        IService_ShipRecToolsSettings settings
    )
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_LotMarkingBackfillPreview>> PreviewUnmarkedLotMarkingsAsync(
        string partId
    )
    {
        var groups = await LoadUnmarkedLotGroupsAsync(partId);
        if (!groups.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarkingBackfillPreview>(
                groups.ErrorMessage,
                groups.Exception
            );
        }

        return Model_Dao_Result_Factory.Success(
            new Model_Tool_LotMarkingBackfillPreview(
                groups.Data!.Count,
                groups.Data.SelectMany(group => group.Rows).Sum(row => row.RowCount)
            )
        );
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<int>> FillUnmarkedLotMarkingsAsync(
        string partId,
        string actingUser
    )
    {
        if (!await _settings.GetLotMarkingEnabledAsync())
        {
            return Model_Dao_Result_Factory.Success<int>(0);
        }

        var groups = await LoadUnmarkedLotGroupsAsync(partId);
        if (!groups.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(groups.ErrorMessage, groups.Exception);
        }

        if (groups.Data!.Count == 0)
        {
            return Model_Dao_Result_Factory.Success<int>(0);
        }

        var policy = await _settings.GetLotMarkingCollisionPolicyAsync();
        var trimmedPartId = partId.Trim();
        var rowsWritten = 0;
        var writesSucceeded = 0;
        var stepsFailed = 0;
        var lastFailure = string.Empty;
        Exception? lastException = null;

        foreach (var group in groups.Data)
        {
            // The lot is the normalized pair, but the ledger is keyed on the spelling it captured,
            // so the first raw spelling of the group stands in for the lot the first time it is seen.
            var representative = group.Rows[0];
            var marking = await ResolveMarkingAsync(
                representative.Heat,
                representative.SupplierName,
                policy
            );

            if (!marking.IsSuccess)
            {
                stepsFailed++;
                lastFailure = marking.ErrorMessage;
                lastException = marking.Exception;
                continue;
            }

            if (marking.Data is null)
            {
                continue;
            }

            // One call per raw spelling, because a group collapses spellings that the stored
            // procedure matches literally. Every one of them ends up carrying the same codes.
            foreach (var row in group.Rows)
            {
                var applied = await _dao.ApplyUnmarkedLotMarkingAsync(
                    trimmedPartId,
                    row.Heat,
                    row.SupplierName,
                    marking.Data.ShapeCode,
                    marking.Data.ColorCode
                );

                if (!applied.IsSuccess)
                {
                    stepsFailed++;
                    lastFailure = applied.ErrorMessage;
                    lastException = applied.Exception;
                    _logger.LogError(
                        $"LotMarking: failed to fill in the unmarked rows of part '{partId}' for heat '{row.Heat}'. {applied.ErrorMessage}",
                        applied.Exception
                    );
                    continue;
                }

                writesSucceeded++;
                rowsWritten += applied.Data;
            }
        }

        if (writesSucceeded == 0 && stepsFailed > 0)
        {
            return Model_Dao_Result_Factory.Failure<int>(lastFailure, lastException);
        }

        if (rowsWritten > 0)
        {
            _logger.LogInfo(
                $"LotMarking: filled in {rowsWritten} unmarked row(s) of part '{partId}' for '{actingUser}'."
            );
        }

        return Model_Dao_Result_Factory.Success<int>(rowsWritten);
    }

    /// <summary>
    /// Reads the unmarked rows of a part and groups them into lots, skipping every row that carries
    /// no heat number so a fill-in never promises to mark one.
    /// </summary>
    private async Task<Model_Dao_Result<List<UnmarkedLotGroup>>> LoadUnmarkedLotGroupsAsync(
        string partId
    )
    {
        if (string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure<List<UnmarkedLotGroup>>(
                "A part number is required."
            );
        }

        var rows = await _dao.GetUnmarkedLotRowsAsync(partId.Trim());
        if (!rows.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to read the unmarked rows of part '{partId}'. {rows.ErrorMessage}",
                rows.Exception
            );
            return Model_Dao_Result_Factory.Failure<List<UnmarkedLotGroup>>(
                rows.ErrorMessage,
                rows.Exception
            );
        }

        var unmarkedRows = rows.Data ?? new List<Model_Tool_UnmarkedLotRow>();

        var groups = unmarkedRows
            .Where(row => !Helper_LotNumberNormalizer.IsBlank(row.Heat))
            .GroupBy(row =>
                (
                    Heat: Helper_LotNumberNormalizer.Normalize(row.Heat),
                    Supplier: Helper_LotMarkingResolver.NormalizeSupplier(row.SupplierName)
                )
            )
            .Select(group => new UnmarkedLotGroup(group.Key.Heat, group.Key.Supplier, group.ToList()))
            .ToList();

        return Model_Dao_Result_Factory.Success(groups);
    }

    /// <summary>
    /// One lot found by the preview, with every raw heat and supplier spelling the stored procedure
    /// reported for it. Normalization collapses spellings, so a group can hold more than one row.
    /// </summary>
    private sealed record UnmarkedLotGroup(
        string NormalizedHeat,
        string NormalizedSupplier,
        IReadOnlyList<Model_Tool_UnmarkedLotRow> Rows
    );
}
