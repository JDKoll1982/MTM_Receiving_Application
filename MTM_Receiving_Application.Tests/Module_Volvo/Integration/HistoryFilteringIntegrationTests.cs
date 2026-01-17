using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Integration;

/// <summary>
/// Integration tests for history filtering by date range and status.
/// </summary>
[Collection("Database")]
public class HistoryFilteringIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public HistoryFilteringIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FilterHistory_ShouldReturnResultsInRange()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        int shipmentId = 0;

        try
        {
            var shipment = new Model_VolvoShipment
            {
                ShipmentDate = DateTime.Today,
                EmployeeNumber = "TEST",
                Notes = "TEST-HISTORY-FILTER"
            };

            var insertResult = await shipmentDao.InsertAsync(shipment);
            insertResult.Success.Should().BeTrue();
            shipmentId = insertResult.Data.ShipmentId;

            var handler = new GetShipmentHistoryQueryHandler(shipmentDao);
            var result = await handler.Handle(new GetShipmentHistoryQuery
            {
                StartDate = DateTimeOffset.Now.AddDays(-2),
                EndDate = DateTimeOffset.Now,
                StatusFilter = "All"
            }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Any(s => s.Id == shipmentId).Should().BeTrue();
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
