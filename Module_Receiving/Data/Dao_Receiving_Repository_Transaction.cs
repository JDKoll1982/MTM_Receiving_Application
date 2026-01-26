using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for receiving transactions
/// Manages CRUD operations for tbl_Receiving_Transaction
/// </summary>
public class Dao_Receiving_Repository_Transaction
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_Transaction(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Inserts a new receiving transaction
    /// </summary>
    /// <param name="transaction">Transaction entity to insert</param>
    /// <returns>Result with TransactionId (GUID string)</returns>
    public async Task<Model_Dao_Result<string>> InsertTransactionAsync(Model_Receiving_TableEntitys_ReceivingTransaction transaction)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(transaction);

            var transactionId = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, object>
            {
                { "p_TransactionId", transactionId },
                { "p_PONumber", (object?)transaction.PONumber ?? DBNull.Value },
                { "p_PartNumber", transaction.UserName }, // Using UserName as PartNumber placeholder
                { "p_TotalLoads", transaction.Loads?.Count ?? 0 },
                { "p_TotalWeight", DBNull.Value }, // Calculated in Line inserts
                { "p_TotalQuantity", DBNull.Value },
                { "p_WorkflowMode", transaction.WorkflowMode.ToString() },
                { "p_Status", "Draft" },
                { "p_IsNonPO", string.IsNullOrEmpty(transaction.PONumber) },
                { "p_RequiresQualityHold", false },
                { "p_QualityHoldAcknowledged", false },
                { "p_SessionId", DBNull.Value },
                { "p_CreatedBy", transaction.UserName }
            };

            _logger.LogInfo($"Inserting transaction: PO={transaction.PONumber}, Mode={transaction.WorkflowMode}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Transaction_Insert",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Transaction inserted successfully: {transactionId}");
                return new Model_Dao_Result<string>
                {
                    Success = true,
                    Data = transactionId,
                    AffectedRows = result.AffectedRows,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            _logger.LogError($"Failed to insert transaction: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<string>(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in InsertTransactionAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Error inserting transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates an existing receiving transaction
    /// </summary>
    /// <param name="transaction">Transaction entity with updated values</param>
    public async Task<Model_Dao_Result> UpdateTransactionAsync(Model_Receiving_TableEntitys_ReceivingTransaction transaction)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(transaction);

            var parameters = new Dictionary<string, object>
            {
                { "p_TransactionId", transaction.TransactionId.ToString() },
                { "p_TotalLoads", transaction.Loads?.Count ?? 0 },
                { "p_TotalWeight", DBNull.Value },
                { "p_TotalQuantity", DBNull.Value },
                { "p_ExportedToCSV", transaction.ExportedToCSV },
                { "p_CSVExportPathLocal", (object?)transaction.CSVExportPathLocal ?? DBNull.Value },
                { "p_CSVExportPathNetwork", (object?)transaction.CSVExportPathNetwork ?? DBNull.Value },
                { "p_ModifiedBy", transaction.UpdatedBy ?? transaction.UserName }
            };

            _logger.LogInfo($"Updating transaction: {transaction.TransactionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Transaction_Update",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Transaction updated successfully: {transaction.TransactionId}");
            }
            else
            {
                _logger.LogError($"Failed to update transaction: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpdateTransactionAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a transaction by its ID
    /// </summary>
    /// <param name="transactionId">Transaction GUID</param>
    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_ReceivingTransaction>> SelectByIdAsync(string transactionId)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(transactionId);

            var parameters = new Dictionary<string, object>
            {
                { "p_TransactionId", transactionId }
            };

            _logger.LogInfo($"Selecting transaction by ID: {transactionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Receiving_Transaction_SelectById",
                MapTransaction,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Transaction found: {transactionId}");
            }
            else
            {
                _logger.LogWarning($"Transaction not found: {transactionId}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByIdAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_ReceivingTransaction>(
                $"Error selecting transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all transactions for a given PO number
    /// </summary>
    /// <param name="poNumber">Purchase Order number</param>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingTransaction>>> SelectByPOAsync(string poNumber)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(poNumber);

            var parameters = new Dictionary<string, object>
            {
                { "p_PONumber", poNumber }
            };

            _logger.LogInfo($"Selecting transactions by PO: {poNumber}");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_Transaction_SelectByPO",
                MapTransaction,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Found {result.Data?.Count ?? 0} transactions for PO: {poNumber}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByPOAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_ReceivingTransaction>>(
                $"Error selecting transactions: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves transactions within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingTransaction>>> SelectByDateRangeAsync(
        DateTime startDate, DateTime endDate)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_StartDate", startDate },
                { "p_EndDate", endDate }
            };

            _logger.LogInfo($"Selecting transactions from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_Transaction_SelectByDateRange",
                MapTransaction,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Found {result.Data?.Count ?? 0} transactions in date range");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByDateRangeAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_ReceivingTransaction>>(
                $"Error selecting transactions: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Soft deletes a transaction and all associated lines
    /// </summary>
    /// <param name="transactionId">Transaction GUID</param>
    /// <param name="modifiedBy">User performing deletion</param>
    public async Task<Model_Dao_Result> DeleteAsync(string transactionId, string modifiedBy)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(transactionId);
            ArgumentException.ThrowIfNullOrEmpty(modifiedBy);

            var parameters = new Dictionary<string, object>
            {
                { "p_TransactionId", transactionId },
                { "p_ModifiedBy", modifiedBy }
            };

            _logger.LogInfo($"Deleting transaction: {transactionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Transaction_Delete",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Transaction deleted successfully: {transactionId}");
            }
            else
            {
                _logger.LogError($"Failed to delete transaction: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in DeleteAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deleting transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Completes a transaction and archives it to completed transactions table
    /// </summary>
    /// <param name="transactionId">Transaction GUID</param>
    /// <param name="completedBy">User completing the transaction</param>
    /// <param name="csvFilePath">Optional CSV export path</param>
    public async Task<Model_Dao_Result> CompleteAsync(string transactionId, string completedBy, string? csvFilePath = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(transactionId);
            ArgumentException.ThrowIfNullOrEmpty(completedBy);

            var parameters = new Dictionary<string, object>
            {
                { "p_TransactionId", transactionId },
                { "p_CompletedBy", completedBy },
                { "p_CSVFilePath", (object?)csvFilePath ?? DBNull.Value }
            };

            _logger.LogInfo($"Completing transaction: {transactionId}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_Transaction_Complete",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Transaction completed and archived: {transactionId}");
            }
            else
            {
                _logger.LogError($"Failed to complete transaction: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in CompleteAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error completing transaction: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps a database reader row to a ReceivingTransaction entity
    /// </summary>
    private static Model_Receiving_TableEntitys_ReceivingTransaction MapTransaction(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_ReceivingTransaction
        {
            TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
            PONumber = reader.IsDBNull(reader.GetOrdinal("PONumber")) ? null : reader.GetString(reader.GetOrdinal("PONumber")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            UserName = reader.GetString(reader.GetOrdinal("UserName")),
            WorkflowMode = Enum.Parse<Enum_Receiving_Mode_WorkflowMode>(reader.GetString(reader.GetOrdinal("WorkflowMode"))),
            TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
            ExportedToCSV = reader.GetBoolean(reader.GetOrdinal("ExportedToCSV")),
            CSVExportPathLocal = reader.IsDBNull(reader.GetOrdinal("CSVExportPathLocal")) ? null : reader.GetString(reader.GetOrdinal("CSVExportPathLocal")),
            CSVExportPathNetwork = reader.IsDBNull(reader.GetOrdinal("CSVExportPathNetwork")) ? null : reader.GetString(reader.GetOrdinal("CSVExportPathNetwork")),
            CSVExportedAt = reader.IsDBNull(reader.GetOrdinal("CSVExportedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CSVExportedAt")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
            Loads = new List<Model_Receiving_TableEntitys_ReceivingLoad>() // Populated separately if needed
        };
    }
}
