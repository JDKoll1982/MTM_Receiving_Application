using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for querying Infor Visual database (SQL Server) for PO and Part information.
/// Read-only operations only - no writes to Infor Visual.
/// </summary>
public interface IService_InforVisual
{
    /// <summary>
    /// Retrieves a purchase order with all associated parts from Infor Visual.
    /// </summary>
    /// <param name="poNumber">6-digit PO number (formatted as "PO-063150")</param>
    /// <returns>Result containing InforVisualPO with Parts collection, or null if not found</returns>
    Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber);

    /// <summary>
    /// Retrieves part information by Part ID for non-PO items.
    /// </summary>
    /// <param name="partID">Part identifier</param>
    /// <returns>Result containing InforVisualPart with details, or null if not found</returns>
    Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID);

    /// <summary>
    /// Queries same-day receiving transactions for a specific PO and Part.
    /// Used to warn users if part was already received today.
    /// </summary>
    /// <param name="poNumber">PO number</param>
    /// <param name="partID">Part identifier</param>
    /// <param name="date">Date to query (typically DateTime.Today)</param>
    /// <returns>Result containing Total quantity received, or 0 if no receipts found</returns>
    Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date);

    /// <summary>
    /// Calculates remaining quantity for a specific PO and Part.
    /// Used to validate that receiving quantity doesn't exceed PO quantity.
    /// </summary>
    /// <param name="poNumber">PO number</param>
    /// <param name="partID">Part identifier</param>
    /// <returns>Result containing Remaining quantity to receive</returns>
    Task<Model_Dao_Result<decimal>> GetRemainingQuantityAsync(string poNumber, string partID);
}


