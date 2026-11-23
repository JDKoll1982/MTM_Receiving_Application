using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Receiving.ViewModels;

/// <summary>
/// FR-015: By Receiver is one of the modes an operator can store as their default, and Receiving
/// opens in it for them afterwards.
/// </summary>
public sealed class ViewModel_Settings_Receiving_ByReceiverDefaultTests
{
    [Fact]
    public void WorkflowDefaults_ShouldOfferByReceiverBesideTheOtherModes()
    {
        var viewModel = CreateWorkflowDefaults();

        viewModel
            .DefaultModeOptions.Should()
            .Contain(option =>
                option.DisplayName == "By Receiver" && option.Value == "by-receiver"
            );
        viewModel
            .DefaultModeOptions.Select(option => option.DisplayName)
            .Should()
            .ContainInOrder("Guided Wizard", "By Receiver", "Manual Entry", "Edit Mode");
    }

    [Fact]
    public void BusinessRules_ShouldOfferTheSameByReceiverOption()
    {
        var viewModel = CreateBusinessRules();

        viewModel
            .DefaultModeOptions.Should()
            .Contain(option =>
                option.DisplayName == "By Receiver" && option.Value == "by-receiver"
            );
        viewModel
            .DefaultModeOptions.Select(option => option.DisplayName)
            .Should()
            .ContainInOrder("Guided Wizard", "By Receiver", "Manual Entry", "Edit Mode");
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldOpenTheReceiverList_WhenTheStoredDefaultIsByReceiver()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = "by-receiver",
        };
        var (service, userPreferences) = CreateWorkflowService(currentUser);

        await service.StartWorkflowAsync();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.ReceiverSelection);
        service
            .IsByReceiverFlow.Should()
            .BeTrue("the walk has to know which door it came through");
        userPreferences.Verify(
            s => s.UpdateDefaultReceivingModeAsync(It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never,
            "a recognised default must not be rewritten"
        );
    }

    [Fact]
    public async Task SwitchingToGuidedAndBack_ShouldReloadTheListWithoutError()
    {
        var step = Enum_ReceivingWorkflowStep.ReceiverSelection;
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupGet(service => service.CurrentStep).Returns(() => step);

        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.GetRecentReceiversAsync(It.IsAny<int>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualReceiverRow>
                    {
                        new() { ReceiverNumber = "TEST-RCV-001", PartNumber = "TEST-PART-001" },
                    }
                )
            );

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service => service.GetIntAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(50);

        var list = new ViewModel_Receiving_ReceiverSelection(
            workflow.Object,
            inforVisual.Object,
            new Service_Pagination(),
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
        await Task.Delay(25);

        step = Enum_ReceivingWorkflowStep.POEntry;
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        step = Enum_ReceivingWorkflowStep.ReceiverSelection;
        workflow.Raise(service => service.StepChanged += null, workflow.Object, EventArgs.Empty);
        await Task.Delay(25);

        inforVisual.Verify(service => service.GetRecentReceiversAsync(1000), Times.Exactly(2));
        list.IsFailed.Should().BeFalse();
        list.Rows.Should().HaveCount(1);
    }

    private static ViewModel_Settings_Receiving_WorkflowDefaults CreateWorkflowDefaults() =>
        new(
            new Mock<IService_AppSettings>().Object,
            new Mock<IService_SettingsCoreFacade>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_UserPreferences>().Object,
            new Mock<IService_UserPrivileges>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static ViewModel_Settings_Receiving_BusinessRules CreateBusinessRules() =>
        new(
            new Mock<IService_AppSettings>().Object,
            new Mock<IService_SettingsCoreFacade>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_UserPreferences>().Object,
            new Mock<IService_UserPrivileges>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static (
        Service_ReceivingWorkflow Service,
        Mock<IService_UserPreferences> UserPreferences
    ) CreateWorkflowService(Model_User currentUser)
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
