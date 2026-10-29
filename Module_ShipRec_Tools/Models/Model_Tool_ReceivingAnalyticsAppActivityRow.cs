using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One received day, material category and unit of measure row for the Receiving
/// Analytics tool's activity panel.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppActivityRow
{
    /// <summary>The calendar date of <c>COALESCE(received_date, transaction_date)</c>.</summary>
    public DateTime BucketDate { get; set; }

    /// <summary>Stored <c>part_type</c>, passed through untranslated.</summary>
    public string PartType { get; set; } = string.Empty;

    /// <summary>Stored unit of measure; blank is allowed and reads as <c>EA</c> in the UI.</summary>
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary><c>SUM(COALESCE(weight_quantity, quantity))</c> for the day, category and unit.</summary>
    public decimal QuantityTotal { get; set; }

    /// <summary>
    /// Receipts for the day, category and unit. One row of the receiving history is one app label,
    /// which is the same unit the ERP counts receipt lines in, so this is the figure the category mix
    /// compares. Unlike <see cref="LoadCount"/> and <see cref="LabelCount"/> it is a per-group figure
    /// and summing it over a day's rows gives that day's label count.
    /// </summary>
    public int ReceiptCount { get; set; }

    /// <summary>
    /// Distinct loads for the day. This is a per-day figure that repeats on every
    /// category and unit row of the same day, so a consumer reads it once per day
    /// and never sums it across rows.
    /// </summary>
    public int LoadCount { get; set; }

    /// <summary>
    /// <c>COUNT(*)</c> for the day. This is a per-day figure that repeats on every
    /// category and unit row of the same day, so a consumer reads it once per day
    /// and never sums it across rows.
    /// </summary>
    public int LabelCount { get; set; }
}
