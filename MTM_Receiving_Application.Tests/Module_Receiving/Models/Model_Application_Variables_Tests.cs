using System;
using System.IO;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_Application_Variables.
    /// Tests default configurations.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_Application_Variables_Tests
    {
        [Fact]
        public void Constructor_Defaults_SetsSensibleValues()
        {
            // Act
            var config = new Model_Application_Variables();

            // Assert
            config.ApplicationName.Should().NotBeEmpty();
            config.Version.Should().MatchRegex(@"\d+\.\d+\.\d+");
            config.ConnectionString.Should().BeEmpty();
            config.EnvironmentType.Should().Be("Development");
        }

        [Fact]
        public void LogDirectory_Default_IsInAppData()
        {
            // Act
            var config = new Model_Application_Variables();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Assert
            config.LogDirectory.Should().StartWith(appData);
            config.LogDirectory.Should().Contain("MTM_Receiving_Application");
        }
    }
}
