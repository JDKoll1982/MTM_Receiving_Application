using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Integration tests for ExportPartsCsvQueryHandler.
/// </summary>
[Collection("Database")]
public class ExportPartsCsvQueryHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public ExportPartsCsvQueryHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldReturnCsvContent()
    {
        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
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

            var handler = new ExportPartsCsvQueryHandler(partDao, componentDao);
            var result = await handler.Handle(new ExportPartsCsvQuery { IncludeInactive = false }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Contain(partNumber);
        }
        finally
        {
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
