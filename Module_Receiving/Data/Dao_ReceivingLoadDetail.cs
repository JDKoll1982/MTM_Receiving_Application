using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for receiving_load_details table
/// Provides CRUD operations for individual load management including bulk operations
/// </summary>
public class Dao_ReceivingLoadDetail
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of Dao_ReceivingLoadDetail
    /// </summary>
    /// <param name="connectionString">MySQL connection string</param>
    public Dao_ReceivingLoadDetail(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Upserts (Insert or Update) a load detail record
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="load">Load detail entity</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> UpsertLoadDetailAsync(Guid sessionId, LoadDetail load)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString()),
                new MySqlParameter("@p_LoadNumber", load.LoadNumber),
                new MySqlParameter("@p_Weight", load.Weight ?? 0),
                new MySqlParameter("@p_Quantity", load.Quantity ?? 0),
                new MySqlParameter("@p_HeatLot", load.HeatLot ?? string.Empty),
                new MySqlParameter("@p_PackageType", load.PackageType ?? string.Empty),
                new MySqlParameter("@p_PackagesPerLoad", load.PackagesPerLoad ?? 0),
                new MySqlParameter("@p_AutoFilledFields", load.AutoFilledFields ?? string.Empty),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Update_Load_Detail",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error upserting load detail: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Retrieves all load details for a given session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>Model_Dao_Result with list of load details or error</returns>
    public async Task<Model_Dao_Result<List<LoadDetail>>> GetLoadsBySessionAsync(Guid sessionId)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString())
            };

            var result = await Helper_Database_StoredProcedure.ExecuteQueryAsync<LoadDetail>(
                "sp_Get_Session_With_Loads",
                parameters,
                _connectionString,
                reader =>
                {
                    return new LoadDetail
                    {
                        LoadNumber = Convert.ToInt32(reader["LoadNumber"]),
                        Weight = reader["Weight"] is DBNull ? null : Convert.ToDecimal(reader["Weight"]),
                        Quantity = reader["Quantity"] is DBNull ? null : Convert.ToInt32(reader["Quantity"]),
                        HeatLot = reader["HeatLot"] as string,
                        PackageType = reader["PackageType"] as string,
                        PackagesPerLoad = reader["PackagesPerLoad"] is DBNull ? null : Convert.ToInt32(reader["PackagesPerLoad"]),
                        AutoFilledFields = reader["AutoFilledFields"] as string ?? string.Empty
                    };
                }
            );

            if (!result.Success)
            {
                return new Model_Dao_Result<List<LoadDetail>>
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    Severity = result.Severity
                };
            }

            return new Model_Dao_Result<List<LoadDetail>>
            {
                Success = true,
                Data = result.Data?.ToList() ?? new List<LoadDetail>()
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<List<LoadDetail>>
            {
                Success = false,
                ErrorMessage = $"Unexpected error retrieving load details: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Copies data from source load to target loads (empty cells only)
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="sourceLoad">Source load number to copy from</param>
    /// <param name="targetLoads">List of target load numbers</param>
    /// <param name="fields">Fields to copy</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> CopyToLoadsAsync(Guid sessionId, int sourceLoad, List<int> targetLoads, CopyFields fields)
    {
        try
        {
            // Convert target loads to comma-separated string
            var targetLoadsStr = string.Join(",", targetLoads);

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString()),
                new MySqlParameter("@p_SourceLoad", sourceLoad),
                new MySqlParameter("@p_TargetLoads", targetLoadsStr),
                new MySqlParameter("@p_CopyFields", fields.ToString()),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Copy_To_Loads",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error copying to loads: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Clears auto-filled data from specified loads
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="loads">List of load numbers to clear</param>
    /// <param name="fields">Fields to clear</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> ClearAutoFilledAsync(Guid sessionId, List<int> loads, CopyFields fields)
    {
        try
        {
            var loadsStr = string.Join(",", loads);

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString()),
                new MySqlParameter("@p_Loads", loadsStr),
                new MySqlParameter("@p_ClearFields", fields.ToString()),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Clear_AutoFilled_Data",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error clearing auto-filled data: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }

    /// <summary>
    /// Saves completed transaction to historical table and exports CSV
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="csvPath">Path where CSV file was saved</param>
    /// <param name="user">User who completed the transaction</param>
    /// <returns>Model_Dao_Result with success status</returns>
    public async Task<Model_Dao_Result> SaveCompletedTransactionAsync(Guid sessionId, string csvPath, string user)
    {
        try
        {
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_SessionId", sessionId.ToString()),
                new MySqlParameter("@p_CSVPath", csvPath),
                new MySqlParameter("@p_CompletedBy", user),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "sp_Save_Completed_Transaction",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error saving completed transaction: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }
}
