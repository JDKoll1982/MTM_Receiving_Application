using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for AddVolvoPartCommandValidator.
/// </summary>
public class AddVolvoPartCommandValidatorTests
{
    private readonly AddVolvoPartCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new AddVolvoPartCommand
        {
            PartNumber = "V-EMB-1",
            QuantityPerSkid = 10
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenQuantityInvalid()
    {
        var command = new AddVolvoPartCommand
        {
            PartNumber = "V-EMB-1",
            QuantityPerSkid = 0
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
