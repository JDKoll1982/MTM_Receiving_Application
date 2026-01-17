using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Integration tests for AddVolvoPartCommandHandler.
/// </summary>
[Collection("Database")]
public class AddVolvoPartCommandHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public AddVolvoPartCommandHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldInsertPart()
    {
        var partDao = _fixture.CreatePartDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        try
        {
            var handler = new AddVolvoPartCommandHandler(partDao);
            var result = await handler.Handle(new AddVolvoPartCommand
            {
                PartNumber = partNumber,
                QuantityPerSkid = 10
            }, default);

            result.IsSuccess.Should().BeTrue();

            var fetch = await partDao.GetByIdAsync(partNumber);
            fetch.IsSuccess.Should().BeTrue();
            fetch.Data.Should().NotBeNull();
        }
        finally
        {
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
