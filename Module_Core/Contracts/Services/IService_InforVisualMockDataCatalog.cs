using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Provides JSON-backed mock data for Infor Visual-dependent workflows.
/// </summary>
public interface IService_InforVisualMockDataCatalog
{
    /// <summary>
    /// Gets the loaded Infor Visual mock catalog.
    /// </summary>
    Model_InforVisualMockDataCatalog GetCatalog();

    /// <summary>
    /// Gets the default purchase order number for mock-mode startup.
    /// </summary>
    string? GetDefaultPurchaseOrderNumber();

    /// <summary>
    /// Gets the unique mock locations.
    /// </summary>
    IReadOnlyList<string> GetLocations();

    /// <summary>
    /// Gets the combined base and runtime mock receiving transactions.
    /// </summary>
    IReadOnlyList<Model_InforVisualMockReceivingTransaction> GetReceivingTransactions();

    /// <summary>
    /// Gets the combined base and runtime Customer Pull n' Pack mock demand rows.
    /// </summary>
    IReadOnlyList<Model_InforVisualCustomerPullPackDemandRow> GetCustomerPullPackDemandRows();

    /// <summary>
    /// Gets the combined base and runtime Customer Pull n' Pack mock location rows.
    /// </summary>
    IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> GetCustomerPullPackLocationRows();

    /// <summary>
    /// Appends or updates mock receiving transactions after a successful receiving save.
    /// </summary>
    /// <param name="transactions"></param>
    Task<Model_Dao_Result<int>> AppendReceivingTransactionsAsync(
        IEnumerable<Model_InforVisualMockReceivingTransaction> transactions
    );
}
