using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing_other_reasons table
/// </summary>
public class Dao_RoutingOtherReason
{
    private readonly string _connectionString;

    public Dao_RoutingOtherReason(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all active other reasons
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_RoutingOtherReason>>> GetAllActiveReasonsAsync()
    {
        try
        {
            return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_RoutingOtherReason>(
                _connectionString,
                "sp_routing_other_reason_get_all_active",
                MapFromReader
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingOtherReason>>($"Error retrieving other reasons: {ex.Message}", ex);
        }
    }

    private Model_RoutingOtherReason MapFromReader(IDataReader reader)
    {
        return new Model_RoutingOtherReason
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ReasonCode = reader.GetString(reader.GetOrdinal("reason_code")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order"))
        };
    }
}
