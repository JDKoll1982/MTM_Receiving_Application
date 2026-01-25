using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts
{
    /// <summary>
    /// Service for saving receiving load data to MySQL database.
    /// </summary>
    public interface IService_Receiving_Business_MySQL_Receiving
    {
        /// <summary>
        /// Saves a batch of receiving loads to the database within a transaction.
        /// All loads succeed or all fail (atomic operation).
        /// </summary>
        /// <param name="loads">List of receiving loads to save</param>
        /// <returns>Number of loads successfully inserted</returns>
        /// <exception cref="ArgumentException">If loads list is null or empty</exception>
        /// <exception cref="InvalidOperationException">If database operation fails</exception>
        public Task<int> SaveReceivingLoadsAsync(List<Model_Receiving_Entity_ReceivingLoad> loads);

        /// <summary>
        /// Retrieves receiving history for a specific part (for reference/audit).
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <param name="startDate">Start date for history</param>
        /// <param name="endDate">End date for history</param>
        /// <returns>List of historical receiving loads</returns>
        public Task<List<Model_Receiving_Entity_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves all receiving loads within a date range.
        /// </summary>
        /// <param name="startDate">Start date for retrieval</param>
        /// <param name="endDate">End date for retrieval</param>
        /// <returns>DAO result containing list of receiving loads</returns>
        public Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates a batch of receiving loads in the database.
        /// </summary>
        /// <param name="loads">List of loads to update</param>
        /// <returns>Number of loads successfully updated</returns>
        public Task<int> UpdateReceivingLoadsAsync(List<Model_Receiving_Entity_ReceivingLoad> loads);

        /// <summary>
        /// Deletes a batch of receiving loads from the database.
        /// </summary>
        /// <param name="loads">List of loads to delete</param>
        /// <returns>Number of loads successfully deleted</returns>
        public Task<int> DeleteReceivingLoadsAsync(List<Model_Receiving_Entity_ReceivingLoad> loads);

        /// <summary>
        /// Validates that the MySQL database connection is available.
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        public Task<bool> TestConnectionAsync();
    }
}

