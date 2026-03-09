using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;

/// <summary>
/// Provides fuzzy-search operations against Infor Visual (READ-ONLY SQL Server) for the
/// Bulk Inventory data-entry grid pickers.
/// </summary>
public interface IService_BulkInventory_FuzzySearch
{
    /// <summary>
    /// Fuzzy-searches parts by ID (LIKE '%term%') and returns up to 50 candidates.
    /// Delegates to <see cref="MTM_Receiving_Application.Module_Core.Contracts.Services.IService_InforVisual.FuzzySearchPartsAsync"/>.
    /// </summary>
    /// <param name="term">Partial part ID entered by the user.</param>
    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchPartsAsync(string term);

    /// <summary>
    /// Fuzzy-searches warehouse locations by ID (LIKE '%term%'), scoped to
    /// <paramref name="warehouseCode"/>, and returns up to 50 candidates.
    /// Delegates to <see cref="MTM_Receiving_Application.Module_Core.Contracts.Services.IService_InforVisual.FuzzySearchLocationsAsync"/>.
    /// </summary>
    /// <param name="term">Partial location ID entered by the user.</param>
    /// <param name="warehouseCode">Warehouse code that scopes the search (e.g. "002").</param>
    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchLocationsAsync(string term, string warehouseCode);
}
