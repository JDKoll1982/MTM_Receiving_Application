using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public sealed class Service_ReceivingWorkflowStartModeTests
{
    [Fact]
    public async Task StartWorkflowAsync_ShouldOpenManualEntry_WhenTheStoredDefaultIsManual()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = "manual",
        };
        var (service, userPreferences) = CreateService(currentUser);

        await service.StartWorkflowAsync();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.ManualEntry);
        service.RequestedEditDataSource.Should().Be(Enum_DataSourceType.Memory);
        userPreferences.Verify(
            s => s.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never
        );
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldOpenEditModeAndUseCurrentLabels_WhenTheStoredDefaultIsEdit()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = "edit",
        };
        var (service, _) = CreateService(currentUser);

        await service.StartWorkflowAsync();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.EditMode);
        service.RequestedEditDataSource.Should().Be(Enum_DataSourceType.CurrentLabels);
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldOpenPoEntryAndStoreGuided_WhenNoDefaultIsStored()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = null,
        };
        var (service, userPreferences) = CreateService(currentUser);

        await service.StartWorkflowAsync();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.POEntry);
        userPreferences.Verify(
            s => s.UpdateDefaultReceivingModeAsync("DOMAIN\\user", "guided"),
            Times.Once
        );
        currentUser
            .DefaultReceivingMode.Should()
            .Be("guided", "the fallback choice must be stored so the next launch agrees");
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldOpenPoEntryAndStoreGuided_WhenTheStoredValueIsUnrecognised()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = "wizard",
        };
        var (service, userPreferences) = CreateService(currentUser);

        await service.StartWorkflowAsync();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.POEntry);
        userPreferences.Verify(
            s => s.UpdateDefaultReceivingModeAsync("DOMAIN\\user", "guided"),
            Times.Once
        );
        currentUser.DefaultReceivingMode.Should().Be("guided");
    }

    private static (
        Service_ReceivingWorkflow Service,
        Mock<IService_UserPreferences> UserPreferences
    ) CreateService(Model_User currentUser)
    {
        var session = new Model_UserSession(currentUser);
        var userSessionManager = new Mock<IService_UserSessionManager>();
        userSessionManager.SetupGet(s => s.CurrentSession).Returns(session);

        var userPreferences = new Mock<IService_UserPreferences>();
        userPreferences
            .Setup(s => s.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(s => s.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);

        var service = new Service_ReceivingWorkflow(
            new Mock<IService_SessionManager>().Object,
            new Mock<IService_ReceivingLabelData>().Object,
            new Mock<IService_MySQL_Receiving>().Object,
            new Mock<IService_ReceivingValidation>().Object,
            new Mock<IService_QualityHoldWarning>().Object,
            receivingSettings.Object,
            new Mock<IService_AppSettings>().Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            userSessionManager.Object,
            new Mock<IService_UserPrivileges>().Object,
            userPreferences.Object
        );

        return (service, userPreferences);
    }
}
