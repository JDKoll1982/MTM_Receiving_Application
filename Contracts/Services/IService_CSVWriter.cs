using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for writing receiving data to CSV files.
    /// Handles both local (%APPDATA%) and network paths with graceful fallback.
    /// </summary>
    public interface IService_CSVWriter
    {
        /// <summary>
        /// Writes receiving loads to both local and network CSV files.
        /// Network failure does not prevent local write (graceful degradation).
        /// </summary>
        /// <param name="loads">List of loads to write</param>
        /// <returns>Result object indicating success/failure for each destination</returns>
        /// <exception cref="ArgumentException">If loads list is null or empty</exception>
        /// <exception cref="InvalidOperationException">If local write fails (critical error)</exception>
        public Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Writes loads to a specific CSV file path.
        /// Used internally but exposed for testing.
        /// </summary>
        /// <param name="filePath">Absolute path to CSV file</param>
        /// <param name="loads">Loads to write</param>
        /// <param name="append">Whether to append to existing file (default true)</param>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        public Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true);

        /// <summary>
        /// Reads receiving loads from a CSV file.
        /// </summary>
        /// <param name="filePath">Absolute path to CSV file</param>
        /// <returns>List of receiving loads</returns>
        /// <exception cref="FileNotFoundException">If file does not exist</exception>
        /// <exception cref="InvalidOperationException">If read fails</exception>
        public Task<List<Model_ReceivingLoad>> ReadFromCSVAsync(string filePath);

        /// <summary>
        /// Deletes CSV files (used for reset on startup).
        /// </summary>
        /// <returns>Result indicating which files were deleted</returns>
        public Task<Model_CSVDeleteResult> DeleteCSVFilesAsync();

        /// <summary>
        /// Checks if CSV files exist.
        /// </summary>
        /// <returns>Result indicating existence of local and network files</returns>
        public Task<Model_CSVExistenceResult> CheckCSVFilesExistAsync();

        /// <summary>
        /// Gets the configured local CSV file path.
        /// </summary>
        /// <returns>Absolute path to local CSV</returns>
        public string GetLocalCSVPath();

        /// <summary>
        /// Gets the configured network CSV file path.
        /// </summary>
        /// <returns>UNC path to network CSV</returns>
        public string GetNetworkCSVPath();
    }
}
