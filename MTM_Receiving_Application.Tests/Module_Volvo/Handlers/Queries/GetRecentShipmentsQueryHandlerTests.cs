using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GetRecentShipmentsQueryHandler.
/// </summary>
[Collection("Database")]
public class GetRecentShipmentsQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GetRecentShipmentsQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnRecentShipments()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        int shipmentId = 0;

        try
        {
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = DateTime.Today,
                EmployeeNumber = "TEST",
                Notes = "TEST-RECENT"
            };

            var insertResult = await shipmentDao.InsertAsync(shipment);
            insertResult.Success.Should().BeTrue();
            shipmentId = insertResult.Data.ShipmentId;

            var handler = new GetRecentShipmentsQueryHandler(shipmentDao);
            var result = await handler.Handle(new GetRecentShipmentsQuery { Days = 30 }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().BeGreaterThan(0);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await shipmentDao.DeleteAsync(shipmentId);
            }
        }
    }
}
