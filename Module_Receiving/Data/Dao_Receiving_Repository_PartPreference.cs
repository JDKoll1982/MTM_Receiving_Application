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
/// Data Access Object for part preferences
/// Manages part-specific configuration and defaults
/// </summary>
public class Dao_Receiving_Repository_PartPreference
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    public Dao_Receiving_Repository_PartPreference(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves part preference by part number
    /// </summary>
    /// <param name="partNumber">Part number to lookup</param>
    /// <param name="scope">Scope: 'System' or 'User'</param>
    /// <param name="scopeUserId">User ID if scope is 'User'</param>
    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_PartPreference>> SelectByPartAsync(
        string partNumber, 
        string scope = "System", 
        string? scopeUserId = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(partNumber);

            var parameters = new Dictionary<string, object>
            {
                { "p_PartNumber", partNumber },
                { "p_Scope", scope },
                { "p_ScopeUserId", (object?)scopeUserId ?? DBNull.Value }
            };

            _logger.LogInfo($"Selecting part preference: Part={partNumber}, Scope={scope}");

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Receiving_PartPreference_SelectByPart",
                MapPartPreference,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Part preference found: {partNumber}");
            }
            else
            {
                _logger.LogInfo($"No part preference found for: {partNumber} (using defaults)");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in SelectByPartAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_PartPreference>(
                $"Error selecting part preference: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Inserts or updates a part preference (UPSERT operation)
    /// </summary>
    /// <param name="preference">Part preference to save</param>
    public async Task<Model_Dao_Result> UpsertAsync(Model_Receiving_TableEntitys_PartPreference preference)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(preference);

            var parameters = new Dictionary<string, object>
            {
                { "p_PartNumber", preference.PartNumber },
                { "p_PartTypeId", (object?)preference.PartTypeId ?? DBNull.Value },
                { "p_DefaultReceivingLocation", (object?)preference.DefaultReceivingLocation ?? DBNull.Value },
                { "p_DefaultPackageType", (object?)preference.DefaultPackageType ?? DBNull.Value },
                { "p_DefaultPackagesPerLoad", (object?)preference.DefaultPackagesPerLoad ?? DBNull.Value },
                { "p_RequiresQualityHold", preference.RequiresQualityHold },
                { "p_QualityHoldProcedure", (object?)preference.QualityHoldProcedure ?? DBNull.Value },
                { "p_Scope", preference.Scope },
                { "p_ScopeUserId", (object?)preference.ScopeUserId ?? DBNull.Value },
                { "p_ModifiedBy", preference.ScopeUserId ?? "System" }
            };

            _logger.LogInfo($"Upserting part preference: {preference.PartNumber}");

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_PartPreference_Upsert",
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Part preference upserted successfully: {preference.PartNumber}");
            }
            else
            {
                _logger.LogError($"Failed to upsert part preference: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpsertAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error upserting part preference: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Maps a database reader row to a PartPreference entity
    /// </summary>
    private static Model_Receiving_TableEntitys_PartPreference MapPartPreference(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_PartPreference
        {
            PartPreferenceId = reader.GetInt32(reader.GetOrdinal("PartPreferenceId")),
            PartNumber = reader.GetString(reader.GetOrdinal("PartNumber")),
            PartTypeId = reader.IsDBNull(reader.GetOrdinal("PartTypeId")) ? null : reader.GetInt32(reader.GetOrdinal("PartTypeId")),
            PartTypeName = reader.IsDBNull(reader.GetOrdinal("PartTypeName")) ? null : reader.GetString(reader.GetOrdinal("PartTypeName")),
            PartTypeCode = reader.IsDBNull(reader.GetOrdinal("PartTypeCode")) ? null : reader.GetString(reader.GetOrdinal("PartTypeCode")),
            DefaultReceivingLocation = reader.IsDBNull(reader.GetOrdinal("DefaultReceivingLocation")) ? null : reader.GetString(reader.GetOrdinal("DefaultReceivingLocation")),
            DefaultPackageType = reader.IsDBNull(reader.GetOrdinal("DefaultPackageType")) ? null : reader.GetString(reader.GetOrdinal("DefaultPackageType")),
            DefaultPackagesPerLoad = reader.IsDBNull(reader.GetOrdinal("DefaultPackagesPerLoad")) ? null : reader.GetInt32(reader.GetOrdinal("DefaultPackagesPerLoad")),
            RequiresQualityHold = reader.GetBoolean(reader.GetOrdinal("RequiresQualityHold")),
            QualityHoldProcedure = reader.IsDBNull(reader.GetOrdinal("QualityHoldProcedure")) ? null : reader.GetString(reader.GetOrdinal("QualityHoldProcedure")),
            Scope = reader.GetString(reader.GetOrdinal("Scope")),
            ScopeUserId = reader.IsDBNull(reader.GetOrdinal("ScopeUserId")) ? null : reader.GetString(reader.GetOrdinal("ScopeUserId"))
        };
    }
}
