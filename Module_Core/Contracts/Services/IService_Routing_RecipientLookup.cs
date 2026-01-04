using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for routing recipient lookup and department auto-fill.
/// Queries routing_recipients table to populate dropdowns and auto-fill departments.
/// </summary>
public interface IService_Routing_RecipientLookup
{
    /// <summary>
    /// Gets all active recipients for dropdown population.
    /// </summary>
    /// <returns>DAO result with list of active recipients</returns>
    Task<Model_Dao_Result<List<Model_Routing_Recipient>>> GetAllRecipientsAsync();

    /// <summary>
    /// Gets the default department for a recipient.
    /// Used for auto-filling department field when recipient is selected.
    /// </summary>
    /// <param name="recipientName">Recipient name</param>
    /// <returns>DAO result with default department, or null if not set</returns>
    Task<Model_Dao_Result<string?>> GetDefaultDepartmentAsync(string recipientName);

    /// <summary>
    /// Adds a new recipient to the lookup table.
    /// </summary>
    /// <param name="recipient">Recipient to add</param>
    /// <returns>DAO result with created recipient ID</returns>
    Task<Model_Dao_Result<int>> AddRecipientAsync(Model_Routing_Recipient recipient);

    /// <summary>
    /// Updates an existing recipient.
    /// </summary>
    /// <param name="recipient">Updated recipient</param>
    /// <returns>DAO result</returns>
    Task<Model_Dao_Result> UpdateRecipientAsync(Model_Routing_Recipient recipient);

    /// <summary>
    /// Deletes a recipient from the lookup table.
    /// </summary>
    /// <param name="recipientId">ID of recipient to delete</param>
    /// <returns>DAO result</returns>
    Task<Model_Dao_Result> DeleteRecipientAsync(int recipientId);
}


