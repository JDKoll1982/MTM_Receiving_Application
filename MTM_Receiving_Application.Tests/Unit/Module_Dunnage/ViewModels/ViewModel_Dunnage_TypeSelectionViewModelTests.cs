using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_TypeSelectionViewModelTests
{
    [Fact]
    public async Task InitializeAsync_ShouldRestoreSavedSortPreference_WithoutPersistingItAgain()
    {
        var dunnageService = CreateDunnageServiceMock(CreateTypes(), CreateLoads());
        var dunnageSettings = CreateSettingsMock("Recently Updated");
        var sessionManager = CreateSessionManager(42);

        var viewModel = CreateViewModel(
            dunnageService: dunnageService,
            dunnageSettings: dunnageSettings,
            sessionManager: sessionManager
        );

        await viewModel.InitializeAsync();

        viewModel.SelectedSortOption.Should().Be("Recently Updated");
        viewModel
            .DisplayedTypes.Select(type => type.TypeName)
            .Should()
            .Equal("Charlie", "Delta", "Alpha", "Bravo");
        dunnageSettings.Verify(
            service => service.GetStringAsync(It.IsAny<string>(), 42),
            Times.Once
        );
        dunnageSettings.Verify(
            service =>
                service.SaveStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ChangingSortOption_ShouldPersistSelectionAndReorderDisplayedTypes()
    {
        var dunnageService = CreateDunnageServiceMock(CreateTypes(), CreateLoads());
        var dunnageSettings = CreateSettingsMock("Name (A-Z)");
        var sessionManager = CreateSessionManager(42);

        var viewModel = CreateViewModel(
            dunnageService: dunnageService,
            dunnageSettings: dunnageSettings,
            sessionManager: sessionManager
        );

        await viewModel.InitializeAsync();

        viewModel.SelectedSortOption = "Times Used";

        viewModel
            .DisplayedTypes.Select(type => type.TypeName)
            .Should()
            .Equal("Bravo", "Alpha", "Charlie", "Delta");
        dunnageSettings.Verify(
            service =>
                service.SaveStringAsync(
                    "Dunnage.UserPreferences.TypeSelectionSort",
                    "Times Used",
                    42
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("Name (A-Z)", new[] { "Alpha", "Bravo", "Charlie", "Delta" })]
    [InlineData("Times Used", new[] { "Bravo", "Alpha", "Charlie", "Delta" })]
    [InlineData("Last Used", new[] { "Bravo", "Charlie", "Alpha", "Delta" })]
    [InlineData("Newest Added", new[] { "Delta", "Bravo", "Charlie", "Alpha" })]
    [InlineData("Recently Updated", new[] { "Charlie", "Delta", "Alpha", "Bravo" })]
    public async Task SortOptions_ShouldReturnExpectedTypeOrder(
        string sortOption,
        string[] expectedOrder
    )
    {
        var dunnageService = CreateDunnageServiceMock(CreateTypes(), CreateLoads());
        var viewModel = CreateViewModel(
            dunnageService: dunnageService,
            dunnageSettings: CreateSettingsMock("Name (A-Z)"),
            sessionManager: CreateSessionManager(42)
        );

        await viewModel.InitializeAsync();

        viewModel.SelectedSortOption = sortOption;

        viewModel.DisplayedTypes.Select(type => type.TypeName).Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task ChangingSortOption_ShouldPersistImmediately_WithoutAsyncDelay()
    {
        var dunnageService = CreateDunnageServiceMock(CreateTypes(), CreateLoads());
        var dunnageSettings = CreateSettingsMock("Name (A-Z)");

        var viewModel = CreateViewModel(
            dunnageService: dunnageService,
            dunnageSettings: dunnageSettings,
            sessionManager: CreateSessionManager(42)
        );

        await viewModel.InitializeAsync();

        viewModel.SelectedSortOption = "Last Used";

        dunnageSettings.Verify(
            service =>
                service.SaveStringAsync(
                    "Dunnage.UserPreferences.TypeSelectionSort",
                    "Last Used",
                    42
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SelectTypeAsync_ShouldUpdateWorkflowSessionAndNavigate()
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        var session = new Model_DunnageSession();

        var viewModel = CreateViewModel(workflow: workflow, workflowSession: session);
        var type = new Model_DunnageType { Id = 7, TypeName = "Totes" };

        await viewModel.SelectTypeCommand.ExecuteAsync(type);

        session.SelectedType.Should().Be(type);
        session.SelectedTypeId.Should().Be(7);
        session.SelectedTypeName.Should().Be("Totes");
        workflow.Verify(
            service => service.GoToStep(Enum_DunnageWorkflowStep.PartSelection),
            Times.Once
        );
    }

    [Fact]
    public void BuildSavedRowRewriteWarning_ShouldDescribeNameAndIconChanges()
    {
        var message = InvokeSavedRowRewriteWarning(
            "Bins",
            "Totes",
            "PackageVariantClosed",
            "Archive"
        );

        message.Should().Contain("current label data and history rows");
        message.Should().Contain("type name from 'Bins' to 'Totes'");
        message.Should().Contain("type icon from 'PackageVariantClosed' to 'Archive'");
    }

    [Fact]
    public void BuildSavedRowRewriteWarning_ShouldReturnNull_WhenNoSavedRowFieldsChange()
    {
        var message = InvokeSavedRowRewriteWarning(
            "Bins",
            "Bins",
            "PackageVariantClosed",
            "PackageVariantClosed"
        );

        message.Should().BeNull();
    }

    private static string? InvokeSavedRowRewriteWarning(
        string originalName,
        string newName,
        string originalIcon,
        string newIcon
    )
    {
        var method = typeof(ViewModel_dunnage_typeselection).GetMethod(
            "BuildSavedRowRewriteWarning",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();

        return method!
            .Invoke(null, [originalName, newName, originalIcon, newIcon])
            .As<string?>();
    }

    [Fact]
    public void BuildTypeDeleteWarning_ShouldListAffectedCounts()
    {
        var message = InvokeTypeDeleteWarning(
            "Bins",
            new Model_DunnageTypeDeleteImpact
            {
                PartsCount = 3,
                LabelDataCount = 4,
                HistoryCount = 2,
            }
        );

        message.Should().Contain("'Bins'");
        message.Should().Contain("3 parts");
        message.Should().Contain("4 current label data entries");
        message.Should().Contain("2 history entries");
        message.Should().Contain("All existing label data and history entries");
    }

    [Fact]
    public void BuildTypeDeleteWarning_ShouldMentionNoReferences_WhenNoImpact()
    {
        var message = InvokeTypeDeleteWarning(
            "Bins",
            new Model_DunnageTypeDeleteImpact()
        );

        message.Should().Contain("No parts, label data, or history entries reference this type.");
    }

    private static string InvokeTypeDeleteWarning(
        string typeName,
        Model_DunnageTypeDeleteImpact impact
    )
    {
        var method = typeof(ViewModel_dunnage_typeselection).GetMethod(
            "BuildTypeDeleteWarning",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();

        return method!.Invoke(null, [typeName, impact]).As<string>();
    }

    private static ViewModel_dunnage_typeselection CreateViewModel(
        Mock<IService_DunnageWorkflow>? workflow = null,
        Mock<IService_MySQL_Dunnage>? dunnageService = null,
        Mock<IService_DunnageSettings>? dunnageSettings = null,
        Mock<IService_UserSessionManager>? sessionManager = null,
        Model_DunnageSession? workflowSession = null
    )
    {
        workflow ??= new Mock<IService_DunnageWorkflow>();
        workflow
            .SetupGet(service => service.CurrentSession)
            .Returns(workflowSession ?? new Model_DunnageSession());

        dunnageService ??= CreateDunnageServiceMock([], []);
        dunnageSettings ??= CreateSettingsMock("Name (A-Z)");
        sessionManager ??= CreateSessionManager(42);

        var userPrivileges = new Mock<IService_UserPrivileges>();
        userPrivileges.SetupGet(service => service.IsInitialized).Returns(true);
        userPrivileges.SetupGet(service => service.CurrentUserId).Returns(42);
        userPrivileges.Setup(service => service.HasAnyRole(It.IsAny<string[]>())).Returns(false);

        var viewModelRegistry = new Mock<IService_ViewModelRegistry>();

        return new ViewModel_dunnage_typeselection(
            workflow.Object,
            dunnageService.Object,
            new Service_Pagination(),
            new Mock<IService_Help>().Object,
            userPrivileges.Object,
            sessionManager.Object,
            dunnageSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            viewModelRegistry.Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Mock<IService_MySQL_Dunnage> CreateDunnageServiceMock(
        List<Model_DunnageType> types,
        List<Model_DunnageLoad> loads
    )
    {
        var dunnageService = new Mock<IService_MySQL_Dunnage>();
        dunnageService
            .Setup(service => service.GetAllTypesAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(types));
        dunnageService
            .Setup(service => service.GetAllLoadsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(loads));

        return dunnageService;
    }

    private static Mock<IService_DunnageSettings> CreateSettingsMock(string sortOption)
    {
        var dunnageSettings = new Mock<IService_DunnageSettings>();
        dunnageSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(sortOption);
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

    private static List<Model_DunnageType> CreateTypes()
    {
        return
        [
            new Model_DunnageType
            {
                Id = 1,
                TypeName = "Alpha",
                CreatedDate = new DateTime(2024, 1, 1),
                ModifiedDate = new DateTime(2024, 4, 1),
            },
            new Model_DunnageType
            {
                Id = 2,
                TypeName = "Bravo",
                CreatedDate = new DateTime(2024, 3, 1),
            },
            new Model_DunnageType
            {
                Id = 3,
                TypeName = "Charlie",
                CreatedDate = new DateTime(2024, 2, 1),
                ModifiedDate = new DateTime(2024, 5, 1),
            },
            new Model_DunnageType
            {
                Id = 4,
                TypeName = "Delta",
                CreatedDate = new DateTime(2024, 4, 1),
                ModifiedDate = new DateTime(2024, 4, 15),
            },
        ];
    }

    private static List<Model_DunnageLoad> CreateLoads()
    {
        return
        [
            new Model_DunnageLoad
            {
                TypeId = 1,
                ReceivedDate = new DateTime(2025, 3, 10),
                CreatedDate = new DateTime(2025, 3, 10),
            },
            new Model_DunnageLoad
            {
                TypeId = 1,
                ReceivedDate = new DateTime(2025, 3, 10),
                CreatedDate = new DateTime(2025, 3, 10),
            },
            new Model_DunnageLoad
            {
                TypeId = 2,
                ReceivedDate = new DateTime(2025, 3, 12),
                CreatedDate = new DateTime(2025, 3, 12),
            },
            new Model_DunnageLoad
            {
                TypeId = 2,
                ReceivedDate = new DateTime(2025, 3, 12),
                CreatedDate = new DateTime(2025, 3, 12),
            },
            new Model_DunnageLoad
            {
                TypeId = 3,
                ReceivedDate = new DateTime(2025, 3, 12),
                CreatedDate = new DateTime(2025, 3, 12),
            },
        ];
    }
}
