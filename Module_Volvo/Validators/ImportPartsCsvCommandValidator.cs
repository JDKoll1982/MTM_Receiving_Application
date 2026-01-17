using System.IO;
using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// Validator for ImportPartsCsvCommand.
/// </summary>
public class ImportPartsCsvCommandValidator : AbstractValidator<ImportPartsCsvCommand>
{
    public ImportPartsCsvCommandValidator()
    {
        RuleFor(x => x.CsvFilePath)
            .NotEmpty().WithMessage("CSV file path is required")
            .Must(File.Exists).WithMessage("CSV file does not exist");
    }
}
