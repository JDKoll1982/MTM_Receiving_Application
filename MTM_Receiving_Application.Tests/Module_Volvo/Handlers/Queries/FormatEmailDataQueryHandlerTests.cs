using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;
using MTM_Receiving_Application.Module_Core.Services.Database;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for FormatEmailDataQueryHandler.
/// </summary>
[Collection("Database")]
public class FormatEmailDataQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public FormatEmailDataQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnEmailData()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        var lineDao = _fixture.CreateShipmentLineDao();
        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
        var logger = new Service_LoggingUtility();

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
                Notes = "TEST-EMAIL"
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

            var handler = new FormatEmailDataQueryHandler(shipmentDao, lineDao, partDao, componentDao, logger);
            var result = await handler.Handle(new FormatEmailDataQuery { ShipmentId = shipmentId }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.RequestedLines.Should().ContainKey(partNumber);
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
