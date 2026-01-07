using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Routing.Interfaces;

/// <summary>
/// Interface for usage tracking data access operations
/// Issue #25: Created for testability and dependency injection
/// </summary>
public interface IDao_RoutingUsageTracking
{
    Task<Model_Dao_Result> IncrementUsageAsync(int employeeNumber, int recipientId);
}
