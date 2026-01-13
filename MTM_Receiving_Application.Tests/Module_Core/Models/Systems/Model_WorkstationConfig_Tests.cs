using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Defaults;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using Xunit;

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
            var config = new Model_WorkstationConfig();

            config.ComputerName.Should().Be(Environment.MachineName);
        }

        [Fact]
        public void Constructor_WithParameter_SetsComputerName()
        {
            var config = new Model_WorkstationConfig("TEST-PC");

            config.ComputerName.Should().Be("TEST-PC");
        }

        [Fact]
        public void IsSharedTerminal_ReturnsTrueForSharedType()
        {
            var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.SharedTerminalWorkstationType };

            config.IsSharedTerminal.Should().BeTrue();
        }

        [Fact]
        public void IsSharedTerminal_ReturnsFalseForSharedType()
        {
            var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.SharedTerminalWorkstationType };

            config.IsPersonalWorkstation.Should().BeFalse();
        }

        [Fact]
        public void IsPersonalWorkstation_ReturnsTrueForPersonalType()
        {
            var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.PersonalWorkstationWorkstationType };

            config.IsPersonalWorkstation.Should().BeTrue();
        }

        [Fact]
        public void IsPersonalWorkstation_ReturnsFalseForPersonalType()
        {
            var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.PersonalWorkstationWorkstationType };

            config.IsSharedTerminal.Should().BeFalse();
        }

        [Fact]
        public void TimeoutDuration_SharedTerminal_ReturnsSharedTimeoutMinutes()
        {
            var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.SharedTerminalWorkstationType };

            config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(WorkstationDefaults.SharedTerminalTimeoutMinutes));
        }

        [Fact]
        public void TimeoutDuration_PersonalWorkstation_ReturnsPersonalTimeoutMinutes()
        {
            var config = new Model_WorkstationConfig { WorkstationType = WorkstationDefaults.PersonalWorkstationWorkstationType };

            config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(WorkstationDefaults.PersonalWorkstationTimeoutMinutes));
        }

        [Fact]
        public void TimeoutDuration_UnknownType_ReturnsPersonalTimeoutMinutesDefault()
        {
            var config = new Model_WorkstationConfig { WorkstationType = "unknown" };

            config.TimeoutDuration.Should().Be(TimeSpan.FromMinutes(WorkstationDefaults.PersonalWorkstationTimeoutMinutes));
        }
    }
}
