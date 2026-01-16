using FluentAssertions;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for AddPartToShipmentCommandValidator
/// </summary>
public class AddPartToShipmentCommandValidatorTests
{
    private readonly AddPartToShipmentCommandValidator _validator;

    public AddPartToShipmentCommandValidatorTests()
    {
        _validator = new AddPartToShipmentCommandValidator();
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 5,
            HasDiscrepancy = false
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartNumberIsEmpty()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "",
            ReceivedSkidCount = 5,
            HasDiscrepancy = false
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PartNumber));
    }

    [Fact]
    public void Validate_ShouldFail_WhenReceivedSkidCountIsZero()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 0,
            HasDiscrepancy = false
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ReceivedSkidCount));
    }

    [Fact]
    public void Validate_ShouldFail_WhenHasDiscrepancyButExpectedSkidCountIsNull()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 5,
            HasDiscrepancy = true,
            ExpectedSkidCount = null,
            DiscrepancyNote = "Some note"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ExpectedSkidCount));
    }

    [Fact]
    public void Validate_ShouldFail_WhenHasDiscrepancyButDiscrepancyNoteIsEmpty()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 5,
            HasDiscrepancy = true,
            ExpectedSkidCount = 10,
            DiscrepancyNote = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.DiscrepancyNote));
    }

    [Fact]
    public void Validate_ShouldPass_WhenHasDiscrepancyAndAllFieldsProvided()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 5,
            HasDiscrepancy = true,
            ExpectedSkidCount = 10,
            DiscrepancyNote = "Discrepancy explanation"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
