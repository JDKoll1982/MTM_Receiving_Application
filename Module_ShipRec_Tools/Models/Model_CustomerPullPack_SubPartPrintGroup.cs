using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_SubPartPrintGroup
{
    public string SubPartId { get; set; } = string.Empty;

    public decimal TotalQuantityNeeded { get; set; }

    public decimal TotalQuantityOnHand { get; set; }

    public List<string> ChosenLocations { get; set; } = [];
}
