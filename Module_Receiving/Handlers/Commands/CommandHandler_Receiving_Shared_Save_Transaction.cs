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
/// <para>
/// This handler processes the CommandRequest_Receiving_Shared_Save_Transaction to save a receiving transaction
/// and all its associated load details to the database. It creates a new transaction entity with the
/// provided PO number, user information, and workflow mode, then converts and attaches the load data
/// before persisting everything via the transaction DAO.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_Transaction: Injected DAO for database operations on transactions.
/// - Model_Receiving_TableEntitys_ReceivingTransaction: Entity model for the transaction data.
/// - Model_Receiving_TableEntitys_ReceivingLoad: Entity model for individual load details.
/// - Enum_Receiving_Mode_WorkflowMode: Enum for specifying the workflow mode (set to Wizard).
/// - CommandRequest_Receiving_Shared_Save_Transaction: The command request containing transaction and load data.
/// - Model_Dao_Result: Result wrapper for DAO operations.
/// - Model_Dao_Result_Factory: Factory for creating success/failure result instances.
/// </para>
/// </summary>
public class CommandHandler_Receiving_Shared_Save_Transaction : IRequestHandler<CommandRequest_Receiving_Shared_Save_Transaction, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_Transaction _transactionDao;

    public CommandHandler_Receiving_Shared_Save_Transaction(Dao_Receiving_Repository_Transaction transactionDao)
    {
        _transactionDao = transactionDao;
    }

    public async Task<Model_Dao_Result> Handle(
        CommandRequest_Receiving_Shared_Save_Transaction request,
        CancellationToken cancellationToken)
    {
        var transaction = new Model_Receiving_TableEntitys_ReceivingTransaction
        {
            PONumber = request.PONumber,
            UserId = request.UserId,
            UserName = request.UserName,
            WorkflowMode = Enum_Receiving_Mode_WorkflowMode.Wizard,
            TransactionDate = System.DateTime.Now,
            Loads = request.Loads.ConvertAll(load => new Model_Receiving_TableEntitys_ReceivingLoad
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
            })
        };

        var result = await _transactionDao.InsertTransactionAsync(transaction);

        if (result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Success(1);
        }

        return Model_Dao_Result_Factory.Failure(result.ErrorMessage ?? "Failed to save transaction");
    }
}
