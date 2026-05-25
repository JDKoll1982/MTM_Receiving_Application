using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public sealed class Service_MySQL_ReceivingAccessTests
{
    [Fact]
    public void ApplyOwnershipFilter_ShouldReturnOnlyOwnedRows_ForStandardUser()
    {
        var result = InvokeApplyOwnershipFilter(
            [
                new Model_ReceivingLoad
                {
                    LoadNumber = 1,
                    EmployeeNumber = 42,
                    UserId = "jkoll",
                },
                new Model_ReceivingLoad
                {
                    LoadNumber = 2,
                    EmployeeNumber = 99,
                    UserId = "other.user",
                },
            ],
            42,
            "jkoll",
            false
        );

        result.Should().ContainSingle();
        result[0].LoadNumber.Should().Be(1);
    }

    [Fact]
    public void ApplyOwnershipFilter_ShouldReturnAllRows_ForFullAccessUser()
    {
        var result = InvokeApplyOwnershipFilter(
            [
                new Model_ReceivingLoad
                {
                    LoadNumber = 1,
                    EmployeeNumber = 42,
                    UserId = "jkoll",
                },
                new Model_ReceivingLoad
                {
                    LoadNumber = 2,
                    EmployeeNumber = 99,
                    UserId = "other.user",
                },
            ],
            42,
            "jkoll",
            true
        );

        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetUnauthorizedLoads_ShouldUseUserIdFallback_WhenEmployeeNumberIsMissing()
    {
        var result = InvokeGetUnauthorizedLoads(
            [
                new Model_ReceivingLoad
                {
                    LoadNumber = 1,
                    EmployeeNumber = 0,
                    UserId = "jkoll",
                },
                new Model_ReceivingLoad
                {
                    LoadNumber = 2,
                    EmployeeNumber = 0,
                    UserId = "other.user",
                },
            ],
            42,
            "jkoll",
            false
        );

        result.Should().ContainSingle();
        result[0].LoadNumber.Should().Be(2);
    }

    private static List<Model_ReceivingLoad> InvokeApplyOwnershipFilter(
        List<Model_ReceivingLoad> loads,
        int employeeNumber,
        string? windowsUsername,
        bool hasFullAccess
    )
    {
        var methodInfo = typeof(Service_MySQL_Receiving).GetMethod(
            "ApplyOwnershipFilter",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(
            null,
            [loads, employeeNumber, windowsUsername, hasFullAccess]
        );

        return result.Should().BeOfType<List<Model_ReceivingLoad>>().Subject;
    }

    private static List<Model_ReceivingLoad> InvokeGetUnauthorizedLoads(
        List<Model_ReceivingLoad> loads,
        int employeeNumber,
        string? windowsUsername,
        bool hasFullAccess
    )
    {
        var methodInfo = typeof(Service_MySQL_Receiving).GetMethod(
            "GetUnauthorizedLoads",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        methodInfo.Should().NotBeNull();

        var result = methodInfo!.Invoke(
            null,
            [loads, employeeNumber, windowsUsername, hasFullAccess]
        );

        return result.Should().BeOfType<List<Model_ReceivingLoad>>().Subject;
    }
}
