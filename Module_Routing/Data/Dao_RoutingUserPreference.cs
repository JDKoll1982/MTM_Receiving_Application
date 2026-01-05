using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing_user_preferences table
/// </summary>
public class Dao_RoutingUserPreference
{
    private readonly string _connectionString;

    public Dao_RoutingUserPreference(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves user preferences by employee number
    /// </summary>
    /// <param name="employeeNumber"></param>
    public async Task<Model_Dao_Result<Model_RoutingUserPreference>> GetUserPreferenceAsync(int employeeNumber)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_employee_number", employeeNumber }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_RoutingUserPreference>(
                _connectionString,
                "sp_routing_user_preference_get",
                MapFromReader,
                parameters
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_RoutingUserPreference>($"Error retrieving user preference: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves or updates user preferences
    /// </summary>
    /// <param name="preference"></param>
    public async Task<Model_Dao_Result> SaveUserPreferenceAsync(Model_RoutingUserPreference preference)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_employee_number", preference.EmployeeNumber),
                new MySqlParameter("@p_default_mode", preference.DefaultMode),
                new MySqlParameter("@p_enable_validation", preference.EnableValidation),
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_user_preference_save",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error saving user preference: {ex.Message}", ex);
        }
    }

    private Model_RoutingUserPreference MapFromReader(IDataReader reader)
    {
        return new Model_RoutingUserPreference
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            EmployeeNumber = reader.GetInt32(reader.GetOrdinal("employee_number")),
            DefaultMode = reader.GetString(reader.GetOrdinal("default_mode")),
            EnableValidation = reader.GetBoolean(reader.GetOrdinal("enable_validation")),
            UpdatedDate = reader.GetDateTime(reader.GetOrdinal("updated_date"))
        };
    }
}
