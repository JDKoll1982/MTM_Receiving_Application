using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for volvo_line_data table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_VolvoShipmentLine
{
    private readonly string _connectionString;

    public Dao_VolvoShipmentLine(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a new shipment line
    /// </summary>
    /// <param name="line"></param>
    public async Task<Model_Dao_Result> InsertAsync(Model_VolvoShipmentLine line)
    {
        var parameters = new Dictionary<string, object>
        {
            { "shipment_id", line.ShipmentId },
            { "part_number", line.PartNumber },
            {
                "location",
                string.IsNullOrWhiteSpace(line.Location)
                    ? (object)DBNull.Value
                    : line.Location.Trim()
            },
            { "quantity_per_skid", line.QuantityPerSkid },
            { "received_skid_count", line.ReceivedSkidCount },
            { "calculated_piece_count", line.CalculatedPieceCount },
            { "po_status", line.PoStatus },
            { "has_discrepancy", line.HasDiscrepancy ? 1 : 0 },
            { "expected_skid_count", line.ExpectedSkidCount ?? (object)DBNull.Value },
            { "discrepancy_note", line.DiscrepancyNote ?? (object)DBNull.Value },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_ShipmentLine_Insert",
            parameters
        );
    }

    /// <summary>
    /// Gets all lines for a shipment
    /// </summary>
    /// <param name="shipmentId"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(
        int shipmentId
    )
    {
        var parameters = new Dictionary<string, object> { { "shipment_id", shipmentId } };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Volvo_ShipmentLine_GetByShipment",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Updates a shipment line
    /// </summary>
    /// <param name="line"></param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipmentLine line)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", line.Id },
            { "part_number", line.PartNumber },
            {
                "location",
                string.IsNullOrWhiteSpace(line.Location)
                    ? (object)DBNull.Value
                    : line.Location.Trim()
            },
            { "quantity_per_skid", line.QuantityPerSkid },
            { "received_skid_count", line.ReceivedSkidCount },
            { "calculated_piece_count", line.CalculatedPieceCount },
            { "po_status", line.PoStatus },
            { "has_discrepancy", line.HasDiscrepancy ? 1 : 0 },
            { "expected_skid_count", line.ExpectedSkidCount ?? (object)DBNull.Value },
            { "discrepancy_note", line.DiscrepancyNote ?? (object)DBNull.Value },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_ShipmentLine_Update",
            parameters
        );
    }

    /// <summary>
    /// Deletes a shipment line
    /// </summary>
    /// <param name="lineId"></param>
    public async Task<Model_Dao_Result> DeleteAsync(int lineId)
    {
        var parameters = new Dictionary<string, object> { { "id", lineId } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_ShipmentLine_Delete",
            parameters
        );
    }

    /// <summary>
    /// Inserts multiple shipment lines within a single transaction.
    /// </summary>
    /// <param name="shipmentId"></param>
    /// <param name="lines"></param>
    public async Task<Model_Dao_Result> InsertBatchAsync(
        int shipmentId,
        IEnumerable<Model_VolvoShipmentLine> lines
    )
    {
        var lineList = lines?.ToList() ?? new List<Model_VolvoShipmentLine>();
        if (lineList.Count == 0)
        {
            return Model_Dao_Result_Factory.Success();
        }

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var line in lineList)
            {
                line.ShipmentId = shipmentId;

                var parameters = new Dictionary<string, object>
                {
                    { "shipment_id", line.ShipmentId },
                    { "part_number", line.PartNumber },
                    { "po_status", line.PoStatus },
                    {
                        "location",
                        string.IsNullOrWhiteSpace(line.Location)
                            ? (object)DBNull.Value
                            : line.Location.Trim()
                    },
                    { "quantity_per_skid", line.QuantityPerSkid },
                    { "received_skid_count", line.ReceivedSkidCount },
                    { "calculated_piece_count", line.CalculatedPieceCount },
                    { "has_discrepancy", line.HasDiscrepancy ? 1 : 0 },
                    { "expected_skid_count", line.ExpectedSkidCount ?? (object)DBNull.Value },
                    { "discrepancy_note", line.DiscrepancyNote ?? (object)DBNull.Value },
                };

                var lineResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    (MySqlTransaction)transaction,
                    "sp_Volvo_ShipmentLine_Insert",
                    parameters
                );

                if (!lineResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Model_Dao_Result_Factory.Failure(
                        $"Failed to insert line for part {line.PartNumber}: {lineResult.ErrorMessage}",
                        lineResult.Exception
                    );
                }
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Failed to insert shipment lines: {ex.Message}",
                ex
            );
        }
    }

    private static Model_VolvoShipmentLine MapFromReader(IDataReader reader)
    {
        return new Model_VolvoShipmentLine
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ShipmentId = reader.GetInt32(reader.GetOrdinal("shipment_id")),
            PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
            Location = reader.IsDBNull(reader.GetOrdinal("location"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("location")),
            QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
            ReceivedSkidCount = reader.GetInt32(reader.GetOrdinal("received_skid_count")),
            CalculatedPieceCount = reader.GetInt32(reader.GetOrdinal("calculated_piece_count")),
            PoStatus = reader.IsDBNull(reader.GetOrdinal("po_status"))
                ? VolvoLinePoStatus.Pending
                : VolvoLinePoStatus.NormalizeStorageValue(
                    reader.GetString(reader.GetOrdinal("po_status"))
                ),
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
