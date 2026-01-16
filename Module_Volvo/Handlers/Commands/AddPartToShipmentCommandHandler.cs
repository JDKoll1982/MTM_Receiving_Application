using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for AddPartToShipmentCommand.
/// Validates part exists in master data and returns success for ViewModel to update its collection.
/// </summary>
public class AddPartToShipmentCommandHandler : IRequestHandler<AddPartToShipmentCommand, Model_Dao_Result>
{
    private readonly Dao_VolvoPart _partDao;

    public AddPartToShipmentCommandHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result> Handle(AddPartToShipmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate part exists in master data
            var partResult = await _partDao.GetByIdAsync(request.PartNumber);

            if (!partResult.IsSuccess || partResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure(
                    $"Part '{request.PartNumber}' not found in master data");
            }

            // Return success - ViewModel will update its ObservableCollection
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                $"Unexpected error adding part: {ex.Message}", ex);
        }
    }
}
