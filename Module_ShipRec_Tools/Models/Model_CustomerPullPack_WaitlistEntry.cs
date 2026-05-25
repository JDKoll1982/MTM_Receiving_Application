using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_WaitlistEntry
{
    public string WaitlistId { get; set; } = string.Empty;

    public string SourceLineKey { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerOrderId { get; set; } = string.Empty;

    public string ParentPartId { get; set; } = string.Empty;

    public decimal RequestedQuantity { get; set; }

    public List<string> SelectedLocations { get; set; } = [];

    public string RequestedByUserId { get; set; } = string.Empty;

    public string RequestedByDisplayName { get; set; } = string.Empty;

    public string RequesterContextNote { get; set; } = string.Empty;

    public Enum_CustomerPullPackWaitlistStatus CurrentStatus { get; set; } =
        Enum_CustomerPullPackWaitlistStatus.Requested;

    public string CurrentOwnerUserId { get; set; } = string.Empty;

    public string CurrentOwnerDisplayName { get; set; } = string.Empty;

    public bool LocationReviewFlag { get; set; }

    public Enum_CustomerPullPackProblemReason ProblemReason { get; set; } =
        Enum_CustomerPullPackProblemReason.None;

    public string HandlerNote { get; set; } = string.Empty;

    public string CompletionUserId { get; set; } = string.Empty;

    public DateTime? CompletionTimestamp { get; set; }

    public string LastUpdatedByUserId { get; set; } = string.Empty;

    public DateTime LastUpdatedTimestamp { get; set; } = DateTime.UtcNow;

    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;

    public bool RecheckIndicator { get; set; }
}
