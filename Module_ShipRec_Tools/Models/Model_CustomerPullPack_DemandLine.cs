using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_DemandLine
{
    public string SourceLineKey { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerOrderId { get; set; } = string.Empty;

    public string ParentPartId { get; set; } = string.Empty;

    public string SourceLocationId { get; set; } = string.Empty;

    public decimal ShipQuantity { get; set; }

    public DateTime PullDate { get; set; }

    public DateTime? OldestAdded { get; set; }

    public decimal QuantityToPack { get; set; }

    public decimal FgOnHandQuantity { get; set; }

    public string FgLocationId { get; set; } = string.Empty;

    public bool ShortageFlag { get; set; }

    public bool LateOrderFlag { get; set; }

    public bool PulledFlag { get; set; }

    public bool HasLinkedWaitlist { get; set; }

    public string LinkedWaitlistId { get; set; } = string.Empty;

    public bool RecheckIndicator { get; set; }

    public bool IsSelected { get; set; }

    public string WaitlistStateDisplay { get; set; } = string.Empty;

    public decimal QtySatisfied { get; set; }

    public string FulfillmentStatusDisplay { get; set; } = string.Empty;

    public string SubPartAvailabilitySummary { get; set; } = string.Empty;

    public string RequesterNote { get; set; } = string.Empty;

    public List<Model_CustomerPullPack_LocationOption> LocationOptions { get; set; } = [];

    public string PullDateDisplay =>
        PullDate == default ? string.Empty : PullDate.ToString("MM/dd/yyyy");

    public string OldestAddedDisplay =>
        OldestAdded.HasValue ? OldestAdded.Value.ToString("MM/dd/yyyy") : string.Empty;

    public string QtySatisfiedDisplay => QtySatisfied.ToString("0.##");

    public string DemandStatusDisplay =>
        ShortageFlag ? "Shortage"
        : LateOrderFlag ? "Late Order"
        : "Normal";

    public string WaitlistDisplay =>
        HasLinkedWaitlist
            ? string.IsNullOrWhiteSpace(WaitlistStateDisplay)
                ? "Linked"
                : WaitlistStateDisplay
            : "None";

    public string LocationSummaryDisplay =>
        string.IsNullOrWhiteSpace(SourceLocationId) ? FgLocationId : SourceLocationId;

    public bool HasSelectableLocations => LocationOptions.Count > 0;

    public bool HasSelectedLocations => LocationOptions.Any(option => option.Selected);

    public bool HasFulfillmentStatus =>
        string.IsNullOrWhiteSpace(FulfillmentStatusDisplay) is false;
}
