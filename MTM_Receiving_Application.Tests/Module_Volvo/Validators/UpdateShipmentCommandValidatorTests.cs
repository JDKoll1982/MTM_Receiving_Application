using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for UpdateShipmentCommandValidator.
/// </summary>
public class UpdateShipmentCommandValidatorTests
{
    private readonly UpdateShipmentCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateShipmentCommand
        {
            ShipmentId = 1,
            ShipmentDate = DateTimeOffset.Now.AddDays(-1),
            Parts = new List<ShipmentLineDto>
            {
                new() { PartNumber = "V-EMB-1", ReceivedSkidCount = 1 }
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenShipmentIdInvalid()
    {
        var command = new UpdateShipmentCommand
        {
            ShipmentId = 0,
            ShipmentDate = DateTimeOffset.Now.AddDays(-1),
            Parts = new List<ShipmentLineDto>
            {
                new() { PartNumber = "V-EMB-1", ReceivedSkidCount = 1 }
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
