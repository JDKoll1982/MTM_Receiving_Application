using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

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

    [Fact]
    public void BuildSavedRowRewriteWarning_ShouldDescribePropagatedPartChanges()
    {
        var message = InvokeSavedRowRewriteWarning(
            "BIN-100",
            "BIN-200",
            "Each",
            "Pallets",
            true,
            "Images/old.png",
            "Images/new.png",
            "RECV",
            "DOCK-4"
        );

        message.Should().Contain("current label data and history rows");
        message.Should().Contain("part number from 'BIN-100' to 'BIN-200'");
        message.Should().Contain("quantity type from 'Each' to 'Pallets'");
        message.Should().Contain("saved spec values");
        message.Should().Contain("default location from 'RECV' to 'DOCK-4'");
    }

    [Fact]
    public void BuildSavedRowRewriteWarning_ShouldReturnNull_WhenNoPropagatedFieldsChange()
    {
        var message = InvokeSavedRowRewriteWarning(
            "BIN-100",
            "BIN-100",
            "Each",
            "Each",
            false,
            "Images/same.png",
            "Images/same.png",
            "RECV",
            "RECV"
        );

        message.Should().BeNull();
    }

    [Fact]
    public void BuildSavedRowRewriteWarning_ShouldIgnoreHomeLocationWhenNewValueIsBlank()
    {
        var message = InvokeSavedRowRewriteWarning(
            "BIN-100",
            "BIN-100",
            "Each",
            "Each",
            false,
            "Images/same.png",
            "Images/same.png",
            "RECV",
            ""
        );

        message.Should().BeNull();
    }

    private static string? InvokeSavedRowRewriteWarning(
        string originalPartId,
        string updatedPartId,
        string originalQuantityType,
        string updatedQuantityType,
        bool specValuesChanged,
        string? originalImagePath,
        string? updatedImagePath,
        string? originalHomeLocation,
        string? updatedHomeLocation
    )
    {
        var method = typeof(ViewModel_Dunnage_PartSelection).GetMethod(
            "BuildSavedRowRewriteWarning",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();

        return method!
            .Invoke(
                null,
                [
                    originalPartId,
                    updatedPartId,
                    originalQuantityType,
                    updatedQuantityType,
                    specValuesChanged,
                    originalImagePath,
                    updatedImagePath,
                    originalHomeLocation,
                    updatedHomeLocation,
                ]
            )
            .As<string?>();
    }

    [Fact]
    public void BuildPartDeleteWarning_ShouldListAffectedCounts()
    {
        var message = InvokePartDeleteWarning(
            "BIN-100",
            new Model_DunnagePartDeleteImpact
            {
                LabelDataCount = 3,
                HistoryCount = 2,
                InventoryCount = 1,
            }
        );

        message.Should().Contain("'BIN-100'");
        message.Should().Contain("3 current label data entries");
        message.Should().Contain("2 history entries");
        message.Should().Contain("1 inventory record");
        message.Should().Contain("All existing label data and history entries");
    }

    [Fact]
    public void BuildPartDeleteWarning_ShouldMentionNoReferences_WhenNoImpact()
    {
        var message = InvokePartDeleteWarning(
            "BIN-100",
            new Model_DunnagePartDeleteImpact()
        );

        message.Should().Contain("No label data or history entries currently reference this part.");
    }

    private static string InvokePartDeleteWarning(
        string partId,
        Model_DunnagePartDeleteImpact impact
    )
    {
        var method = typeof(ViewModel_Dunnage_PartSelection).GetMethod(
            "BuildPartDeleteWarning",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();

        return method!.Invoke(null, [partId, impact]).As<string>();
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
