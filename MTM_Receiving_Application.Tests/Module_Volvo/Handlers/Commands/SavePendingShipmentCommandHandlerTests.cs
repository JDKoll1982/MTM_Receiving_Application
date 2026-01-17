using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Tests.Helpers;
using Xunit;


namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Integration tests for SavePendingShipmentCommandHandler.
/// </summary>
[Collection("Database")]
public class SavePendingShipmentCommandHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public SavePendingShipmentCommandHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task Handle_ShouldSavePendingShipment()
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

            var handler = new SavePendingShipmentCommandHandler(shipmentDao, lineDao, partDao);
            var result = await handler.Handle(new SavePendingShipmentCommand
            {
                ShipmentDate = DateTimeOffset.Now.AddDays(-1),
                ShipmentNumber = 1,
                Notes = "TEST",
                Parts = new List<ShipmentLineDto>
                {
                    new() { PartNumber = partNumber, ReceivedSkidCount = 1 }
                }
            }, default);

            result.IsSuccess.Should().BeTrue();
            shipmentId = result.Data;
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
