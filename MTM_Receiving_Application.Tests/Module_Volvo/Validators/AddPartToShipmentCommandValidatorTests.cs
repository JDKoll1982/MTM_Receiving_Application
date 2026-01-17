using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for AddPartToShipmentCommandValidator.
/// </summary>
public class AddPartToShipmentCommandValidatorTests
{
    private readonly AddPartToShipmentCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-1",
            ReceivedSkidCount = 1,
            HasDiscrepancy = false
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartNumberMissing()
    {
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "",
            ReceivedSkidCount = 1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDiscrepancyFieldsMissing()
    {
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-1",
            ReceivedSkidCount = 1,
            HasDiscrepancy = true,
            ExpectedSkidCount = null,
            DiscrepancyNote = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
