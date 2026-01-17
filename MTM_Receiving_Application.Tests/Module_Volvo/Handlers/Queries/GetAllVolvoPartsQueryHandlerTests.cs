using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for GetAllVolvoPartsQueryHandler.
/// </summary>
[Collection("Database")]
public class GetAllVolvoPartsQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public GetAllVolvoPartsQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnParts()
    {
        var partDao = _fixture.CreatePartDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        try
        {
            var insertResult = await partDao.InsertAsync(new Model_VolvoPart
            {
                PartNumber = partNumber,
                QuantityPerSkid = 10,
                IsActive = true
            });
            insertResult.Success.Should().BeTrue();

            var handler = new GetAllVolvoPartsQueryHandler(partDao);
            var result = await handler.Handle(new GetAllVolvoPartsQuery { IncludeInactive = false }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Any(p => p.PartNumber == partNumber).Should().BeTrue();
        }
        finally
        {
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
