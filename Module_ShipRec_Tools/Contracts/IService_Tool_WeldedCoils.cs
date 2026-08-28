using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Service contract for the Welded Coils tool: manages the settings_weldedcoils table.
/// </summary>
public interface IService_Tool_WeldedCoils
{
    /// <summary>Returns all welded-coil rows.</summary>
    Task<Model_Dao_Result<List<Model_Tool_WeldedCoil>>> GetAllAsync();

    /// <summary>Adds a part (defaults active). Returns the new row id.</summary>
    Task<Model_Dao_Result<int>> InsertAsync(string partId);

    /// <summary>Renames the part number on an existing row.</summary>
    Task<Model_Dao_Result> UpdateAsync(int id, string partId);

    /// <summary>Flips a row between Active and Inactive.</summary>
    Task<Model_Dao_Result> SetActiveAsync(int id, bool isActive);

    /// <summary>Removes a welded-coil row.</summary>
    Task<Model_Dao_Result> DeleteAsync(int id);
}
