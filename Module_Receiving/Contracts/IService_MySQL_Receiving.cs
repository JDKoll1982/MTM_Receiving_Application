using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts
{
    /// <summary>
    /// Service for saving receiving load data to MySQL database.
    /// </summary>
    public interface IService_MySQL_Receiving
    {
        /// <summary>
        /// Saves a batch of receiving loads to the database within a transaction.
        /// All loads succeed or all fail (atomic operation).
        /// </summary>
        /// <param name="loads">List of receiving loads to save</param>
        /// <returns>Number of loads successfully inserted</returns>
        /// <exception cref="ArgumentException">If loads list is null or empty</exception>
        /// <exception cref="InvalidOperationException">If database operation fails</exception>
        public Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Retrieves receiving history for a specific part (for reference/audit).
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <param name="startDate">Start date for history</param>
        /// <param name="endDate">End date for history</param>
        /// <returns>List of historical receiving loads</returns>
        public Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(
            string partID,
            DateTime startDate,
            DateTime endDate
        );

        /// <summary>
        /// Retrieves all receiving loads within a date range.
        /// </summary>
        /// <param name="startDate">Start date for retrieval</param>
        /// <param name="endDate">End date for retrieval</param>
        /// <returns>DAO result containing list of receiving loads</returns>
        public Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(
            DateTime startDate,
            DateTime endDate
        );

        /// <summary>
        /// Updates a batch of receiving loads in the database.
        /// </summary>
        /// <param name="loads">List of loads to update</param>
        /// <returns>Number of loads successfully updated</returns>
        public Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Deletes a batch of receiving loads from the database.
        /// </summary>
        /// <param name="loads">List of loads to delete</param>
        /// <returns>Number of loads successfully deleted</returns>
        public Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Validates that the MySQL database connection is available.
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        public Task<bool> TestConnectionAsync();

        /// <summary>
        /// Moves active label queue rows to history and clears the queue.
        /// </summary>
        /// <param name="archivedBy">User performing the archive action</param>
        /// <param name="employeeNumber"></param>
        /// <param name="clearAllRows"></param>
        /// <returns>DAO result containing number of rows moved</returns>
        public Task<Model_Dao_Result<int>> ClearLabelDataToHistoryAsync(
            string archivedBy,
            int employeeNumber,
            bool clearAllRows
        );

        /// <summary>
        /// Retrieves all rows from the active label queue (receiving_label_data).
        /// Used by Edit Mode "Current Labels" to load today's pending labels.
        /// </summary>
        /// <returns>DAO result containing list of current label loads</returns>
        public Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetCurrentLabelDataAsync();

        /// <summary>
        /// Checks whether the active receiving label queue currently contains any rows.
        /// </summary>
        public Task<bool> HasActiveLabelDataAsync();

        /// <summary>
        /// Deletes rows from the active receiving_label_data print queue.
        /// Used by Edit Mode when rows are removed from Current Labels.
        /// </summary>
        /// <param name="loads">The list of receiving loads to delete.</param>
        public Task<int> DeleteCurrentLabelDataAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Updates rows in the active receiving_label_data print queue.
        /// Used by Edit Mode when saving edits to Current Labels records.
        /// </summary>
        /// <param name="loads">The list of receiving loads to update.</param>
        public Task<int> UpdateCurrentLabelDataAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Copies a single row from receiving_history back into receiving_label_data
        /// so it can be re-printed. Sets is_reprint = 1 on the queued row.
        /// </summary>
        /// <param name="historyId">The receiving_history.id of the row to requeue.</param>
        public Task<Model_Dao_Result<int>> InsertFromHistoryAsync(int historyId);

        /// <summary>
        /// Loads receiving history rows for the Reprint Labels page, including whether each row
        /// is already queued for reprint.
        /// </summary>
        public Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
            Model_ReprintHistoryFilter filter
        );

        /// <summary>
        /// Returns all reusable non-PO reference entries for Receiving.
        /// </summary>
        public Task<Model_Dao_Result<List<Model_ReceivingNonPOEntry>>> GetNonPOEntriesAsync();

        /// <summary>
        /// Saves or increments a reusable non-PO reference entry for Receiving.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="createdBy"></param>
        public Task<Model_Dao_Result> SaveNonPOEntryAsync(string value, string createdBy);

        /// <summary>
        /// Deletes a reusable non-PO reference entry for Receiving.
        /// </summary>
        /// <param name="id"></param>
        public Task<Model_Dao_Result> DeleteNonPOEntryAsync(int id);

        /// <summary>
        /// Returns the saved per-part default non-PO reference for a Receiving part.
        /// </summary>
        /// <param name="partId"></param>
        public Task<Model_Dao_Result<string?>> GetNonPOPartDefaultAsync(string partId);

        /// <summary>
        /// Saves the per-part default non-PO reference for a Receiving part.
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="value"></param>
        /// <param name="updatedBy"></param>
        public Task<Model_Dao_Result> SaveNonPOPartDefaultAsync(
            string partId,
            string value,
            string updatedBy
        );
    }
}
