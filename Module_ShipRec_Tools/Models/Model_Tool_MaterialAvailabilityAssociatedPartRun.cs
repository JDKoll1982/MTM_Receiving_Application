using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI model for one associated parent part and its best available next run date.
/// </summary>
public class Model_Tool_MaterialAvailabilityAssociatedPartRun
{
    public string AssociatedPartNumber { get; set; } = string.Empty;

    public string AssociatedPartDescription { get; set; } = string.Empty;

    public DateTime NextDueToRunDate { get; set; }

    public bool IsFutureOrTodayRun { get; set; }

    public string NextDueDateSource { get; set; } = string.Empty;

    public string WorkOrderDisplay { get; set; } = string.Empty;

    public string NextRunDateDisplay => NextDueToRunDate.ToString("MM/dd/yyyy");

    public string RunTimingLabel => IsFutureOrTodayRun ? "Next run" : "Latest known run";
}
