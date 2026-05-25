using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Mock-mode demand row used by the Customer Pull n' Pack feature when the live Infor Visual server is unavailable.
/// Linked-waitlist scenario flags are embedded here so report flows can be tested without a separate server dependency.
/// </summary>
public class Model_InforVisualCustomerPullPackDemandRow
{
    public string SourceLineKey { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerOrderId { get; set; } = string.Empty;

    public string ParentPartId { get; set; } = string.Empty;

    public string SourceLocationId { get; set; } = string.Empty;

    public decimal ShipQuantity { get; set; }

    public DateTime PullDate { get; set; }

    public decimal QuantityToPack { get; set; }

    public decimal FgOnHandQuantity { get; set; }

    public string FgLocationId { get; set; } = string.Empty;

    public bool ShortageFlag { get; set; }

    public bool LateOrderFlag { get; set; }

    public bool PulledFlag { get; set; }

    public bool HasLinkedWaitlist { get; set; }

    public string LinkedWaitlistId { get; set; } = string.Empty;

    public string LinkedWaitlistStatus { get; set; } = string.Empty;

    public string RequesterNote { get; set; } = string.Empty;

    public bool RecheckIndicator { get; set; }
}
