using System.Collections.Generic;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_ReceivingWorkflowStepResult.
    /// Tests factory methods and properties.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_ReceivingWorkflowStepResult_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var result = new Model_ReceivingWorkflowStepResult();

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().BeEmpty();
            result.ValidationErrors.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void SuccessResult_Factory_CreatesSuccessObject()
        {
            // Act
            var step = Enum_ReceivingWorkflowStep.Review;
            var message = "Success";
            var result = Model_ReceivingWorkflowStepResult.SuccessResult(step, message);

            // Assert
            result.Success.Should().BeTrue();
            result.NewStep.Should().Be(step);
            result.Message.Should().Be(message);
        }

        [Fact]
        public void ErrorResult_Factory_CreatesErrorObject()
        {
            // Act
            var errors = new List<string> { "Error 1", "Error 2" };
            var result = Model_ReceivingWorkflowStepResult.ErrorResult(errors);

            // Assert
            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().BeEquivalentTo(errors);
        }
    }
}
