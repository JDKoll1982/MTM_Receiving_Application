using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts
{
    /// <summary>
    /// Service for writing receiving data to XLS files.
    /// Uses the configured network path.
    /// </summary>
    public interface IService_XLSWriter
    {
        /// <summary>
        /// Writes receiving loads to the network XLS file.
        /// </summary>
        /// <param name="loads">List of loads to write</param>
        /// <returns>Result object indicating success/failure</returns>
        /// <exception cref="ArgumentException">If loads list is null or empty</exception>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        public Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Writes loads to a specific XLS file path.
        /// Used internally but exposed for testing.
        /// </summary>
        /// <param name="filePath">Absolute path to XLS file</param>
        /// <param name="loads">Loads to write</param>
        /// <param name="append">Whether to append to existing file (default true)</param>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        public Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true);

        /// <summary>
        /// Reads receiving loads from an XLS file.
        /// </summary>
        /// <param name="filePath">Absolute path to XLS file</param>
        /// <returns>List of receiving loads</returns>
        /// <exception cref="FileNotFoundException">If file does not exist</exception>
        /// <exception cref="InvalidOperationException">If read fails</exception>
        public Task<List<Model_ReceivingLoad>> ReadFromXLSAsync(string filePath);

        /// <summary>
        /// Clears XLS files (used for reset on startup).
        /// </summary>
        /// <returns>Result indicating which files were cleared</returns>
        public Task<Model_XLSDeleteResult> ClearXLSFilesAsync();

        /// <summary>
        /// Checks if XLS files exist.
        /// </summary>
        /// <returns>Result indicating existence of network files</returns>
        public Task<Model_XLSExistenceResult> CheckXLSFilesExistAsync();

        /// <summary>
        /// Gets the configured network XLS file path.
        /// Respects user-configured path from settings if available.
        /// </summary>
        /// <returns>UNC path to network XLS</returns>
        public Task<string> GetNetworkXLSPathAsync();
    }
}
