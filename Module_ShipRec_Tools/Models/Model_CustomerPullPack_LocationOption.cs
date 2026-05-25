using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public class Model_CustomerPullPack_LocationOption
{
    public string LocationKey { get; set; } = string.Empty;

    public string SourceLineKey { get; set; } = string.Empty;

    public string ParentPartId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public decimal OnHandQuantity { get; set; }

    public Enum_CustomerPullPackLocationSourceType SourceType { get; set; } =
        Enum_CustomerPullPackLocationSourceType.SubPartOnHand;

    public bool Selected { get; set; }
}
