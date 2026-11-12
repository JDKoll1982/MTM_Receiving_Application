using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Assesses receipts that carry no purchase order. They are grouped by part,
/// transaction date and vendor under the same quantity proof and the same outcomes,
/// and a row without a vendor is grouped under an empty vendor key rather than
/// dropped, so it still reaches the day's counts (FR-020, LG-13).
/// </summary>
public sealed class Service_ReconciliationNonPurchaseOrderResolver
    : IService_ReconciliationNonPurchaseOrderResolver
{
    /// <inheritdoc />
    public Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> Resolve(
        IReadOnlyList<Model_ReconciliationRow> rows
    )
    {
        ArgumentNullException.ThrowIfNull(rows);

        List<Model_ReconciliationGroup> groups = rows.Where(row => row.IsNonPurchaseOrder)
            .GroupBy(row =>
                new Model_ReconciliationGroupKey(
                    string.Empty,
                    Helper_ReconciliationCanonical.Normalize(row.PartId),
                    row.TransactionDate,
                    Helper_ReconciliationCanonical.Normalize(row.VendorName)
                )
            )
            .Select(group => CreateGroup(group.Key, group.ToList()))
            .OrderBy(group => group.Key.PartId, StringComparer.Ordinal)
            .ThenBy(group => group.Key.VendorKey, StringComparer.Ordinal)
            .ThenBy(group => group.Key.TransactionDate)
            .ToList();

        return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ReconciliationGroup>>(groups);
    }

    /// <summary>
    /// Why a receipt without a purchase order cannot be assessed, or an empty string
    /// when it can. Only a missing part fails: a missing vendor is stated as the
    /// reason a line could not be proven, never as a reason to skip it (FR-020).
    /// </summary>
    /// <param name="row">The row to test.</param>
    public static string GetAssessmentFailureReason(Model_ReconciliationRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return string.IsNullOrWhiteSpace(row.PartId)
            ? "The row has no part, so it cannot be reconciled."
            : string.Empty;
    }

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
}
