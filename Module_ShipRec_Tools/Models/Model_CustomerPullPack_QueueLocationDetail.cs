namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

public sealed class Model_CustomerPullPack_QueueLocationDetail
{
    public string ParentPartId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public decimal OnHandQuantity { get; set; }
}
