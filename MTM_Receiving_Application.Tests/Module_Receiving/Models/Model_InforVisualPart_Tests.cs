using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_InforVisualPart.
    /// Tests display text logic.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_InforVisualPart_Tests
    {
        [Fact]
        public void DisplayText_FormatsCorrectly()
        {
            // Arrange
            var part = new Model_InforVisualPart
            {
                PartID = "PART-01",
                Description = "A Widget",
                POLineNumber = "001"
            };

            // Act
            var text = part.DisplayText;

            // Assert
            text.Should().Be("PART-01 - A Widget (Line 001)");
        }

        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var part = new Model_InforVisualPart();

            // Assert
            part.UnitOfMeasure.Should().Be("EA");
            part.QtyOrdered.Should().Be(0);
        }
    }
}
