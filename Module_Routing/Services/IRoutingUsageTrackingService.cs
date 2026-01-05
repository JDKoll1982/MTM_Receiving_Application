using System.Threading.Tasks;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service interface for usage tracking: increment counts, smart sorting calculation
/// </summary>
public interface IRoutingUsageTrackingService
{
    /// <summary>
    /// Increments usage count for employee-recipient pair (called after label creation)
    /// </summary>
    /// <param name="employeeNumber">Employee who created the label</param>
    /// <param name="recipientId">Recipient selected for the label</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Model_Dao_Result> IncrementUsageCountAsync(int employeeNumber, int recipientId);

    /// <summary>
    /// Retrieves usage count for a specific employee-recipient pair
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <param name="recipientId">Recipient ID</param>
    /// <returns>Result with usage count (0 if no usage record exists)</returns>
    Task<Model_Dao_Result<int>> GetUsageCountAsync(int employeeNumber, int recipientId);

    /// <summary>
    /// Retrieves total number of labels created by employee (for 20-label threshold check)
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <returns>Result with label count</returns>
    Task<Model_Dao_Result<int>> GetEmployeeLabelCountAsync(int employeeNumber);
}
