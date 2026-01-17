using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Tests.Helpers;
using Xunit;


namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Integration tests for DeactivateVolvoPartCommandHandler.
/// </summary>
[Collection("Database")]
public class DeactivateVolvoPartCommandHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public DeactivateVolvoPartCommandHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task Handle_ShouldDeactivatePart()
    {
        await _fixture.InitializeAsync();
        Skip.If(!_fixture.IsDatabaseReady, _fixture.DatabaseNotReadyReason ?? "Database not ready");

        var partDao = _fixture.CreatePartDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        var insertResult = await partDao.InsertAsync(new Model_VolvoPart
        {
            PartNumber = partNumber,
            QuantityPerSkid = 5,
            IsActive = true
        });
        insertResult.Success.Should().BeTrue();

        var handler = new DeactivateVolvoPartCommandHandler(partDao);
        var result = await handler.Handle(new DeactivateVolvoPartCommand
        {
            PartNumber = partNumber
        }, default);

        result.IsSuccess.Should().BeTrue();

        var fetch = await partDao.GetByIdAsync(partNumber);
        fetch.Data.Should().NotBeNull();
        fetch.Data!.IsActive.Should().BeFalse();
    }
}
