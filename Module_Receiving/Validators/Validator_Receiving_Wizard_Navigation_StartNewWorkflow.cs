using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators
{
    /// <summary>
    /// Validator for StartWorkflowCommand.
    /// Ensures workflow mode is either "Guided" or "Manual".
    /// </summary>
    public class Validator_Receiving_Wizard_Navigation_StartNewWorkflow : AbstractValidator<Command_ReceivingWizard_Navigation_StartNewWorkflow>
    {
        public Validator_Receiving_Wizard_Navigation_StartNewWorkflow()
        {
            RuleFor(x => x.Mode)
                .NotEmpty()
                .WithMessage("Workflow mode is required")
                .Must(m => m == "Guided" || m == "Manual")
                .WithMessage("Mode must be either 'Guided' or 'Manual'");
        }
    }
}
