using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for StartWorkflowCommand.
    /// Ensures workflow mode is either "Guided" or "Manual".
    /// </summary>
    public class StartWorkflowCommandValidator : AbstractValidator<StartWorkflowCommand>
    {
        public StartWorkflowCommandValidator()
        {
            RuleFor(x => x.Mode)
                .NotEmpty()
                .WithMessage("Workflow mode is required")
                .Must(m => m == "Guided" || m == "Manual")
                .WithMessage("Mode must be either 'Guided' or 'Manual'");
        }
    }
}
