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
        string? searchText,
        string? partId = null,
        Model_Tool_LotMarkingFilter? filter = null
    )
    {
        var applied = filter ?? Model_Tool_LotMarkingFilter.Default;

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

        // A range that runs backwards matches nothing, and the database would be asked to prove it.
        // Saying so here keeps a typo from looking like "no lots came in".
        if (applied.IsBackwardsRange)
        {
            return Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking>());
        }

        var rows = await _dao.GetReceivingHeatLotKeysAsync(applied.FromDate, applied.ToDate);
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

        var scope = ScopeOf(enabled.Data, partId);

        var scopedRows = (rows.Data ?? new List<Model_Tool_ReceivingHeatLotKey>())
            .Where(row => IsCarriedByScope(row, scope) && IsInsideRange(row, applied))
            .ToList();

        var lots = BuildLots(scopedRows, markings.Data);

        return Model_Dao_Result_Factory.Success(Sort(FilterBySearch(lots, searchText), applied));
    }

    /// <summary>Orders the lot list the way the caller asked for, with a stable tie-break.</summary>
    /// <remarks>
    /// The heat number settles ties in every case, so two lots that share a value the caller sorted on
    /// keep one order between reads. Without that, a list re-read after any edit could shuffle rows
    /// nobody changed.
    /// </remarks>
    private static List<Model_Tool_LotMarking> Sort(
        List<Model_Tool_LotMarking> lots,
        Model_Tool_LotMarkingFilter filter
    )
    {
        var primary = Order(lots, filter.SortField, filter.IsDescending);

        return primary.ThenBy(lot => lot.HeatNumber, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IOrderedEnumerable<Model_Tool_LotMarking> Order(
        List<Model_Tool_LotMarking> lots,
        Enum_LotMarkingSortField sortField,
        bool isDescending
    )
    {
        switch (sortField)
        {
            case Enum_LotMarkingSortField.HeatNumber:
                return isDescending
                    ? lots.OrderByDescending(lot => lot.HeatNumber, StringComparer.OrdinalIgnoreCase)
                    : lots.OrderBy(lot => lot.HeatNumber, StringComparer.OrdinalIgnoreCase);

            case Enum_LotMarkingSortField.SkidCount:
                return isDescending
                    ? lots.OrderByDescending(lot => lot.LineCount)
                    : lots.OrderBy(lot => lot.LineCount);

            case Enum_LotMarkingSortField.Supplier:
                return isDescending
                    ? lots.OrderByDescending(lot => lot.SupplierText, StringComparer.OrdinalIgnoreCase)
                    : lots.OrderBy(lot => lot.SupplierText, StringComparer.OrdinalIgnoreCase);

            case Enum_LotMarkingSortField.PartNumber:
                return isDescending
                    ? lots.OrderByDescending(lot => lot.PartsText, StringComparer.OrdinalIgnoreCase)
                    : lots.OrderBy(lot => lot.PartsText, StringComparer.OrdinalIgnoreCase);

            case Enum_LotMarkingSortField.ShapeAndColor:
                return isDescending
                    ? lots.OrderByDescending(lot => lot.ShapeCode, StringComparer.Ordinal)
                        .ThenByDescending(lot => lot.ColorCode, StringComparer.OrdinalIgnoreCase)
                    : lots.OrderBy(lot => lot.ShapeCode, StringComparer.Ordinal)
                        .ThenBy(lot => lot.ColorCode, StringComparer.OrdinalIgnoreCase);

            // A lot with no date has nothing to sort by, so it goes last whichever way the list runs.
            case Enum_LotMarkingSortField.LastReceiptDate:
            default:
                return isDescending
                    ? lots.OrderByDescending(lot => lot.LastLineDate ?? DateTime.MinValue)
                    : lots.OrderBy(lot => lot.LastLineDate ?? DateTime.MaxValue);
        }
    }

    /// <summary>
    /// The part numbers whose material this list covers: the enabled part list, or the one part a
    /// user chose to read the list by.
    /// </summary>
    /// <remarks>
    /// A chosen part replaces the enabled list rather than adding to it, so choosing a part narrows to
    /// that part alone. It is honoured whether or not the part is still enabled, because choosing a
    /// part is a deliberate act and a removed part's recorded markings must stay reachable.
    /// </remarks>
    private static HashSet<string> ScopeOf(
        IReadOnlyList<Model_Tool_LotMarkingScope>? enabledParts,
        string? partId
    ) =>
        string.IsNullOrWhiteSpace(partId)
            ? EnabledPartNumbers(enabledParts)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { partId.Trim() };

    /// <summary>Whether a receiving row carries material this list covers.</summary>
    private static bool IsCarriedByScope(
        Model_Tool_ReceivingHeatLotKey row,
        IReadOnlySet<string> scope
    ) => !string.IsNullOrWhiteSpace(row.PartId) && scope.Contains(row.PartId.Trim());

    /// <summary>
    /// Whether a receiving row was received inside the chosen range. A row carrying no date cannot be
    /// shown to be inside one, so it is left out of a ranged read.
    /// </summary>
    /// <remarks>
    /// The stored procedure is the authority here: it applies the same range before grouping, so its
    /// rows already fall inside it and this check removes nothing. It runs anyway because the list must
    /// never show a lot received outside the window a user asked for. A database still running the
    /// earlier version of the procedure ignores the parameters and returns every row, and without this
    /// check that reads as a filter that does nothing rather than one that is out of date.
    /// </remarks>
    private static bool IsInsideRange(
        Model_Tool_ReceivingHeatLotKey row,
        Model_Tool_LotMarkingFilter filter
    )
    {
        if (!filter.HasDateRange)
        {
            return true;
        }

        if (row.LastLineDate is null)
        {
            return false;
        }

        var day = row.LastLineDate.Value.Date;

        return (filter.FromDate is null || day >= filter.FromDate.Value.Date)
            && (filter.ToDate is null || day <= filter.ToDate.Value.Date);
    }

    /// <summary>
    /// Builds the list from the receiving lines inside the range, so a lot is shown because material
    /// carrying it came in rather than because a marking for it was recorded.
    /// </summary>
    /// <remarks>
    /// A lot's marking is copied onto each line when the line is saved, and the ledger is only read
    /// again when the list is read. Reading the ledger as the source of the list hid a lot whenever
    /// the two disagreed: a marking recorded against a supplier id while the lines carry the supplier
    /// name, or recorded under a lot number spelled differently, matched no line and the material it
    /// marks was never listed. Reading the lines instead means the worst case is a lot shown with the
    /// codes its own lines carry, which is what a handler saw on the receipt, rather than material
    /// that is silently missing from the list.
    /// </remarks>
    private static List<Model_Tool_LotMarking> BuildLots(
        IReadOnlyList<Model_Tool_ReceivingHeatLotKey> rows,
        IReadOnlyList<Model_Tool_LotMarking>? markings
    )
    {
        var groups = rows
            .Where(row => !Helper_LotNumberNormalizer.IsBlank(row.Heat))
            .GroupBy(RowKey, StringComparer.Ordinal)
            .Select(group => LotGroup.Of(group.ToList()))
            .OrderBy(group => group.HeatText, StringComparer.OrdinalIgnoreCase)
            .ThenBy(group => group.SupplierGroup, StringComparer.Ordinal)
            .ToList();

        var recorded = RecordedMarkings(groups, markings ?? new List<Model_Tool_LotMarking>());

        return groups.ConvertAll(group => ToLot(group, recorded));
    }

    /// <summary>
    /// Finds the marking the ledger holds for each lot on the list, if it holds one at all.
    /// </summary>
    /// <remarks>
    /// The first pass takes the entry recorded against the same supplier, which is the lot as the
    /// ledger defines it. The second pass takes an entry for the same lot number recorded against
    /// another supplier spelling, because a supplier that resolves to its official id on one receipt
    /// and to its name on another is the same supplier and the same lot. An entry is used once, so
    /// two lots can never be shown under one marking.
    /// </remarks>
    private static Dictionary<string, Model_Tool_LotMarking> RecordedMarkings(
        IReadOnlyList<LotGroup> groups,
        IReadOnlyList<Model_Tool_LotMarking> markings
    )
    {
        var available = markings.OrderBy(marking => marking.Id).ToList();
        var used = new HashSet<int>();
        var found = new Dictionary<string, Model_Tool_LotMarking>(StringComparer.Ordinal);

        foreach (var group in groups)
        {
            var exact = available.FirstOrDefault(marking =>
                !used.Contains(marking.Id) && IsRecordedAgainst(marking, group, sameSupplier: true)
            );

            if (exact is not null)
            {
                used.Add(exact.Id);
                found[group.Key] = exact;
            }
        }

        foreach (var group in groups.Where(group => !found.ContainsKey(group.Key)))
        {
            var relaxed = available.FirstOrDefault(marking =>
                !used.Contains(marking.Id) && IsRecordedAgainst(marking, group, sameSupplier: false)
            );

            if (relaxed is not null)
            {
                used.Add(relaxed.Id);
                found[group.Key] = relaxed;
            }
        }

        return found;
    }

    /// <summary>Whether a recorded marking is for the lot a group of receiving lines carries.</summary>
    private static bool IsRecordedAgainst(
        Model_Tool_LotMarking marking,
        LotGroup group,
        bool sameSupplier
    )
    {
        var heat = Helper_LotNumberNormalizer.Normalize(marking.HeatNumberNormalized);

        if (!string.Equals(heat, group.NormalizedHeat, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !sameSupplier
            || Same(marking.SupplierId, group.SupplierGroup)
            || Same(
                Helper_LotMarkingResolver.NormalizeSupplier(marking.SupplierName),
                group.SupplierGroup
            );
    }

    /// <summary>
    /// The row the list shows for one lot: the recorded marking when the ledger holds one, and the
    /// codes the lot's own receipt lines carry when it does not.
    /// </summary>
    private static Model_Tool_LotMarking ToLot(
        LotGroup group,
        IReadOnlyDictionary<string, Model_Tool_LotMarking> recorded
    )
    {
        var lot = recorded.TryGetValue(group.Key, out var marking)
            ? marking
            : new Model_Tool_LotMarking
            {
                HeatNumber = group.HeatText,
                HeatNumberNormalized = group.NormalizedHeat,
                SupplierId = group.SupplierText,
                SupplierName = group.SupplierText,
                ShapeCode = group.ShapeCode,
                ColorCode = group.ColorCode,
            };

        // A marking recorded before the receipt's supplier name was stored names only its group, which
        // is the id for most vendors. The lines carry the name every other screen shows, so it fills in
        // the empty name and the list reads as the vendor rather than as a number no handler knows.
        if (string.IsNullOrWhiteSpace(lot.SupplierName) && group.SupplierText.Length > 0)
        {
            lot.SupplierName = group.SupplierText;
        }

        lot.LineCount = group.LineCount;
        lot.LastLineDate = group.LastLineDate;
        lot.PartsText = group.PartNumbers.Count == 0 ? "-" : string.Join(", ", group.PartNumbers);

        return lot;
    }

    /// <summary>
    /// One lot's receiving lines inside the range, gathered under the key the ledger and the lines are
    /// matched by, with what the list reports about them.
    /// </summary>
    private sealed record LotGroup(
        string Key,
        string HeatText,
        string NormalizedHeat,
        string SupplierText,
        string SupplierGroup,
        int LineCount,
        DateTime? LastLineDate,
        string ShapeCode,
        string ColorCode,
        IReadOnlyList<string> PartNumbers
    )
    {
        /// <summary>Reads one group of lines into the values the list reports for that lot.</summary>
        /// <remarks>
        /// The newest line names the lot, so the number and supplier shown are the ones a handler saw
        /// most recently even when the same lot was typed slightly differently over time.
        /// </remarks>
        public static LotGroup Of(IReadOnlyList<Model_Tool_ReceivingHeatLotKey> rows)
        {
            var naming = rows
                .OrderByDescending(row => row.LastLineDate ?? DateTime.MinValue)
                .ThenBy(row => row.Heat, StringComparer.Ordinal)
                .First();

            return new LotGroup(
                RowKey(naming),
                naming.Heat,
                Helper_LotNumberNormalizer.Normalize(naming.Heat),
                naming.SupplierName,
                Helper_LotMarkingResolver.NormalizeSupplier(naming.SupplierName),
                rows.Sum(row => row.RowCount),
                rows.Max(row => row.LastLineDate),
                FirstCode(rows.Select(row => row.ShapeCode)),
                FirstCode(rows.Select(row => row.ColorCode)),
                rows.Select(row => row.PartId.Trim())
                    .Where(part => part.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(part => part, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            );
        }

        private static string FirstCode(IEnumerable<string> codes) =>
            codes.FirstOrDefault(code => !string.IsNullOrWhiteSpace(code)) ?? string.Empty;
    }

    /// <summary>
    /// Applies the list's search, if one was given, to the lot number and to the part numbers the lot
    /// is carried by.
    /// </summary>
    /// <remarks>
    /// A handler looking at a pile of material knows either the lot number or the part number, so one
    /// box answers both questions rather than making them choose which one to narrow by. The match is
    /// the same shape on both sides: the value contains what was typed, ignoring case, so a part
    /// number can be typed whole or by its tail. The normalized comparison is what lets a lot be found
    /// by a number typed without the separators the receipt carries.
    /// </remarks>
    private static List<Model_Tool_LotMarking> FilterBySearch(
        IReadOnlyList<Model_Tool_LotMarking> lots,
        string? searchText
    )
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return lots.ToList();
        }

        var needle = searchText.Trim();
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
                || lot.PartsText.Contains(needle, StringComparison.OrdinalIgnoreCase)
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

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveMarkingAsync(
        string? heatNumber,
        string? partId,
        string? supplierKey,
        Enum_LotMarkingCollisionPolicy policy,
        string? receiptSupplierName = null
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

        // The same vendor can be keyed by its official id on one receipt and by its name on the next,
        // so the lot may already be recorded under the spelling this receipt carries. Looking there
        // before creating anything is what stops one physical lot holding two markings, which a
        // handler reads as the same material being two colors. A different supplier really is a
        // different lot, and its name is a different key, so it is still given a marking of its own.
        var receiptName = Helper_LotMarkingResolver.NormalizeSupplier(receiptSupplierName);
        if (!string.Equals(receiptName, supplier, StringComparison.OrdinalIgnoreCase))
        {
            var byReceiptName = await _dao.ResolveAsync(normalizedHeat, receiptName);
            if (!byReceiptName.IsSuccess)
            {
                _logger.LogError(
                    $"LotMarking: failed to read the marking for '{normalizedHeat}' under supplier "
                        + $"'{receiptName}'. {byReceiptName.ErrorMessage}",
                    byReceiptName.Exception
                );
                return Model_Dao_Result_Factory.Failure<Model_Tool_LotMarking?>(
                    byReceiptName.ErrorMessage,
                    byReceiptName.Exception
                );
            }

            if (byReceiptName.Data is { Count: > 0 })
            {
                return Model_Dao_Result_Factory.Success<Model_Tool_LotMarking?>(
                    byReceiptName.Data[0]
                );
            }
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
            ReceiptSupplierName(receiptSupplierName, supplierKey),
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

    /// <summary>
    /// The supplier name a new marking records: the spelling on the receipt when there is one, so the
    /// group and the name stay related, and the group itself when the receipt named no supplier.
    /// </summary>
    private static string ReceiptSupplierName(string? receiptSupplierName, string? supplierKey) =>
        string.IsNullOrWhiteSpace(receiptSupplierName)
            ? Helper_LotMarkingResolver.NormalizeSupplier(supplierKey)
            : receiptSupplierName.Trim();

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
                    SpellingsOf(group),
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
            var spellings = SpellingsOf(group);

            if (ledger.TryGetValue(group.Key, out var recorded))
            {
                // Every row of the lot is compared, not just the one that happens to come first,
                // because a lot whose rows disagree with each other is exactly what this run is for.
                // Comparing one row let a lot keep two colors for as long as the first row matched.
                var wrong = group
                    .Where(row =>
                        !Same(recorded.ShapeCode, row.ShapeCode)
                        || !Same(recorded.ColorCode, row.ColorCode)
                    )
                    .ToList();

                if (wrong.Count == 0)
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
                        wrong[0].ShapeCode,
                        wrong[0].ColorCode
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

    /// <summary>
    /// The raw spellings one lot has to be written under, with their line counts added up.
    /// </summary>
    /// <remarks>
    /// A lot arrives as several row groups whenever its rows disagree, because the read groups by the
    /// marking as well as by the number and supplier. Writing each of those groups separately would
    /// match the same rows more than once and report more rows changed than the lot actually holds,
    /// so the spellings are gathered by the pair the write matches on.
    /// </remarks>
    private static List<Model_Tool_LotMarkingRealignSpelling> SpellingsOf(
        IEnumerable<Model_Tool_ReceivingHeatLotKey> rows
    ) =>
        rows.GroupBy(row => (row.Heat, row.SupplierName))
            .Select(sameSpelling =>
                new Model_Tool_LotMarkingRealignSpelling(
                    sameSpelling.Key.Heat,
                    sameSpelling.Key.SupplierName,
                    sameSpelling.Sum(row => row.RowCount)
                )
            )
            .ToList();

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
