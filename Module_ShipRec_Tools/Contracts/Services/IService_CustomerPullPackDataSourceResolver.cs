using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;

public interface IService_CustomerPullPackDataSourceResolver
{
    Enum_CustomerPullPackDataSourceMode CurrentMode { get; }

    bool IsMockMode { get; }

    void ResolveForWorkflow();
}
