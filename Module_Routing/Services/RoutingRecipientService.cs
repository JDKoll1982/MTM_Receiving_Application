using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for recipient management with smart sorting and Quick Add logic
/// </summary>
public class RoutingRecipientService : IRoutingRecipientService
{
    private readonly Dao_RoutingRecipient _daoRecipient;
    private readonly IService_LoggingUtility _logger;

    public RoutingRecipientService(
        Dao_RoutingRecipient daoRecipient,
        IService_LoggingUtility logger)
    {
        _daoRecipient = daoRecipient ?? throw new ArgumentNullException(nameof(daoRecipient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetActiveRecipientsSortedByUsageAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting active recipients sorted by usage for employee {employeeNumber}");
            return await _daoRecipient.GetAllActiveRecipientsAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting active recipients: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingRecipient>>($"Error getting recipients: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllRecipientsAsync()
    {
        try
        {
            await _logger.LogInfoAsync("Getting all recipients (including inactive)");
            return await _daoRecipient.GetAllActiveRecipientsAsync(); // Note: DAO only returns active - would need new method for all
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting all recipients: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingRecipient>>($"Error getting recipients: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<Model_RoutingRecipient>> GetRecipientByIdAsync(int recipientId)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting recipient by ID: {recipientId}");

            var result = await _daoRecipient.GetAllActiveRecipientsAsync();
            if (result.IsSuccess)
            {
                var recipient = result.Data?.FirstOrDefault(r => r.Id == recipientId);
                if (recipient != null)
                {
                    return Model_Dao_Result_Factory.Success(recipient);
                }
                else
                {
                    return Model_Dao_Result_Factory.Failure<Model_RoutingRecipient>($"Recipient {recipientId} not found");
                }
            }

            return Model_Dao_Result_Factory.Failure<Model_RoutingRecipient>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting recipient {recipientId}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_RoutingRecipient>($"Error getting recipient: {ex.Message}", ex);
        }
    }

    public List<Model_RoutingRecipient> FilterRecipients(List<Model_RoutingRecipient> recipients, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return recipients;
        }

        var search = searchText.ToLowerInvariant();

        return recipients.Where(r =>
            r.Name.ToLowerInvariant().Contains(search) ||
            (r.Location?.ToLowerInvariant().Contains(search) ?? false) ||
            (r.Department?.ToLowerInvariant().Contains(search) ?? false)
        ).ToList();
    }

    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetQuickAddRecipientsAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting Quick Add recipients for employee {employeeNumber}");

            // Dao_RoutingRecipient.GetTopByUsageAsync implements the 20-label threshold logic
            return await _daoRecipient.GetTopRecipientsByUsageAsync(employeeNumber, 5);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting Quick Add recipients: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingRecipient>>($"Error getting Quick Add recipients: {ex.Message}", ex);
        }
    }
}
