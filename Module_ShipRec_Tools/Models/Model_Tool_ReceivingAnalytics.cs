using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Container returned by the Receiving Analytics service with the two datasets
/// the tool toggles between:
///   History  — past received items (by receipt date).
///   Forecast — incoming items (open PO lines by due date).
/// </summary>
public class Model_Tool_ReceivingAnalytics
{
    public List<Model_Tool_ReceivingAnalyticsPoint> History { get; set; } = [];

    public List<Model_Tool_ReceivingAnalyticsPoint> Forecast { get; set; } = [];
}
