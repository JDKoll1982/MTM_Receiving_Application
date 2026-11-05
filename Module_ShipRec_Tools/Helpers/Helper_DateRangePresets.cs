using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// One quick date range a user can pick instead of typing two dates.
/// </summary>
/// <param name="Key">The stable key the view sends back.</param>
/// <param name="DisplayName">The wording a user reads.</param>
public sealed record Model_DateRangePreset(string Key, string DisplayName);

/// <summary>
/// The quick date ranges the app offers, and the dates each one resolves to.
/// </summary>
/// <remarks>
/// Every preset looks backwards, because the records these pages read are always in the past. Each one
/// is bounded at both ends so a preset can never ask for an open-ended scan.
/// The list matches the Date Type menu on Receiving Analytics, so a user meets the same nine choices
/// wherever a date range is asked for.
/// </remarks>
public static class Helper_DateRangePresets
{
    public const string Last7Days = "Last7Days";
    public const string Last30Days = "Last30Days";
    public const string Last90Days = "Last90Days";
    public const string ThisMonth = "ThisMonth";
    public const string LastMonth = "LastMonth";
    public const string ThisQuarter = "ThisQuarter";
    public const string LastQuarter = "LastQuarter";
    public const string ThisYear = "ThisYear";
    public const string Last12Months = "Last12Months";

    /// <summary>Every preset, in menu order.</summary>
    public static IReadOnlyList<Model_DateRangePreset> All { get; } =
    [
        new(Last7Days, "Last 7 Days"),
        new(Last30Days, "Last 30 Days"),
        new(Last90Days, "Last 90 Days"),
        new(ThisMonth, "This Month"),
        new(LastMonth, "Last Month"),
        new(ThisQuarter, "This Quarter"),
        new(LastQuarter, "Last Quarter"),
        new(ThisYear, "This Year"),
        new(Last12Months, "Last 12 Months"),
    ];

    /// <summary>
    /// The first and last day a preset covers, both inclusive. An unknown key resolves to the last
    /// 30 days, which is the same fallback the analytics pages use.
    /// </summary>
    public static (DateTime From, DateTime To) Resolve(string? presetKey)
    {
        var today = DateTime.Today;

        return presetKey switch
        {
            Last7Days => (today.AddDays(-6), today),
            Last90Days => (today.AddDays(-89), today),
            ThisMonth => (StartOfMonth(today), today),
            LastMonth => (StartOfMonth(today.AddMonths(-1)), EndOfMonth(today.AddMonths(-1))),
            ThisQuarter => (StartOfQuarter(today), today),
            LastQuarter => (
                StartOfQuarter(today.AddMonths(-3)),
                EndOfQuarter(today.AddMonths(-3))
            ),
            ThisYear => (new DateTime(today.Year, 1, 1), today),
            Last12Months => (today.AddMonths(-12), today),
            _ => (today.AddDays(-29), today),
        };
    }

    private static DateTime StartOfMonth(DateTime value) => new(value.Year, value.Month, 1);

    private static DateTime EndOfMonth(DateTime value) =>
        StartOfMonth(value).AddMonths(1).AddDays(-1);

    private static DateTime StartOfQuarter(DateTime value)
    {
        var startMonth = ((value.Month - 1) / 3) * 3 + 1;
        return new DateTime(value.Year, startMonth, 1);
    }

    private static DateTime EndOfQuarter(DateTime value) =>
        StartOfQuarter(value).AddMonths(3).AddDays(-1);
}
