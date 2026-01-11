using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for usage tracking to enable personalized Quick Add
/// </summary>
public class RoutingUsageTrackingService : IRoutingUsageTrackingService
{
    private readonly Dao_RoutingUsageTracking _daoUsageTracking;
    private readonly IService_LoggingUtility _logger;

    public RoutingUsageTrackingService(
        Dao_RoutingUsageTracking daoUsageTracking,
        IService_LoggingUtility logger)
    {
        _daoUsageTracking = daoUsageTracking ?? throw new ArgumentNullException(nameof(daoUsageTracking));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Increments the usage count for a recipient-employee pair
    /// </summary>
    /// <param name="employeeNumber">Employee number creating the label</param>
    /// <param name="recipientId">ID of the recipient</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Model_Dao_Result> IncrementUsageCountAsync(int employeeNumber, int recipientId)
    {
        try
        {
            await _logger.LogInfoAsync($"Incrementing usage count for employee {employeeNumber}, recipient {recipientId}");
            return await _daoUsageTracking.IncrementUsageAsync(employeeNumber, recipientId);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error incrementing usage count: {ex.Message}", ex);
            return new Model_Dao_Result { Success = false, ErrorMessage = $"Error updating usage tracking: {ex.Message}", Exception = ex };
        }
    }

    /// <summary>
    /// Gets the usage count for a specific employee-recipient pair
    /// </summary>
    /// <param name="employeeNumber">Employee number</param>
    /// <param name="recipientId">Recipient ID</param>
    /// <returns>Result with usage count</returns>
    public async Task<Model_Dao_Result<int>> GetUsageCountAsync(int employeeNumber, int recipientId)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting usage count for employee {employeeNumber}, recipient {recipientId}");

            // Issue #13: GetUsageCountAsync requires new DAO method
            // Would need: sp_routing_recipient_tracker_get_count(p_employee_number, p_recipient_id)
            // Returns: COUNT(*) or SUM(usage_count) from routing_recipient_tracker table
            // Priority: LOW - Feature not critical for production
            // For now, returning success with 0 as fallback
            await Task.CompletedTask;
            return Model_Dao_Result_Factory.Success<int>(0);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting usage count: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error getting usage count: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> GetEmployeeLabelCountAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting label count for employee {employeeNumber}");

            // Issue #13: GetEmployeeLabelCountAsync requires aggregation query
            // Would need: sp_routing_recipient_tracker_get_employee_total(p_employee_number)
            // Returns: Total label count for employee across all recipients
            // Issue #28: When implementing, add zero-check guard to prevent division by zero
            // Example: if (totalCount == 0) return 0; before calculating percentages
            // Priority: LOW - Analytics feature, not blocking for core functionality
            // For now, returning success with 0 as fallback
            await Task.CompletedTask;
            return Model_Dao_Result_Factory.Success<int>(0);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting employee label count: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error getting label count: {ex.Message}", ex);
        }
    }
}
