using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts
{
    /// <summary>
    /// Service for reading and writing receiving label data.
    /// Uses the configured label-data source path when needed.
    /// </summary>
    public interface IService_ReceivingLabelData
    {
        /// <summary>
        /// Saves receiving loads to the current label-data store.
        /// </summary>
        /// <param name="loads">List of loads to write</param>
        /// <returns>Result object indicating success/failure</returns>
        /// <exception cref="ArgumentException">If loads list is null or empty</exception>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        public Task<Model_LabelDataSaveResult> SaveLabelDataAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Saves loads to a specific label-data path.
        /// Used internally but exposed for testing.
        /// </summary>
        /// <param name="filePath">Absolute path to the label-data source</param>
        /// <param name="loads">Loads to write</param>
        /// <param name="append">Whether to append to existing file (default true)</param>
        /// <exception cref="InvalidOperationException">If write fails</exception>
        public Task SaveLabelDataToPathAsync(
            string filePath,
            List<Model_ReceivingLoad> loads,
            bool append = true
        );

        /// <summary>
        /// Loads receiving loads from the current label-data source.
        /// </summary>
        /// <param name="filePath">Absolute path to the label-data source</param>
        /// <returns>List of receiving loads</returns>
        /// <exception cref="System.IO.FileNotFoundException">If file does not exist</exception>
        /// <exception cref="InvalidOperationException">If read fails</exception>
        public Task<List<Model_ReceivingLoad>> LoadLabelDataAsync(string filePath);

        /// <summary>
        /// Clears label data (used for reset on startup).
        /// </summary>
        /// <returns>Result indicating which label-data targets were cleared</returns>
        public Task<Model_LabelDataClearResult> ClearLabelDataAsync();

        /// <summary>
        /// Checks whether the label-data source is available.
        /// </summary>
        /// <returns>Result indicating label-data availability</returns>
        public Task<Model_LabelDataAvailabilityResult> CheckLabelDataAvailabilityAsync();

        /// <summary>
        /// Gets the configured label-data source path.
        /// Respects user-configured path from settings if available.
        /// </summary>
        /// <returns>Path to the label-data source</returns>
        public Task<string> GetLabelDataPathAsync();
    }
}
