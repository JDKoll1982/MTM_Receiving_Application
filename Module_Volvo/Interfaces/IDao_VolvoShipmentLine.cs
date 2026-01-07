using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Interfaces;

/// <summary>
/// Interface for Volvo shipment line data access operations
/// Created for testability and dependency injection
/// </summary>
public interface IDao_VolvoShipmentLine
{
    Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(int shipmentId);
    Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoShipmentLine line);
    Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipmentLine line);
    Task<Model_Dao_Result> DeleteAsync(int lineId);
}
