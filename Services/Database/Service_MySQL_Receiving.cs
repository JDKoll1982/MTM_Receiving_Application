using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

namespace MTM_Receiving_Application.Services.Database
{
    /// <summary>
    /// Service for saving receiving load data to MySQL database.
    /// </summary>
    public class Service_MySQL_Receiving : IService_MySQL_Receiving
    {
        private readonly string _connectionString;
        private readonly ILoggingService _logger;

        public Service_MySQL_Receiving(string connectionString, ILoggingService logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string? CleanPONumber(string? poNumber)
        {
            if (string.IsNullOrEmpty(poNumber)) return null;
            // Remove "PO-" prefix if present to fit in VARCHAR(6) database column
            return poNumber.Replace("PO-", "", StringComparison.OrdinalIgnoreCase).Trim();
        }

        public async Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            _logger.LogInfo($"Saving {loads?.Count ?? 0} loads to database.");
            if (loads == null || loads.Count == 0)
                throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));

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
                _logger.LogInfo($"Successfully saved {savedCount} loads.");
                return savedCount;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Failed to save loads to database.", ex);
                throw;
            }
        }

        public async Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            _logger.LogInfo($"Updating {loads?.Count ?? 0} loads in database.");
            if (loads == null || loads.Count == 0)
                throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));

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
                _logger.LogInfo($"Successfully updated {updatedCount} loads.");
                return updatedCount;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Failed to update loads in database.", ex);
                throw;
            }
        }

        public async Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            _logger.LogInfo($"Deleting {loads?.Count ?? 0} loads from database.");
            if (loads == null || loads.Count == 0)
                return 0;

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
                _logger.LogInfo($"Successfully deleted {deletedCount} loads.");
                return deletedCount;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Failed to delete loads from database.", ex);
                throw;
            }
        }

        public async Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate)
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
                    loads.Add(new Model_ReceivingLoad
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
                    });
                }
            }

            return loads;
        }

        public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInfo($"Retrieving all receiving loads from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
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

                if (!result.IsSuccess)
                {
                    _logger.LogError($"Failed to retrieve receiving loads: {result.ErrorMessage}");
                    return Model_Dao_Result<List<Model_ReceivingLoad>>.Failure(result.ErrorMessage);
                }

                if (result.Data != null)
                {
                    foreach (DataRow row in result.Data.Rows)
                    {
                        loads.Add(new Model_ReceivingLoad
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
                        });
                    }
                }

                _logger.LogInfo($"Retrieved {loads.Count} receiving loads from database");
                return Model_Dao_Result<List<Model_ReceivingLoad>>.SuccessResult(loads);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception retrieving all receiving loads: {ex.Message}", ex);
                return Model_Dao_Result<List<Model_ReceivingLoad>>.Failure($"Database error: {ex.Message}");
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}

