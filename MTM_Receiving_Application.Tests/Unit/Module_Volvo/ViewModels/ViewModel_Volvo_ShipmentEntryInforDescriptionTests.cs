using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.ViewModels;

public sealed class ViewModel_Volvo_ShipmentEntryInforDescriptionTests
{
    [Fact]
    public async Task AddPartFromDialogAsync_ShouldPreferInforVisualDescription_WhenAvailable()
    {
        var mediatorMock = CreateMediatorMock();
        var inforVisualMock = new Mock<IService_InforVisual>();
        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("V-EMB-500"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(
                    new Model_InforVisualPart
                    {
                        PartID = "V-EMB-500",
                        Description = "Infor Description",
                    }
                )
            );

        var viewModel = CreateViewModel(mediatorMock, inforVisualMock);

        var result = await viewModel.AddPartFromDialogAsync(
            new Model_VolvoPart
            {
                PartNumber = "V-EMB-500",
                Description = "Volvo Description",
                QuantityPerSkid = 12,
            },
            2,
            "RECV"
        );

        result.IsSuccess.Should().BeTrue();
        viewModel.Parts.Should().ContainSingle();
        viewModel.Parts[0].PartDescription.Should().Be("Infor Description");
    }

    [Fact]
    public async Task AddPartFromDialogAsync_ShouldFallBackToVolvoDescription_WhenInforVisualReturnsNoDescription()
    {
        var mediatorMock = CreateMediatorMock();
        var inforVisualMock = new Mock<IService_InforVisual>();
        inforVisualMock
            .Setup(service => service.GetPartByIDAsync("V-EMB-750"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(null));

        var viewModel = CreateViewModel(mediatorMock, inforVisualMock);

        var result = await viewModel.AddPartFromDialogAsync(
            new Model_VolvoPart
            {
                PartNumber = "V-EMB-750",
                Description = "Volvo Description",
                QuantityPerSkid = 8,
            },
            1,
            "LINE-1"
        );

        result.IsSuccess.Should().BeTrue();
        viewModel.Parts.Should().ContainSingle();
        viewModel.Parts[0].PartDescription.Should().Be("Volvo Description");
    }

    private static Mock<IMediator> CreateMediatorMock()
    {
        var mediatorMock = new Mock<IMediator>();

        mediatorMock
            .Setup(mediator =>
                mediator.Send(It.IsAny<SavePendingShipmentCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(123));

        mediatorMock
            .Setup(mediator =>
                mediator.Send(It.IsAny<GetAllVolvoPartsQuery>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_VolvoPart>()));

        mediatorMock
            .Setup(mediator =>
                mediator.Send(
                    It.IsAny<SyncVolvoGeneratedLabelDataCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));

        return mediatorMock;
    }

    private static ViewModel_Volvo_ShipmentEntry CreateViewModel(
        Mock<IMediator> mediatorMock,
        Mock<IService_InforVisual> inforVisualMock
    )
    {
        var receivingValidationMock = new Mock<IService_ReceivingValidation>();
        receivingValidationMock
            .SetupGet(service => service.PresetLocations)
            .Returns(Array.Empty<string>());
        receivingValidationMock.SetupGet(service => service.UseMockLocationList).Returns(false);

        return new ViewModel_Volvo_ShipmentEntry(
            mediatorMock.Object,
            inforVisualMock.Object,
            receivingValidationMock.Object,
            new Mock<IService_ReportingClipboard>().Object,
            new Mock<IService_VolvoRecipientSettings>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
