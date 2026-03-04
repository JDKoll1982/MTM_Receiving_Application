using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Models;
using Model_InforVisualPO = MTM_Receiving_Application.Module_Receiving.Models.Model_InforVisualPO;
using Model_InforVisualPart = MTM_Receiving_Application.Module_Receiving.Models.Model_InforVisualPart;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
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
        public Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber);

        /// <summary>
        /// Retrieves part information by Part ID for non-PO items.
        /// </summary>
        /// <param name="partID">Part identifier</param>
        /// <returns>Result containing InforVisualPart with details, or null if not found</returns>
        public Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID);

        /// <summary>
        /// Queries same-day receiving transactions for a specific PO and Part.
        /// Used to warn users if part was already received today.
        /// </summary>
        /// <param name="poNumber">PO number</param>
        /// <param name="partID">Part identifier</param>
        /// <param name="date">Date to query (typically DateTime.Today)</param>
        /// <returns>Result containing Total quantity received, or 0 if no receipts found</returns>
        public Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date);

        /// <summary>
        /// Calculates remaining quantity for a specific PO and Part.
        /// Remaining Quantity = Quantity Ordered - Quantity Received.
        /// Returns whole number only (no decimals).
        /// </summary>
        /// <param name="poNumber">PO number</param>
        /// <param name="partID">Part identifier</param>
        /// <returns>Result containing remaining quantity as integer</returns>
        public Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID);

        /// <summary>
        /// Validates that the Infor Visual database connection is available.
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        public Task<bool> TestConnectionAsync();

        /// <summary>
        /// Retrieves outside service dispatch history for a specific part number.
        /// Queries SERVICE_DISP_LINE joined to SERVICE_DISPATCH and VENDOR.
        /// </summary>
        /// <param name="partNumber">The part ID to search for in SERVICE_DISP_LINE.</param>
        /// <returns>Result containing a list of dispatch records, sorted newest first.</returns>
        public Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetOutsideServiceHistoryByPartAsync(string partNumber);

        /// <summary>
        /// Retrieves outside service dispatch history for a specific vendor ID.
        /// Queries SERVICE_DISP_LINE joined to SERVICE_DISPATCH and VENDOR.
        /// </summary>
        /// <param name="vendorId">The vendor ID to filter dispatch records by.</param>
        /// <returns>Result containing a list of dispatch records, sorted newest first.</returns>
        public Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetOutsideServiceHistoryByVendorAsync(string vendorId);

        /// <summary>
        /// Fuzzy-searches parts by ID using a LIKE '%term%' query against Infor Visual.
        /// Returns up to 50 candidates for display in a selection picker.
        /// </summary>
        /// <param name="term">Partial part ID entered by the user.</param>
        public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(string term);

        /// <summary>
        /// Fuzzy-searches vendors by name using a LIKE '%term%' query against Infor Visual.
        /// Returns up to 50 candidates for display in a selection picker.
        /// </summary>
        /// <param name="term">Partial vendor name entered by the user.</param>
        public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchVendorsAsync(string term);

        /// <summary>
        /// Returns all distinct part numbers serviced by a specific vendor,
        /// along with dispatch count and last dispatch date.
        /// Used to populate the part selection picker after vendor confirmation.
        /// </summary>
        /// <param name="vendorId">The vendor ID to query parts for.</param>
        public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(string vendorId);

        /// <summary>
        /// Retrieves outside service dispatch history filtered by both vendor ID and part number.
        /// </summary>
        /// <param name="vendorId">The vendor ID to filter by.</param>
        /// <param name="partNumber">The part number to filter by.</param>
        public Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetOutsideServiceHistoryByVendorAndPartAsync(string vendorId, string partNumber);
    }
}

