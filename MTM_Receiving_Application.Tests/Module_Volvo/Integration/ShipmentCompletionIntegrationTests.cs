using System.IO;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Integration;

/// <summary>
/// Integration tests for completing a shipment end-to-end.
/// </summary>
[Collection("Database")]
public class ShipmentCompletionIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public ShipmentCompletionIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CompleteShipment_Workflow_ShouldPersistAndGenerateCsv()
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

            var completeHandler = new CompleteShipmentCommandHandler(shipmentDao, lineDao, authService);
            var completeResult = await completeHandler.Handle(new CompleteShipmentCommand
            {
                ShipmentDate = DateTimeOffset.Now.AddDays(-1),
                ShipmentNumber = 1,
                Notes = "TEST",
                PONumber = "PO-TEST",
                ReceiverNumber = "RCV-TEST",
                Parts = new List<ShipmentLineDto>
                {
                    new() { PartNumber = partNumber, ReceivedSkidCount = 1 }
                }
            }, default);

            completeResult.IsSuccess.Should().BeTrue();
            shipmentId = completeResult.Data;

            var generateHandler = new GenerateLabelCsvQueryHandler(
                shipmentDao,
                lineDao,
                partDao,
                componentDao,
                authService,
                logger);

            var csvResult = await generateHandler.Handle(new GenerateLabelCsvQuery { ShipmentId = shipmentId }, default);
            csvResult.IsSuccess.Should().BeTrue();
            filePath = csvResult.Data;
            File.Exists(filePath).Should().BeTrue();

            var shipmentResult = await shipmentDao.GetByIdAsync(shipmentId);
            shipmentResult.IsSuccess.Should().BeTrue();
            shipmentResult.Data.Should().NotBeNull();
            shipmentResult.Data!.Status.Should().Contain("Completed", StringComparison.OrdinalIgnoreCase);
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
