using FluentValidation;
using MTM_Receiving_Application.Module_Settings.Core.Services;

namespace MTM_Receiving_Application.Module_Settings.Core.Validators;

/// <summary>
/// Validates SetSettingCommand requests.
/// </summary>
public class SetSettingCommandValidator : AbstractValidator<SetSettingCommand>
{
    public SetSettingCommandValidator()
    {
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Value).NotNull();
    }
}
