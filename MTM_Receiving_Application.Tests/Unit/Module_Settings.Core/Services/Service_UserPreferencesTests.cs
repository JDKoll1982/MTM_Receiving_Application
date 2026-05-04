using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

public sealed class Service_UserPreferencesTests
{
    [Fact]
    public async Task GetLatestUserPreferenceAsync_ShouldPreserveMissingModuleDefaults()
    {
        var userDao = new Mock<Dao_User>(
            "Server=172.16.1.104;Database=test;",
            new Mock<IService_AuthCredentialProtection>().Object
        );
        userDao
            .Setup(dao => dao.GetUserByWindowsUsernameAsync("DOMAIN\\user"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_User
                    {
                        EmployeeNumber = 6229,
                        WindowsUsername = "DOMAIN\\user",
                        DefaultReceivingMode = " ",
                        DefaultDunnageMode = null,
                    }
                )
            );

        var service = CreateService(userDao.Object);

        var result = await service.GetLatestUserPreferenceAsync("DOMAIN\\user");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DefaultReceivingMode.Should().BeNull();
        result.Data.DefaultDunnageMode.Should().BeNull();
    }

    [Fact]
    public async Task UpdateDefaultDunnageModeAsync_ShouldNormalizeModeAndPassUserIdToDao()
    {
        var userDao = new Mock<Dao_User>(
            "Server=172.16.1.104;Database=test;",
            new Mock<IService_AuthCredentialProtection>().Object
        );
        userDao
            .Setup(dao => dao.GetUserByWindowsUsernameAsync("DOMAIN\\dunnage"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_User { EmployeeNumber = 6227, WindowsUsername = "DOMAIN\\dunnage" }
                )
            );
        userDao
            .Setup(dao => dao.UpdateDefaultDunnageModeAsync(6227, "image-search"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var service = CreateService(userDao.Object);

        var result = await service.UpdateDefaultDunnageModeAsync(
            "DOMAIN\\dunnage",
            " Image-Search "
        );

        result.IsSuccess.Should().BeTrue();
        userDao.Verify(dao => dao.UpdateDefaultDunnageModeAsync(6227, "image-search"), Times.Once);
    }

    [Fact]
    public async Task UpdateDefaultDunnageModeAsync_ShouldSucceedWithoutUpdating_WhenUserIsMissing()
    {
        var userDao = new Mock<Dao_User>(
            "Server=172.16.1.104;Database=test;",
            new Mock<IService_AuthCredentialProtection>().Object
        );
        userDao
            .Setup(dao => dao.GetUserByWindowsUsernameAsync("DOMAIN\\missing"))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_User>("User not found"));

        var service = CreateService(userDao.Object);

        var result = await service.UpdateDefaultDunnageModeAsync("DOMAIN\\missing", "guided");

        result.IsSuccess.Should().BeTrue();
        userDao.Verify(
            dao => dao.UpdateDefaultDunnageModeAsync(It.IsAny<int>(), It.IsAny<string?>()),
            Times.Never
        );
    }

    private static Service_UserPreferences CreateService(Dao_User userDao)
    {
        return new Service_UserPreferences(
            userDao,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object
        );
    }
}
