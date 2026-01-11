using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;

namespace MTM_Receiving_Application.Module_Reporting.Data;

/// <summary>
/// Data Access Object for Reporting module
/// Queries database views for end-of-day reporting
/// Uses raw SQL for read-only reporting queries from views
/// </summary>
public class Dao_Reporting
{
    private readonly string _connectionString;

    public Dao_Reporting(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves receiving history from view_receiving_history view
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetReceivingHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var query = @"
                SELECT
                    id,
                    po_number,
                    part_number,
                    part_description,
                    quantity,
                    weight_lbs,
                    heat_lot_number,
                    created_date,
                    employee_number,
                    source_module
                FROM view_receiving_history
                WHERE created_date BETWEEN @StartDate AND @EndDate
                ORDER BY created_date DESC, id DESC";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date);

            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<Model_ReportRow>();

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetGuid("id").ToString(),
                    PONumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? null : reader.GetString("po_number"),
                    PartNumber = reader.IsDBNull(reader.GetOrdinal("part_number")) ? null : reader.GetString("part_number"),
                    PartDescription = reader.IsDBNull(reader.GetOrdinal("part_description")) ? null : reader.GetString("part_description"),
                    Quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? null : reader.GetDecimal("quantity"),
                    WeightLbs = reader.IsDBNull(reader.GetOrdinal("weight_lbs")) ? null : reader.GetDecimal("weight_lbs"),
                    HeatLotNumber = reader.IsDBNull(reader.GetOrdinal("heat_lot_number")) ? null : reader.GetString("heat_lot_number"),
                    CreatedDate = reader.GetDateTime("created_date"),
                    EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null : reader.GetString("employee_number"),
                    SourceModule = reader.GetString("source_module")
                });
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReportRow>>(
                $"Error retrieving receiving history: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Retrieves dunnage history from view_dunnage_history view
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetDunnageHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var query = @"
                SELECT
                    id,
                    dunnage_type,
                    part_number,
                    specs_combined,
                    quantity,
                    created_date,
                    employee_number,
                    source_module
                FROM view_dunnage_history
                WHERE created_date BETWEEN @StartDate AND @EndDate
                ORDER BY created_date DESC";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date);

            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<Model_ReportRow>();

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetGuid("id").ToString(),
                    DunnageType = reader.IsDBNull(reader.GetOrdinal("dunnage_type")) ? null : reader.GetString("dunnage_type"),
                    PartNumber = reader.IsDBNull(reader.GetOrdinal("part_number")) ? null : reader.GetString("part_number"),
                    SpecsCombined = reader.IsDBNull(reader.GetOrdinal("specs_combined")) ? null : reader.GetString("specs_combined"),
                    Quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? null : reader.GetDecimal("quantity"),
                    CreatedDate = reader.GetDateTime("created_date"),
                    EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null : reader.GetString("employee_number"),
                    SourceModule = reader.GetString("source_module")
                });
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReportRow>>(
                $"Error retrieving dunnage history: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Retrieves routing history from view_routing_history view
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetRoutingHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var query = @"
                SELECT
                    id,
                    po_number,
                    line_number,
                    package_description,
                    deliver_to,
                    department,
                    location,
                    quantity,
                    employee_number,
                    created_date,
                    other_reason,
                    source_module
                FROM view_routing_history
                WHERE created_date BETWEEN @StartDate AND @EndDate
                ORDER BY created_date DESC, id DESC";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date);

            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<Model_ReportRow>();

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetInt32("id").ToString(),
                    PONumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? null : reader.GetString("po_number"),
                    LineNumber = reader.IsDBNull(reader.GetOrdinal("line_number")) ? null : reader.GetString("line_number"),
                    PackageDescription = reader.IsDBNull(reader.GetOrdinal("package_description")) ? null : reader.GetString("package_description"),
                    DeliverTo = reader.IsDBNull(reader.GetOrdinal("deliver_to")) ? null : reader.GetString("deliver_to"),
                    Department = reader.IsDBNull(reader.GetOrdinal("department")) ? null : reader.GetString("department"),
                    Location = reader.IsDBNull(reader.GetOrdinal("location")) ? null : reader.GetString("location"),
                    Quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) ? null : reader.GetDecimal("quantity"),
                    EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null : reader.GetInt32("employee_number").ToString(),
                    CreatedDate = reader.GetDateTime("created_date"),
                    OtherReason = reader.IsDBNull(reader.GetOrdinal("other_reason")) ? null : reader.GetString("other_reason"),
                    SourceModule = reader.GetString("source_module")
                });
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReportRow>>(
                $"Error retrieving routing history: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Retrieves Volvo history from view_volvo_history view
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public async Task<Model_Dao_Result<List<Model_ReportRow>>> GetVolvoHistoryAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var query = @"
                SELECT
                    id,
                    shipment_number,
                    shipment_date,
                    po_number,
                    receiver_number,
                    status,
                    employee_number,
                    notes,
                    part_count,
                    created_date,
                    source_module
                FROM view_volvo_history
                WHERE created_date BETWEEN @StartDate AND @EndDate
                ORDER BY created_date DESC";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date);

            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<Model_ReportRow>();

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetInt32("id").ToString(),
                    ShipmentNumber = reader.IsDBNull(reader.GetOrdinal("shipment_number")) ? null : reader.GetInt32("shipment_number"),
                    PONumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? null : reader.GetString("po_number"),
                    ReceiverNumber = reader.IsDBNull(reader.GetOrdinal("receiver_number")) ? null : reader.GetString("receiver_number"),
                    Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status"),
                    EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null : reader.GetString("employee_number"),
                    PartCount = reader.IsDBNull(reader.GetOrdinal("part_count")) ? null : reader.GetInt32("part_count"),
                    CreatedDate = reader.GetDateTime("created_date"),
                    SourceModule = reader.GetString("source_module")
                });
            }

            return Model_Dao_Result_Factory.Success(rows);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReportRow>>(
                $"Error retrieving Volvo history: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Checks data availability for each module in date range
    /// Returns dictionary of module name → record count
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    public async Task<Model_Dao_Result<Dictionary<string, int>>> CheckAvailabilityAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var availability = new Dictionary<string, int>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check Receiving
            var receivingCount = await GetCountAsync(connection, "view_receiving_history", startDate, endDate);
            availability["Receiving"] = receivingCount;

            // Check Dunnage
            var dunnageCount = await GetCountAsync(connection, "view_dunnage_history", startDate, endDate);
            availability["Dunnage"] = dunnageCount;

            // Check Routing
            var routingCount = await GetCountAsync(connection, "view_routing_history", startDate, endDate);
            availability["Routing"] = routingCount;

            // Check Volvo (placeholder)
            availability["Volvo"] = 0;

            return Model_Dao_Result_Factory.Success(availability);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                $"Error checking availability: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Helper method to get count from a view
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="viewName"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    private async Task<int> GetCountAsync(
        MySqlConnection connection,
        string viewName,
        DateTime startDate,
        DateTime endDate)
    {
        var query = $@"
            SELECT COUNT(*)
            FROM {viewName}
            WHERE created_date BETWEEN @StartDate AND @EndDate";

        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate.Date);
        command.Parameters.AddWithValue("@EndDate", endDate.Date);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
