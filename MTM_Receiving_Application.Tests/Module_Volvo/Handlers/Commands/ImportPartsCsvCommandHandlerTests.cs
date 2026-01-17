using System.IO;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Integration tests for ImportPartsCsvCommandHandler.
/// </summary>
[Collection("Database")]
public class ImportPartsCsvCommandHandlerTests
{
    private readonly DatabaseFixture _fixture;

    public ImportPartsCsvCommandHandlerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ShouldImportPartsFromCsv()
    {
        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        var tempFile = Path.Combine(Path.GetTempPath(), $"volvo_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(tempFile, $"PartNumber,QuantityPerSkid,Components\n{partNumber},10,");

        try
        {
            var handler = new ImportPartsCsvCommandHandler(partDao, componentDao);
            var result = await handler.Handle(new ImportPartsCsvCommand
            {
                CsvFilePath = tempFile
            }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.SuccessCount.Should().BeGreaterThan(0);
        }
        finally
        {
            File.Delete(tempFile);
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
