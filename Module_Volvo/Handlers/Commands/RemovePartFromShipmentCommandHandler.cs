using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for RemovePartFromShipmentCommand.
/// Returns success - ViewModel manages its own ObservableCollection removal.
/// </summary>
public class RemovePartFromShipmentCommandHandler : IRequestHandler<RemovePartFromShipmentCommand, Model_Dao_Result>
{
    public async Task<Model_Dao_Result> Handle(RemovePartFromShipmentCommand request, CancellationToken cancellationToken)
    {
        // Simple validation - part number should not be empty
        if (string.IsNullOrWhiteSpace(request.PartNumber))
        {
            return Model_Dao_Result_Factory.Failure("Part number is required");
        }

        // Return success - ViewModel will remove from ObservableCollection
        return await Task.FromResult(Model_Dao_Result_Factory.Success());
    }
}
