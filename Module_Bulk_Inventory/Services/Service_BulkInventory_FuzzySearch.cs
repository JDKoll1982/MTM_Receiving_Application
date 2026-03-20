using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Services;

/// <summary>
/// Thin adapter that routes Bulk Inventory fuzzy-picker requests to the shared
/// <see cref="IService_InforVisual"/> (READ-ONLY SQL Server) service.
/// </summary>
public class Service_BulkInventory_FuzzySearch : IService_BulkInventory_FuzzySearch
{
    private readonly IService_InforVisual _inforVisual;

    public Service_BulkInventory_FuzzySearch(IService_InforVisual inforVisual)
    {
        ArgumentNullException.ThrowIfNull(inforVisual);
        _inforVisual = inforVisual;
    }

    /// <inheritdoc />
    public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchPartsAsync(string term) =>
        _inforVisual.FuzzySearchPartsAsync(term);

    /// <inheritdoc />
    public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchLocationsAsync(
        string term,
        string warehouseCode
    ) => _inforVisual.FuzzySearchLocationsAsync(term, warehouseCode);
}
