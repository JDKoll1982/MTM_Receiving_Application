using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for GetWorkflowSessionQuery
/// Retrieves active workflow session state
/// </summary>
public class GetWorkflowSessionQueryHandler
    : IRequestHandler<GetWorkflowSessionQuery, Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>>
{
    private readonly Dao_Receiving_Repository_WorkflowSession _sessionDao;

    public GetWorkflowSessionQueryHandler(Dao_Receiving_Repository_WorkflowSession sessionDao)
    {
        _sessionDao = sessionDao;
    }

    public async Task<Model_Dao_Result<Model_Receiving_Entity_WorkflowSession>> Handle(
        GetWorkflowSessionQuery request,
        CancellationToken cancellationToken)
    {
        // If SessionId is provided, retrieve by ID
        if (request.SessionId.HasValue && request.SessionId.Value != System.Guid.Empty)
        {
            return await _sessionDao.GetSessionByIdAsync(request.SessionId.Value);
        }

        // Otherwise, retrieve active session for user
        if (request.UserId.HasValue && request.UserId.Value > 0)
        {
            return await _sessionDao.GetActiveSessionByUserIdAsync(request.UserId.Value);
        }

        return Model_Dao_Result_Factory.Failure<Model_Receiving_Entity_WorkflowSession>(
            "Either SessionId or UserId must be provided");
    }
}
