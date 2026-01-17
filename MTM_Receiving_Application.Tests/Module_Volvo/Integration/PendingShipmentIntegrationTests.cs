using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;
using Xunit;


namespace MTM_Receiving_Application.Tests.Module_Volvo.Integration;

/// <summary>
/// Integration tests for pending shipment save/load workflow.
/// </summary>
[Collection("Database")]
public class PendingShipmentIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public PendingShipmentIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task PendingShipment_ShouldSaveAndLoad()
    {
        await _fixture.InitializeAsync();
        Skip.If(!_fixture.IsDatabaseReady, _fixture.DatabaseNotReadyReason ?? "Database not ready");

        var shipmentDao = _fixture.CreateShipmentDao();
        var lineDao = _fixture.CreateShipmentLineDao();
        var partDao = _fixture.CreatePartDao();

        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();
        int shipmentId = 0;

        try
        {
            var partResult = await partDao.InsertAsync(new Model_VolvoPart
            {
                PartNumber = partNumber,
                QuantityPerSkid = 10,
                IsActive = true
            });
            partResult.Success.Should().BeTrue();

            var saveHandler = new SavePendingShipmentCommandHandler(shipmentDao, lineDao);
            var saveResult = await saveHandler.Handle(new SavePendingShipmentCommand
            {
                ShipmentDate = DateTimeOffset.Now.AddDays(-1),
                ShipmentNumber = 1,
                Notes = "TEST",
                Parts = new List<ShipmentLineDto>
                {
                    new() { PartNumber = partNumber, ReceivedSkidCount = 1 }
                }
            }, default);

            saveResult.IsSuccess.Should().BeTrue();
            shipmentId = saveResult.Data;

            var pendingHandler = new GetPendingShipmentQueryHandler(shipmentDao);
            var pendingResult = await pendingHandler.Handle(new GetPendingShipmentQuery { UserName = Environment.UserName }, default);

            pendingResult.IsSuccess.Should().BeTrue();
            pendingResult.Data.Should().NotBeNull();
        }
        finally
        {
            if (shipmentId > 0)
            {
                await shipmentDao.DeleteAsync(shipmentId);
            }
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
