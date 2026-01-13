using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_PackageTypePreference.
    /// Tests simple property behavior and default DateTime.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_PackageTypePreference_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var model = new Model_PackageTypePreference();

            // Assert
            model.PartID.Should().BeEmpty();
            model.PackageTypeName.Should().BeEmpty();
            model.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        }
    }
}
