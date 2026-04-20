using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for ActivateVolvoPartCommand.
/// </summary>
public class ActivateVolvoPartCommandHandler
    : IRequestHandler<ActivateVolvoPartCommand, Model_Dao_Result>
{
    private readonly Dao_VolvoPart _partDao;

    public ActivateVolvoPartCommandHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result> Handle(
        ActivateVolvoPartCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _partDao.ActivateAsync(request.PartNumber);
    }
}
