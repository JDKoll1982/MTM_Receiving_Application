using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.DTOs;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to retrieve enriched part details with preferences and type information
/// Used in Wizard Mode Step 1 for part selection and auto-population
/// </summary>
public class QueryRequest_Receiving_Shared_Get_PartDetails : IRequest<Model_Dao_Result<Model_Receiving_DataTransferObjects_PartDetails>>
{
    /// <summary>
    /// Part number to look up
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Scope for preferences (System or User)
    /// </summary>
    public string Scope { get; set; } = "System";

    /// <summary>
    /// User ID for User-scoped preferences
    /// </summary>
    public string? ScopeUserId { get; set; }
}
