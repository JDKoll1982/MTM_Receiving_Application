using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to search receiving transactions with multiple filter criteria
/// Used in history/search views
/// </summary>
public class QueryRequest_Receiving_Shared_Search_Transactions : IRequest<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingTransaction>>>
{
    /// <summary>
    /// Filter by PO Number (partial match)
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Filter by Part Number (partial match)
    /// </summary>
    public string? PartNumber { get; set; }

    /// <summary>
    /// Filter by transactions created after this date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter by transactions created before this date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by transaction status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Include soft-deleted transactions in results
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;
}
