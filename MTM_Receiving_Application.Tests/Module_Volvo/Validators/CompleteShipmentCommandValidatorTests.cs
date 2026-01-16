using FluentAssertions;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Validators;
using MTM_Receiving_Application.Module_Volvo.Requests;
using Xunit;
using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Validators;

/// <summary>
/// Unit tests for CompleteShipmentCommandValidator
/// </summary>
public class CompleteShipmentCommandValidatorTests
{
    private readonly CompleteShipmentCommandValidator _validator;

    public CompleteShipmentCommandValidatorTests()
    {
        _validator = new CompleteShipmentCommandValidator();
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now.AddDays(-1),
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
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
    public void Validate_ShouldFail_WhenPONumberIsEmpty()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "", // Required field
            ReceiverNumber = "RCV-67890",
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PONumber));
    }

    [Fact]
    public void Validate_ShouldFail_WhenReceiverNumberIsEmpty()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "", // Required field
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ReceiverNumber));
    }

    [Fact]
    public void Validate_ShouldFail_WhenShipmentDateIsInFuture()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now.AddDays(1), // Future date
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
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
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
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
    public void Validate_ShouldFail_WhenPartNumberIsEmpty()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
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
    public void Validate_ShouldPass_WhenPartHasDiscrepancyWithAllRequiredFields()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto
                {
                    PartNumber = "V-EMB-500",
                    ReceivedSkidCount = 5,
                    HasDiscrepancy = true,
                    ExpectedSkidCount = 10,
                    DiscrepancyNote = "Shortage explanation"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPartHasDiscrepancyButMissingExpectedSkidCount()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto
                {
                    PartNumber = "V-EMB-500",
                    ReceivedSkidCount = 5,
                    HasDiscrepancy = true,
                    ExpectedSkidCount = null, // Required when HasDiscrepancy = true
                    DiscrepancyNote = "Shortage explanation"
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ExpectedSkidCount"));
    }
}
