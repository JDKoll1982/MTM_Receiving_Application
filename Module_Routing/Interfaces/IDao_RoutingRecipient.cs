using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Interfaces;

/// <summary>
/// Interface for routing recipient data access operations
/// Issue #25: Created for testability and dependency injection
/// </summary>
public interface IDao_RoutingRecipient
{
    Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllActiveRecipientsAsync();
    Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetTopRecipientsByUsageAsync(int employeeNumber, int topCount = 5);
}
