using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Contracts;

/// <summary>
/// Maintains the welded part numbers list shown on the Receiving settings page.
/// </summary>
public interface IService_Settings_ReceivingWeldedParts
{
    /// <summary>Returns every welded part number.</summary>
    Task<Model_Dao_Result<List<Model_Settings_ReceivingWeldedPart>>> GetAllAsync();

    /// <summary>Adds a part number after trimming, checking its length and the part master.</summary>
    Task<Model_Dao_Result<int>> AddAsync(string partId);

    /// <summary>Removes a part number.</summary>
    Task<Model_Dao_Result> DeleteAsync(int id);
}
