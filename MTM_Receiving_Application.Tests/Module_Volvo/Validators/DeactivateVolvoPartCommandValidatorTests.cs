using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for DeactivateVolvoPartCommandValidator.
/// </summary>
public class DeactivateVolvoPartCommandValidatorTests
{
    private readonly DeactivateVolvoPartCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new DeactivateVolvoPartCommand
        {
            PartNumber = "V-EMB-1"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartNumberMissing()
    {
        var command = new DeactivateVolvoPartCommand
        {
            PartNumber = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
