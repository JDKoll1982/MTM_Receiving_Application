using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service interface for recipient management: retrieval, sorting, filtering
/// </summary>
public interface IRoutingRecipientService
{
    /// <summary>
    /// Retrieves all active recipients sorted by usage count (personalized for employee)
    /// </summary>
    /// <param name="employeeNumber">Employee number for personalized sorting</param>
    /// <returns>Result with list of active recipients sorted by usage count DESC</returns>
    public Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetActiveRecipientsSortedByUsageAsync(int employeeNumber);

    /// <summary>
    /// Retrieves all recipients (including inactive) for admin purposes
    /// </summary>
    /// <returns>Result with list of all recipients</returns>
    public Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllRecipientsAsync();

    /// <summary>
    /// Retrieves a single recipient by ID
    /// </summary>
    /// <param name="recipientId">Recipient ID to retrieve</param>
    /// <returns>Result with recipient data or error</returns>
    public Task<Model_Dao_Result<Model_RoutingRecipient>> GetRecipientByIdAsync(int recipientId);

    /// <summary>
    /// Filters recipients by search text (name, location, department)
    /// </summary>
    /// <param name="recipients">Full list of recipients to filter</param>
    /// <param name="searchText">Search text (case-insensitive)</param>
    /// <returns>Filtered list of recipients</returns>
    public List<Model_RoutingRecipient> FilterRecipients(List<Model_RoutingRecipient> recipients, string searchText);

    /// <summary>
    /// Calculates top 5 Quick Add recipients for employee
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <returns>Result with top 5 recipients (personalized if employee has 20+ labels, otherwise system-wide)</returns>
    public Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetQuickAddRecipientsAsync(int employeeNumber);

    /// <summary>
    /// Validates that a recipient ID exists and is active (Issue #9: FK validation)
    /// </summary>
    /// <param name="recipientId">Recipient ID to validate</param>
    /// <returns>Result with true if recipient exists and is active, false otherwise</returns>
    public Task<Model_Dao_Result<bool>> ValidateRecipientExistsAsync(int recipientId);
}
