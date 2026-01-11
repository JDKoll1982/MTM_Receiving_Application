using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing_recipient_tracker table
/// </summary>
public class Dao_RoutingUsageTracking
{
    private readonly string _connectionString;

    public Dao_RoutingUsageTracking(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Increments usage count for employee-recipient pair
    /// </summary>
    /// <param name="employeeNumber"></param>
    /// <param name="recipientId"></param>
    public async Task<Model_Dao_Result> IncrementUsageAsync(int employeeNumber, int recipientId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_employee_number", employeeNumber),
                new MySqlParameter("@p_recipient_id", recipientId),
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_usage_increment",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error incrementing usage: {ex.Message}", ex);
        }
    }
}
