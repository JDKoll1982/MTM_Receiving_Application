using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    /// <summary>
    /// Service for persisting and restoring session state to/from JSON file.
    /// Session file location: %APPDATA%\MTM_Receiving_Application\session.json
    /// </summary>
    public interface IService_SessionManager
    {
        /// <summary>
        /// Saves the current session to JSON file.
        /// Automatically creates directory if it doesn't exist.
        /// </summary>
        /// <param name="session">Session to persist</param>
        /// <exception cref="ArgumentNullException">If session is null</exception>
        /// <exception cref="InvalidOperationException">If file write fails</exception>
        public Task SaveSessionAsync(Model_ReceivingSession session);

        /// <summary>
        /// Loads the persisted session from JSON file.
        /// Returns null if no session file exists or if file is corrupted.
        /// Corrupted files are automatically deleted.
        /// </summary>
        /// <returns>Restored session or null</returns>
        public Task<Model_ReceivingSession?> LoadSessionAsync();

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

