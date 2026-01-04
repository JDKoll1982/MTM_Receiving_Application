using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for routing_labels table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_RoutingLabel
{
    private readonly string _connectionString;

    public Dao_RoutingLabel(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a new routing label record
    /// </summary>
    public async Task<Model_Dao_Result<int>> InsertLabelAsync(Model_RoutingLabel label)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_po_number", label.PONumber),
                new MySqlParameter("@p_line_number", label.LineNumber),
                new MySqlParameter("@p_description", label.Description),
                new MySqlParameter("@p_recipient_id", label.RecipientId),
                new MySqlParameter("@p_quantity", label.Quantity),
                new MySqlParameter("@p_created_by", label.CreatedBy),
                new MySqlParameter("@p_other_reason_id", (object?)label.OtherReasonId ?? DBNull.Value),
                new MySqlParameter("@p_new_label_id", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = ParameterDirection.Output }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_label_insert",
                parameters,
                _connectionString
            );

            if (result.Success)
            {
                var newIdParam = Array.Find(parameters, p => p.ParameterName == "@p_new_label_id");
                int newId = newIdParam?.Value != DBNull.Value ? Convert.ToInt32(newIdParam?.Value) : 0;

                return new Model_Dao_Result<int>
                {
                    Success = true,
                    Data = newId,
                    AffectedRows = 1,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            return new Model_Dao_Result<int>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                Severity = result.Severity
            };
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>($"Error inserting routing label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates an existing routing label
    /// </summary>
    public async Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_label_id", label.Id),
                new MySqlParameter("@p_po_number", label.PONumber),
                new MySqlParameter("@p_line_number", label.LineNumber),
                new MySqlParameter("@p_description", label.Description),
                new MySqlParameter("@p_recipient_id", label.RecipientId),
                new MySqlParameter("@p_quantity", label.Quantity),
                new MySqlParameter("@p_other_reason_id", (object?)label.OtherReasonId ?? DBNull.Value),
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = ParameterDirection.Output }
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_label_update",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error updating routing label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a single label by ID
    /// </summary>
    public async Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_label_id", labelId }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_RoutingLabel>(
                _connectionString,
                "sp_routing_label_get_by_id",
                MapFromReader,
                parameters
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_RoutingLabel>($"Error retrieving routing label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all labels with pagination
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_limit", limit },
                { "p_offset", offset }
            };

            return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_RoutingLabel>(
                _connectionString,
                "sp_routing_label_get_all",
                MapFromReader,
                parameters
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_RoutingLabel>>($"Error retrieving routing labels: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Soft deletes a label
    /// </summary>
    public async Task<Model_Dao_Result> DeleteLabelAsync(int labelId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_label_id", labelId),
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = ParameterDirection.Output }
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_label_delete",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error deleting routing label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Marks label as CSV exported
    /// </summary>
    public async Task<Model_Dao_Result> MarkLabelExportedAsync(int labelId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_label_id", labelId),
                new MySqlParameter("@p_status", MySqlDbType.Int32) { Direction = ParameterDirection.Output },
                new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500) { Direction = ParameterDirection.Output }
            };

            return await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_routing_label_mark_exported",
                parameters,
                _connectionString
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure($"Error marking label as exported: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks for duplicate labels within time window
    /// </summary>
    public async Task<Model_Dao_Result<Model_RoutingLabel>> CheckDuplicateLabelAsync(
        string poNumber, string lineNumber, int recipientId, int hoursWindow = 24)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_po_number", poNumber },
                { "p_line_number", lineNumber },
                { "p_recipient_id", recipientId },
                { "p_hours_window", hoursWindow }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_RoutingLabel>(
                _connectionString,
                "sp_routing_label_check_duplicate",
                MapFromReader,
                parameters
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_RoutingLabel>($"Error checking duplicate label: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps IDataReader to Model_RoutingLabel
    /// </summary>
    private Model_RoutingLabel MapFromReader(IDataReader reader)
    {
        return new Model_RoutingLabel
        {
            Id = reader.GetInt32("id"),
            PONumber = reader.GetString("po_number"),
            LineNumber = reader.GetString("line_number"),
            Description = reader.GetString("description"),
            RecipientId = reader.GetInt32("recipient_id"),
            RecipientName = reader.IsDBNull(reader.GetOrdinal("recipient_name")) ? string.Empty : reader.GetString("recipient_name"),
            RecipientLocation = reader.IsDBNull(reader.GetOrdinal("recipient_location")) ? string.Empty : reader.GetString("recipient_location"),
            Quantity = reader.GetInt32("quantity"),
            CreatedBy = reader.GetInt32("created_by"),
            CreatedDate = reader.GetDateTime("created_date"),
            OtherReasonId = reader.IsDBNull(reader.GetOrdinal("other_reason_id")) ? null : reader.GetInt32("other_reason_id"),
            OtherReasonDescription = reader.IsDBNull(reader.GetOrdinal("other_reason_description")) ? null : reader.GetString("other_reason_description"),
            IsActive = true,  // Always true from query (WHERE is_active = 1)
            CsvExported = reader.GetBoolean("csv_exported"),
            CsvExportDate = reader.IsDBNull(reader.GetOrdinal("csv_export_date")) ? null : reader.GetDateTime("csv_export_date")
        };
    }
}
