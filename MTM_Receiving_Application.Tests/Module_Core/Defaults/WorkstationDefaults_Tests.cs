using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Defaults;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Defaults;

public class WorkstationDefaults_Tests
{
    [Fact]
    public void SharedTerminalWorkstationType_ShouldNotBeNullOrWhiteSpace()
    {
        WorkstationDefaults.SharedTerminalWorkstationType.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PersonalWorkstationWorkstationType_ShouldNotBeNullOrWhiteSpace()
    {
        WorkstationDefaults.PersonalWorkstationWorkstationType.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SharedTerminalTimeoutMinutes_ShouldBePositive()
    {
        WorkstationDefaults.SharedTerminalTimeoutMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PersonalWorkstationTimeoutMinutes_ShouldBePositive()
    {
        WorkstationDefaults.PersonalWorkstationTimeoutMinutes.Should().BeGreaterThan(0);
    }
}
