namespace MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

/// <summary>
/// Time granularity for bucketing Receiving Analytics data.
/// </summary>
public enum Enum_ReceivingAnalyticsPeriod
{
    /// <summary>
    /// One bucket per calendar day.
    /// </summary>
    Day,

    /// <summary>
    /// One bucket per calendar week.
    /// </summary>
    Week,

    /// <summary>
    /// One bucket per calendar month.
    /// </summary>
    Month,
}
