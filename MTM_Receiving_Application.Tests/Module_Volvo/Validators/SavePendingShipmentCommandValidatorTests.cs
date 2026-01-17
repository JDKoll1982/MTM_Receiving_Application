using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for SavePendingShipmentCommandValidator.
/// </summary>
public class SavePendingShipmentCommandValidatorTests
{
    private readonly SavePendingShipmentCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now.AddDays(-1),
            ShipmentNumber = 1,
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new() { PartNumber = "V-EMB-1", ReceivedSkidCount = 1 }
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartsMissing()
    {
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now.AddDays(-1),
            ShipmentNumber = 1,
            Notes = "",
            Parts = new List<ShipmentLineDto>()
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
