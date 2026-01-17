using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GetInitialShipmentDataQueryHandler.
/// </summary>
[Collection("Database")]
public class GetInitialShipmentDataQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GetInitialShipmentDataQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnNextShipmentNumber()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        var handler = new GetInitialShipmentDataQueryHandler(shipmentDao);

        var result = await handler.Handle(new GetInitialShipmentDataQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.NextShipmentNumber.Should().BeGreaterThan(0);
    }
}
