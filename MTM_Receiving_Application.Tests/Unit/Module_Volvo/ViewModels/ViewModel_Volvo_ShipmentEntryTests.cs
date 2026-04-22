using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.ViewModels;

public sealed class ViewModel_Volvo_ShipmentEntryTests
{
    [Fact]
    public void PreviewEmailCommand_ShouldBeDisabled_WhenNoVisiblePartsExist()
    {
        var viewModel = CreateViewModel();

        viewModel.PreviewEmailCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void PreviewEmailCommand_ShouldBeEnabled_WhenVisiblePartsContainShipmentData()
    {
        var viewModel = CreateViewModel();

        viewModel.Parts = new ObservableCollection<Model_VolvoShipmentLine>
        {
            new()
            {
                PartNumber = "V-EMB-500",
                ReceivedSkidCount = 2,
                QuantityPerSkid = 15,
                Location = "RECV",
            },
        };

        viewModel.HasAnyParts.Should().BeTrue();
        viewModel.PreviewEmailCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void PreviewEmailCommand_ShouldBeDisabled_WhenVisiblePartsAreIncomplete()
    {
        var viewModel = CreateViewModel();

        viewModel.Parts = new ObservableCollection<Model_VolvoShipmentLine>
        {
            new()
            {
                PartNumber = "V-EMB-500",
                ReceivedSkidCount = 0,
                QuantityPerSkid = 15,
                Location = "RECV",
            },
        };

        viewModel.HasAnyParts.Should().BeTrue();
        viewModel.PreviewEmailCommand.CanExecute(null).Should().BeFalse();
    }

    private static ViewModel_Volvo_ShipmentEntry CreateViewModel()
    {
        var receivingValidationMock = new Mock<IService_ReceivingValidation>();
        receivingValidationMock
            .SetupGet(service => service.PresetLocations)
            .Returns(Array.Empty<string>());
        receivingValidationMock.SetupGet(service => service.UseMockLocationList).Returns(false);

        return new ViewModel_Volvo_ShipmentEntry(
            new Mock<IMediator>().Object,
            new Mock<IService_InforVisual>().Object,
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
