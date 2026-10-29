namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Presentation row for the Receiving Analytics supplier tab. One row per
/// supplier and unit of measure.
/// </summary>
public class Model_Tool_ReceivingAnalyticsAppSupplierRow
{
    /// <summary>
    /// Supplier name, grouped case-insensitively with whitespace trimmed. Blank
    /// values are substituted with "Unknown" by the stored procedure.
    /// </summary>
    public string VendorName { get; set; } = string.Empty;

    /// <summary>
    /// Stored unit of measure. Blank is allowed; blank reads as "EA" in the UI,
    /// not here.
    /// </summary>
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// Label count for the supplier and unit. This is a per-supplier figure that
    /// repeats on every unit row of the same supplier, so an assembler reads it
    /// once per supplier when ranking by labels.
    /// </summary>
    public int LabelCount { get; set; }

    /// <summary>
    /// Load count for the supplier and unit. This is a per-supplier figure that
    /// repeats on every unit row of the same supplier, so an assembler reads it
    /// once per supplier when ranking by loads.
    /// </summary>
    public int LoadCount { get; set; }

    /// <summary>
    /// Total quantity for the supplier and unit, computed as
    /// SUM(COALESCE(weight_quantity, quantity)).
    /// </summary>
    public decimal QuantityTotal { get; set; }
}
