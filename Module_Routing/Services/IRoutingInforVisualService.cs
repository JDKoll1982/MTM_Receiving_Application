using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service interface for Infor Visual ERP integration: PO validation, line retrieval (READ ONLY)
/// </summary>
public interface IRoutingInforVisualService
{
    /// <summary>
    /// Validates PO number exists in Infor Visual MTMFG database
    /// </summary>
    /// <param name="poNumber">PO number to validate</param>
    /// <returns>Result with validation status (IsSuccess=true if found, false if not found or error)</returns>
    public Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber);

    /// <summary>
    /// Retrieves line items for a valid PO number from Infor Visual
    /// </summary>
    /// <param name="poNumber">PO number to retrieve lines for</param>
    /// <returns>Result with list of PO line items (part number, description, quantity) or error</returns>
    public Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetPoLinesAsync(string poNumber);

    /// <summary>
    /// Retrieves a single line item for a PO from Infor Visual
    /// </summary>
    /// <param name="poNumber">PO number</param>
    /// <param name="lineNumber">Line number</param>
    /// <returns>Result with PO line data or error</returns>
    public Task<Model_Dao_Result<Model_InforVisualPOLine>> GetPoLineAsync(string poNumber, string lineNumber);

    /// <summary>
    /// Checks if Infor Visual connection is available (health check)
    /// </summary>
    /// <returns>Result with connection status (IsSuccess=true if available)</returns>
    public Task<Model_Dao_Result<bool>> CheckConnectionAsync();
}
