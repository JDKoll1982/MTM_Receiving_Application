using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Helpers.Database;
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
                        { "PONumber", load.PoNumber ?? (object)DBNull.Value },
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

