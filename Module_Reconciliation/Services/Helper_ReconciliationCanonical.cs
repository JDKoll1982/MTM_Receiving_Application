using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// The one place the module canonicalises the text it compares. Part and location
/// matching is case-insensitive and whitespace-insensitive, and a purchase order
/// has exactly one accepted canonical form: <c>PO-######</c> with an optional
/// single letter suffix (FR-015, LG-01).
/// </summary>
public static class Helper_ReconciliationCanonical
{
    private static readonly Regex CanonicalPurchaseOrderPattern = new(
        @"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[A-Za-z]?)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    /// <summary>Trims and upper-cases a value for comparison. Null becomes empty.</summary>
    /// <param name="value">The value to canonicalise.</param>
    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    /// <summary>
    /// Canonicalises a purchase order number to <c>PO-######[A-Z]</c>, or returns
    /// empty when the value cannot be normalised.
    /// </summary>
    /// <param name="purchaseOrderNumber">The raw purchase order number.</param>
    public static string CanonicalPurchaseOrder(string? purchaseOrderNumber)
    {
        if (string.IsNullOrWhiteSpace(purchaseOrderNumber))
        {
            return string.Empty;
        }

        Match match = CanonicalPurchaseOrderPattern.Match(purchaseOrderNumber.Trim());

        if (match.Success is false)
        {
            return string.Empty;
        }

        string digits = match.Groups["digits"].Value.PadLeft(6, '0');
        string suffix = match.Groups["suffix"].Value.ToUpperInvariant();

        return $"PO-{digits}{suffix}";
    }

    /// <summary>
    /// The purchase order key a movement and a row must share to be the same
    /// evidence: the canonical purchase order number and the canonical line.
    /// </summary>
    /// <param name="purchaseOrderNumber">The raw purchase order number.</param>
    /// <param name="purchaseOrderLineNumber">The raw line number.</param>
    public static string PurchaseOrderKey(
        string? purchaseOrderNumber,
        string? purchaseOrderLineNumber
    )
    {
        string canonical = CanonicalPurchaseOrder(purchaseOrderNumber);

        return canonical.Length == 0 ? string.Empty : $"{canonical}|{Normalize(purchaseOrderLineNumber)}";
    }

    /// <summary>True when two location ids name the same location.</summary>
    /// <param name="left">The first location id.</param>
    /// <param name="right">The second location id.</param>
    public static bool LocationsEqual(string? left, string? right) =>
        string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);

    /// <summary>
    /// The saved location a group's rows most often name. Two rows of one group
    /// saved to two different locations is real physical activity, not a data
    /// error, so the modal value anchors the residual.
    /// </summary>
    /// <param name="savedLocations">The raw saved locations of the group's rows.</param>
    public static string MostCommonLocation(IReadOnlyList<string> savedLocations)
    {
        ArgumentNullException.ThrowIfNull(savedLocations);

        if (savedLocations.Count == 0)
        {
            return string.Empty;
        }

        return savedLocations
            .Where(location => string.IsNullOrWhiteSpace(location) is false)
            .GroupBy(Normalize, StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.First().Trim())
            .FirstOrDefault() ?? string.Empty;
    }
}
