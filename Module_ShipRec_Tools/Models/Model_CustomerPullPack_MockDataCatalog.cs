using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// Module-owned JSON-backed mock data for the Customer Pull n' Pack workflow.
/// </summary>
public sealed class Model_CustomerPullPack_MockDataCatalog
{
    public List<Model_InforVisualCustomerPullPackDemandRow> DemandRows { get; set; } = [];

    public List<Model_InforVisualCustomerPullPackLocationRow> LocationRows { get; set; } = [];
}
