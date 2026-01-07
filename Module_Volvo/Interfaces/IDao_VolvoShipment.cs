using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Interfaces;

/// <summary>
/// Interface for Volvo shipment data access operations
/// Created for testability and dependency injection
/// </summary>
public interface IDao_VolvoShipment
{
    Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetAllAsync(bool includeArchived = false);
    Task<Model_Dao_Result<Model_VolvoShipment>> GetByIdAsync(int shipmentId);
    Task<Model_Dao_Result<Model_VolvoShipment>> GetPendingAsync();
    Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoShipment shipment);
    Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipment shipment);
    Task<Model_Dao_Result> ArchiveAsync(int shipmentId);
}
