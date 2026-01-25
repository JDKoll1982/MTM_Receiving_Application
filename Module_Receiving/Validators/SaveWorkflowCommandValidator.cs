using System;
using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for SaveWorkflowCommand.
    /// Validates CSV output path and save options.
    /// </summary>
    public class SaveWorkflowCommandValidator : AbstractValidator<Command_ReceivingWizard_Data_SaveAndExportCSV>
    {
        public SaveWorkflowCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required");

            RuleFor(x => x.CsvOutputPath)
                .NotEmpty()
                .WithMessage("CSV output path is required")
                .Must(path => !string.IsNullOrWhiteSpace(path))
                .WithMessage("CSV output path cannot be empty or whitespace");

            // Additional validation: ensure path doesn't contain invalid characters
            // This is a basic check; more sophisticated validation can be added
            RuleFor(x => x.CsvOutputPath)
                .Must(path => !path.Contains("\""))
                .When(x => !string.IsNullOrEmpty(x.CsvOutputPath))
                .WithMessage("CSV output path cannot contain quotes");
        }
    }
}
