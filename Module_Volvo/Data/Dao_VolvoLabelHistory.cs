using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data access for the Volvo label history archive tables.
/// <c>Clear Label Data</c> calls <see cref="ClearToHistoryAsync"/> which atomically
/// moves all rows from <c>volvo_label_data</c> and
/// <c>volvo_line_data</c> into <c>volvo_label_history</c> and <c>volvo_line_history</c>.
/// </summary>
public class Dao_VolvoLabelHistory : IDao_VolvoLabelHistory
{
    private readonly string _connectionString;

    public Dao_VolvoLabelHistory(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Atomically moves all active Volvo shipments (and their lines) from the active tables
    /// to the history archive tables via <c>sp_Volvo_LabelData_ClearToHistory</c>.
    /// Returns a tuple of <c>(HeadersMoved, LinesMoved)</c> on success.
    /// </summary>
    /// <param name="archivedBy">Employee identifier to stamp on history records.</param>
    public async Task<Model_Dao_Result<(int HeadersMoved, int LinesMoved)>> ClearToHistoryAsync(
        string archivedBy
    )
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "sp_Volvo_LabelData_ClearToHistory",
                connection
            )
            {
                CommandType = CommandType.StoredProcedure,
            };

            command.Parameters.AddWithValue("p_archived_by", archivedBy ?? "SYSTEM");

            var headersMovedParam = new MySqlParameter("p_headers_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(headersMovedParam);

            var linesMovedParam = new MySqlParameter("p_lines_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(linesMovedParam);

            var batchIdParam = new MySqlParameter("p_archive_batch_id", MySqlDbType.VarChar, 36)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(batchIdParam);

            var statusParam = new MySqlParameter("p_status", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(statusParam);

            var errorParam = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            var status = statusParam.Value == DBNull.Value ? 1 : Convert.ToInt32(statusParam.Value);
            var errorMessage =
                errorParam.Value == DBNull.Value ? null : errorParam.Value?.ToString();

            if (status != 0)
            {
                return Model_Dao_Result_Factory.Failure<(int, int)>(
                    errorMessage ?? "Clear Label Data failed"
                );
            }

            var headersMoved =
                headersMovedParam.Value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(headersMovedParam.Value);
            var linesMoved =
                linesMovedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(linesMovedParam.Value);

            return Model_Dao_Result_Factory.Success<(int, int)>((headersMoved, linesMoved));
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<(int, int)>(
                $"Failed to clear Volvo label data to history: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetArchivedShipmentByIdAsync(
        int shipmentHistoryId
    )
    {
        try
        {
            var parameters = new Dictionary<string, object> { { "id", shipmentHistoryId } };
            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Volvo_ShipmentHistory_GetById",
                MapShipmentFromReader,
                parameters
            );

            if (result.Success)
            {
                return new Model_Dao_Result<Model_VolvoShipment?>
                {
                    Success = true,
                    Data = result.Data,
                    AffectedRows = result.AffectedRows,
                };
            }

            return new Model_Dao_Result<Model_VolvoShipment?>
            {
                Success = false,
                Data = null,
                ErrorMessage = result.ErrorMessage ?? "Archived shipment not found",
                Severity = result.Severity,
                Exception = result.Exception,
            };
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_VolvoShipment?>(
                $"Failed to retrieve archived shipment: {ex.Message}",
                ex
            );
        }
    }

    public async Task<
        Model_Dao_Result<List<Model_VolvoShipmentLine>>
    > GetArchivedLinesByShipmentHistoryIdAsync(int shipmentHistoryId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "shipment_history_id", shipmentHistoryId },
            };

            return await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId",
                MapLineFromReader,
                parameters
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoShipmentLine>>(
                $"Failed to retrieve archived shipment lines: {ex.Message}",
                ex
            );
        }
    }

    private static Model_VolvoShipment MapShipmentFromReader(IDataReader reader)
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
            IsArchived = true,
        };
    }

    private static Model_VolvoShipmentLine MapLineFromReader(IDataReader reader)
    {
        return new Model_VolvoShipmentLine
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ShipmentId = reader.GetInt32(reader.GetOrdinal("shipment_id")),
            PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
            PoStatus = reader.IsDBNull(reader.GetOrdinal("po_status"))
                ? VolvoLinePoStatus.Received
                : VolvoLinePoStatus.NormalizeStorageValue(
                    reader.GetString(reader.GetOrdinal("po_status"))
                ),
            Location = reader.IsDBNull(reader.GetOrdinal("location"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("location")),
            QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
            ReceivedSkidCount = reader.GetInt32(reader.GetOrdinal("received_skid_count")),
            CalculatedPieceCount = reader.GetInt32(reader.GetOrdinal("calculated_piece_count")),
            HasDiscrepancy = reader.GetBoolean(reader.GetOrdinal("has_discrepancy")),
            ExpectedSkidCount = reader.IsDBNull(reader.GetOrdinal("expected_skid_count"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("expected_skid_count")),
            DiscrepancyNote = reader.IsDBNull(reader.GetOrdinal("discrepancy_note"))
                ? null
                : reader.GetString(reader.GetOrdinal("discrepancy_note")),
        };
    }
}
