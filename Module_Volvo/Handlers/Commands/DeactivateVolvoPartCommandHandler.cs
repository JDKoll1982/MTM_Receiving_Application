using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for DeactivateVolvoPartCommand - deactivates a part.
/// </summary>
public class DeactivateVolvoPartCommandHandler : IRequestHandler<DeactivateVolvoPartCommand, Model_Dao_Result>
{
    private readonly Dao_VolvoPart _partDao;

    public DeactivateVolvoPartCommandHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result> Handle(DeactivateVolvoPartCommand request, CancellationToken cancellationToken)
    {
        return await _partDao.DeactivateAsync(request.PartNumber);
    }
}
