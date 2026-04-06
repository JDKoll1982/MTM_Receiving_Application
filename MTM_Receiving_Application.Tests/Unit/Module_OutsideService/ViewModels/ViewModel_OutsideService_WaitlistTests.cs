using System;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_OutsideService.Contracts;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_OutsideService.ViewModels;

public class ViewModel_OutsideService_WaitlistTests
{
    [Fact]
    public async Task MarkSelectedLineCompleteAsync_ShouldMarkSetupLineComplete()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.MarkCompleteAsync(15, null))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = new ViewModel_OutsideService_Waitlist(
            outsideServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        var lineCompletedRaised = false;
        viewModel.LineCompleted += () => lineCompletedRaised = true;
        var selectedLine = new Model_OutsideServiceRequestLine
        {
            OutsideServiceRequestLineId = 15,
            RequestNumber = "OS-3000",
            LineNumber = 1,
            PartId = "PART-300",
            LinePhase = Enum_OutsideServiceLinePhase.Setup,
            CreatedUtc = DateTime.UtcNow.AddHours(-4),
        };
        viewModel.SelectedLine = selectedLine;

        await viewModel.MarkSelectedLineCompleteCommand.ExecuteAsync(null);

        outsideServiceMock.Verify(service => service.MarkCompleteAsync(15, null), Times.Once);
        selectedLine.IsComplete.Should().BeTrue();
        viewModel.SelectedLine.Should().BeNull();
        lineCompletedRaised.Should().BeTrue();
    }
}
