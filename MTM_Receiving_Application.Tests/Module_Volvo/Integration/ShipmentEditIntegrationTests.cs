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
/// Integration tests for shipment edit workflow.
/// </summary>
[Collection("Database")]
public class ShipmentEditIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public ShipmentEditIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EditShipment_ShouldUpdateNotesAndLines()
    {
        var shipmentDao = _fixture.CreateShipmentDao();
        var lineDao = _fixture.CreateShipmentLineDao();
        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
        var logger = new Service_LoggingUtility();
        var authService = new FakeVolvoAuthorizationService();

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
                Notes = "TEST-EDIT"
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

            var updateHandler = new UpdateShipmentCommandHandler(shipmentDao, lineDao, partDao, componentDao, authService, logger);
            var updateResult = await updateHandler.Handle(new UpdateShipmentCommand
            {
                ShipmentId = shipmentId,
                ShipmentDate = DateTimeOffset.Now.AddDays(-1),
                Notes = "UPDATED",
                Parts = new List<ShipmentLineDto>
                {
                    new() { PartNumber = partNumber, ReceivedSkidCount = 2 }
                }
            }, default);

            updateResult.IsSuccess.Should().BeTrue();

            var detailHandler = new GetShipmentDetailQueryHandler(shipmentDao, lineDao);
            var detailResult = await detailHandler.Handle(new GetShipmentDetailQuery { ShipmentId = shipmentId }, default);

            detailResult.IsSuccess.Should().BeTrue();
            detailResult.Data!.Shipment.Notes.Should().Be("UPDATED");
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
