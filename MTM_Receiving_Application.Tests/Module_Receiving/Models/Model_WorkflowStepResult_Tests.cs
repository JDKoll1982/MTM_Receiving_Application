using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_WorkflowStepResult.
    /// Tests property defaults and setting values.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_WorkflowStepResult_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var model = new Model_WorkflowStepResult();

            // Assert
            model.IsSuccess.Should().BeFalse();
            model.ErrorMessage.Should().BeEmpty();
            model.TargetStep.Should().BeNull();
        }

        [Fact]
        public void Properties_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var model = new Model_WorkflowStepResult();
            var step = Enum_DunnageWorkflowStep.Review;

            // Act
            model.IsSuccess = true;
            model.ErrorMessage = "Operation successful";
            model.TargetStep = step;

            // Assert
            model.IsSuccess.Should().BeTrue();
            model.ErrorMessage.Should().Be("Operation successful");
            model.TargetStep.Should().Be(step);
        }
    }
}
