using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GetShipmentDetailQueryHandler.
/// </summary>
[Collection("Database")]
public class GetShipmentDetailQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GetShipmentDetailQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentAndLines()
    {
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

            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = DateTime.Today,
                EmployeeNumber = "TEST",
                Notes = "TEST-DETAIL"
            };

            var insertResult = await shipmentDao.InsertAsync(shipment);
            insertResult.Success.Should().BeTrue();
            shipmentId = insertResult.Data.ShipmentId;

            var line = new Model_VolvoShipmentLine
            {
                ShipmentId = shipmentId,
                PartNumber = partNumber,
                QuantityPerSkid = 10,
                ReceivedSkidCount = 1,
                CalculatedPieceCount = 10
            };

            var lineResult = await lineDao.InsertAsync(line);
            lineResult.Success.Should().BeTrue();

            var handler = new GetShipmentDetailQueryHandler(shipmentDao, lineDao);
            var result = await handler.Handle(new GetShipmentDetailQuery { ShipmentId = shipmentId }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Shipment.Id.Should().Be(shipmentId);
            result.Data.Lines.Count.Should().BeGreaterThan(0);
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
