using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Data;

public class Dao_ReceivingLoad
{
    private readonly string _connectionString;

    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private string? CleanPONumber(string? poNumber)
    {
        if (string.IsNullOrEmpty(poNumber))
        {
            return null;
        }

        return poNumber.Replace("PO-", "", StringComparison.OrdinalIgnoreCase).Trim();
    }

    private static bool IsDuplicateKeyError(Model_Dao_Result result)
    {
        if (result.Exception is MySqlException mySqlException && mySqlException.Number == 1062)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            return false;
        }

        var errorMessage = result.ErrorMessage;
        return errorMessage.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
            && (errorMessage.Contains("1062", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("PRIMARY", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Model_Dao_Result<int>> SaveLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<int>("Loads list cannot be null or empty");
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int savedCount = 0;

            foreach (var load in loads)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "LoadGuid",        load.LoadID.ToString() },
                    { "Quantity",        (int)load.WeightQuantity },
                    { "PartID",          load.PartID },
                    { "PONumber",        CleanPONumber(load.PoNumber) ?? (object)DBNull.Value },
                    { "EmployeeNumber",  load.EmployeeNumber },
                    { "Heat",            string.IsNullOrWhiteSpace(load.HeatLotNumber) ? (object)DBNull.Value : load.HeatLotNumber },
                    { "TransactionDate", load.ReceivedDate.Date },
                    { "InitialLocation", (object)DBNull.Value },
                    { "CoilsOnSkid",     (object)DBNull.Value },
                    { "LabelNumber",     load.PackagesPerLoad },
                    { "VendorName",      string.IsNullOrWhiteSpace(load.PoVendor) ? (object)DBNull.Value : load.PoVendor },
                    { "PartDescription", string.IsNullOrWhiteSpace(load.PartDescription) ? (object)DBNull.Value : load.PartDescription }
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_Load_Insert",
                    parameters
                );

                if (!result.Success)
                {
                    if (IsDuplicateKeyError(result))
                    {
                        continue;
                    }

                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                savedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(savedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>($"Failed to save loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> UpdateLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<int>("Loads list cannot be null or empty");
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int updatedCount = 0;

            foreach (var load in loads)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "LoadGuid",        load.LoadID.ToString() },
                    { "Quantity",        (int)load.WeightQuantity },
                    { "PartID",          load.PartID },
                    { "PONumber",        CleanPONumber(load.PoNumber) ?? (object)DBNull.Value },
                    { "EmployeeNumber",  load.EmployeeNumber },
                    { "Heat",            string.IsNullOrWhiteSpace(load.HeatLotNumber) ? (object)DBNull.Value : load.HeatLotNumber },
                    { "TransactionDate", load.ReceivedDate.Date },
                    { "InitialLocation", (object)DBNull.Value },
                    { "CoilsOnSkid",     (object)DBNull.Value },
                    { "LabelNumber",     load.PackagesPerLoad },
                    { "VendorName",      string.IsNullOrWhiteSpace(load.PoVendor) ? (object)DBNull.Value : load.PoVendor },
                    { "PartDescription", string.IsNullOrWhiteSpace(load.PartDescription) ? (object)DBNull.Value : load.PartDescription }
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_Load_Update",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                updatedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(updatedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>($"Failed to update loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> DeleteLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Success<int>(0);
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int deletedCount = 0;

            foreach (var load in loads)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "LoadGuid", load.LoadID.ToString() }
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_Load_Delete",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                deletedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(deletedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>($"Failed to delete loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetHistoryAsync(string partID, DateTime startDate, DateTime endDate)
    {
        try
        {
            var loads = new List<Model_ReceivingLoad>();
            var parameters = new Dictionary<string, object>
            {
                { "PartID",         partID },
                { "StartDate",      startDate.Date },
                { "EndDate",        endDate.Date },
                { "PONumber",       DBNull.Value },
                { "EmployeeNumber", DBNull.Value }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
                _connectionString,
                "sp_Receiving_History_Get",
                parameters
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return Model_Dao_Result_Factory.Success(loads);
            }
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>($"Error retrieving history: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var loads = new List<Model_ReceivingLoad>();
            var parameters = new Dictionary<string, object>
            {
                { "StartDate", startDate },
                { "EndDate", endDate }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
                _connectionString,
                "sp_Receiving_Load_GetAll",
                parameters
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return Model_Dao_Result_Factory.Success(loads);
            }
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>($"Error retrieving all loads: {ex.Message}", ex);
        }
    }

    private Model_ReceivingLoad MapRowToLoad(DataRow row)
    {
        var loadGuidStr = row["load_guid"] == DBNull.Value ? null : row["load_guid"]?.ToString();
        return new Model_ReceivingLoad
        {
            LoadID = Guid.TryParse(loadGuidStr, out var guid) ? guid : Guid.NewGuid(),
            PartID = row["part_id"]?.ToString() ?? string.Empty,
            WeightQuantity = row["quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(row["quantity"]),
            PoNumber = row["po_number"] == DBNull.Value ? null : row["po_number"]?.ToString(),
            EmployeeNumber = row["employee_number"] == DBNull.Value ? 0 : Convert.ToInt32(row["employee_number"]),
            HeatLotNumber = row["heat"] == DBNull.Value ? string.Empty : row["heat"]?.ToString() ?? string.Empty,
            ReceivedDate = row["transaction_date"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(row["transaction_date"]),
            PackagesPerLoad = row["label_number"] == DBNull.Value ? 1 : Convert.ToInt32(row["label_number"]),
            PartDescription = row["part_description"] == DBNull.Value ? string.Empty : row["part_description"]?.ToString() ?? string.Empty,
            PoVendor = row["vendor_name"] == DBNull.Value ? string.Empty : row["vendor_name"]?.ToString() ?? string.Empty
        };
    }
}

