using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for AddVolvoPartCommand - adds a new part to master data.
/// </summary>
public class AddVolvoPartCommandHandler : IRequestHandler<AddVolvoPartCommand, Model_Dao_Result>
{
    private readonly Dao_VolvoPart _partDao;

    public AddVolvoPartCommandHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result> Handle(AddVolvoPartCommand request, CancellationToken cancellationToken)
    {
        var part = new Model_VolvoPart
        {
            PartNumber = request.PartNumber.Trim().ToUpperInvariant(),
            QuantityPerSkid = request.QuantityPerSkid,
            IsActive = true
        };

        return await _partDao.InsertAsync(part);
    }
}
