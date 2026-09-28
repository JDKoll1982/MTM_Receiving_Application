using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// One aggregated receiving data point (date + category + line count) used by the
/// Receiving Analytics chart tool for either the History (past received) or
/// Forecast (incoming) dataset. Read from Infor Visual (MTMFG).
/// </summary>
public class Model_InforVisualReceivingAnalyticsPoint
{
    public DateTime ActivityDate { get; set; }

    /// <summary>Parts | MMC Coils | MMF Flat | Outside Service | Uninventoried.</summary>
    public string Category { get; set; } = string.Empty;

    public int LineCount { get; set; }
}
