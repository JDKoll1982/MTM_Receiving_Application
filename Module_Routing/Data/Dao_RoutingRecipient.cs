using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing_recipients table
/// </summary>
public class Dao_RoutingRecipient
{
    private readonly string _connectionString;

    public Dao_RoutingRecipient(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all active recipients
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetAllActiveRecipientsAsync()
    {
        try
        {
            return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_RoutingRecipient>(
                _connectionString,
                "sp_routing_recipient_get_all_active",
                MapFromReader
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingRecipient>>($"Error retrieving recipients: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves top recipients by usage (personalized or system-wide)
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_RoutingRecipient>>> GetTopRecipientsByUsageAsync(int employeeNumber, int topCount = 5)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_employee_number", employeeNumber },
                { "p_top_count", topCount }
            };

            return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_RoutingRecipient>(
                _connectionString,
                "sp_routing_recipient_get_top_by_usage",
                MapFromReaderWithUsage,
                parameters
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingRecipient>>($"Error retrieving top recipients: {ex.Message}", ex);
        }
    }

    private Model_RoutingRecipient MapFromReader(IDataReader reader)
    {
        return new Model_RoutingRecipient
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            Location = reader.GetString("location"),
            Department = reader.IsDBNull(reader.GetOrdinal("department")) ? null : reader.GetString("department"),
            IsActive = reader.GetBoolean("is_active"),
            CreatedDate = reader.GetDateTime("created_date"),
            UpdatedDate = reader.GetDateTime("updated_date")
        };
    }

    private Model_RoutingRecipient MapFromReaderWithUsage(IDataReader reader)
    {
        var recipient = MapFromReader(reader);
        recipient.UsageCount = reader.GetInt32("usage_count");
        return recipient;
    }
}
