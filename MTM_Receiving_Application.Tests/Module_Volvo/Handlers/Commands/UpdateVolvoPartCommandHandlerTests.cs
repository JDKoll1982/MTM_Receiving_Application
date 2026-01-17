using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Integration tests for UpdateVolvoPartCommandHandler.
/// </summary>
[Collection("Database")]
public class UpdateVolvoPartCommandHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public UpdateVolvoPartCommandHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldUpdatePart()
    {
        var partDao = _fixture.CreatePartDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        try
        {
            var insertResult = await partDao.InsertAsync(new Model_VolvoPart
            {
                PartNumber = partNumber,
                QuantityPerSkid = 5,
                IsActive = true
            });
            insertResult.Success.Should().BeTrue();

            var handler = new UpdateVolvoPartCommandHandler(partDao);
            var result = await handler.Handle(new UpdateVolvoPartCommand
            {
                PartNumber = partNumber,
                QuantityPerSkid = 12
            }, default);

            result.IsSuccess.Should().BeTrue();

            var fetch = await partDao.GetByIdAsync(partNumber);
            fetch.Data.Should().NotBeNull();
            fetch.Data!.QuantityPerSkid.Should().Be(12);
        }
        finally
        {
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
