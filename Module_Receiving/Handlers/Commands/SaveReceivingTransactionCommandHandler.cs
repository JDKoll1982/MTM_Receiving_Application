using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

/// <summary>
/// Handler for SaveReceivingTransactionCommand
/// Persists complete receiving transaction with all loads
/// </summary>
public class SaveReceivingTransactionCommandHandler : IRequestHandler<SaveReceivingTransactionCommand, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_ReceivingTransaction _transactionDao;

    public SaveReceivingTransactionCommandHandler(Dao_Receiving_Repository_ReceivingTransaction transactionDao)
    {
        _transactionDao = transactionDao;
    }

    public async Task<Model_Dao_Result> Handle(
        SaveReceivingTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = new Model_Receiving_Entity_ReceivingTransaction
        {
            PONumber = request.PONumber,
            UserId = request.UserId,
            UserName = request.UserName,
            WorkflowMode = Enum_Receiving_Mode_WorkflowMode.Wizard,
            TransactionDate = System.DateTime.Now,
            Loads = request.Loads.Select(load => new Model_Receiving_Entity_ReceivingLoad
            {
                LoadNumber = load.LoadNumber,
                PartId = load.PartId,
                Quantity = load.Quantity,
                UnitOfMeasure = load.UnitOfMeasure,
                HeatLotNumber = load.HeatLotNumber,
                PackageType = load.PackageType,
                PackagesPerLoad = load.PackagesPerLoad,
                WeightPerPackage = load.WeightPerPackage,
                ReceivingLocation = load.ReceivingLocation,
                QualityHoldAcknowledged = load.QualityHoldAcknowledged
            }).ToList()
        };

        var result = await _transactionDao.InsertTransactionAsync(transaction);

        if (result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Success(1);
        }

        return Model_Dao_Result_Factory.Failure(result.ErrorMessage ?? "Failed to save transaction");
    }
}
