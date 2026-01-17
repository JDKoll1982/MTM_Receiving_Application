using System.IO;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Tests.Helpers;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Integration;

/// <summary>
/// Integration tests for CSV import workflow with validation errors.
/// </summary>
[Collection("Database")]
public class ImportCsvIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public ImportCsvIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ImportCsv_ShouldReportFailures()
    {
        var partDao = _fixture.CreatePartDao();
        var componentDao = _fixture.CreatePartComponentDao();
        var partNumber = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        var tempFile = Path.Combine(Path.GetTempPath(), $"volvo_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(tempFile, $"PartNumber,QuantityPerSkid,Components\n{partNumber},10,\nINVALID,,");

        try
        {
            var handler = new ImportPartsCsvCommandHandler(partDao, componentDao);
            var result = await handler.Handle(new ImportPartsCsvCommand
            {
                CsvFilePath = tempFile
            }, default);

            result.IsSuccess.Should().BeTrue();
            result.Data!.FailureCount.Should().BeGreaterThan(0);
        }
        finally
        {
            File.Delete(tempFile);
            await partDao.DeactivateAsync(partNumber);
        }
    }
}
