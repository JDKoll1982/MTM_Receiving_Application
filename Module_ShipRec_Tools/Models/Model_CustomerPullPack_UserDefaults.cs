using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_UserDefaults
{
    public string UserId { get; set; } = string.Empty;

    public string DefaultCustomerId { get; set; } = string.Empty;

    public List<string> FavoriteCustomerIds { get; set; } = [];

    public string LastGoodDateRangeType { get; set; } = string.Empty;

    public DateTime? LastGoodDateFrom { get; set; }

    public DateTime? LastGoodDateTo { get; set; }

    public Enum_CustomerPullPackSortMode DefaultSortMode { get; set; } =
        Enum_CustomerPullPackSortMode.PullDate;

    public bool DefaultShortagesOnly { get; set; }

    public bool DefaultUnpulledOnly { get; set; }

    public bool DefaultLateOrdersOnly { get; set; }

    public List<Enum_CustomerPullPackWaitlistStatus> DefaultWaitlistStatusSet { get; set; } = [];

    public Enum_CustomerPullPackPrintMode DefaultPrintPreset { get; set; } =
        Enum_CustomerPullPackPrintMode.CurrentView;
}
