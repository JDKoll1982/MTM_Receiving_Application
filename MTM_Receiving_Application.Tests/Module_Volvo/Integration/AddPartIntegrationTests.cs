using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Integration;

/// <summary>
/// Integration tests for add part workflow.
/// </summary>
[Collection("Database")]
public class AddPartIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public AddPartIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task AddPart_ShouldAppearInList()
    {
        await _fixture.InitializeAsync();
        Skip.If(!_fixture.IsDatabaseReady, _fixture.DatabaseNotReadyReason ?? "Database not ready");

        var partDao = _fixture.CreatePartDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        try
        {
            var addHandler = new AddVolvoPartCommandHandler(partDao);
            var addResult = await addHandler.Handle(new AddVolvoPartCommand
            {
                PartNumber = partNumber,
                QuantityPerSkid = 10
            }, default);

            addResult.IsSuccess.Should().BeTrue();

            var listHandler = new GetAllVolvoPartsQueryHandler(partDao);
            var listResult = await listHandler.Handle(new GetAllVolvoPartsQuery { IncludeInactive = false }, default);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Data!.Any(p => p.PartNumber == partNumber).Should().BeTrue();
        }
        finally
        {
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
