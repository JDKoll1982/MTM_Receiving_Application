using System.Collections.Generic;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_InforVisualPO.
    /// Tests calculated properties and default lists.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_InforVisualPO_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var po = new Model_InforVisualPO();

            // Assert
            po.Parts.Should().NotBeNull().And.BeEmpty();
            po.HasParts.Should().BeFalse();
            po.PONumber.Should().BeEmpty();
        }

        [Fact]
        public void HasParts_ReturnsTrueOnlyWhenPartsExist()
        {
            // Arrange
            var po = new Model_InforVisualPO();

            // Act
            po.Parts.Add(new Model_InforVisualPart());

            // Assert
            po.HasParts.Should().BeTrue();
        }

        [Theory]
        [InlineData("R", "Open")]
        [InlineData("O", "Open")]
        [InlineData("C", "Closed")]
        [InlineData("X", "Cancelled")]
        [InlineData("P", "Partially Received")]
        [InlineData("F", "Firm")]
        [InlineData("", "Unknown")]
        [InlineData("Z", "Status: Z")] // Unknown code fallback
        public void StatusDescription_MapsCodesCorrectly(string code, string expected)
        {
            // Arrange
            var po = new Model_InforVisualPO { Status = code };

            // Act
            var desc = po.StatusDescription;

            // Assert
            desc.Should().Be(expected);
        }

        [Theory]
        [InlineData("C", true)]
        [InlineData("X", true)]
        [InlineData("R", false)]
        [InlineData("O", false)]
        [InlineData("P", false)]
        public void IsClosed_ReturnsTrueForClosedOrCancelled(string status, bool expected)
        {
            // Arrange
            var po = new Model_InforVisualPO { Status = status };

            // Act
            var isClosed = po.IsClosed;

            // Assert
            isClosed.Should().Be(expected);
        }
    }
}
