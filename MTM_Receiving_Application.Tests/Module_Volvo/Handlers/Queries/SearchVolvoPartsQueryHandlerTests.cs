using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for SearchVolvoPartsQueryHandler.
/// </summary>
[Collection("Database")]
public class SearchVolvoPartsQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public SearchVolvoPartsQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnMatchingParts()
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

            var handler = new SearchVolvoPartsQueryHandler(partDao);
            var result = await handler.Handle(new SearchVolvoPartsQuery
            {
                SearchText = partNumber.Substring(0, 6),
                MaxResults = 10
            }, default);

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
