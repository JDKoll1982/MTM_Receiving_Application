using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_CSVWriteResult.
    /// Tests success calculation logic.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_CSVWriteResult_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var result = new Model_CSVWriteResult();

            // Assert
            result.LocalSuccess.Should().BeFalse();
            result.NetworkSuccess.Should().BeFalse();
            result.ErrorMessage.Should().BeEmpty();
        }

        [Fact]
        public void IsFullSuccess_ReturnsTrueOnlyIfBothSucceed()
        {
            // Arrange
            var result = new Model_CSVWriteResult
            {
                LocalSuccess = true,
                NetworkSuccess = true
            };

            // Assert
            result.IsFullSuccess.Should().BeTrue();
            result.IsPartialSuccess.Should().BeFalse();
            result.IsFailure.Should().BeFalse();
        }

        [Fact]
        public void IsPartialSuccess_ReturnsTrueOnlyIfLocalSucceedsButNetworkFails()
        {
            // Arrange
            var result = new Model_CSVWriteResult
            {
                LocalSuccess = true,
                NetworkSuccess = false
            };

            // Assert
            result.IsFullSuccess.Should().BeFalse();
            result.IsPartialSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
        }

        [Fact]
        public void IsFailure_ReturnsTrueIfLocalFails()
        {
            // Arrange
            var result = new Model_CSVWriteResult
            {
                LocalSuccess = false,
                NetworkSuccess = true // Weird case, but if local fails it's considered failure per logic
            };

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
