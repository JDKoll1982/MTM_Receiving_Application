using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Contract for searching PO line binary specs with weighted fuzzy ranking.
/// </summary>
public interface IService_Tool_POLineSpecSearch
{
    Task<Model_Dao_Result<List<Model_Tool_POLineSpecSearchResult>>> SearchAsync(
        string searchTerm,
        Model_Tool_POLineSpecSearchOptions options,
        int maxResults = 200
    );
}
