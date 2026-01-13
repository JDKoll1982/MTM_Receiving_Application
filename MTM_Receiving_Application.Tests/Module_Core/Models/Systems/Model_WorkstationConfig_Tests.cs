using System;
using Xunit;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Tests.Module_Core.Models.Systems
{
    /// <summary>
    /// Unit tests for Model_WorkstationConfig logic
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_WorkstationConfig_Tests
    {
        [Fact]
        public void Constructor_Default_UsesMachineName()
        {
            // Act
            var config = new Model_WorkstationConfig();

            // Assert
            config.ComputerName.Should().Be(Environment.MachineName);
        }

        [Fact]
        public void Constructor_WithParameter_SetsComputerName()
        {
            // Act
            var config = new Model_WorkstationConfig("TEST-PC");

            // Assert
            config.ComputerName.Should().Be("TEST-PC");
        }

        [Fact]
        public void IsSharedTerminal_ReturnsTrueForSharedType()
        {
            // Act
            var config = new Model_WorkstationConfig { WorkstationType = "shared_terminal" };

            // Assert
            config.IsSharedTerminal.Should().BeTrue();
            config.IsPersonalWorkstation.Should().BeFalse();
        }

        [Fact]
        public void IsPersonalWorkstation_ReturnsTrueForPersonalType()
        {
             // Act
            var config = new Model_WorkstationConfig { WorkstationType = "personal_workstation" };

            // Assert
            config.IsPersonalWorkstation.Should().BeTrue();
            config.IsSharedTerminal.Should().BeFalse();
        }

        [Fact]
        public void TimeoutDuration_SharedTerminal_Returns15Minutes()
        {
            // Act
            var config = new Model_WorkstationConfig { WorkstationType = "shared_terminal" };

            // Assert
            config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(15));
        }

        [Fact]
        public void TimeoutDuration_PersonalWorkstation_Returns30Minutes()
        {
            // Act
            var config = new Model_WorkstationConfig { WorkstationType = "personal_workstation" };

            // Assert
            config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(30));
        }

        [Fact]
        public void TimeoutDuration_UnknownType_Returns30MinutesDefault()
        {
            // Act
            var config = new Model_WorkstationConfig { WorkstationType = "unknown" };

            // Assert
            config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(30));
        }
    }
}
