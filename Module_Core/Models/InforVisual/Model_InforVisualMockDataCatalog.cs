using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// JSON-backed mock data catalog used when Infor Visual mock mode is enabled.
/// </summary>
public class Model_InforVisualMockDataCatalog
{
    public string DefaultPurchaseOrderNumber { get; set; } = string.Empty;

    public List<string> Locations { get; set; } = [];

    public List<Model_InforVisualPO> PurchaseOrders { get; set; } = [];

    public List<Model_InforVisualPart> Parts { get; set; } = [];

    public List<Model_InforVisualMockReceivingTransaction> ReceivingTransactions { get; set; } = [];

    public List<Model_InforVisualAssociatedPartRunRow> AssociatedPartRuns { get; set; } = [];

    public List<Model_OutsideServiceHistory> OutsideServiceHistory { get; set; } = [];
}
