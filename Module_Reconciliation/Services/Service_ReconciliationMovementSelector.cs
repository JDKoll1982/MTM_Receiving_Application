using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Narrows a part's movements down to the ones reachable from one group's own
/// evidence. A movement is reachable when either of its locations has already been
/// reached or its purchase order key is one of the group's seeds, and the pass
/// repeats until nothing more is admitted so a chained transfer is followed to its
/// end (FR-017, FR-030).
/// </summary>
/// <remarks>
/// The walk is bidirectional on purpose. A row's saved location is frequently the
/// destination the material already reached, so matching on the source alone would
/// reject the very movement that delivered it and report the part as not found.
/// Transfer rows in Infor Visual usually carry no purchase order, so the purchase
/// order branch cannot rescue those groups.
/// <para>
/// Only movements booked at or after the group's own receipt time count. Another
/// user moving the same part into the same location earlier in the day is not
/// transport of this material, and counting it would report the destination as
/// having received more than these rows account for.
/// </para>
/// </remarks>
public sealed class Service_ReconciliationMovementSelector
    : IService_ReconciliationMovementSelector
{
    /// <inheritdoc />
    public IReadOnlyList<Model_ReconciliationMovement> Select(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationMovement> movements
    )
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(movements);

        var reachedLocations = new HashSet<string>(
            group.SourceLocations.Select(Helper_ReconciliationCanonical.Normalize),
            StringComparer.Ordinal
        );

        var allowedPurchaseOrderKeys = new HashSet<string>(
            group.Rows
                .Select(row =>
                    Helper_ReconciliationCanonical.PurchaseOrderKey(
                        row.PurchaseOrderNumber,
                        row.PurchaseOrderLineNumber
                    )
                )
                .Where(key => key.Length != 0),
            StringComparer.Ordinal
        );

        DateTime? receivedNotBefore = EarliestReceiptTime(group);

        var pending = movements
            .Where(movement => movement.IsSameLocation is false)
            .Where(movement => WasBookedAfterReceipt(movement, receivedNotBefore))
            .ToList();
        var selected = new List<Model_ReconciliationMovement>();

        bool admittedAny = true;

        while (admittedAny && pending.Count > 0)
        {
            admittedAny = false;

            for (int index = pending.Count - 1; index >= 0; index--)
            {
                Model_ReconciliationMovement movement = pending[index];

                if (IsReachable(movement, reachedLocations, allowedPurchaseOrderKeys) is false)
                {
                    continue;
                }

                selected.Add(movement);
                pending.RemoveAt(index);
                reachedLocations.Add(
                    Helper_ReconciliationCanonical.Normalize(movement.SourceLocationId)
                );
                reachedLocations.Add(
                    Helper_ReconciliationCanonical.Normalize(movement.DestinationLocationId)
                );
                admittedAny = true;
            }
        }

        return selected
            .OrderBy(movement => movement.TransactionDate)
            .ThenBy(movement => movement.DestinationTransactionId)
            .ThenBy(movement => movement.SourceTransactionId)
            .ToList();
    }

    /// <summary>
    /// The moment the group's material arrived, which is the earliest receipt stamp
    /// its rows carry. A group with no stamp keeps every movement in the window.
    /// </summary>
    private static DateTime? EarliestReceiptTime(Model_ReconciliationGroup group)
    {
        DateTime? earliest = null;

        foreach (Model_ReconciliationRow row in group.Rows)
        {
            if (row.ReceivedAt is not DateTime received)
            {
                continue;
            }

            if (earliest is null || received < earliest.Value)
            {
                earliest = received;
            }
        }

        return earliest;
    }

    /// <summary>
    /// A movement booked before the material was received cannot have carried it.
    /// A stamp with no time of day is a date-only record - Visual had no create date
    /// and the date column holds no clock - so it is never excluded on time alone.
    /// </summary>
    private static bool WasBookedAfterReceipt(
        Model_ReconciliationMovement movement,
        DateTime? receivedNotBefore
    )
    {
        if (receivedNotBefore is not DateTime notBefore)
        {
            return true;
        }

        DateTime booked = movement.MovementDateTime;

        if (booked.TimeOfDay == TimeSpan.Zero)
        {
            return true;
        }

        return booked >= notBefore;
    }

    private static bool IsReachable(
        Model_ReconciliationMovement movement,
        HashSet<string> reachedLocations,
        HashSet<string> allowedPurchaseOrderKeys
    )
    {
        if (
            reachedLocations.Contains(
                Helper_ReconciliationCanonical.Normalize(movement.SourceLocationId)
            )
            || reachedLocations.Contains(
                Helper_ReconciliationCanonical.Normalize(movement.DestinationLocationId)
            )
        )
        {
            return true;
        }

        string purchaseOrderKey = Helper_ReconciliationCanonical.PurchaseOrderKey(
            movement.PurchaseOrderNumber,
            movement.PurchaseOrderLineNumber
        );

        return purchaseOrderKey.Length != 0 && allowedPurchaseOrderKeys.Contains(purchaseOrderKey);
    }
}
