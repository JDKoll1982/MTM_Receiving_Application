using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for volvo_shipments table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_VolvoShipment
{
    private readonly string _connectionString;

    public Dao_VolvoShipment(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a new Volvo shipment and returns the generated ID and shipment number
    /// </summary>
    /// <param name="shipment">Volvo shipment model to insert</param>
    /// <returns>Model_Dao_Result with shipment ID and number in Data property</returns>
    public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> InsertAsync(Model_VolvoShipment shipment)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_volvo_shipment_insert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Input parameters
            command.Parameters.AddWithValue("@p_shipment_date", shipment.ShipmentDate);
            command.Parameters.AddWithValue("@p_employee_number", shipment.EmployeeNumber);
            command.Parameters.AddWithValue("@p_notes", shipment.Notes ?? (object)DBNull.Value);

            // Output parameters
            var newIdParam = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            var shipmentNumberParam = new MySqlParameter("@p_shipment_number", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(newIdParam);
            command.Parameters.Add(shipmentNumberParam);

            await command.ExecuteNonQueryAsync();

            var shipmentId = Convert.ToInt32(newIdParam.Value);
            var shipmentNumber = Convert.ToInt32(shipmentNumberParam.Value);

            return new Model_Dao_Result<(int, int)>
            {
                Success = true,
                Data = (shipmentId, shipmentNumber),
                AffectedRows = 1
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<(int, int)>
            {
                Success = false,
                ErrorMessage = $"Error inserting Volvo shipment: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Updates a Volvo shipment
    /// </summary>
    /// <param name="shipment">Volvo shipment model to update</param>
    /// <returns>Model_Dao_Result indicating success/failure</returns>
    public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipment shipment)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand
            {
                Connection = connection,
                CommandText = @"
                    UPDATE volvo_shipments 
                    SET notes = @p_notes,
                        modified_date = CURRENT_TIMESTAMP
                    WHERE id = @p_id",
                CommandType = CommandType.Text
            };

            command.Parameters.AddWithValue("@p_id", shipment.Id);
            command.Parameters.AddWithValue("@p_notes", shipment.Notes ?? (object)DBNull.Value);

            var affectedRows = await command.ExecuteNonQueryAsync();

            return new Model_Dao_Result
            {
                Success = affectedRows > 0,
                AffectedRows = affectedRows,
                ErrorMessage = affectedRows == 0 ? "Shipment not found" : string.Empty
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Error updating Volvo shipment: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Completes a shipment with PO and Receiver numbers
    /// </summary>
    /// <param name="shipmentId">Shipment ID</param>
    /// <param name="poNumber">Purchase order number</param>
    /// <param name="receiverNumber">Receiver number</param>
    /// <returns>Model_Dao_Result indicating success/failure</returns>
    public async Task<Model_Dao_Result> CompleteAsync(int shipmentId, string poNumber, string receiverNumber)
    {
        var parameters = new Dictionary<string, object>
        {
            { "shipment_id", shipmentId },
            { "po_number", poNumber },
            { "receiver_number", receiverNumber }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_shipment_complete",
            parameters
        );
    }

    /// <summary>
    /// Gets the pending shipment if one exists
    /// </summary>
    /// <returns>Model_Dao_Result with pending shipment or null if none exists</returns>
    public async Task<Model_Dao_Result<Model_VolvoShipment>> GetPendingAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_volvo_shipment_get_pending",
            MapFromReader,
            null
        );
    }

    /// <summary>
    /// Gets a shipment by ID
    /// </summary>
    /// <param name="shipmentId">Shipment ID</param>
    /// <returns>Model_Dao_Result with shipment or null if not found</returns>
    public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetByIdAsync(int shipmentId)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand
            {
                Connection = connection,
                CommandText = @"
                    SELECT id, shipment_date, shipment_number, po_number, receiver_number,
                           employee_number, notes, status, created_date, modified_date, is_archived
                    FROM volvo_shipments
                    WHERE id = @p_id",
                CommandType = CommandType.Text
            };

            command.Parameters.AddWithValue("@p_id", shipmentId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Model_Dao_Result<Model_VolvoShipment?>
                {
                    Success = true,
                    Data = MapFromReader(reader),
                    AffectedRows = 1
                };
            }

            return new Model_Dao_Result<Model_VolvoShipment?>
            {
                Success = false,
                Data = null,
                ErrorMessage = "Shipment not found"
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<Model_VolvoShipment?>
            {
                Success = false,
                Data = null,
                ErrorMessage = $"Error retrieving shipment: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Gets shipment history with filtering
    /// </summary>
    /// <param name="startDate">Start date for filter</param>
    /// <param name="endDate">End date for filter</param>
    /// <param name="status">Status filter ('pending_po', 'completed', or 'all')</param>
    /// <returns>Model_Dao_Result with list of shipments</returns>
    public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
        DateTime startDate,
        DateTime endDate,
        string status = "all")
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_date", startDate.Date },
            { "end_date", endDate.Date },
            { "status", status }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_volvo_shipment_history_get",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Maps DataReader to Model_VolvoShipment
    /// </summary>
    private static Model_VolvoShipment MapFromReader(IDataReader reader)
    {
        return new Model_VolvoShipment
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ShipmentDate = reader.GetDateTime(reader.GetOrdinal("shipment_date")),
            ShipmentNumber = reader.GetInt32(reader.GetOrdinal("shipment_number")),
            PONumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
                ? null
                : reader.GetString(reader.GetOrdinal("po_number")),
            ReceiverNumber = reader.IsDBNull(reader.GetOrdinal("receiver_number"))
                ? null
                : reader.GetString(reader.GetOrdinal("receiver_number")),
            EmployeeNumber = reader.GetString(reader.GetOrdinal("employee_number")),
            Notes = reader.IsDBNull(reader.GetOrdinal("notes"))
                ? null
                : reader.GetString(reader.GetOrdinal("notes")),
            Status = reader.GetString(reader.GetOrdinal("status")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
            IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived"))
        };
    }
}
