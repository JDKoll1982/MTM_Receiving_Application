using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for ExportShipmentsQueryHandler.
/// </summary>
[Collection("Database")]
public class ExportShipmentsQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public ExportShipmentsQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnCsvContent()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        int shipmentId = 0;

        try
        {
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = DateTime.Today,
                EmployeeNumber = "TEST",
                Notes = "TEST-EXPORT"
            };

            var insertResult = await shipmentDao.InsertAsync(shipment);
            insertResult.Success.Should().BeTrue();
            shipmentId = insertResult.Data.ShipmentId;

            var handler = new ExportShipmentsQueryHandler(shipmentDao);
            var result = await handler.Handle(new ExportShipmentsQuery
            {
                StartDate = DateTimeOffset.Now.AddDays(-2),
                EndDate = DateTimeOffset.Now,
                StatusFilter = "All"
            }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNullOrWhiteSpace();
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
