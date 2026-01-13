using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_UserPreference.
    /// Tests ObservableObject properties.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_UserPreference_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var prefs = new Model_UserPreference();

            // Assert
            prefs.Username.Should().BeEmpty();
            prefs.PreferredPackageType.Should().Be("Package");
            prefs.Workstation.Should().BeEmpty();
        }

        [Fact]
        public void Properties_NotifyChanges()
        {
            // Arrange
            var prefs = new Model_UserPreference();
            var propertyChanged = false;
            prefs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Model_UserPreference.Username))
                    propertyChanged = true;
            };

            // Act
            prefs.Username = "NewUser";

            // Assert
            propertyChanged.Should().BeTrue();
            prefs.Username.Should().Be("NewUser");
        }
    }
}
