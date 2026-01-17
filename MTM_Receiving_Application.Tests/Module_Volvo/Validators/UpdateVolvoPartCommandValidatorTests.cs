using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for UpdateVolvoPartCommandValidator.
/// </summary>
public class UpdateVolvoPartCommandValidatorTests
{
    private readonly UpdateVolvoPartCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateVolvoPartCommand
        {
            PartNumber = "V-EMB-1",
            QuantityPerSkid = 5
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartNumberMissing()
    {
        var command = new UpdateVolvoPartCommand
        {
            PartNumber = "",
            QuantityPerSkid = 5
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
