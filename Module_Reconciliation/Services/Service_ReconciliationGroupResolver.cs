using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Partitions candidate rows into the pools that compete for movement evidence, by
/// purchase order, part and transaction date, and hands the receipts without a
/// purchase order to the non-purchase-order resolver (FR-015, FR-019, FR-020,
/// decision D-06).
/// </summary>
public sealed class Service_ReconciliationGroupResolver : IService_ReconciliationGroupResolver
{
    private readonly IService_ReconciliationNonPurchaseOrderResolver _nonPurchaseOrderResolver;

    /// <summary>Creates the resolver over the non-purchase-order resolver.</summary>
    /// <param name="nonPurchaseOrderResolver">The resolver for receipts without a purchase order.</param>
    public Service_ReconciliationGroupResolver(
        IService_ReconciliationNonPurchaseOrderResolver nonPurchaseOrderResolver
    )
    {
        _nonPurchaseOrderResolver = nonPurchaseOrderResolver;
    }

    /// <summary>
    /// Why a row cannot join a group, or an empty string when it can. A row that
    /// fails here is reported as Skipped and never reaches the quantity proof
    /// (FR-032). A non-purchase-order row is never failed for lacking a purchase
    /// order (FR-020).
    /// </summary>
    /// <param name="row">The row to test.</param>
    public static string GetCandidacyFailureReason(Model_ReconciliationRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (string.IsNullOrWhiteSpace(row.PartId))
        {
            return "The row has no part, so it cannot be reconciled.";
        }

        if (row.IsNonPurchaseOrder)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(row.PurchaseOrderNumber))
        {
            return "The row has no purchase order, so its movement evidence cannot be identified.";
        }

        if (
            Helper_ReconciliationCanonical.CanonicalPurchaseOrder(row.PurchaseOrderNumber).Length == 0
        )
        {
            return $"The purchase order '{row.PurchaseOrderNumber}' is not a recognisable purchase order number.";
        }

        return string.Empty;
    }

    /// <inheritdoc />
    public Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> Resolve(
        IReadOnlyList<Model_ReconciliationRow> rows
    )
    {
        ArgumentNullException.ThrowIfNull(rows);

        List<Model_ReconciliationRow> eligible = rows
            .Where(row => GetCandidacyFailureReason(row).Length == 0)
            .ToList();

        List<Model_ReconciliationGroup> groups = BuildPurchaseOrderGroups(eligible);

        List<Model_ReconciliationRow> nonPurchaseOrderRows = eligible
            .Where(row => row.IsNonPurchaseOrder)
            .ToList();

        if (nonPurchaseOrderRows.Count > 0)
        {
            Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> nonPurchaseOrderGroups =
                _nonPurchaseOrderResolver.Resolve(nonPurchaseOrderRows);

            if (nonPurchaseOrderGroups.IsSuccess is false)
            {
                return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_ReconciliationGroup>>(
                    nonPurchaseOrderGroups.ErrorMessage,
                    nonPurchaseOrderGroups.Exception
                );
            }

            groups.AddRange(nonPurchaseOrderGroups.Data ?? []);
        }

        return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ReconciliationGroup>>(
            Order(groups)
        );
    }

    /// <inheritdoc />
    public Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> ResolvePurchaseOrderOnly(
        IReadOnlyList<Model_ReconciliationRow> rows
    )
    {
        ArgumentNullException.ThrowIfNull(rows);

        List<Model_ReconciliationRow> eligible = rows
            .Where(row => GetCandidacyFailureReason(row).Length == 0 && row.IsNonPurchaseOrder is false)
            .ToList();

        return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ReconciliationGroup>>(
            Order(BuildPurchaseOrderGroups(eligible))
        );
    }

    private static List<Model_ReconciliationGroup> BuildPurchaseOrderGroups(
        IReadOnlyList<Model_ReconciliationRow> rows
    ) =>
        rows.Where(row => row.IsNonPurchaseOrder is false)
            .GroupBy(
                row => new Model_ReconciliationGroupKey(
                    Helper_ReconciliationCanonical.CanonicalPurchaseOrder(row.PurchaseOrderNumber),
                    Helper_ReconciliationCanonical.Normalize(row.PartId),
                    row.TransactionDate,
                    string.Empty
                )
            )
            .Select(group => CreateGroup(group.Key, group.ToList()))
            .ToList();

    private static Model_ReconciliationGroup CreateGroup(
        Model_ReconciliationGroupKey key,
        IReadOnlyList<Model_ReconciliationRow> rows
    )
    {
        string[] savedLocations = rows
            .Select(row => row.SavedLocation.Trim())
            .Where(location => location.Length != 0)
            .ToArray();

        var sourceLocations = new HashSet<string>(
            savedLocations.Select(Helper_ReconciliationCanonical.Normalize),
            StringComparer.Ordinal
        );
        sourceLocations.Remove(string.Empty);

        return new Model_ReconciliationGroup
        {
            Key = key,
            Rows = rows.OrderBy(row => row.LoadNumber ?? int.MaxValue)
                .ThenBy(row => row.LoadId ?? Guid.Empty)
                .ToList(),
            SourceLocations = sourceLocations,
            ReceivingLocation = Helper_ReconciliationCanonical.MostCommonLocation(savedLocations),
        };
    }

    private static List<Model_ReconciliationGroup> Order(
        IEnumerable<Model_ReconciliationGroup> groups
    ) =>
        groups.OrderBy(group => group.Key.PartId, StringComparer.Ordinal)
            .ThenBy(group => group.Key.PurchaseOrderNumber, StringComparer.Ordinal)
            .ThenBy(group => group.Key.VendorKey, StringComparer.Ordinal)
            .ThenBy(group => group.Key.TransactionDate)
            .ToList();
}
