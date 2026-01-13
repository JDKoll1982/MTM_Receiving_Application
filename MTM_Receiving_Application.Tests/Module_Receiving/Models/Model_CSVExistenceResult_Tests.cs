using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_CSVExistenceResult.
    /// Tests simple property holders.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_CSVExistenceResult_Tests
    {
        [Fact]
        public void Properties_SetAndGet_Works()
        {
            // Arrange
            var result = new Model_CSVExistenceResult();

            // Act
            result.LocalExists = true;
            result.NetworkExists = true;
            result.NetworkAccessible = true;

            // Assert
            result.LocalExists.Should().BeTrue();
            result.NetworkExists.Should().BeTrue();
            result.NetworkAccessible.Should().BeTrue();
        }
    }
}
