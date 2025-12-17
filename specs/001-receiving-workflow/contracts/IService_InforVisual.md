```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for querying Infor Visual database (SQL Server) for PO and Part information.
    /// ⚠️ STRICTLY READ ONLY - NO WRITES TO INFOR VISUAL ⚠️
    /// Server: VISUAL, Database: MTMFG, Warehouse: 002
    /// Schema: PURCHASE_ORDER, PURC_ORDER_LINE, PART, INVENTORY_TRANS
    /// </summary>
    public interface IService_InforVisual
    {
        /// <summary>
        /// Retrieves a purchase order with all associated parts from Infor Visual.
        /// Queries PURCHASE_ORDER and PURC_ORDER_LINE tables.
        /// </summary>
        /// <param name="poNumber">PO number (PURCHASE_ORDER.ID)</param>
        /// <returns>InforVisualPO with Parts collection, or null if not found</returns>
        /// <exception cref="ArgumentException">If poNumber is invalid format</exception>
        /// <exception cref="InvalidOperationException">If database connection fails</exception>
        Task<Model_InforVisualPO?> GetPOWithPartsAsync(string poNumber);

        /// <summary>
        /// Retrieves part information by Part ID for non-PO items.
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <returns>InforVisualPart with details, or null if not found</returns>
        /// <exception cref="ArgumentException">If partID is null or empty</exception>
        /// <exception cref="InvalidOperationException">If database connection fails</exception>
        Task<Model_InforVisualPart?> GetPartByIDAsync(string partID);

        /// <summary>
        /// Queries same-day receiving transactions for a specific PO and Part.
        /// Used to warn users if part was already received today.
        /// </summary>
        /// <param name="poNumber">PO number</param>
        /// <param name="partID">Part identifier</param>
        /// <param name="date">Date to query (typically DateTime.Today)</param>
        /// <returns>Total quantity received, or 0 if no receipts found</returns>
        /// <exception cref="InvalidOperationException">If database connection fails</exception>
        Task<decimal> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date);

        /// <summary>
        /// Validates that the Infor Visual database connection is available.
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> TestConnectionAsync();
    }
}
```
