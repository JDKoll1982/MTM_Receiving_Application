namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Summary statistics shown as stat cards on the Receiving Analytics tool.
/// </summary>
public class Model_Tool_ReceivingAnalyticsStats
{
    public int TotalLines { get; set; }

    public double AvgPerDay { get; set; }

    public int PeakDay { get; set; }

    /// <summary>Percent change in total lines vs the previous equivalent period.</summary>
    public double TrendVsLastWeek { get; set; }
}
