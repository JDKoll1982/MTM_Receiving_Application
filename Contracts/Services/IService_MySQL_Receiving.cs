using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
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
        Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads);

        /// <summary>
        /// Retrieves receiving history for a specific part (for reference/audit).
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <param name="startDate">Start date for history</param>
        /// <param name="endDate">End date for history</param>
        /// <returns>List of historical receiving loads</returns>
        Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Validates that the MySQL database connection is available.
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> TestConnectionAsync();
    }
}
