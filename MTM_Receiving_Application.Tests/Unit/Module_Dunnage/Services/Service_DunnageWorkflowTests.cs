using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageWorkflowTests
{
    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldGenerateLoadsAndApplyDetails_ForGuidedWorkflowBatch()
    {
        var service = CreateService();

        service.CurrentSession.SelectedTypeId = 5;
        service.CurrentSession.SelectedTypeName = "Pallet";
        service.CurrentSession.SelectedType = new Model_DunnageType
        {
            Id = 5,
            TypeName = "Pallet",
            Icon = "PackageVariantClosed",
        };
        service.CurrentSession.SelectedPart = new Model_DunnagePart
        {
            PartId = "DUN-100",
            HomeLocation = "RACK-A1",
        };
        service.CurrentSession.SpecValues = new Dictionary<string, object> { ["Length"] = 48 };
        service.NumberOfLoads = 3;
        service.CurrentSession.LoadQuantities.Add(10m);
        service.CurrentSession.LoadQuantities.Add(20m);
        service.CurrentSession.LoadQuantities.Add(30m);

        service.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);

        var quantityStepResult = await service.AdvanceToNextStepAsync();

        quantityStepResult.IsSuccess.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.DetailsEntry);
        service.CurrentSession.Loads.Should().HaveCount(3);
        service.CurrentSession.Loads.Select(load => load.LoadNumber).Should().Equal(1, 2, 3);
        service.CurrentSession.Loads.Select(load => load.Quantity).Should().Equal(10m, 20m, 30m);

        service.CurrentSession.PONumber = "PO-7788";
        service.CurrentSession.Location = "DOCK-4";

        var detailsStepResult = await service.AdvanceToNextStepAsync();

        detailsStepResult.IsSuccess.Should().BeTrue();
        service.CurrentStep.Should().Be(Enum_DunnageWorkflowStep.Review);
        service.CurrentSession.Loads.Should().OnlyContain(load => load.PoNumber == "PO-7788");
        service.CurrentSession.Loads.Should().OnlyContain(load => load.Location == "DOCK-4");
        service.CurrentSession.Loads.Should().OnlyContain(load => load.TypeName == "Pallet");
        service.CurrentSession.Loads.Should().OnlyContain(load => load.TypeId == 5);
        service.CurrentSession.Loads.Should().OnlyContain(load => load.Specs.ContainsKey("Length"));
    }

    private static Service_DunnageWorkflow CreateService()
    {
        return new Service_DunnageWorkflow(
            new Mock<IService_MySQL_Dunnage>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_ViewModelRegistry>().Object
        );
    }
}
