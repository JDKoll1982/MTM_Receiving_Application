using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Receiving;
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Services.Database
{
    /// <summary>
    /// Service for managing package type preferences in MySQL database.
    /// </summary>
    public class Service_MySQL_PackagePreferences : IService_MySQL_PackagePreferences
    {
        private readonly Dao_PackageTypePreference _dao;

        public Service_MySQL_PackagePreferences(Dao_PackageTypePreference dao)
        {
            _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        }

        // Constructor for backward compatibility if needed
        public Service_MySQL_PackagePreferences(string connectionString)
        {
            _dao = new Dao_PackageTypePreference(connectionString);
        }

        public async Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
            {
                throw new ArgumentException("Part ID cannot be null or empty", nameof(partID));
            }

            var result = await _dao.GetPreferenceAsync(partID);

            if (result.IsSuccess)
            {
                return result.Data;
            }

            return null;
        }

        public async Task SavePreferenceAsync(Model_PackageTypePreference preference)
        {
            if (preference == null)
            {
                throw new ArgumentNullException(nameof(preference));
            }

            var result = await _dao.SavePreferenceAsync(preference);

            if (!result.IsSuccess)
            {
                throw new Exception(result.ErrorMessage);
            }
        }

        public async Task<bool> DeletePreferenceAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
            {
                throw new ArgumentException("Part ID cannot be null or empty", nameof(partID));
            }

            var result = await _dao.DeletePreferenceAsync(partID);

            return result.IsSuccess;
        }
    }
}
