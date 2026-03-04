using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Contract for the Outside Service Provider History lookup tool.
/// Delegates to <see cref="MTM_Receiving_Application.Module_Core.Contracts.Services.IService_InforVisual"/>
/// for read-only Infor Visual queries.
/// </summary>
public interface IService_Tool_OutsideServiceHistory
{
    /// <summary>
    /// Retrieves all outside service dispatch records for the specified part number.
    /// </summary>
    /// <param name="partNumber">Part ID to search for in SERVICE_DISP_LINE.</param>
    Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByPartAsync(string partNumber);

    /// <summary>
    /// Retrieves all outside service dispatch records for the specified vendor ID.
    /// </summary>
    /// <param name="vendorId">Vendor ID to filter dispatch records by.</param>
    Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByVendorAsync(string vendorId);

    /// <summary>
    /// Fuzzy-searches Infor Visual for parts whose ID contains <paramref name="term"/>.
    /// Returns candidates for display in the selection picker.
    /// </summary>
    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(string term);

    /// <summary>
    /// Fuzzy-searches Infor Visual for vendors whose name contains <paramref name="term"/>.
    /// Returns candidates for display in the selection picker.
    /// </summary>
    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchVendorsAsync(string term);

    /// <summary>
    /// Returns all distinct part numbers serviced by the specified vendor,
    /// with dispatch count and last dispatch date for each part.
    /// </summary>
    /// <param name="vendorId">The vendor ID confirmed by the user.</param>
    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(string vendorId);

    /// <summary>
    /// Retrieves outside service dispatch history filtered by both vendor ID and part number.
    /// </summary>
    /// <param name="vendorId">The vendor ID selected by the user.</param>
    /// <param name="partNumber">The part number selected from the vendor's parts list.</param>
    Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetHistoryByVendorAndPartAsync(string vendorId, string partNumber);
}
