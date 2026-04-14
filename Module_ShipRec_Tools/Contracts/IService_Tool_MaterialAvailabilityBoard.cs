using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Contract for the location and part-based material availability board.
/// </summary>
public interface IService_Tool_MaterialAvailabilityBoard
{
    Task<Model_Dao_Result<List<Model_Tool_MaterialAvailabilityCard>>> GetBoardByLocationAsync(
        string locationId,
        string warehouseCode,
        int? incomingWindowDays
    );

    Task<Model_Dao_Result<List<Model_Tool_MaterialAvailabilityCard>>> GetBoardByPartAsync(
        string partId,
        string warehouseCode,
        int? incomingWindowDays
    );

    Task<Model_Dao_Result<Model_FormattedReportDocument>> FormatBoardForPrintAsync(
        IReadOnlyList<Model_Tool_MaterialAvailabilityCard> cards,
        string searchLabel,
        string searchTerm,
        string warehouseCode,
        string lookAheadOption
    );

    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(string term);

    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchLocationsAsync(
        string term,
        string warehouseCode
    );

    Task<Model_Dao_Result<bool>> PartExistsAsync(string partId);

    Task<Model_Dao_Result<bool>> LocationExistsAsync(string locationId, string warehouseCode);
}
