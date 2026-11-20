using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// Narrows Edit Mode's loaded rows by text, over every column the row is read from the
/// database with rather than only the columns the grid happens to show.
/// </summary>
/// <remarks>
/// A clerk chasing one skid usually knows a value that is not on screen - the PO status, the
/// package count, the user who entered it, the quality-hold flag. Searching only the visible
/// columns makes those rows unreachable without re-sorting the grid by something else. Every
/// match is a case-insensitive substring test over the value as the clerk would read it, and
/// nothing is compared numerically, so "1" finds 10070 as well as load 1.
/// </remarks>
public static class Helper_ReceivingLoadSearch
{
    /// <summary>The option that searches every column, and the value a fresh page starts on.</summary>
    public const string AllFieldsKey = "All Fields";

    private static readonly string[] ColumnKeysInOrder =
    [
        AllFieldsKey,
        "Part ID",
        "Part Description",
        "Part Type",
        "PO Number",
        "PO Line",
        "PO Status",
        "PO Due Date",
        "Qty Ordered",
        "Unit of Measure",
        "Vendor",
        "Heat/Lot",
        "Heat Shape",
        "Heat Color",
        "Location",
        "Load #",
        "Weight/Qty",
        "Remaining Qty",
        "Packages/Load",
        "Weight/Package",
        "Package Type",
        "Received Date",
        "Created At",
        "User",
        "Employee #",
        "Quality Hold Required",
        "Quality Hold Acknowledged",
        "Hold Restriction",
    ];

    /// <summary>The columns the Find picker offers, all-fields first.</summary>
    public static IReadOnlyList<string> ColumnKeys { get; } = ColumnKeysInOrder;

    /// <summary>
    /// Tests one row against the Find picker's selection.
    /// </summary>
    /// <param name="load">The row to test.</param>
    /// <param name="columnKey">The picker's selection; an unknown or empty key searches every column.</param>
    /// <param name="term">What the clerk typed.</param>
    /// <returns>True when the row carries the text in the chosen column.</returns>
    public static bool Matches(Model_ReceivingLoad load, string? columnKey, string? term)
    {
        ArgumentNullException.ThrowIfNull(load);

        string wanted = term?.Trim() ?? string.Empty;
        if (wanted.Length == 0)
        {
            return true;
        }

        return ValuesFor(load, columnKey).Any(value =>
            value.Contains(wanted, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Reads the column a key names, or every column when the key is the all-fields option
    /// or one this build no longer offers.
    /// </summary>
    private static IEnumerable<string> ValuesFor(Model_ReceivingLoad load, string? columnKey) =>
        (columnKey?.Trim() ?? string.Empty) switch
        {
            "Part ID" => [load.PartID],
            "Part Description" => [load.PartDescription],
            "Part Type" => [load.PartType],
            "PO Number" => [load.PoNumber ?? string.Empty],
            "PO Line" => [load.PoLineNumber],
            "PO Status" => [load.PoStatus],
            "PO Due Date" => Dates(load.PoDueDate),
            "Qty Ordered" => Number(load.QtyOrdered),
            "Unit of Measure" => [load.UnitOfMeasure],
            "Vendor" => [load.PoVendor],
            "Heat/Lot" => [load.HeatLotNumber],
            "Heat Shape" => [load.HeatShapeCode],
            "Heat Color" => [load.HeatColorCode],
            "Location" => [load.InitialLocation],
            "Load #" => Number(load.LoadNumber),
            "Weight/Qty" => Number(load.WeightQuantity),
            "Remaining Qty" => Number(load.RemainingQuantity),
            "Packages/Load" => Number(load.PackagesPerLoad),
            "Weight/Package" => Number(load.WeightPerPackage),
            "Package Type" => [load.PackageTypeName, load.PackageType.ToString()],
            "Received Date" => Dates(load.ReceivedDate),
            "Created At" => Dates(load.CreatedAt),
            "User" => [load.UserId ?? string.Empty],
            "Employee #" => Number(load.EmployeeNumber),
            "Quality Hold Required" => YesNo(load.IsQualityHoldRequired),
            "Quality Hold Acknowledged" => YesNo(load.IsQualityHoldAcknowledged),
            "Hold Restriction" => [load.QualityHoldRestrictionType],
            _ => EveryColumn(load),
        };

    private static IEnumerable<string> EveryColumn(Model_ReceivingLoad load) =>
        ColumnKeysInOrder
            .Where(key => key != AllFieldsKey)
            .SelectMany(key => ValuesFor(load, key));

    private static IEnumerable<string> Dates(DateTime? value) =>
        value is { } moment
            ?
            [
                moment.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                moment.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
            ]
            : [string.Empty];

    private static IEnumerable<string> Dates(DateTime value) => Dates((DateTime?)value);

    private static IEnumerable<string> Number(decimal value) =>
        [
            value.ToString("0.##", CultureInfo.InvariantCulture),
            value.ToString("0.00", CultureInfo.InvariantCulture),
        ];

    private static IEnumerable<string> Number(int value) =>
        [value.ToString(CultureInfo.InvariantCulture)];

    private static IEnumerable<string> YesNo(bool value) => [value ? "yes" : "no"];
}
