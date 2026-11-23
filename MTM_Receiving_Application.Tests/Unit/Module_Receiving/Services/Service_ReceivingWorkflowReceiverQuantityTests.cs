using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

/// <summary>
/// A by-receiver walk with a single load starts the Weight/Quantity page at the quantity the
/// receiver recorded, so the operator confirms it rather than retyping it. Guided walks and walks
/// with several loads keep starting from zero.
/// </summary>
public sealed class Service_ReceivingWorkflowReceiverQuantityTests
{
    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldSeedTheSingleLoadWithTheReceiverQty_WhenTheWalkCameFromAReceiver()
    {
        var service = CreateService();

        await BeginLoadStepAsync(service, loadCount: 1);
        service.SelectedReceiver = CreateReceiver(qtyReceived: 1200.5m);

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_ReceivingWorkflowStep.WeightQuantityEntry);
        service.CurrentSession.Loads.Should().ContainSingle();
        service.CurrentSession.Loads[0].WeightQuantity.Should().Be(1200.5m);
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldLeaveTheLoadsAtZero_WhenThereIsMoreThanOneLoad()
    {
        var service = CreateService();

        await BeginLoadStepAsync(service, loadCount: 3);
        service.SelectedReceiver = CreateReceiver(qtyReceived: 1200.5m);

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeTrue();
        service.CurrentSession.Loads.Should().HaveCount(3);
        service
            .CurrentSession.Loads.Should()
            .OnlyContain(load => load.WeightQuantity == 0m, "only a single load is prefilled");
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldLeaveTheLoadAtZero_WhenTheWalkIsGuided()
    {
        var service = CreateService();

        await BeginLoadStepAsync(service, loadCount: 1);

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeTrue();
        service.CurrentSession.Loads.Should().ContainSingle();
        service
            .CurrentSession.Loads[0]
            .WeightQuantity.Should()
            .Be(0m, "a guided walk has no receiver quantity to carry");
    }

    private static Model_InforVisualReceiverRow CreateReceiver(decimal qtyReceived) =>
        new()
        {
            ReceiverNumber = "TEST-RCV-001",
            PartNumber = "TEST-PART-001",
            POLineNumber = "1",
            QtyReceived = qtyReceived,
        };

    private static async Task BeginLoadStepAsync(
        Service_ReceivingWorkflow service,
        int loadCount
    )
    {
        await service.StartWorkflowAsync();
        service.CurrentPONumber = "PO-TEST-001";
        service.CurrentPart = new Model_InforVisualPart
        {
            PartID = "TEST-PART-001",
            POLineNumber = "1",
            Description = "Receiver quantity test part",
            UnitOfMeasure = "EA",
        };
        service.NumberOfLoads = loadCount;
        service.CurrentLocation = "TEST-RECV";
        service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry);
    }

    private static Service_ReceivingWorkflow CreateService()
    {
        var validation = new Mock<IService_ReceivingValidation>();
        validation
            .Setup(service => service.ValidateLocationAsync(It.IsAny<string?>(), It.IsAny<string>()))
            .ReturnsAsync(Model_ReceivingValidationResult.Success());
        validation
            .Setup(service => service.ValidateWeightQuantityAsync(It.IsAny<decimal>()))
            .ReturnsAsync(Model_ReceivingValidationResult.Success());

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);

        var appSettings = new Mock<IService_AppSettings>();
        appSettings.Setup(service => service.GetUseInforVisualMockData()).Returns(false);

        var sessionManager = new Mock<IService_SessionManager>();
        sessionManager.Setup(service => service.SessionExists()).Returns(false);

        return new Service_ReceivingWorkflow(
            sessionManager.Object,
            new Mock<IService_ReceivingLabelData>().Object,
            new Mock<IService_MySQL_Receiving>().Object,
            validation.Object,
            new Mock<IService_QualityHoldWarning>().Object,
            receivingSettings.Object,
            appSettings.Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_UserPrivileges>().Object,
            new Mock<IService_UserPreferences>().Object
        );
    }
}
