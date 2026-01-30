using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for receiving lines/loads
/// Manages CRUD operations for tbl_Receiving_Line
/// </summary>
public class Dao_Receiving_Repository_Line
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_Line(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Inserts a new receiving line/load
    /// </summary>
    /// <param name="line">Load entity to insert</param>
    /// <returns>Result with LineId (GUID string)</returns>
    public async Task<Model_Dao_Result<string>> InsertLineAsync(Model_Receiving_TableEntitys_ReceivingLoad line)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(line);

            var lineId = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, object>
            {
                { "p_LineId", lineId },
                { "p_TransactionId", line.TransactionId.ToString() },
                { "p_LineNumber", line.LoadNumber },
                { "p_PONumber", DBNull.Value }, // Derived from transaction
                { "p_PartNumber", line.PartId },
                { "p_LoadNumber", line.LoadNumber },
                { "p_Quantity", line.Quantity },
                { "p_Weight", line.Quantity }, // Using Quantity as Weight for now
                { "p_HeatLot", (object?)line.HeatLotNumber ?? DBNull.Value },
                { "p_PackageType", (object?)line.PackageType ?? DBNull.Value },
                { "p_PackagesPerLoad", line.PackagesPerLoad },
                { "p_WeightPerPackage", (object?)line.WeightPerPackage ?? DBNull.Value },
                { "p_ReceivingLocation", (object?)line.ReceivingLocation ?? DBNull.Value },
                { "p_PartType", (object?)line.PartType ?? DBNull.Value },
                { "p_IsNonPO", line.Transaction?.PONumber == null },
                { "p_IsAutoFilled", false },
                { "p_AutoFillSource", DBNull.Value },
                { "p_CreatedBy", "System" } // TODO: Get from context
            };

            _logger.LogInfo($"Inserting line: TransactionId={line.TransactionId}, LoadNumber={line.LoadNumber}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Line_Insert",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Line inserted successfully: {lineId}");
                return new Model_Dao_Result<string>
                {
                    Success = true,
                    Data = lineId,
                    AffectedRows = result.AffectedRows,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            _logger.LogError($"Failed to insert line: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<string>(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in InsertLineAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Error inserting line: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates an existing receiving line/load
    /// </summary>
    /// <param name="line">Load entity with updated values</param>
    public async Task<Model_Dao_Result> UpdateLineAsync(Model_Receiving_TableEntitys_ReceivingLoad line)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(line);

            var parameters = new Dictionary<string, object>
            {
                { "p_LineId", line.LoadId.ToString() },
                { "p_Quantity", line.Quantity },
                { "p_Weight", line.Quantity },
                { "p_HeatLot", (object?)line.HeatLotNumber ?? DBNull.Value },
                { "p_PackageType", (object?)line.PackageType ?? DBNull.Value },
                { "p_PackagesPerLoad", line.PackagesPerLoad },
                { "p_WeightPerPackage", (object?)line.WeightPerPackage ?? DBNull.Value },
                { "p_ReceivingLocation", (object?)line.ReceivingLocation ?? DBNull.Value },
                { "p_ModifiedBy", "System" } // TODO: Get from context
            };

            _logger.LogInfo($"Updating line: {line.LoadId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Line_Update",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Line updated successfully: {line.LoadId}");
            }
            else
            {
                _logger.LogError($"Failed to update line: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpdateLineAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating line: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Soft deletes a receiving line/load
    /// </summary>
    /// <param name="lineId">Line GUID</param>
    /// <param name="modifiedBy">User performing deletion</param>
    public async Task<Model_Dao_Result> DeleteAsync(string lineId, string modifiedBy)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(lineId);
            ArgumentException.ThrowIfNullOrEmpty(modifiedBy);

            var parameters = new Dictionary<string, object>
            {
                { "p_LineId", lineId },
                { "p_ModifiedBy", modifiedBy }
            };

            _logger.LogInfo($"Deleting line: {lineId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Line_Delete",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Line deleted successfully: {lineId}");
            }
            else
            {
                _logger.LogError($"Failed to delete line: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in DeleteAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deleting line: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a line by its ID
    /// </summary>
    /// <param name="lineId">Line GUID</param>
    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_ReceivingLoad>> SelectByIdAsync(string lineId)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(lineId);

            var parameters = new Dictionary<string, object>
            {
                { "p_LineId", lineId }
            };

            _logger.LogInfo($"Selecting line by ID: {lineId}");

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Receiving_Line_SelectById",
                MapLoad,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Line found: {lineId}");
            }
            else
            {
                _logger.LogWarning($"Line not found: {lineId}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByIdAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_ReceivingLoad>(
                $"Error selecting line: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all lines for a given transaction
    /// </summary>
    /// <param name="transactionId">Transaction GUID</param>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>> SelectByTransactionAsync(string transactionId)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(transactionId);

            var parameters = new Dictionary<string, object>
            {
                { "p_TransactionId", transactionId }
            };

            _logger.LogInfo($"Selecting lines by transaction: {transactionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_Line_SelectByTransaction",
                MapLoad,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Found {result.Data?.Count ?? 0} lines for transaction: {transactionId}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByTransactionAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_ReceivingLoad>>(
                $"Error selecting lines: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all lines for a given PO number
    /// </summary>
    /// <param name="poNumber">Purchase Order number</param>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>> SelectByPOAsync(string poNumber)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(poNumber);

            var parameters = new Dictionary<string, object>
            {
                { "p_PONumber", poNumber }
            };

            _logger.LogInfo($"Selecting lines by PO: {poNumber}");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_Line_SelectByPO",
                MapLoad,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Found {result.Data?.Count ?? 0} lines for PO: {poNumber}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByPOAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_ReceivingLoad>>(
                $"Error selecting lines: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps a database reader row to a ReceivingLoad entity
    /// </summary>
    private static Model_Receiving_TableEntitys_ReceivingLoad MapLoad(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_ReceivingLoad
        {
            LoadId = reader.GetInt32(reader.GetOrdinal("LoadId")),
            TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
            LoadNumber = reader.GetInt32(reader.GetOrdinal("LoadNumber")),
            PartId = reader.GetString(reader.GetOrdinal("PartId")),
            PartType = reader.IsDBNull(reader.GetOrdinal("PartType")) ? null : reader.GetString(reader.GetOrdinal("PartType")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
            UnitOfMeasure = reader.GetString(reader.GetOrdinal("UnitOfMeasure")),
            HeatLotNumber = reader.IsDBNull(reader.GetOrdinal("HeatLotNumber")) ? null : reader.GetString(reader.GetOrdinal("HeatLotNumber")),
            PackageType = reader.IsDBNull(reader.GetOrdinal("PackageType")) ? null : reader.GetString(reader.GetOrdinal("PackageType")),
            PackagesPerLoad = reader.GetInt32(reader.GetOrdinal("PackagesPerLoad")),
            WeightPerPackage = reader.IsDBNull(reader.GetOrdinal("WeightPerPackage")) ? null : reader.GetDecimal(reader.GetOrdinal("WeightPerPackage")),
            ReceivingLocation = reader.IsDBNull(reader.GetOrdinal("ReceivingLocation")) ? null : reader.GetString(reader.GetOrdinal("ReceivingLocation")),
            QualityHoldAcknowledged = reader.GetBoolean(reader.GetOrdinal("QualityHoldAcknowledged")),
            QualityHoldAcknowledgedAt = reader.IsDBNull(reader.GetOrdinal("QualityHoldAcknowledgedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("QualityHoldAcknowledgedAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
