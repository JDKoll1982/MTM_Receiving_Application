using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_CSVDeleteResult.
    /// Tests simple property behavior.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_CSVDeleteResult_Tests
    {
        [Fact]
        public void Properties_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var result = new Model_CSVDeleteResult();

            // Act
            result.LocalDeleted = true;
            result.NetworkDeleted = false;
            result.LocalError = null;
            result.NetworkError = "Access Denied";

            // Assert
            result.LocalDeleted.Should().BeTrue();
            result.NetworkDeleted.Should().BeFalse();
            result.LocalError.Should().BeNull();
            result.NetworkError.Should().Be("Access Denied");
        }
    }
}
