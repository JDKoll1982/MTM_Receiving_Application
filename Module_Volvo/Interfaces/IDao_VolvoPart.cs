using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Interfaces;

/// <summary>
/// Interface for Volvo part data access operations
/// Created for testability and dependency injection
/// </summary>
public interface IDao_VolvoPart
{
    Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(bool includeInactive = false);
    Task<Model_Dao_Result<Model_VolvoPart>> GetByPartNumberAsync(string partNumber);
    Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoPart part);
    Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part);
    Task<Model_Dao_Result> DeactivateAsync(string partNumber);
}
