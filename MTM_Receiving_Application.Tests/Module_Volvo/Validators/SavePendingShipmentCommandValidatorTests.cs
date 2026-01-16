using FluentAssertions;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;
using MTM_Receiving_Application.Module_Volvo.Requests;
using Xunit;
using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for SavePendingShipmentCommandValidator
/// </summary>
public class SavePendingShipmentCommandValidatorTests
{
    private readonly SavePendingShipmentCommandValidator _validator;

    public SavePendingShipmentCommandValidatorTests()
    {
        _validator = new SavePendingShipmentCommandValidator();
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now.AddDays(-1),
            ShipmentNumber = 100,
            Notes = "Test shipment",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto
                {
                    PartNumber = "V-EMB-500",
                    ReceivedSkidCount = 5,
                    HasDiscrepancy = false
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldFail_WhenShipmentDateIsInFuture()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now.AddDays(1), // Future date
            ShipmentNumber = 100,
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ShipmentDate));
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartsListIsEmpty()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "",
            Parts = new List<ShipmentLineDto>() // Empty list
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Parts));
    }

    [Fact]
    public void Validate_ShouldPass_WhenNotesAreEmpty()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "", // Empty notes are allowed
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartNumberIsEmpty()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto
                {
                    PartNumber = "", // Empty part number
                    ReceivedSkidCount = 5,
                    HasDiscrepancy = false
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PartNumber"));
    }

    [Fact]
    public void Validate_ShouldFail_WhenReceivedSkidCountIsZero()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto
                {
                    PartNumber = "V-EMB-500",
                    ReceivedSkidCount = 0, // Invalid count
                    HasDiscrepancy = false
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ReceivedSkidCount"));
    }
}
