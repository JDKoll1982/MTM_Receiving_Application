using System;
using System.Collections.Generic;
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
                    part_id AS part_number,
                    part_description,
                    quantity,
                    CAST(NULL AS DECIMAL(18,2)) AS weight_lbs,
                    heat AS heat_lot_number,
                    DATE(COALESCE(transaction_date, created_at)) AS created_date,
                    employee_number,
                    'Receiving' AS source_module
                FROM view_receiving_history
                WHERE DATE(COALESCE(transaction_date, created_at)) BETWEEN @StartDate AND @EndDate
                ORDER BY DATE(COALESCE(transaction_date, created_at)) DESC, id DESC";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.Date);
            command.Parameters.AddWithValue("@EndDate", endDate.Date);

            await using var reader = await command.ExecuteReaderAsync();
            var rows = new List<Model_ReportRow>();

            var idOrdinal = reader.GetOrdinal("id");
            var poNumberOrdinal = reader.GetOrdinal("po_number");
            var partNumberOrdinal = reader.GetOrdinal("part_number");
            var partDescriptionOrdinal = reader.GetOrdinal("part_description");
            var quantityOrdinal = reader.GetOrdinal("quantity");
            var weightOrdinal = reader.GetOrdinal("weight_lbs");
            var heatLotOrdinal = reader.GetOrdinal("heat_lot_number");
            var createdDateOrdinal = reader.GetOrdinal("created_date");
            var employeeNumberOrdinal = reader.GetOrdinal("employee_number");
            var sourceModuleOrdinal = reader.GetOrdinal("source_module");

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetGuid(idOrdinal).ToString(),
                    PONumber = reader.IsDBNull(poNumberOrdinal) ? null : reader.GetString(poNumberOrdinal),
                    PartNumber = reader.IsDBNull(partNumberOrdinal) ? null : reader.GetString(partNumberOrdinal),
                    PartDescription = reader.IsDBNull(partDescriptionOrdinal) ? null : reader.GetString(partDescriptionOrdinal),
                    Quantity = reader.IsDBNull(quantityOrdinal) ? null : reader.GetDecimal(quantityOrdinal),
                    WeightLbs = reader.IsDBNull(weightOrdinal) ? null : reader.GetDecimal(weightOrdinal),
                    HeatLotNumber = reader.IsDBNull(heatLotOrdinal) ? null : reader.GetString(heatLotOrdinal),
                    CreatedDate = reader.GetDateTime(createdDateOrdinal),
                    EmployeeNumber = reader.IsDBNull(employeeNumberOrdinal) ? null : reader.GetString(employeeNumberOrdinal),
                    SourceModule = reader.GetString(sourceModuleOrdinal)
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

            var idOrdinal = reader.GetOrdinal("id");
            var dunnageTypeOrdinal = reader.GetOrdinal("dunnage_type");
            var partNumberOrdinal = reader.GetOrdinal("part_number");
            var specsOrdinal = reader.GetOrdinal("specs_combined");
            var quantityOrdinal = reader.GetOrdinal("quantity");
            var createdDateOrdinal = reader.GetOrdinal("created_date");
            var employeeNumberOrdinal = reader.GetOrdinal("employee_number");
            var sourceModuleOrdinal = reader.GetOrdinal("source_module");

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetGuid(idOrdinal).ToString(),
                    DunnageType = reader.IsDBNull(dunnageTypeOrdinal) ? null : reader.GetString(dunnageTypeOrdinal),
                    PartNumber = reader.IsDBNull(partNumberOrdinal) ? null : reader.GetString(partNumberOrdinal),
                    SpecsCombined = reader.IsDBNull(specsOrdinal) ? null : reader.GetString(specsOrdinal),
                    Quantity = reader.IsDBNull(quantityOrdinal) ? null : reader.GetDecimal(quantityOrdinal),
                    CreatedDate = reader.GetDateTime(createdDateOrdinal),
                    EmployeeNumber = reader.IsDBNull(employeeNumberOrdinal) ? null : reader.GetString(employeeNumberOrdinal),
                    SourceModule = reader.GetString(sourceModuleOrdinal)
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

            var idOrdinal = reader.GetOrdinal("id");
            var shipmentNumberOrdinal = reader.GetOrdinal("shipment_number");
            var poNumberOrdinal = reader.GetOrdinal("po_number");
            var receiverNumberOrdinal = reader.GetOrdinal("receiver_number");
            var statusOrdinal = reader.GetOrdinal("status");
            var employeeNumberOrdinal = reader.GetOrdinal("employee_number");
            var partCountOrdinal = reader.GetOrdinal("part_count");
            var createdDateOrdinal = reader.GetOrdinal("created_date");
            var sourceModuleOrdinal = reader.GetOrdinal("source_module");

            while (await reader.ReadAsync())
            {
                rows.Add(new Model_ReportRow
                {
                    Id = reader.GetInt32(idOrdinal).ToString(),
                    ShipmentNumber = reader.IsDBNull(shipmentNumberOrdinal) ? null : reader.GetInt32(shipmentNumberOrdinal),
                    PONumber = reader.IsDBNull(poNumberOrdinal) ? null : reader.GetString(poNumberOrdinal),
                    ReceiverNumber = reader.IsDBNull(receiverNumberOrdinal) ? null : reader.GetString(receiverNumberOrdinal),
                    Status = reader.IsDBNull(statusOrdinal) ? null : reader.GetString(statusOrdinal),
                    EmployeeNumber = reader.IsDBNull(employeeNumberOrdinal) ? null : reader.GetString(employeeNumberOrdinal),
                    PartCount = reader.IsDBNull(partCountOrdinal) ? null : reader.GetInt32(partCountOrdinal),
                    CreatedDate = reader.GetDateTime(createdDateOrdinal),
                    SourceModule = reader.GetString(sourceModuleOrdinal)
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
            var receivingCount = await GetCountAsync(
                connection,
                "view_receiving_history",
                startDate,
                endDate,
                "DATE(COALESCE(transaction_date, created_at))");
            availability["Receiving"] = receivingCount;

            // Check Dunnage
            var dunnageCount = await GetCountAsync(connection, "view_dunnage_history", startDate, endDate);
            availability["Dunnage"] = dunnageCount;

            // Check Volvo
            var volvoCount = await GetCountAsync(connection, "view_volvo_history", startDate, endDate);
            availability["Volvo"] = volvoCount;

            return Model_Dao_Result_Factory.Success(availability);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Dictionary<string, int>>(
                $"Error checking availability: {ex.Message}",
                ex);
        }
    }

    private async Task<int> GetCountAsync(
        MySqlConnection connection,
        string viewName,
        DateTime startDate,
        DateTime endDate,
        string dateExpression = "created_date")
    {
        var query = $@"
            SELECT COUNT(*)
            FROM {viewName}
            WHERE {dateExpression} BETWEEN @StartDate AND @EndDate";

        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate.Date);
        command.Parameters.AddWithValue("@EndDate", endDate.Date);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
