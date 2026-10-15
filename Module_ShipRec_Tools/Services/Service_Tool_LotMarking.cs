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
    public async Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetLedgerAsync(
        string? heatSearch,
        string? partId = null
    )
    {
        var markings = await _dao.GetAllMarkingsAsync();
        if (!markings.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to load the lot list. {markings.ErrorMessage}",
                markings.Exception
            );
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_LotMarking>>(
                markings.ErrorMessage,
                markings.Exception
            );
        }

        var rows = await _dao.GetReceivingHeatLotKeysAsync();
        if (!rows.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to read the receiving lines behind the lot list. {rows.ErrorMessage}",
                rows.Exception
            );
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_LotMarking>>(
                rows.ErrorMessage,
                rows.Exception
            );
        }

        var enabled = await _dao.GetEnabledPartsAsync();
        if (!enabled.IsSuccess)
        {
            // Scoping the list needs this list. Without it the list would silently show every lot
            // again, which is the reading this feature exists to stop, so it fails rather than guesses.
            _logger.LogError(
                $"LotMarking: failed to load the enabled-part list for the lot list. {enabled.ErrorMessage}",
                enabled.Exception
            );
            return Model_Dao_Result_Factory.Failure<List<Model_Tool_LotMarking>>(
                enabled.ErrorMessage,
                enabled.Exception
            );
        }

        var lots = markings.Data ?? new List<Model_Tool_LotMarking>();
        var lineGroups = rows.Data ?? new List<Model_Tool_ReceivingHeatLotKey>();

        ApplyLineUsage(lots, lineGroups);

        var partIndex = BuildPartIndex(lineGroups);
        ApplyPartUsage(lots, partIndex);

        var scoped = FilterByPart(lots, partIndex, EnabledPartNumbers(enabled.Data), partId);

        return Model_Dao_Result_Factory.Success(FilterByHeat(scoped, heatSearch));
    }

    /// <summary>
    /// Fills in how many receiving lines carry each lot and when the lot was last seen on one, by
    /// normalizing the line groups exactly the way the rest of the feature does.
    /// </summary>
    /// <remarks>
    /// The counting happens here rather than in SQL because the matching key is produced here, and a
    /// count that matched raw text would miss every other spelling of a lot that normalization exists
    /// to unify. A lot whose number appears on no line keeps a count of zero and no date, which is the
    /// honest answer for a number that has been corrected away.
    /// </remarks>
    private static void ApplyLineUsage(
        IReadOnlyList<Model_Tool_LotMarking> lots,
        IReadOnlyList<Model_Tool_ReceivingHeatLotKey> rows
    )
    {
        var usage = new Dictionary<string, (int Lines, DateTime? LastLine)>(StringComparer.Ordinal);

        foreach (var row in rows)
        {
            if (Helper_LotNumberNormalizer.IsBlank(row.Heat))
            {
                continue;
            }

            var key = RowKey(row);

            if (!usage.TryGetValue(key, out var running))
            {
                usage[key] = (row.RowCount, row.LastLineDate);
                continue;
            }

            var lastLine =
                row.LastLineDate is not null
                && (running.LastLine is null || row.LastLineDate > running.LastLine)
                    ? row.LastLineDate
                    : running.LastLine;

            usage[key] = (running.Lines + row.RowCount, lastLine);
        }

        foreach (var lot in lots)
        {
            var heat = Helper_LotNumberNormalizer.Normalize(lot.HeatNumberNormalized);

            // A lot is recorded under either the supplier's official id or the supplier's name, and a
            // receiving line only ever carries the name, so both are tried. The first that has lines
            // behind it answers; trying both would count the same lines twice.
            foreach (var supplier in new[] { lot.SupplierId, lot.SupplierName })
            {
                if (
                    !usage.TryGetValue(
                        CombineKey(heat, Helper_LotMarkingResolver.NormalizeSupplier(supplier)),
                        out var found
                    )
                )
                {
                    continue;
                }

                lot.LineCount = found.Lines;
                lot.LastLineDate = found.LastLine;
                break;
            }
        }
    }

    /// <summary>Applies the list's lot-number filter, if one was given.</summary>
    private static List<Model_Tool_LotMarking> FilterByHeat(
        IReadOnlyList<Model_Tool_LotMarking> lots,
        string? heatSearch
    )
    {
        if (string.IsNullOrWhiteSpace(heatSearch))
        {
            return lots.ToList();
        }

        var needle = heatSearch.Trim();
        var normalizedNeedle = Helper_LotNumberNormalizer.Normalize(needle);

        return lots
            .Where(lot =>
                lot.HeatNumber.Contains(needle, StringComparison.OrdinalIgnoreCase)
                || (
                    normalizedNeedle.Length > 0
                    && lot.HeatNumberNormalized.Contains(
                        normalizedNeedle,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            .ToList();
    }

    /// <summary>
    /// The part numbers currently on the enabled list, as a set that ignores case and surrounding
    /// space, because a part number is stored as it was typed on each side.
    /// </summary>
    private static HashSet<string> EnabledPartNumbers(
        IReadOnlyList<Model_Tool_LotMarkingScope>? parts
    ) =>
        new(
            (parts ?? new List<Model_Tool_LotMarkingScope>())
                .Where(part => part.IsEnabled && !string.IsNullOrWhiteSpace(part.PartId))
                .Select(part => part.PartId.Trim()),
            StringComparer.OrdinalIgnoreCase
        );

    /// <summary>Whether a part number is on the enabled list.</summary>
    /// <remarks>
    /// Fails closed. If the list cannot be read the marking is refused rather than risked, because
    /// the alternative is marking material nobody approved, and refusing costs nothing here: the
    /// receipt is still saved, it is simply saved unmarked.
    /// </remarks>
    private async Task<bool> IsPartOnTheEnabledListAsync(string partId)
    {
        var enabled = await _dao.GetEnabledPartsAsync();
        if (!enabled.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to read the enabled-part list before marking part '{partId}'. "
                    + enabled.ErrorMessage,
                enabled.Exception
            );
            return false;
        }

        var wanted = partId.Trim();

        return (enabled.Data ?? new List<Model_Tool_LotMarkingScope>()).Any(part =>
            part.IsEnabled
            && string.Equals(part.PartId?.Trim(), wanted, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Indexes the part numbers that carry each receiving group, so a lot can be attributed to the
    /// parts that carry its number.
    /// </summary>
    private static Dictionary<string, HashSet<string>> BuildPartIndex(
        IReadOnlyList<Model_Tool_ReceivingHeatLotKey> rows
    )
    {
        var index = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var row in rows)
        {
            if (
                string.IsNullOrWhiteSpace(row.PartId)
                || Helper_LotNumberNormalizer.IsBlank(row.Heat)
            )
            {
                continue;
            }

            var key = RowKey(row);

            if (!index.TryGetValue(key, out var parts))
            {
                parts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                index[key] = parts;
            }

            parts.Add(row.PartId.Trim());
        }

        return index;
    }

    /// <summary>
    /// The part numbers that carry a lot.
    /// </summary>
    /// <remarks>
    /// Both supplier spellings are tried and their parts unioned, unlike the line count which stops
    /// at the first spelling that has lines. The difference is deliberate: counting twice would
    /// overstate the lines, whereas a part found under either spelling really does carry the lot, so
    /// dropping it would hide the material a handler is looking for.
    /// </remarks>
    private static HashSet<string> PartsOf(
        Model_Tool_LotMarking lot,
        IReadOnlyDictionary<string, HashSet<string>> partIndex
    )
    {
        var heat = Helper_LotNumberNormalizer.Normalize(lot.HeatNumberNormalized);
        var parts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var supplier in new[] { lot.SupplierId, lot.SupplierName })
        {
            if (
                partIndex.TryGetValue(
                    CombineKey(heat, Helper_LotMarkingResolver.NormalizeSupplier(supplier)),
                    out var found
                )
            )
            {
                parts.UnionWith(found);
            }
        }

        return parts;
    }

    /// <summary>
    /// Records the part numbers that carry each lot, so the list can show whose material a lot is.
    /// Worked out when the list is read and never stored, like the line count beside it.
    /// </summary>
    private static void ApplyPartUsage(
        IReadOnlyList<Model_Tool_LotMarking> lots,
        IReadOnlyDictionary<string, HashSet<string>> partIndex
    )
    {
        foreach (var lot in lots)
        {
            var parts = PartsOf(lot, partIndex);

            lot.PartsText =
                parts.Count == 0
                    ? "-"
                    : string.Join(
                        ", ",
                        parts.OrderBy(part => part, StringComparer.OrdinalIgnoreCase)
                    );
        }
    }

    /// <summary>Narrows the lot list to the material that was approved for marking.</summary>
    /// <remarks>
    /// A lot is carried by a set of parts rather than owned by one, so a lot is kept when any part
    /// that carries it is on the enabled list. A lot that no receiving row carries is kept too,
    /// because it cannot be attributed to a part at all and dropping it would hide a lot whose number
    /// was corrected away, which is a lot a person still needs to see.
    /// Choosing a part narrows to that part alone, and is honoured whether or not the part is still
    /// enabled, because choosing a part is a deliberate act and a removed part's recorded markings
    /// must stay reachable.
    /// </remarks>
    private static List<Model_Tool_LotMarking> FilterByPart(
        IReadOnlyList<Model_Tool_LotMarking> lots,
        IReadOnlyDictionary<string, HashSet<string>> partIndex,
        IReadOnlySet<string> enabledParts,
        string? partId
    )
    {
        var chosen = partId?.Trim();

        if (!string.IsNullOrEmpty(chosen))
        {
            return lots
                .Where(lot => PartsOf(lot, partIndex).Contains(chosen))
                .ToList();
        }

        return lots
            .Where(lot =>
            {
                var parts = PartsOf(lot, partIndex);
                return parts.Count == 0 || parts.Any(enabledParts.Contains);
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveMarkingAsync(
        string? heatNumber,
        string? partId,
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

        // The enabled list is a list of part numbers, so a receipt carrying no part number cannot be
        // matched against it, and a marking nothing can be attributed to is not one worth keeping.
        if (string.IsNullOrWhiteSpace(partId))
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

        // The lot has no marking yet, so continuing would create one, and a marking is only ever
        // created for a part on the enabled list. Checking it after the read above rather than
        // before it is deliberate: a lot that already has a marking keeps it whatever part arrives
        // against it, so removing a part cannot repaint a label a handler has already seen.
        if (!await IsPartOnTheEnabledListAsync(partId))
        {
            return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(null);
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
            partId.Trim(),
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
                trimmedPartId,
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

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_LotMarkingRealignPreview>> PreviewUpdateAllLotsAsync()
    {
        var rows = await _dao.GetReceivingHeatLotKeysAsync();
        if (!rows.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to read the receiving lot keys. {rows.ErrorMessage}",
                rows.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarkingRealignPreview>(
                rows.ErrorMessage,
                rows.Exception
            );
        }

        var markings = await _dao.GetAllMarkingsAsync();
        if (!markings.IsSuccess)
        {
            _logger.LogError(
                $"LotMarking: failed to load the markings for the update. {markings.ErrorMessage}",
                markings.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarkingRealignPreview>(
                markings.ErrorMessage,
                markings.Exception
            );
        }

        var policy = await _settings.GetLotMarkingCollisionPolicyAsync();

        return Model_Dao_Result_Factory.Success(
            BuildRealignPlan(
                rows.Data ?? new List<Model_Tool_ReceivingHeatLotKey>(),
                markings.Data ?? new List<Model_Tool_LotMarking>(),
                policy
            )
        );
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_LotMarkingRealignResult>> UpdateAllLotsAsync(
        string actingUser
    )
    {
        // The plan is built here rather than taken as an argument, so an interval between the report
        // and the confirmation can never cause a change that was no longer true when it was made.
        var planned = await PreviewUpdateAllLotsAsync();
        if (!planned.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarkingRealignResult>(
                planned.ErrorMessage,
                planned.Exception
            );
        }

        var preview = planned.Data!;
        var policy = await _settings.GetLotMarkingCollisionPolicyAsync();

        var rowsChanged = 0;
        var rowsCleared = 0;
        var lotsCreated = 0;
        var lotsRemoved = 0;
        var lotsSkipped = preview.Skips.Count;
        var stepsFailed = 0;
        var writesSucceeded = 0;
        var lastFailure = string.Empty;
        Exception? lastException = null;

        foreach (var target in preview.Targets)
        {
            var shape = target.ShapeCode;
            var color = target.ColorCode;

            if (target.Action == Enum_LotMarkingRealignAction.CreateLot)
            {
                var recorded = await RecordNewLotAsync(target, actingUser);
                if (!recorded.IsSuccess)
                {
                    stepsFailed++;
                    lastFailure = recorded.ErrorMessage;
                    lastException = recorded.Exception;
                    continue;
                }

                shape = recorded.Data!.ShapeCode;
                color = recorded.Data.ColorCode;
                lotsCreated++;
            }

            foreach (var spelling in target.Spellings)
            {
                var written =
                    target.Action == Enum_LotMarkingRealignAction.ClearMarking
                        ? await _dao.ClearLotMarkingRowsAsync(spelling.Heat, spelling.SupplierName)
                        : await _dao.RealignLotMarkingRowsAsync(
                            spelling.Heat,
                            spelling.SupplierName,
                            shape,
                            color
                        );

                if (!written.IsSuccess)
                {
                    stepsFailed++;
                    lastFailure = written.ErrorMessage;
                    lastException = written.Exception;
                    _logger.LogError(
                        $"LotMarking: failed to put the marking right for heat '{spelling.Heat}'. {written.ErrorMessage}",
                        written.Exception
                    );
                    continue;
                }

                writesSucceeded++;

                if (target.Action == Enum_LotMarkingRealignAction.ClearMarking)
                {
                    rowsCleared += written.Data;
                }
                else
                {
                    rowsChanged += written.Data;
                }
            }
        }

        foreach (var removal in preview.Removals)
        {
            var deleted = await _dao.DeleteMarkingAsync(removal.Id);
            if (!deleted.IsSuccess)
            {
                // A protected lot is refused by the procedure, which is the rule rather than a
                // failure, so it is reported as left alone rather than as an error.
                lotsSkipped++;
                continue;
            }

            writesSucceeded++;
            lotsRemoved++;
        }

        if (writesSucceeded == 0 && stepsFailed > 0)
        {
            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarkingRealignResult>(
                lastFailure,
                lastException
            );
        }

        if (stepsFailed > 0)
        {
            _logger.LogError(
                $"LotMarking: Update All Lots finished with {stepsFailed} failure(s). Last: {lastFailure}",
                lastException
            );
        }

        if (rowsChanged > 0 || rowsCleared > 0 || lotsCreated > 0 || lotsRemoved > 0)
        {
            _logger.LogInfo(
                $"LotMarking: Update All Lots for '{actingUser}' changed {rowsChanged} row(s), cleared {rowsCleared} row(s), created {lotsCreated} lot(s) and removed {lotsRemoved} lot(s)."
            );
        }

        return Model_Dao_Result_Factory.Success(
            new Model_Tool_LotMarkingRealignResult(
                rowsChanged,
                rowsCleared,
                lotsCreated,
                lotsRemoved,
                lotsSkipped
            )
        );
    }

    /// <summary>
    /// Records the lot a corrected number has become, with the shape and color the report showed, and
    /// returns the lot that is actually held once the write is done.
    /// </summary>
    /// <remarks>
    /// If another caller records the same lot first, the write is refused and the row that won is read
    /// back and used instead, because a recorded marking is never replaced. The number is recorded
    /// against the acting user, so a lot created by this action says who created it.
    /// </remarks>
    private async Task<Model_Dao_Result<Model_Tool_LotMarking>> RecordNewLotAsync(
        Model_Tool_LotMarkingRealignTarget target,
        string actingUser
    )
    {
        var inserted = await _dao.InsertAsync(
            target.Heat.Trim(),
            Helper_LotNumberNormalizer.Normalize(target.Heat),
            Helper_LotMarkingResolver.NormalizeSupplier(target.SupplierName),
            target.SupplierName,
            target.ShapeCode,
            target.ColorCode,
            nameof(Enum_LotMarkingSource.Auto),
            null,
            actingUser
        );

        if (!inserted.IsSuccess)
        {
            // Either the insert genuinely failed, or another caller recorded this number first. If a
            // row now exists the record wins, because an existing marking is never replaced.
            var raced = await _dao.ResolveAsync(
                Helper_LotNumberNormalizer.Normalize(target.Heat),
                Helper_LotMarkingResolver.NormalizeSupplier(target.SupplierName)
            );

            if (raced.IsSuccess && raced.Data is { Count: > 0 })
            {
                return Model_Dao_Result_Factory.Success(raced.Data[0]);
            }

            return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarking>(
                inserted.ErrorMessage,
                inserted.Exception
            );
        }

        var readBack = await _dao.ResolveAsync(
            Helper_LotNumberNormalizer.Normalize(target.Heat),
            Helper_LotMarkingResolver.NormalizeSupplier(target.SupplierName)
        );

        if (readBack.IsSuccess && readBack.Data is { Count: > 0 })
        {
            return Model_Dao_Result_Factory.Success(readBack.Data[0]);
        }

        // The write succeeded but the row could not be read back, so the codes the report showed are
        // used and the rows are still put right.
        return Model_Dao_Result_Factory.Success(
            new Model_Tool_LotMarking
            {
                ShapeCode = target.ShapeCode,
                ColorCode = target.ColorCode,
            }
        );
    }

    /// <summary>
    /// Works out what Update All Lots would do, from the ledger and the lot numbers the receiving rows
    /// carry. Pure: it reads nothing and writes nothing, so a test can drive it directly.
    /// </summary>
    /// <remarks>
    /// Only rows that already carry a marking are in scope, because this action puts markings right
    /// rather than introducing them. A lot number is treated as still in use when any row carries it,
    /// whether or not that row is marked, so a number in use is never mistaken for one that has gone.
    /// </remarks>
    private static Model_Tool_LotMarkingRealignPreview BuildRealignPlan(
        IReadOnlyList<Model_Tool_ReceivingHeatLotKey> rows,
        IReadOnlyList<Model_Tool_LotMarking> markings,
        Enum_LotMarkingCollisionPolicy policy
    )
    {
        var targets = new List<Model_Tool_LotMarkingRealignTarget>();
        var skips = new List<Model_Tool_LotMarkingRealignSkip>();
        var removals = new List<Model_Tool_LotMarkingRealignRemoval>();
        var alreadyCorrect = 0;

        var marked = rows.Where(row => !string.IsNullOrWhiteSpace(row.ShapeCode)).ToList();

        var liveKeys = new HashSet<string>(
            rows.Where(row => !Helper_LotNumberNormalizer.IsBlank(row.Heat)).Select(RowKey),
            StringComparer.Ordinal
        );

        var ledger = BuildLedgerIndex(markings);

        // A number that was cleared is not a lot, so its rows lose their marking. There is no key to
        // gather these under, so they are gathered by supplier spelling instead.
        foreach (
            var group in marked
                .Where(row => Helper_LotNumberNormalizer.IsBlank(row.Heat))
                .GroupBy(row => row.SupplierName, StringComparer.Ordinal)
        )
        {
            var first = group.First();
            targets.Add(
                new Model_Tool_LotMarkingRealignTarget(
                    Enum_LotMarkingRealignAction.ClearMarking,
                    string.Empty,
                    group.Key,
                    group.Select(SpellingOf).ToList(),
                    string.Empty,
                    string.Empty,
                    first.ShapeCode,
                    first.ColorCode
                )
            );
        }

        // Lots that still exist, and the marking each one now carries. A candidate is added here as
        // soon as it is planned, so two lots created in one run can never be given the same pair.
        var working = new List<Model_Tool_LotMarking>(markings);

        foreach (
            var group in marked
                .Where(row => !Helper_LotNumberNormalizer.IsBlank(row.Heat))
                .GroupBy(RowKey, StringComparer.Ordinal)
        )
        {
            var first = group.First();
            var spellings = group.Select(SpellingOf).ToList();

            if (ledger.TryGetValue(group.Key, out var recorded))
            {
                if (
                    Same(recorded.ShapeCode, first.ShapeCode)
                    && Same(recorded.ColorCode, first.ColorCode)
                )
                {
                    alreadyCorrect++;
                    continue;
                }

                targets.Add(
                    new Model_Tool_LotMarkingRealignTarget(
                        Enum_LotMarkingRealignAction.ApplyRecordedMarking,
                        first.Heat,
                        first.SupplierName,
                        spellings,
                        recorded.ShapeCode,
                        recorded.ColorCode,
                        first.ShapeCode,
                        first.ColorCode
                    )
                );
                continue;
            }

            // The number the rows now carry has no marking, so it becomes a lot and is given one the
            // same way a receipt gives one, which is what makes a later receipt agree with the rows.
            if (
                !Helper_LotMarkingResolver.TryResolve(
                    first.Heat,
                    first.SupplierName,
                    working,
                    policy,
                    out var shape,
                    out var color
                )
            )
            {
                continue;
            }

            working.Add(
                new Model_Tool_LotMarking
                {
                    HeatNumber = first.Heat,
                    HeatNumberNormalized = Helper_LotNumberNormalizer.Normalize(first.Heat),
                    SupplierId = Helper_LotMarkingResolver.NormalizeSupplier(first.SupplierName),
                    ShapeCode = shape,
                    ColorCode = color,
                }
            );

            targets.Add(
                new Model_Tool_LotMarkingRealignTarget(
                    Enum_LotMarkingRealignAction.CreateLot,
                    first.Heat,
                    first.SupplierName,
                    spellings,
                    shape,
                    color,
                    first.ShapeCode,
                    first.ColorCode
                )
            );
        }

        foreach (var marking in markings)
        {
            if (IsStillInUse(marking, liveKeys))
            {
                continue;
            }

            if (marking.IsLocked)
            {
                skips.Add(
                    new Model_Tool_LotMarkingRealignSkip(
                        marking.HeatNumber,
                        marking.SupplierId,
                        "Its marking is protected, so it is never removed."
                    )
                );
                continue;
            }

            removals.Add(
                new Model_Tool_LotMarkingRealignRemoval(
                    marking.Id,
                    marking.HeatNumber,
                    marking.SupplierId,
                    marking.ShapeCode,
                    marking.ColorCode
                )
            );
        }

        return new Model_Tool_LotMarkingRealignPreview(targets, removals, skips, alreadyCorrect);
    }

    private static Model_Tool_LotMarkingRealignSpelling SpellingOf(
        Model_Tool_ReceivingHeatLotKey row
    ) => new(row.Heat, row.SupplierName, row.RowCount);

    /// <summary>
    /// The matching key for a receiving row: its normalized heat number and supplier group.
    /// </summary>
    private static string RowKey(Model_Tool_ReceivingHeatLotKey row) =>
        CombineKey(
            Helper_LotNumberNormalizer.Normalize(row.Heat),
            Helper_LotMarkingResolver.NormalizeSupplier(row.SupplierName)
        );

    private static string CombineKey(string normalizedHeat, string supplierGroup) =>
        string.Concat(normalizedHeat, "\u0001", supplierGroup);

    /// <summary>
    /// Indexes the ledger so a receiving row can find its lot.
    /// </summary>
    /// <remarks>
    /// A lot is recorded under either the supplier's official id or the supplier's name, depending on
    /// whether the receiving side managed to resolve the vendor to a single match. A row only ever
    /// carries the name, so both spellings are indexed and the first lot to claim a key keeps it. That
    /// is deliberately forgiving: failing to find a lot that exists would create a second lot for it
    /// and give one physical lot two markings.
    /// </remarks>
    private static Dictionary<string, Model_Tool_LotMarking> BuildLedgerIndex(
        IReadOnlyList<Model_Tool_LotMarking> markings
    )
    {
        var index = new Dictionary<string, Model_Tool_LotMarking>(StringComparer.Ordinal);

        foreach (var marking in markings)
        {
            var heat = Helper_LotNumberNormalizer.Normalize(marking.HeatNumberNormalized);

            foreach (var supplier in new[] { marking.SupplierId, marking.SupplierName })
            {
                var key = CombineKey(
                    heat,
                    Helper_LotMarkingResolver.NormalizeSupplier(supplier)
                );

                index.TryAdd(key, marking);
            }
        }

        return index;
    }

    /// <summary>
    /// <c>true</c> when some receiving row still carries this lot's number. A lot number in use is
    /// never removed, even when only one row carries it.
    /// </summary>
    private static bool IsStillInUse(
        Model_Tool_LotMarking marking,
        HashSet<string> liveKeys
    )
    {
        var heat = Helper_LotNumberNormalizer.Normalize(marking.HeatNumberNormalized);

        return liveKeys.Contains(
                CombineKey(heat, Helper_LotMarkingResolver.NormalizeSupplier(marking.SupplierId))
            )
            || liveKeys.Contains(
                CombineKey(heat, Helper_LotMarkingResolver.NormalizeSupplier(marking.SupplierName))
            );
    }

    private static bool Same(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

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
