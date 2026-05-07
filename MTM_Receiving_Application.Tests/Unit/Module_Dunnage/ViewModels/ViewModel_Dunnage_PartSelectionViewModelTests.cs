using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_PartSelectionViewModelTests
{
    [Fact]
    public async Task InitializeAsync_ShouldRestoreSavedImageDisplayPreference_WithoutPersistingAgain()
    {
        var dunnageSettings = CreateDunnageSettingsMock(savedPreference: false);
        var sessionManager = CreateSessionManager(42);

        var viewModel = CreateViewModel(
            dunnageSettings: dunnageSettings,
            sessionManager: sessionManager
        );

        await viewModel.InitializeAsync();

        viewModel.IsImageDisplayPreferred.Should().BeFalse();
        dunnageSettings.Verify(
            service =>
                service.GetBoolAsync(DunnageSettingsKeys.UserPreferences.PreferPartImages, 42),
            Times.Once
        );
        dunnageSettings.Verify(
            service =>
                service.SaveStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleDisplayFormatChangedAsync_ShouldPersistUpdatedDisplayPreference()
    {
        var dunnageSettings = CreateDunnageSettingsMock(savedPreference: true);
        var sessionManager = CreateSessionManager(42);

        var viewModel = CreateViewModel(
            dunnageSettings: dunnageSettings,
            sessionManager: sessionManager
        );

        await viewModel.InitializeAsync();
        await viewModel.HandleDisplayFormatChangedAsync(false);

        viewModel.IsImageDisplayPreferred.Should().BeFalse();
        viewModel.StatusMessage.Should().Be("Part selection display set to drop-down");
        dunnageSettings.Verify(
            service =>
                service.SaveStringAsync(
                    DunnageSettingsKeys.UserPreferences.PreferPartImages,
                    "false",
                    42
                ),
            Times.Once
        );
    }

    private static ViewModel_Dunnage_PartSelection CreateViewModel(
        Mock<IService_DunnageSettings>? dunnageSettings = null,
        Mock<IService_UserSessionManager>? sessionManager = null,
        Mock<IService_MySQL_Dunnage>? dunnageService = null,
        Mock<IService_DunnageWorkflow>? workflow = null
    )
    {
        dunnageSettings ??= CreateDunnageSettingsMock(savedPreference: true);
        sessionManager ??= CreateSessionManager(42);
        dunnageService ??= CreateDunnageServiceMock();
        workflow ??= CreateWorkflowMock();

        var dispatcher = new Mock<IService_Dispatcher>();
        dispatcher.Setup(service => service.TryEnqueue(It.IsAny<System.Action>())).Returns(true);

        var userPrivileges = new Mock<IService_UserPrivileges>();
        userPrivileges.SetupGet(service => service.IsInitialized).Returns(true);
        userPrivileges.SetupGet(service => service.CurrentUserId).Returns(42);
        userPrivileges.Setup(service => service.HasAnyRole(It.IsAny<string[]>())).Returns(false);

        return new ViewModel_Dunnage_PartSelection(
            workflow.Object,
            dunnageService.Object,
            dispatcher.Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            userPrivileges.Object,
            sessionManager.Object,
            dunnageSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Mock<IService_DunnageSettings> CreateDunnageSettingsMock(bool savedPreference)
    {
        var dunnageSettings = new Mock<IService_DunnageSettings>();
        dunnageSettings
            .Setup(service => service.GetBoolAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(savedPreference);
        dunnageSettings
            .Setup(service =>
                service.SaveStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>())
            )
            .Returns(Task.CompletedTask);

        return dunnageSettings;
    }

    private static Mock<IService_UserSessionManager> CreateSessionManager(int employeeNumber)
    {
        var sessionManager = new Mock<IService_UserSessionManager>();
        sessionManager
            .SetupGet(service => service.CurrentSession)
            .Returns(
                new Model_UserSession(
                    new Model_User
                    {
                        EmployeeNumber = employeeNumber,
                        WindowsUsername = "DOMAIN\\dunnage",
                    }
                )
            );

        return sessionManager;
    }

    private static Mock<IService_DunnageWorkflow> CreateWorkflowMock()
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow
            .SetupGet(service => service.CurrentSession)
            .Returns(
                new Model_DunnageSession
                {
                    SelectedTypeId = 7,
                    SelectedTypeName = "Bins",
                    SelectedType = new Model_DunnageType
                    {
                        Id = 7,
                        TypeName = "Bins",
                        Icon = "PackageVariantClosed",
                    },
                }
            );

        return workflow;
    }

    private static Mock<IService_MySQL_Dunnage> CreateDunnageServiceMock()
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetPartsByTypeAsync(7))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnagePart>
                    {
                        new()
                        {
                            Id = 1,
                            TypeId = 7,
                            PartId = "BIN-100",
                            QuantityType = "Each",
                        },
                    }
                )
            );

        return dunnageService;
    }
}