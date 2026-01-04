using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing recipient operations.
/// Handles all database interactions for the routing_recipients table.
/// </summary>
public class Dao_Routing_Recipient
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor with connection string injection
    /// </summary>
    /// <param name="connectionString">MySQL connection string</param>
    public Dao_Routing_Recipient(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // Select Operations
    // ====================================================================

    /// <summary>
    /// Gets all active routing recipients for dropdown population.
    /// </summary>
    /// <returns>Result containing list of active recipients or error</returns>
    public async Task<Model_Dao_Result<List<Model_Routing_Recipient>>> GetAllAsync()
    {
        var parameters = new Dictionary<string, object>();

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_Routing_Recipient>(
            _connectionString,
            "sp_routing_recipient_get_all",
            reader => MapReaderToRecipient((MySqlDataReader)reader),
            parameters
        );
    }

    /// <summary>
    /// Gets a routing recipient by exact name match for department lookup.
    /// </summary>
    /// <param name="name">Recipient name</param>
    /// <returns>Result containing recipient or error</returns>
    public async Task<Model_Dao_Result<Model_Routing_Recipient>> GetByNameAsync(string name)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_name", name }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_Routing_Recipient>(
            _connectionString,
            "sp_routing_recipient_get_by_name",
            reader => MapReaderToRecipient((MySqlDataReader)reader),
            parameters
        );
    }

    // ====================================================================
    // Insert Operations
    // ====================================================================

    /// <summary>
    /// Inserts a new routing recipient.
    /// </summary>
    /// <param name="recipient">Recipient to insert</param>
    /// <returns>Result containing new recipient ID or error</returns>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_Routing_Recipient recipient)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_recipient_insert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Input parameters
            command.Parameters.AddWithValue("@p_name", recipient.Name);
            command.Parameters.AddWithValue("@p_default_department", recipient.DefaultDepartment ?? (object)DBNull.Value);

            // Output parameters
            var newIdParam = new MySqlParameter("@p_new_recipient_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(newIdParam);

            var errorParam = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            // Check for errors
            var errorMessage = errorParam.Value?.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Model_Dao_Result_Factory.Failure<int>(errorMessage);
            }

            var newId = newIdParam.Value != DBNull.Value ? Convert.ToInt32(newIdParam.Value) : 0;
            return Model_Dao_Result_Factory.Success<int>(newId);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>($"Error inserting routing recipient: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // Update Operations
    // ====================================================================

    /// <summary>
    /// Updates an existing routing recipient.
    /// </summary>
    /// <param name="recipient">Updated recipient data</param>
    /// <returns>Result indicating success or error</returns>
    public async Task<Model_Dao_Result> UpdateAsync(Model_Routing_Recipient recipient)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_recipient_update", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Input parameters
            command.Parameters.AddWithValue("@p_id", recipient.Id);
            command.Parameters.AddWithValue("@p_name", recipient.Name);
            command.Parameters.AddWithValue("@p_default_department", recipient.DefaultDepartment ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_is_active", recipient.IsActive);

            // Output parameter
            var errorParam = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            // Check for errors
            var errorMessage = errorParam.Value?.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Model_Dao_Result_Factory.Failure(errorMessage);
            }

            return Model_Dao_Result_Factory.Success("Routing recipient updated successfully");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error updating routing recipient: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deletes a routing recipient by ID.
    /// </summary>
    /// <param name="recipientId">ID of recipient to delete</param>
    /// <returns>Result indicating success or error</returns>
    public async Task<Model_Dao_Result> DeleteAsync(int recipientId)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_recipient_delete", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_id", recipientId);

            var errorParam = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            var errorMessage = errorParam.Value?.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Model_Dao_Result_Factory.Failure(errorMessage);
            }

            return Model_Dao_Result_Factory.Success("Routing recipient deleted successfully");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error deleting routing recipient: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // Mapping Methods
    // ====================================================================

    /// <summary>
    /// Maps a MySqlDataReader row to a Model_Routing_Recipient object.
    /// </summary>
    private Model_Routing_Recipient MapReaderToRecipient(MySqlDataReader reader)
    {
        return new Model_Routing_Recipient
        {
            Id = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            DefaultDepartment = reader.IsDBNull(reader.GetOrdinal("default_department"))
                ? null
                : reader.GetString("default_department"),
            IsActive = reader.GetBoolean("is_active"),
            CreatedDate = reader.GetDateTime("created_date")
        };
    }
}
