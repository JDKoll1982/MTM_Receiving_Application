using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for querying Infor Visual database (SQL Server) for PO and Part information.
    /// Read-only operations only - no writes to Infor Visual.
    /// </summary>
    public interface IService_InforVisual
    {
        /// <summary>
        /// Retrieves a purchase order with all associated parts from Infor Visual.
        /// </summary>
        /// <param name="poNumber">6-digit PO number</param>
        /// <returns>Result containing InforVisualPO with Parts collection, or null if not found</returns>
        Task<DaoResult<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber);

        /// <summary>
        /// Retrieves part information by Part ID for non-PO items.
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <returns>Result containing InforVisualPart with details, or null if not found</returns>
        Task<DaoResult<Model_InforVisualPart?>> GetPartByIDAsync(string partID);

        /// <summary>
        /// Queries same-day receiving transactions for a specific PO and Part.
        /// Used to warn users if part was already received today.
        /// </summary>
        /// <param name="poNumber">PO number</param>
        /// <param name="partID">Part identifier</param>
        /// <param name="date">Date to query (typically DateTime.Today)</param>
        /// <returns>Result containing Total quantity received, or 0 if no receipts found</returns>
        Task<DaoResult<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date);

        /// <summary>
        /// Calculates remaining quantity for a specific PO and Part.
        /// Remaining Quantity = Quantity Ordered - Quantity Received.
        /// Returns whole number only (no decimals).
        /// </summary>
        /// <param name="poNumber">PO number</param>
        /// <param name="partID">Part identifier</param>
        /// <returns>Result containing remaining quantity as integer</returns>
        Task<DaoResult<int>> GetRemainingQuantityAsync(string poNumber, string partID);

        /// <summary>
        /// Validates that the Infor Visual database connection is available.
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> TestConnectionAsync();
    }
}
