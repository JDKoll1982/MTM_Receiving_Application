using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    /// <summary>
    /// Service for managing package type preferences in MySQL database.
    /// </summary>
    public interface IService_MySQL_PackagePreferences
    {
        /// <summary>
        /// Retrieves the saved package type preference for a part ID.
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <returns>PackageTypePreference if found, null otherwise</returns>
        /// <exception cref="ArgumentException">If partID is null or empty</exception>
        public Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID);

        /// <summary>
        /// Saves or updates the package type preference for a part ID.
        /// Uses UPSERT logic (INSERT or UPDATE based on existence).
        /// </summary>
        /// <param name="preference">Preference to save</param>
        /// <exception cref="ArgumentNullException">If preference is null</exception>
        /// <exception cref="ArgumentException">If preference.PartID is null or empty</exception>
        /// <exception cref="InvalidOperationException">If database operation fails</exception>
        public Task SavePreferenceAsync(Model_PackageTypePreference preference);

        /// <summary>
        /// Deletes a package type preference for a part ID.
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <returns>True if deleted, false if not found</returns>
        public Task<bool> DeletePreferenceAsync(string partID);
    }
}

