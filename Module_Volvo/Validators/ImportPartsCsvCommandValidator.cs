using System.IO;
using FluentValidation;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Validators;

/// <summary>
/// [STUB] Validator for import command.
/// TODO: Update validation for database import operation.
/// </summary>
public class ImportPartsCommandValidator : AbstractValidator<ImportPartsCommand>
{
    public ImportPartsCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .Must(File.Exists).WithMessage("File does not exist");
    }
}
