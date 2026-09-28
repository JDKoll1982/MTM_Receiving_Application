using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One chart data point for the Receiving Analytics tool: a date, a category, and
/// the number of receiving lines on that date for that category (History or Forecast).
/// </summary>
public class Model_Tool_ReceivingAnalyticsPoint
{
    public DateTime Date { get; set; }

    public string Category { get; set; } = string.Empty;

    public int LineCount { get; set; }
}
