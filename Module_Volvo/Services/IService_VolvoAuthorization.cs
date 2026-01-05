using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Authorization service for Volvo module operations
/// Implements role-based access control (RBAC) for Volvo features
/// </summary>
public interface IService_VolvoAuthorization
{
    /// <summary>
    /// Checks if current user can create/modify Volvo shipments
    /// </summary>
    Task<Model_Dao_Result> CanManageShipmentsAsync();

    /// <summary>
    /// Checks if current user can manage Volvo master data (parts, components)
    /// </summary>
    Task<Model_Dao_Result> CanManageMasterDataAsync();

    /// <summary>
    /// Checks if current user can complete/close Volvo shipments
    /// </summary>
    Task<Model_Dao_Result> CanCompleteShipmentsAsync();

    /// <summary>
    /// Checks if current user can generate labels for Volvo shipments
    /// </summary>
    Task<Model_Dao_Result> CanGenerateLabelsAsync();
}
