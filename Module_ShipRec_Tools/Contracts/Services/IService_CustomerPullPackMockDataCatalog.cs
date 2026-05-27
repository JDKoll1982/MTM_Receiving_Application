using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;

/// <summary>
/// Provides Customer Pull n' Pack-specific mock data from module-owned JSON assets.
/// </summary>
public interface IService_CustomerPullPackMockDataCatalog
{
    Model_CustomerPullPack_MockDataCatalog GetCatalog();

    IReadOnlyList<Model_InforVisualCustomerPullPackDemandRow> GetDemandRows();

    IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> GetLocationRows();
}
