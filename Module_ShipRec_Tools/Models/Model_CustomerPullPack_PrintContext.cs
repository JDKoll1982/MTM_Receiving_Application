using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_PrintContext
{
    public Enum_CustomerPullPackPrintMode PrintMode { get; set; } =
        Enum_CustomerPullPackPrintMode.CurrentView;

    public string Title { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string ActiveFiltersSummary { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public List<Model_CustomerPullPack_DemandLine> DemandLines { get; set; } = [];

    public List<Model_CustomerPullPack_WaitlistEntry> WaitlistEntries { get; set; } = [];

    public List<Model_CustomerPullPack_SubPartPrintGroup> GroupedSubPartTotals { get; set; } = [];
}
