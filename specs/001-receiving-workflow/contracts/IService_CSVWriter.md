```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;

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
        Task<CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Writes loads to a specific CSV file path.
        /// Used internally but exposed for testing.
        /// </summary>
        /// <param name="filePath">Absolute path to CSV file</param>
        /// <param name="loads">Loads to write</param>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Deletes CSV files (used for reset on startup).
        /// </summary>
        /// <returns>Result indicating which files were deleted</returns>
        Task<CSVDeleteResult> DeleteCSVFilesAsync();

        /// <summary>
        /// Checks if CSV files exist.
        /// </summary>
        /// <returns>Result indicating existence of local and network files</returns>
        Task<CSVExistenceResult> CheckCSVFilesExistAsync();

        /// <summary>
        /// Gets the configured local CSV file path.
        /// </summary>
        /// <returns>Absolute path to local CSV</returns>
        string GetLocalCSVPath();

        /// <summary>
        /// Gets the configured network CSV file path.
        /// </summary>
        /// <returns>UNC path to network CSV</returns>
        string GetNetworkCSVPath();
    }

    /// <summary>
    /// Result of CSV write operation.
    /// </summary>
    public class CSVWriteResult
    {
        public bool LocalSuccess { get; set; }
        public bool NetworkSuccess { get; set; }
        public string? LocalError { get; set; }
        public string? NetworkError { get; set; }
        public int RecordsWritten { get; set; }

        public bool IsFullSuccess => LocalSuccess && NetworkSuccess;
        public bool IsPartialSuccess => LocalSuccess && !NetworkSuccess;
        public bool IsFailure => !LocalSuccess;
    }

    /// <summary>
    /// Result of CSV delete operation.
    /// </summary>
    public class CSVDeleteResult
    {
        public bool LocalDeleted { get; set; }
        public bool NetworkDeleted { get; set; }
        public string? LocalError { get; set; }
        public string? NetworkError { get; set; }
    }

    /// <summary>
    /// Result of CSV existence check.
    /// </summary>
    public class CSVExistenceResult
    {
        public bool LocalExists { get; set; }
        public bool NetworkExists { get; set; }
        public bool NetworkAccessible { get; set; }
    }
}
```
