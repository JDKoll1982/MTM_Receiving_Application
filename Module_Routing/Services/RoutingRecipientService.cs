using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for recipient management with smart sorting and Quick Add logic
/// </summary>
public class RoutingRecipientService : IRoutingRecipientService
{
    private readonly Dao_RoutingRecipient _daoRecipient;
    private readonly ILoggingService _logger;

    public RoutingRecipientService(
        Dao_RoutingRecipient daoRecipient,
        ILoggingService logger)
    {
        _daoRecipient = daoRecipient ?? throw new ArgumentNullException(nameof(daoRecipient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetActiveRecipientsSortedByUsageAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Getting active recipients sorted by usage for employee {employeeNumber}");
            return await _daoRecipient.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting active recipients: {ex.Message}", ex);
            return Model_Dao_Result<List<Model_RoutingRecipient>>.Failure($"Error getting recipients: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllRecipientsAsync()
    {
        try
        {
            await _logger.LogInformationAsync("Getting all recipients (including inactive)");
            return await _daoRecipient.GetAllActiveAsync(); // Note: DAO only returns active - would need new method for all
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting all recipients: {ex.Message}", ex);
            return Model_Dao_Result<List<Model_RoutingRecipient>>.Failure($"Error getting recipients: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<Model_RoutingRecipient>> GetRecipientByIdAsync(int recipientId)
    {
        try
        {
            await _logger.LogInformationAsync($"Getting recipient by ID: {recipientId}");
            
            var result = await _daoRecipient.GetAllActiveAsync();
            if (result.IsSuccess)
            {
                var recipient = result.Data?.FirstOrDefault(r => r.Id == recipientId);
                if (recipient != null)
                {
                    return Model_Dao_Result<Model_RoutingRecipient>.Success(recipient, "Recipient found", 1);
                }
                else
                {
                    return Model_Dao_Result<Model_RoutingRecipient>.Failure($"Recipient {recipientId} not found");
                }
            }
            
            return Model_Dao_Result<Model_RoutingRecipient>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting recipient {recipientId}: {ex.Message}", ex);
            return Model_Dao_Result<Model_RoutingRecipient>.Failure($"Error getting recipient: {ex.Message}", ex);
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
            (r.Location != null && r.Location.ToLowerInvariant().Contains(search)) ||
            (r.Department != null && r.Department.ToLowerInvariant().Contains(search))
        ).ToList();
    }

    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetQuickAddRecipientsAsync(int employeeNumber)
    {
        try
        {
            await _logger.LogInformationAsync($"Getting Quick Add recipients for employee {employeeNumber}");
            
            // Dao_RoutingRecipient.GetTopByUsageAsync implements the 20-label threshold logic
            return await _daoRecipient.GetTopByUsageAsync(employeeNumber, 5);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting Quick Add recipients: {ex.Message}", ex);
            return Model_Dao_Result<List<Model_RoutingRecipient>>.Failure($"Error getting Quick Add recipients: {ex.Message}", ex);
        }
    }
}
