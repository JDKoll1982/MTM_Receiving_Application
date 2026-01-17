using System.IO;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GenerateLabelCsvQueryHandler.
/// </summary>
[Collection("Database")]
public class GenerateLabelCsvQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GenerateLabelCsvQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldGenerateCsvFile()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        var lineDao = _fixture.CreateShipmentLineDao();
        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
        var logger = new Service_LoggingUtility();
        var authService = new FakeVolvoAuthorizationService();

        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();
        int shipmentId = 0;
        string? filePath = null;

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
                Notes = "TEST"
            };

            var insertResult = await shipmentDao.InsertAsync(shipment);
            insertResult.Success.Should().BeTrue();
            shipmentId = insertResult.Data.ShipmentId;

            var line = new Model_VolvoShipmentLine
            {
                ShipmentId = shipmentId,
                PartNumber = partNumber,
                QuantityPerSkid = 10,
                ReceivedSkidCount = 2,
                CalculatedPieceCount = 20,
                HasDiscrepancy = false
            };

            var lineResult = await lineDao.InsertAsync(line);
            lineResult.Success.Should().BeTrue();

            var handler = new GenerateLabelCsvQueryHandler(
                shipmentDao,
                lineDao,
                partDao,
                componentDao,
                authService,
                logger);

            var result = await handler.Handle(new GenerateLabelCsvQuery { ShipmentId = shipmentId }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNullOrWhiteSpace();
            filePath = result.Data;
            File.Exists(filePath).Should().BeTrue();
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (shipmentId > 0)
            {
                await shipmentDao.DeleteAsync(shipmentId);
            }
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
