using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;
using Xunit;


namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GetPartComponentsQueryHandler.
/// </summary>
[Collection("Database")]
public class GetPartComponentsQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GetPartComponentsQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task Handle_ShouldReturnComponents()
    {
        await _fixture.InitializeAsync();
        Skip.If(!_fixture.IsDatabaseReady, _fixture.DatabaseNotReadyReason ?? "Database not ready");

        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
        var parentPart = $"TEST-P-{Guid.NewGuid():N}".ToUpperInvariant();
        var componentPart = $"TEST-C-{Guid.NewGuid():N}".ToUpperInvariant();

        try
        {
            var parentResult = await partDao.InsertAsync(new Model_VolvoPart
            {
                PartNumber = parentPart,
                QuantityPerSkid = 10,
                IsActive = true
            });
            parentResult.Success.Should().BeTrue();

            var componentResult = await partDao.InsertAsync(new Model_VolvoPart
            {
                PartNumber = componentPart,
                QuantityPerSkid = 5,
                IsActive = true
            });
            componentResult.Success.Should().BeTrue();

            var insertComponent = await componentDao.InsertAsync(new Model_VolvoPartComponent
            {
                ParentPartNumber = parentPart,
                ComponentPartNumber = componentPart,
                Quantity = 1
            });
            insertComponent.Success.Should().BeTrue();

            var handler = new GetPartComponentsQueryHandler(componentDao);
            var result = await handler.Handle(new GetPartComponentsQuery { PartNumber = parentPart }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Any(c => c.ComponentPartNumber == componentPart).Should().BeTrue();
        }
        finally
        {
            await componentDao.DeleteByParentPartAsync(parentPart);
            await partDao.DeactivateAsync(parentPart);
            await partDao.DeactivateAsync(componentPart);
        }
    }
}
