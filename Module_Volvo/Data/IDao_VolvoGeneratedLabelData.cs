using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Abstraction over the active/history Volvo generated-label queue.
/// </summary>
public interface IDao_VolvoGeneratedLabelData
{
    Task<Model_Dao_Result<int>> ReplaceForShipmentAsync(
        int shipmentId,
        List<Model_VolvoGeneratedLabelData> rows
    );

    Task<Model_Dao_Result<List<Model_VolvoGeneratedLabelData>>> GetActiveLabelDataAsync();

    Task<Model_Dao_Result<int>> DeleteByShipmentAsync(int shipmentId);

    Task<Model_Dao_Result<int>> ClearToHistoryAsync(string archivedBy);
}
