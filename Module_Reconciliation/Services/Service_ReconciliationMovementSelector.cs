using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Narrows a part's movements down to the ones reachable from one group's own
/// evidence. A movement is reachable when its source location has already been
/// reached or its purchase order key is one of the group's seeds, and the pass
/// repeats until nothing more is admitted so a chained transfer is followed to its
/// end (FR-017, FR-030).
/// </summary>
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

        var pending = movements
            .Where(movement => movement.IsSameLocation is false)
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
