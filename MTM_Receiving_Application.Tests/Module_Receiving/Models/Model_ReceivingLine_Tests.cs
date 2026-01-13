using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_ReceivingLine.
    /// Tests property defaults and formatting.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_ReceivingLine_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var line = new Model_ReceivingLine();

            // Assert
            line.Date.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            line.LabelNumber.Should().Be(1);
            line.TotalLabels.Should().Be(1);
            line.VendorName.Should().Be("Unknown");
            line.PartID.Should().BeEmpty();
        }

        [Fact]
        public void LabelText_ReturnsFormattedString()
        {
            // Arrange
            var line = new Model_ReceivingLine
            {
                LabelNumber = 2,
                TotalLabels = 5
            };

            // Act
            var text = line.LabelText;

            // Assert
            text.Should().Be("2 / 5");
        }

        [Fact]
        public void LabelText_UpdatesWhenPropertiesChange()
        {
            // Arrange
            var line = new Model_ReceivingLine
            {
                LabelNumber = 1,
                TotalLabels = 1
            };

            // Act
            line.LabelNumber = 3;
            line.TotalLabels = 10;

            // Assert
            line.LabelText.Should().Be("3 / 10");
        }
    }
}
