using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Interfaces;

/// <summary>
/// Interface for label history data access operations
/// Issue #25: Created for testability and dependency injection
/// </summary>
public interface IDao_RoutingLabelHistory
{
    Task<Model_Dao_Result> InsertHistoryAsync(Model_RoutingLabelHistory history);
    Task<Model_Dao_Result> InsertHistoryBatchAsync(List<Model_RoutingLabelHistory> historyEntries);
    Task<Model_Dao_Result<List<Model_RoutingLabelHistory>>> GetHistoryByLabelAsync(int labelId);
}
