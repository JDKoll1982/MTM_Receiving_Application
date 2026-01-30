using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for QueryRequest_Receiving_Shared_Get_WorkflowSession
/// Retrieves active workflow session state
/// </summary>
public class QueryHandler_Receiving_Shared_Get_WorkflowSession
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_WorkflowSession, Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>>
{
    private readonly Dao_Receiving_Repository_WorkflowSession _sessionDao;

    public QueryHandler_Receiving_Shared_Get_WorkflowSession(Dao_Receiving_Repository_WorkflowSession sessionDao)
    {
        _sessionDao = sessionDao;
    }

    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>> Handle(
        QueryRequest_Receiving_Shared_Get_WorkflowSession request,
        CancellationToken cancellationToken)
    {
        // If SessionId is provided, retrieve by ID
        if (request.SessionId.HasValue && request.SessionId.Value != System.Guid.Empty)
        {
            return await _sessionDao.SelectByIdAsync(request.SessionId.Value.ToString());
        }

        // Otherwise, retrieve active session for user
        if (request.UserId.HasValue && request.UserId.Value > 0)
        {
            var userSessions = await _sessionDao.SelectByUserAsync($"User_{request.UserId.Value}");
            if (userSessions.Success && userSessions.Data?.Count > 0)
            {
                // Return the most recently updated session
                var mostRecent = userSessions.Data.OrderByDescending(s => s.UpdatedAt).First();
                return new Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>
                {
                    Success = true,
                    Data = mostRecent
                };
            }
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_WorkflowSession>(
                "No active session found for user");
        }

        return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_WorkflowSession>(
            "Either SessionId or UserId must be provided");
    }
}
