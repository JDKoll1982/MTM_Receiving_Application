using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GetPendingShipmentQueryHandler.
/// </summary>
[Collection("Database")]
public class GetPendingShipmentQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GetPendingShipmentQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnPendingShipment_WhenExists()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        int shipmentId = 0;

        try
        {
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = DateTime.Today,
                EmployeeNumber = "TEST",
                Notes = "TEST-PENDING"
            };

            var insertResult = await shipmentDao.InsertAsync(shipment);
            insertResult.Success.Should().BeTrue();
            shipmentId = insertResult.Data.ShipmentId;

            var handler = new GetPendingShipmentQueryHandler(shipmentDao);
            var result = await handler.Handle(new GetPendingShipmentQuery { UserName = Environment.UserName }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
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
