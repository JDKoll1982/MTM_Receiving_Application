using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Scanner.Services;
using MTM_Receiving_Application.Module_Scanner.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Services;

public sealed class Service_ScannerAccessPolicyTests
{
    [Fact]
    public async Task IsUserAllowedAsync_ShouldAllowDeveloper_WhenAllowListIsEmpty()
    {
        var service = CreateService(allowedEmployeeNumbers: "[]");

        var isAllowed = await service.IsUserAllowedAsync(
            new Model_User
            {
                EmployeeNumber = 42,
                WindowsUsername = "someone",
                FullName = "Some One",
                Department = Helper_ScannerAccess.DeveloperUserType,
            }
        );

        isAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserAllowedAsync_ShouldDenyRegularUser_WhenAllowListIsEmpty()
    {
        var service = CreateService(allowedEmployeeNumbers: "[]");

        var isAllowed = await service.IsUserAllowedAsync(
            new Model_User
            {
                EmployeeNumber = 43,
                WindowsUsername = "operator",
                FullName = "Casey Operator",
                Department = "Receiving",
            }
        );

        isAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserAllowedAsync_ShouldAllowListedEmployee_WhenAllowListIsConfigured()
    {
        var service = CreateService(allowedEmployeeNumbers: "[7,9]");

        var isAllowed = await service.IsUserAllowedAsync(
            new Model_User { EmployeeNumber = 9, WindowsUsername = "operator" }
        );

        isAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserAllowedAsync_ShouldDenyAdmin_WhenAllowListDoesNotContainAdmin()
    {
        var service = CreateService(allowedEmployeeNumbers: "[7]");

        var isAllowed = await service.IsUserAllowedAsync(
            new Model_User
            {
                EmployeeNumber = 5,
                WindowsUsername = "admin",
                Department = Helper_ScannerAccess.DeveloperUserType,
            }
        );

        isAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserAllowedAsync_ShouldDeny_WhenAllowListExistsAndNoUserResolved()
    {
        var service = CreateService(allowedEmployeeNumbers: "[7]");

        var isAllowed = await service.IsUserAllowedAsync(null);

        isAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserAllowedAsync_ShouldFallBackToDeveloperRule_WhenSettingReadFails()
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync(
                    ScannerSettingsKeys.Category,
                    ScannerSettingsKeys.Access.AllowedEmployeeNumbers,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_SettingsValue>("db offline"));

        var service = new Service_ScannerAccessPolicy(
            settingsCore.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var isAllowed = await service.IsUserAllowedAsync(
            new Model_User
            {
                EmployeeNumber = 42,
                WindowsUsername = "someone",
                Department = Helper_ScannerAccess.DeveloperUserType,
            }
        );

        isAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAllowedEmployeeNumbersAsync_ShouldPersistDistinctSortedArray()
    {
        string? capturedValue = null;
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.SetSettingAsync(
                    ScannerSettingsKeys.Category,
                    ScannerSettingsKeys.Access.AllowedEmployeeNumbers,
                    It.IsAny<string>(),
                    It.IsAny<int?>()
                )
            )
            .Callback<string, string, string, int?>(
                (_, _, value, _) => capturedValue = value
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var service = new Service_ScannerAccessPolicy(
            settingsCore.Object,
            new Mock<IService_LoggingUtility>().Object
        );

        var result = await service.SaveAllowedEmployeeNumbersAsync([9, 7, 7, 0, -3]);

        result.Success.Should().BeTrue();
        capturedValue.Should().Be("[7,9]");
    }

    private static Service_ScannerAccessPolicy CreateService(string allowedEmployeeNumbers)
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(service =>
                service.GetSettingAsync(
                    ScannerSettingsKeys.Category,
                    ScannerSettingsKeys.Access.AllowedEmployeeNumbers,
                    It.IsAny<int?>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_SettingsValue { Value = allowedEmployeeNumbers }
                )
            );

        return new Service_ScannerAccessPolicy(
            settingsCore.Object,
            new Mock<IService_LoggingUtility>().Object
        );
    }
}
