using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    /// <summary>
    /// Service for persisting and restoring session state to/from JSON file.
    /// Session file location: %APPDATA%\MTM_Receiving_Application\session.json
    /// </summary>
    public interface IService_SessionManager
    {
        /// <summary>
        /// Deletes the persisted session file.
        /// Called after successful save to prevent stale data.
        /// </summary>
        /// <returns>True if deleted, false if file didn't exist</returns>
        public Task<bool> ClearSessionAsync();

        /// <summary>
        /// Checks if a persisted session file exists.
        /// </summary>
        /// <returns>True if session file exists, false otherwise</returns>
        public bool SessionExists();

        /// <summary>
        /// Gets the full path to the session JSON file.
        /// </summary>
        /// <returns>Absolute file path</returns>
        public string GetSessionFilePath();
    }
}

