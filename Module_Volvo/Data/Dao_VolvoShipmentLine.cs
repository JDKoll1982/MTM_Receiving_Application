using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for volvo_shipment_lines table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_VolvoShipmentLine
{
    private readonly string _connectionString;

    public Dao_VolvoShipmentLine(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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
            { "quantity_per_skid", line.QuantityPerSkid },
            { "received_skid_count", line.ReceivedSkidCount },
            { "calculated_piece_count", line.CalculatedPieceCount },
            { "has_discrepancy", line.HasDiscrepancy ? 1 : 0 },
            { "expected_skid_count", line.ExpectedSkidCount ?? (object)DBNull.Value },
            { "discrepancy_note", line.DiscrepancyNote ?? (object)DBNull.Value }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_shipment_line_insert",
            parameters
        );
    }

    /// <summary>
    /// Gets all lines for a shipment
    /// </summary>
    /// <param name="shipmentId"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(int shipmentId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "shipment_id", shipmentId }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_volvo_shipment_line_get_by_shipment",
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
            { "received_skid_count", line.ReceivedSkidCount },
            { "calculated_piece_count", line.CalculatedPieceCount },
            { "has_discrepancy", line.HasDiscrepancy ? 1 : 0 },
            { "expected_skid_count", line.ExpectedSkidCount ?? (object)DBNull.Value },
            { "discrepancy_note", line.DiscrepancyNote ?? (object)DBNull.Value }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_shipment_line_update",
            parameters
        );
    }

    /// <summary>
    /// Deletes a shipment line
    /// </summary>
    /// <param name="lineId"></param>
    public async Task<Model_Dao_Result> DeleteAsync(int lineId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", lineId }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_volvo_shipment_line_delete",
            parameters
        );
    }

    private static Model_VolvoShipmentLine MapFromReader(IDataReader reader)
    {
        return new Model_VolvoShipmentLine
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ShipmentId = reader.GetInt32(reader.GetOrdinal("shipment_id")),
            PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
            QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
            ReceivedSkidCount = reader.GetInt32(reader.GetOrdinal("received_skid_count")),
            CalculatedPieceCount = reader.GetInt32(reader.GetOrdinal("calculated_piece_count")),
            HasDiscrepancy = reader.GetBoolean(reader.GetOrdinal("has_discrepancy")),
            ExpectedSkidCount = reader.IsDBNull(reader.GetOrdinal("expected_skid_count"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("expected_skid_count")),
            DiscrepancyNote = reader.IsDBNull(reader.GetOrdinal("discrepancy_note"))
                ? null
                : reader.GetString(reader.GetOrdinal("discrepancy_note"))
        };
    }
}
