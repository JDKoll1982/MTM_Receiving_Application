using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Helpers.Database;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Services.Database
{
    /// <summary>
    /// Service for managing package type preferences in MySQL database.
    /// </summary>
    public class Service_MySQL_PackagePreferences : IService_MySQL_PackagePreferences
    {
        private readonly string _connectionString;

        public Service_MySQL_PackagePreferences(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
                throw new ArgumentException("Part ID cannot be null or empty", nameof(partID));

            var parameters = new Dictionary<string, object>
            {
                { "@p_PartID", partID }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_GetPackageTypePreference",
                reader => new Model_PackageTypePreference
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
                return result.Data;
            }
            
            return null;
        }

        public async Task SavePreferenceAsync(Model_PackageTypePreference preference)
        {
            if (preference == null)
                throw new ArgumentNullException(nameof(preference));

            var parameters = new Dictionary<string, object>
            {
                { "@p_PartID", preference.PartID },
                { "@p_PackageTypeName", preference.PackageTypeName },
                { "@p_CustomTypeName", preference.CustomTypeName ?? (object)DBNull.Value }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_SavePackageTypePreference",
                parameters
            );

            if (!result.Success)
            {
                throw new Exception(result.ErrorMessage);
            }
        }

        public async Task<bool> DeletePreferenceAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
                throw new ArgumentException("Part ID cannot be null or empty", nameof(partID));

            var parameters = new Dictionary<string, object>
            {
                { "@p_PartID", partID }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_DeletePackageTypePreference",
                parameters
            );

            return result.Success;
        }
    }
}
