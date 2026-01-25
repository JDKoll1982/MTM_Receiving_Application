using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Receiving.Data;

public class Dao_Receiving_Repository_PackageTypePreference
{
    private readonly string _connectionString;

    public Dao_Receiving_Repository_PackageTypePreference(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ====================================================================
    // User Preferences (New Feature)
    // ====================================================================

    public async Task<Model_Dao_Result<Model_Receiving_Entity_UserPreference>> GetByUserAsync(string username)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "username", username }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_Receiving_Entity_UserPreference>(
                _connectionString,
                "sp_package_preferences_get_by_user",
                reader => new Model_Receiving_Entity_UserPreference
                {
                    Username = reader["username"].ToString() ?? string.Empty,
                    PreferredPackageType = reader["preferred_package_type"].ToString() ?? string.Empty,
                    Workstation = reader["workstation"].ToString() ?? string.Empty,
                    LastUpdated = Convert.ToDateTime(reader["last_modified"])
                },
                parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_UserPreference>(
                $"Error retrieving user package preference: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result> UpsertAsync(Model_Receiving_Entity_UserPreference preference)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "username", preference.Username },
                { "preferred_package_type", preference.PreferredPackageType },
                { "workstation", preference.Workstation }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_package_preferences_upsert",
                parameters);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Error upserting user package preference: {ex.Message}",
                ex);
        }
    }

    // ====================================================================
    // Part Preferences (Existing Feature)
    // ====================================================================

    public async Task<Model_Dao_Result<Model_Receiving_Entity_PackageTypePreference?>> GetPreferenceAsync(string partID)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_PartID", partID }
            };

            // Using custom mapper as in original service
            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_Receiving_Entity_PackageTypePreference?>(
                _connectionString,
                "sp_Receiving_PackageTypePreference_Get",
                reader => new Model_Receiving_Entity_PackageTypePreference
                {
                    PreferenceID = Convert.ToInt32(reader["PreferenceID"]),
                    PartID = reader["PartID"].ToString() ?? string.Empty,
                    PackageTypeName = reader["PackageTypeName"].ToString() ?? string.Empty,
                    CustomTypeName = reader["CustomTypeName"] == DBNull.Value ? null : reader["CustomTypeName"].ToString(),
                    LastModified = Convert.ToDateTime(reader["LastModified"])
                },
                parameters
            );

            if (result.Success)
            {
                return Model_Dao_Result_Factory.Success(result.Data);
            }
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_PackageTypePreference?>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_PackageTypePreference?>(
                $"Error retrieving part package preference: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result> SavePreferenceAsync(Model_Receiving_Entity_PackageTypePreference preference)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_PartID", preference.PartID },
                { "@p_PackageTypeName", preference.PackageTypeName },
                { "@p_CustomTypeName", preference.CustomTypeName ?? (object)DBNull.Value },
                { "@p_LastModified", DateTime.Now }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_PackageTypePreference_Save",
                parameters
            );

            if (result.Success)
            {
                return Model_Dao_Result_Factory.Success();
            }
            return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Error saving part package preference: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result<bool>> DeletePreferenceAsync(string partID)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_PartID", partID }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Receiving_PackageTypePreference_Delete",
                parameters
            );

            if (result.Success)
            {
                return Model_Dao_Result_Factory.Success(true);
            }
            return Model_Dao_Result_Factory.Failure<bool>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Error deleting part package preference: {ex.Message}",
                ex);
        }
    }
}

