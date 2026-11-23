using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_ByReceiverBackTests
{
    [Fact]
    public void GoToPreviousStep_ShouldReturnToReceiverSelection_WhenByReceiverWalkIsOnLoadEntry()
    {
        var (service, _) = CreateService();

        service.GoToStep(Enum_ReceivingWorkflowStep.ReceiverSelection);
        var row = new Model_InforVisualReceiverRow
        {
            ReceiverNumber = "TEST-RCV-001",
            PartNumber = "TEST-PART-001",
            POLineNumber = "1",
        };
        service.SelectedReceiver = row;
        var keyBefore = service.SelectedReceiverKey;

        service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry);
        service.GoToPreviousStep();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.ReceiverSelection);
        service.SelectedReceiverKey.Should().Be(keyBefore);
        service.SelectedReceiverKey.Should().Be(row.SelectionKey);
    }

    [Fact]
    public void GoToPreviousStep_ShouldReturnToPartSelection_WhenGuidedWalkIsOnLoadEntry()
    {
        var (service, _) = CreateService();

        service.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
        service.CurrentPart = new Model_InforVisualPart { PartID = "TEST-PART-001" };

        service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry);
        service.GoToPreviousStep();

        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.PartSelection);
    }

    private static (
        Service_ReceivingWorkflow Service,
        Mock<IService_UserPreferences> UserPreferences
    ) CreateService()
    {
        var currentUser = new Model_User
        {
            WindowsUsername = "DOMAIN\\user",
            EmployeeNumber = 6229,
            DefaultReceivingMode = "guided",
        };
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
