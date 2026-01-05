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

    public async Task<Model_Dao_Result<int>> GetUsageCountAsync(int employeeNumber, int recipientId)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting usage count for employee {employeeNumber}, recipient {recipientId}");

            // TODO: Would need a new DAO method to get specific count
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

            // TODO: Would need a new DAO method or query routing_usage_tracking SUM(usage_count)
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
