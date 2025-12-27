using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Data.Receiving;

public class Dao_ReceivingLoad
{
    private readonly string _connectionString;

    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private string? CleanPONumber(string? poNumber)
    {
        if (string.IsNullOrEmpty(poNumber)) return null;
        return poNumber.Replace("PO-", "", StringComparison.OrdinalIgnoreCase).Trim();
    }

    public async Task<Model_Dao_Result<int>> SaveLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
            return DaoResultFactory.Failure<int>("Loads list cannot be null or empty");

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int savedCount = 0;

            foreach (var load in loads)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "LoadID", load.LoadID.ToString() },
                    { "PartID", load.PartID },
                    { "PartType", load.PartType },
                    { "PONumber", CleanPONumber(load.PoNumber) ?? (object)DBNull.Value },
                    { "POLineNumber", load.PoLineNumber },
                    { "LoadNumber", load.LoadNumber },
                    { "WeightQuantity", load.WeightQuantity },
                    { "HeatLotNumber", load.HeatLotNumber },
                    { "PackagesPerLoad", load.PackagesPerLoad },
                    { "PackageTypeName", load.PackageTypeName },
                    { "WeightPerPackage", load.WeightPerPackage },
                    { "IsNonPOItem", load.IsNonPOItem },
                    { "ReceivedDate", load.ReceivedDate }
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_InsertReceivingLoad",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                savedCount++;
            }

            await transaction.CommitAsync();
            return DaoResultFactory.Success<int>(savedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return DaoResultFactory.Failure<int>($"Failed to save loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> UpdateLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
            return DaoResultFactory.Failure<int>("Loads list cannot be null or empty");

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int updatedCount = 0;

            foreach (var load in loads)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "LoadID", load.LoadID.ToString() },
                    { "PartID", load.PartID },
                    { "PartType", load.PartType },
                    { "PONumber", CleanPONumber(load.PoNumber) ?? (object)DBNull.Value },
                    { "POLineNumber", load.PoLineNumber },
                    { "LoadNumber", load.LoadNumber },
                    { "WeightQuantity", load.WeightQuantity },
                    { "HeatLotNumber", load.HeatLotNumber },
                    { "PackagesPerLoad", load.PackagesPerLoad },
                    { "PackageTypeName", load.PackageTypeName },
                    { "WeightPerPackage", load.WeightPerPackage },
                    { "IsNonPOItem", load.IsNonPOItem },
                    { "ReceivedDate", load.ReceivedDate }
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_UpdateReceivingLoad",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                updatedCount++;
            }

            await transaction.CommitAsync();
            return DaoResultFactory.Success<int>(updatedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return DaoResultFactory.Failure<int>($"Failed to update loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> DeleteLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
            return DaoResultFactory.Success<int>(0);

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int deletedCount = 0;

            foreach (var load in loads)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "p_LoadID", load.LoadID.ToString() }
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_DeleteReceivingLoad",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                deletedCount++;
            }

            await transaction.CommitAsync();
            return DaoResultFactory.Success<int>(deletedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return DaoResultFactory.Failure<int>($"Failed to delete loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetHistoryAsync(string partID, DateTime startDate, DateTime endDate)
    {
        try
        {
            var loads = new List<Model_ReceivingLoad>();
            var parameters = new Dictionary<string, object>
            {
                { "PartID", partID },
                { "StartDate", startDate },
                { "EndDate", endDate }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
                _connectionString,
                "sp_GetReceivingHistory",
                parameters
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return DaoResultFactory.Success(loads);
            }
            return DaoResultFactory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<List<Model_ReceivingLoad>>($"Error retrieving history: {ex.Message}", ex);
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
                "sp_GetAllReceivingLoads",
                parameters
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return DaoResultFactory.Success(loads);
            }
            return DaoResultFactory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return DaoResultFactory.Failure<List<Model_ReceivingLoad>>($"Error retrieving all loads: {ex.Message}", ex);
        }
    }

    private Model_ReceivingLoad MapRowToLoad(DataRow row)
    {
        return new Model_ReceivingLoad
        {
            LoadID = Guid.Parse(row["LoadID"]?.ToString() ?? Guid.Empty.ToString()),
            PartID = row["PartID"]?.ToString() ?? string.Empty,
            PartType = row["PartType"]?.ToString() ?? string.Empty,
            PoNumber = row["PONumber"] == DBNull.Value ? null : row["PONumber"].ToString(),
            PoLineNumber = row["POLineNumber"]?.ToString() ?? string.Empty,
            LoadNumber = Convert.ToInt32(row["LoadNumber"]),
            WeightQuantity = Convert.ToDecimal(row["WeightQuantity"]),
            HeatLotNumber = row["HeatLotNumber"]?.ToString() ?? string.Empty,
            PackagesPerLoad = Convert.ToInt32(row["PackagesPerLoad"]),
            PackageTypeName = row["PackageTypeName"]?.ToString() ?? string.Empty,
            WeightPerPackage = Convert.ToDecimal(row["WeightPerPackage"]),
            IsNonPOItem = Convert.ToBoolean(row["IsNonPOItem"]),
            ReceivedDate = Convert.ToDateTime(row["ReceivedDate"])
        };
    }
}
