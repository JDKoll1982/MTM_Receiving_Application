using FluentAssertions;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using MTM_Receiving_Application.Module_Settings.Core.Validators;

namespace MTM_Receiving_Application.Tests.Module_Settings.Core.Validators;

/// <summary>
/// Unit tests for SetSettingCommandValidator.
/// </summary>
public class SetSettingCommandValidatorTests
{
    private readonly SetSettingCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new SetSettingCommand("System", "Core.Theme", "Dark");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCategoryMissing()
    {
        var command = new SetSettingCommand("", "Core.Theme", "Dark");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
