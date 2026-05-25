using System;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_DemandFilter
{
    public string CustomerId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public DateTime DateFrom { get; set; } = DateTime.Today;

    public DateTime DateTo { get; set; } = DateTime.Today;

    public bool ShortagesOnly { get; set; }

    public bool UnpulledOnly { get; set; }

    public bool LateOrdersOnly { get; set; }

    public bool LinkedWaitlistOnly { get; set; }

    public bool RequesterWorkOnly { get; set; }

    public Enum_CustomerPullPackSortMode SortMode { get; set; } =
        Enum_CustomerPullPackSortMode.PullDate;

    public string SearchText { get; set; } = string.Empty;
}
