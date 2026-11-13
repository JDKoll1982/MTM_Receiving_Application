using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// Finds the labels on a receipt whose part number is on the list of parts that get the lot shape
/// treatment, so the results page can say which printed tags still need their colored symbol.
/// </summary>
/// <remarks>
/// One place decides this because two pages ask: the Review page, before the receipt is saved, and the
/// Complete page, for the manual and non-PO flows which save without passing through Review. A part is
/// matched case-insensitively, since the enabled list and the receipt are keyed by different screens.
/// </remarks>
public static class Helper_ReceivingLotMarkingNeeds
{
    private static readonly IReadOnlyList<Model_ReceivingLotMarkingNeed> None = [];

    /// <summary>
    /// Builds the reminder list from a receipt and the enabled part list, read from the lot marking tool.
    /// A list that cannot be read yields nothing, so a failed read hides the reminder rather than sending
    /// the operator after a tag that needs nothing.
    /// </summary>
    /// <param name="lotMarking">The lot marking service that owns the enabled part list.</param>
    /// <param name="loads">The receipt's loads.</param>
    public static async Task<IReadOnlyList<Model_ReceivingLotMarkingNeed>> BuildAsync(
        IService_Tool_LotMarking lotMarking,
        IEnumerable<Model_ReceivingLoad> loads
    )
    {
        if (lotMarking is null)
        {
            return None;
        }

        var result = await lotMarking.GetEnabledPartsAsync();

        if (!result.IsSuccess || result.Data is null)
        {
            return None;
        }

        return Build(
            loads,
            result
                .Data.Where(scope => scope.IsEnabled)
                .Select(scope => scope.PartId)
                .Where(part => !string.IsNullOrWhiteSpace(part))
        );
    }

    /// <summary>
    /// The same list, from the part numbers that carry the treatment, so the matching can be checked
    /// without a database.
    /// </summary>
    /// <param name="loads">The receipt's loads.</param>
    /// <param name="enabledParts">The part numbers that get the lot shape treatment.</param>
    public static IReadOnlyList<Model_ReceivingLotMarkingNeed> Build(
        IEnumerable<Model_ReceivingLoad>? loads,
        IEnumerable<string>? enabledParts
    )
    {
        if (loads is null || enabledParts is null)
        {
            return None;
        }

        var parts = enabledParts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (parts.Count == 0)
        {
            return None;
        }

        var needs = new List<Model_ReceivingLotMarkingNeed>();

        foreach (var load in loads)
        {
            if (load is null || string.IsNullOrWhiteSpace(load.PartID))
            {
                continue;
            }

            var partId = load.PartID.Trim();

            if (!parts.Contains(partId))
            {
                continue;
            }

            needs.Add(
                new Model_ReceivingLotMarkingNeed(
                    partId,
                    load.HeatLotNumber?.Trim() ?? string.Empty,
                    load.LoadNumber,
                    DescribeMarking(load)
                )
            );
        }

        return needs;
    }

    /// <summary>The marking already recorded against a load, or empty while it has not been resolved yet.</summary>
    private static string DescribeMarking(Model_ReceivingLoad load) =>
        string.IsNullOrWhiteSpace(load.HeatShapeCode) || string.IsNullOrWhiteSpace(load.HeatColorCode)
            ? string.Empty
            : $"{load.HeatShapeCode} {load.HeatColorCode}";

    /// <summary>
    /// True when one part number is among the labels that need the overlay. The reviewing page asks this per
    /// entry so a part that is not on the list is offered no shape or color card at all.
    /// </summary>
    /// <param name="needs">The labels needing the overlay, as <see cref="Build"/> produced them.</param>
    /// <param name="partId">The part number of the entry on show.</param>
    public static bool NeedsMarking(
        IEnumerable<Model_ReceivingLotMarkingNeed>? needs,
        string? partId
    )
    {
        if (needs is null || string.IsNullOrWhiteSpace(partId))
        {
            return false;
        }

        var part = partId.Trim();

        return needs.Any(need =>
            need is not null
            && string.Equals(need.PartId?.Trim(), part, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Names the labels that need the overlay in one short line, grouped by part with each lot listed once,
    /// so a receipt of five tags of the same part reads as one part and its lots rather than five repeat
    /// entries. How many labels that means is said separately by the count text.
    /// </summary>
    /// <param name="needs">The labels needing the overlay.</param>
    public static string Summarize(IEnumerable<Model_ReceivingLotMarkingNeed>? needs)
    {
        if (needs is null)
        {
            return string.Empty;
        }

        var lotsByPart = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var partOrder = new List<string>();

        foreach (var need in needs)
        {
            if (need is null || string.IsNullOrWhiteSpace(need.PartId))
            {
                continue;
            }

            var partId = need.PartId.Trim();

            if (!lotsByPart.TryGetValue(partId, out var lots))
            {
                lots = [];
                lotsByPart[partId] = lots;
                partOrder.Add(partId);
            }

            var lot = need.HeatLotNumber?.Trim() ?? string.Empty;

            if (lot.Length > 0 && !lots.Contains(lot, StringComparer.OrdinalIgnoreCase))
            {
                lots.Add(lot);
            }
        }

        return string.Join(
            "; ",
            partOrder.Select(partId => DescribePart(partId, lotsByPart[partId]))
        );
    }

    private static string DescribePart(string partId, List<string> lots) =>
        lots.Count switch
        {
            0 => partId,
            1 => $"{partId} · lot {lots[0]}",
            _ => $"{partId} · lots {string.Join(", ", lots)}",
        };
}
