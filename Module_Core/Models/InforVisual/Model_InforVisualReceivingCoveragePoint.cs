using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// One aggregated receiving line count at date, category and part grain, used by the App Data
/// coverage panel so Infor Visual can be compared against the receiving app on the same material.
/// Read from Infor Visual (MTMFG).
/// </summary>
public class Model_InforVisualReceivingCoveragePoint
{
    public DateTime ActivityDate { get; set; }

    /// <summary>Parts | MMC Coils | MMF Flat | Outside Service | Uninventoried.</summary>
    public string Category { get; set; } = string.Empty;

    public string PartId { get; set; } = string.Empty;

    public int LineCount { get; set; }
}
