using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_ReceivingValidationResult.
    /// Tests factory methods and default states.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_ReceivingValidationResult_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreSafe()
        {
            // Act
            var result = new Model_ReceivingValidationResult();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(Enum_ValidationSeverity.Error);
            result.Message.Should().BeEmpty();
            result.Errors.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Success_Factory_ReturnsValidObject()
        {
            // Act
            var result = Model_ReceivingValidationResult.Success();

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().BeEmpty();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Error_Factory_ReturnsErrorObject()
        {
            // Act
            var msg = "Some error";
            var result = Model_ReceivingValidationResult.Error(msg);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Severity.Should().Be(Enum_ValidationSeverity.Error);
            result.Message.Should().Be(msg);
            result.Errors.Should().Contain(msg);
        }

        [Fact]
        public void Warning_Factory_ReturnsWarningObject()
        {
            // Act
            var msg = "Pay attention";
            var result = Model_ReceivingValidationResult.Warning(msg);

            // Assert
            result.IsValid.Should().BeTrue("Warnings should not block validity");
            result.Severity.Should().Be(Enum_ValidationSeverity.Warning);
            result.Message.Should().Be(msg);
        }
    }
}
