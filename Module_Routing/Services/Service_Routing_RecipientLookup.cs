using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for recipient lookup and department auto-fill functionality.
/// Manages routing recipient data for dropdown population and default department selection.
/// </summary>
public class Service_Routing_RecipientLookup : IService_Routing_RecipientLookup
{
    private readonly Dao_Routing_Recipient _daoRecipient;
    private readonly IService_LoggingUtility _logger;
    private List<Model_Routing_Recipient>? _cachedRecipients;
    private DateTime _cacheTimestamp = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public Service_Routing_RecipientLookup(
        Dao_Routing_Recipient daoRecipient,
        IService_LoggingUtility logger)
    {
        _daoRecipient = daoRecipient ?? throw new ArgumentNullException(nameof(daoRecipient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<List<Model_Routing_Recipient>>> GetAllRecipientsAsync(bool forceRefresh)
    {
        try
        {
            // Check cache validity
            var cacheExpired = DateTime.Now - _cacheTimestamp > _cacheExpiration;

            if (!forceRefresh && !cacheExpired && _cachedRecipients != null)
            {
                _logger.LogInfo($"Returning cached recipients ({_cachedRecipients.Count} items)");
                return Model_Dao_Result_Factory.Success(_cachedRecipients);
            }

            // Fetch from database
            _logger.LogInfo("Fetching recipients from database");
            var result = await _daoRecipient.GetAllAsync();

            if (result.IsSuccess && result.Data != null)
            {
                _cachedRecipients = result.Data;
                _cacheTimestamp = DateTime.Now;
                _logger.LogInfo($"Cached {_cachedRecipients.Count} recipients");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving recipients: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Routing_Recipient>>($"Error retrieving recipients: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<string?>> GetDefaultDepartmentAsync(string recipientName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                return Model_Dao_Result_Factory.Failure<string?>("Recipient name is required");
            }

            _logger.LogInfo($"Looking up default department for recipient: {recipientName}");

            // Try to get from cache first
            if (_cachedRecipients != null)
            {
                var cachedRecipient = _cachedRecipients.FirstOrDefault(r =>
                    r.Name.Equals(recipientName, StringComparison.OrdinalIgnoreCase));

                if (cachedRecipient != null)
                {
                    _logger.LogInfo($"Found in cache: {cachedRecipient.DefaultDepartment ?? "(no default)"}");
                    return Model_Dao_Result_Factory.Success(cachedRecipient.DefaultDepartment);
                }
            }

            // Fetch from database
            var result = await _daoRecipient.GetByNameAsync(recipientName);
            if (!result.IsSuccess)
            {
                // Recipient not found - not an error, just no default department
                _logger.LogInfo($"Recipient '{recipientName}' not found in database");
                return Model_Dao_Result_Factory.Success<string?>(null);
            }

            var defaultDept = result.Data?.DefaultDepartment;
            _logger.LogInfo($"Default department for '{recipientName}': {defaultDept ?? "(none)"}");
            return Model_Dao_Result_Factory.Success(defaultDept);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error looking up default department: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string?>($"Error looking up department: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> AddRecipientAsync(Model_Routing_Recipient recipient)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recipient.Name))
            {
                return Model_Dao_Result_Factory.Failure<int>("Recipient name is required");
            }

            _logger.LogInfo($"Adding new recipient: {recipient.Name}");
            var result = await _daoRecipient.InsertAsync(recipient);

            if (result.IsSuccess)
            {
                // Invalidate cache
                _cachedRecipients = null;
                _logger.LogInfo("Cache invalidated after adding new recipient");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding recipient: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error adding recipient: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result> UpdateRecipientAsync(Model_Routing_Recipient recipient)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recipient.Name))
            {
                return Model_Dao_Result_Factory.Failure("Recipient name is required");
            }

            _logger.LogInfo($"Updating recipient ID: {recipient.Id}");
            var result = await _daoRecipient.UpdateAsync(recipient);

            if (result.IsSuccess)
            {
                // Invalidate cache
                _cachedRecipients = null;
                _logger.LogInfo("Cache invalidated after updating recipient");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating recipient: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating recipient: {ex.Message}", ex);
        }
    }

    public Task<Model_Dao_Result<List<Model_Routing_Recipient>>> GetAllRecipientsAsync()
    {
        return GetAllRecipientsAsync(false);
    }

    public async Task<Model_Dao_Result> DeleteRecipientAsync(int recipientId)
    {
        try
        {
            _logger.LogInfo($"Deleting recipient ID: {recipientId}");
            var result = await _daoRecipient.DeleteAsync(recipientId);

            if (result.IsSuccess)
            {
                _cachedRecipients = null; // Invalidate cache
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting recipient: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deleting recipient: {ex.Message}", ex);
        }
    }
}
