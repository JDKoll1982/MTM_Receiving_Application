using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to get the current workflow session for the user
/// Used across all 3 steps to load session state
/// </summary>
public record QueryRequest_Receiving_Shared_Get_WorkflowSession : IRequest<Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>>
{
    /// <summary>
    /// Session ID to retrieve (if known)
    /// If null, retrieves the active session for the user
    /// </summary>
    public Guid? SessionId { get; init; }

    /// <summary>
    /// User ID who owns the session
    /// Required if SessionId is null
    /// </summary>
    public int? UserId { get; init; }
}
