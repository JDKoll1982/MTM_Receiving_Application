using System.Collections.Generic;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_SaveResult.
    /// Tests success calculation logic and property behavior.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_SaveResult_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var result = new Model_SaveResult();

            // Assert
            result.Success.Should().BeFalse();
            result.LoadsSaved.Should().Be(0);
            result.LocalCSVSuccess.Should().BeFalse();
            result.NetworkCSVSuccess.Should().BeFalse();
            result.DatabaseSuccess.Should().BeFalse();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void IsFullSuccess_WhenAllTrue_ReturnsTrue()
        {
            // Arrange
            var result = new Model_SaveResult
            {
                LocalCSVSuccess = true,
                NetworkCSVSuccess = true,
                DatabaseSuccess = true
            };

            // Assert
            result.IsFullSuccess.Should().BeTrue();
            result.IsPartialSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(false, false, false)]
        public void IsFullSuccess_WhenAnyFalse_ReturnsFalse(bool local, bool network, bool db)
        {
            // Arrange
            var result = new Model_SaveResult
            {
                LocalCSVSuccess = local,
                NetworkCSVSuccess = network,
                DatabaseSuccess = db
            };

            // Assert
            result.IsFullSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, false, false)] // Only Local
        [InlineData(false, false, true)] // Only DB
        [InlineData(true, false, true)] // Local + DB
        public void IsPartialSuccess_WhenMixedSuccessMatchesCriteria_ReturnsTrue(bool local, bool network, bool db)
        {
            // Arrange
            var result = new Model_SaveResult
            {
                LocalCSVSuccess = local,
                NetworkCSVSuccess = network,
                DatabaseSuccess = db
            };

            // Assert
            result.IsPartialSuccess.Should().BeTrue();
        }

        [Fact]
        public void IsPartialSuccess_WhenFullSuccess_ReturnsFalse()
        {
            // Arrange
            var result = new Model_SaveResult
            {
                LocalCSVSuccess = true,
                NetworkCSVSuccess = true,
                DatabaseSuccess = true
            };

            // Assert
            result.IsPartialSuccess.Should().BeFalse();
        }

        [Fact]
        public void IsPartialSuccess_WhenNoSuccess_ReturnsFalse()
        {
            // Arrange
            var result = new Model_SaveResult
            {
                LocalCSVSuccess = false,
                NetworkCSVSuccess = false,
                DatabaseSuccess = false
            };

            // Assert
            result.IsPartialSuccess.Should().BeFalse();
        }

        [Fact]
        public void LegacyProperties_MapToNewProperties()
        {
            // Arrange
            var result = new Model_SaveResult();

            // Act
            result.RecordsSaved = 10;
            result.IsSuccess = true;

            // Assert
            result.LoadsSaved.Should().Be(10);
            result.Success.Should().BeTrue();
        }
    }
}
