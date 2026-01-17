using System.IO;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for ImportPartsCsvCommandValidator.
/// </summary>
public class ImportPartsCsvCommandValidatorTests
{
    private readonly ImportPartsCsvCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenFileExists()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var command = new ImportPartsCsvCommand
            {
                CsvFilePath = tempFile
            };

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Validate_ShouldFail_WhenFileMissing()
    {
        var command = new ImportPartsCsvCommand
        {
            CsvFilePath = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.csv")
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
