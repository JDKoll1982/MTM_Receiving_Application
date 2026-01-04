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
/// Data Access Object for routing label operations.
/// Handles all database interactions for the routing_labels table.
/// </summary>
public class Dao_Routing_Label
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor with connection string injection
    /// </summary>
    /// <param name="connectionString">MySQL connection string</param>
    public Dao_Routing_Label(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // Insert Operations
    // ====================================================================

    /// <summary>
    /// Inserts a new routing label into the database.
    /// </summary>
    /// <param name="label">Routing label to insert</param>
    /// <returns>Result containing new label ID or error</returns>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_Routing_Label label)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_label_insert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Input parameters
            command.Parameters.AddWithValue("@p_label_number", label.LabelNumber);
            command.Parameters.AddWithValue("@p_deliver_to", label.DeliverTo);
            command.Parameters.AddWithValue("@p_department", label.Department);
            command.Parameters.AddWithValue("@p_package_description", label.PackageDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_po_number", label.PoNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_work_order", label.WorkOrder ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_employee_number", label.EmployeeNumber);
            command.Parameters.AddWithValue("@p_created_date", label.CreatedDate);

            // Output parameters
            var newIdParam = new MySqlParameter("@p_new_label_id", MySqlDbType.Int32)
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
            return Model_Dao_Result_Factory.Success(newId, $"Routing label inserted successfully (ID: {newId})");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>($"Error inserting routing label: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // Select Operations
    // ====================================================================

    /// <summary>
    /// Gets all non-archived labels for today's date.
    /// </summary>
    /// <param name="todayDate">Today's date</param>
    /// <returns>Result containing list of current labels or error</returns>
    public async Task<Model_Dao_Result<List<Model_Routing_Label>>> GetTodayLabelsAsync(DateTime todayDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_today_date", todayDate.Date }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_routing_label_get_today",
            MapReaderToLabel,
            parameters
        );
    }

    /// <summary>
    /// Gets archived routing labels within a date range for history view.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Result containing list of archived labels or error</returns>
    public async Task<Model_Dao_Result<List<Model_Routing_Label>>> GetHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@p_start_date", startDate.Date },
            { "@p_end_date", endDate.Date }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_routing_label_get_history",
            MapReaderToLabel,
            parameters
        );
    }

    // ====================================================================
    // Update Operations
    // ====================================================================

    /// <summary>
    /// Updates an existing routing label.
    /// </summary>
    /// <param name="label">Updated label data</param>
    /// <returns>Result indicating success or error</returns>
    public async Task<Model_Dao_Result> UpdateAsync(Model_Routing_Label label)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_label_update", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Input parameters
            command.Parameters.AddWithValue("@p_id", label.Id);
            command.Parameters.AddWithValue("@p_label_number", label.LabelNumber);
            command.Parameters.AddWithValue("@p_deliver_to", label.DeliverTo);
            command.Parameters.AddWithValue("@p_department", label.Department);
            command.Parameters.AddWithValue("@p_package_description", label.PackageDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_po_number", label.PoNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@p_work_order", label.WorkOrder ?? (object)DBNull.Value);

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

            return Model_Dao_Result_Factory.Success("Routing label updated successfully");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error updating routing label: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // Delete Operations
    // ====================================================================

    /// <summary>
    /// Deletes a routing label by ID.
    /// </summary>
    /// <param name="labelId">ID of label to delete</param>
    /// <returns>Result indicating success or error</returns>
    public async Task<Model_Dao_Result> DeleteAsync(int labelId)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_label_delete", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_id", labelId);

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

            return Model_Dao_Result_Factory.Success("Routing label deleted successfully");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error deleting routing label: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // Archive Operations
    // ====================================================================

    /// <summary>
    /// Archives routing labels (marks as is_archived = 1) for transfer to history.
    /// </summary>
    /// <param name="labelIds">List of label IDs to archive</param>
    /// <returns>Result containing count of archived labels or error</returns>
    public async Task<Model_Dao_Result<int>> ArchiveAsync(List<int> labelIds)
    {
        try
        {
            if (labelIds == null || labelIds.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<int>("No label IDs provided for archiving");
            }

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_routing_label_archive", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Convert list to comma-separated string
            var idsString = string.Join(",", labelIds);
            command.Parameters.AddWithValue("@p_label_ids", idsString);

            var archivedCountParam = new MySqlParameter("@p_archived_count", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(archivedCountParam);

            var errorParam = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            var errorMessage = errorParam.Value?.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Model_Dao_Result_Factory.Failure<int>(errorMessage);
            }

            var archivedCount = archivedCountParam.Value != DBNull.Value 
                ? Convert.ToInt32(archivedCountParam.Value) 
                : 0;

            return Model_Dao_Result_Factory.Success(archivedCount, $"{archivedCount} labels archived successfully");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>($"Error archiving routing labels: {ex.Message}", ex);
        }
    }

    // ====================================================================
    // Mapping Methods
    // ====================================================================

    /// <summary>
    /// Maps a MySqlDataReader row to a Model_Routing_Label object.
    /// </summary>
    private Model_Routing_Label MapReaderToLabel(MySqlDataReader reader)
    {
        return new Model_Routing_Label
        {
            Id = reader.GetInt32("id"),
            LabelNumber = reader.GetInt32("label_number"),
            DeliverTo = reader.GetString("deliver_to"),
            Department = reader.GetString("department"),
            PackageDescription = reader.IsDBNull(reader.GetOrdinal("package_description")) 
                ? null 
                : reader.GetString("package_description"),
            PoNumber = reader.IsDBNull(reader.GetOrdinal("po_number")) 
                ? null 
                : reader.GetString("po_number"),
            WorkOrder = reader.IsDBNull(reader.GetOrdinal("work_order")) 
                ? null 
                : reader.GetString("work_order"),
            EmployeeNumber = reader.GetString("employee_number"),
            CreatedDate = reader.GetDateTime("created_date"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }
}
