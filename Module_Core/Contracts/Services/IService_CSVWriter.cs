using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
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
        public Task<Model_Receiving_Result_CSVWrite> WriteToCSVAsync(List<Model_Receiving_Entity_ReceivingLoad> loads);

        /// <summary>
        /// Writes loads to a specific CSV file path.
        /// Used internally but exposed for testing.
        /// </summary>
        /// <param name="filePath">Absolute path to CSV file</param>
        /// <param name="loads">Loads to write</param>
        /// <param name="append">Whether to append to existing file (default true)</param>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        public Task WriteToFileAsync(string filePath, List<Model_Receiving_Entity_ReceivingLoad> loads, bool append = true);

        /// <summary>
        /// Reads receiving loads from a CSV file.
        /// </summary>
        /// <param name="filePath">Absolute path to CSV file</param>
        /// <returns>List of receiving loads</returns>
        /// <exception cref="FileNotFoundException">If file does not exist</exception>
        /// <exception cref="InvalidOperationException">If read fails</exception>
        public Task<List<Model_Receiving_Entity_ReceivingLoad>> ReadFromCSVAsync(string filePath);

        /// <summary>
        /// Clears CSV files (used for reset on startup).
        /// </summary>
        /// <returns>Result indicating which files were cleared</returns>
        public Task<Model_Receiving_Result_CSVDelete> ClearCSVFilesAsync();

        /// <summary>
        /// Checks if CSV files exist.
        /// </summary>
        /// <returns>Result indicating existence of local and network files</returns>
        public Task<Model_Receiving_Result_CSVExistence> CheckCSVFilesExistAsync();

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

