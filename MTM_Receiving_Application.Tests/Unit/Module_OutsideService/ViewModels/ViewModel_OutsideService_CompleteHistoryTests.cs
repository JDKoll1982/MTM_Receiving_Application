using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_OutsideService.Contracts;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_OutsideService.ViewModels;

public class ViewModel_OutsideService_CompleteHistoryTests
{
    [Fact]
    public async Task LoadAsync_ShouldIncludeOlderCompletedRows_ByDefault()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        var completedLines = new List<Model_OutsideServiceRequestLine>
        {
            new()
            {
                OutsideServiceRequestLineId = 1,
                RequestNumber = "OS-1000",
                LineNumber = 1,
                PartId = "PART-OLD",
                LinePhase = Enum_OutsideServiceLinePhase.Complete,
                CreatedUtc = DateTime.UtcNow.AddDays(-120),
                CompletedUtc = DateTime.UtcNow.AddDays(-90),
            },
            new()
            {
                OutsideServiceRequestLineId = 2,
                RequestNumber = "OS-1001",
                LineNumber = 1,
                PartId = "PART-NEW",
                LinePhase = Enum_OutsideServiceLinePhase.Complete,
                CreatedUtc = DateTime.UtcNow.AddDays(-2),
                CompletedUtc = DateTime.UtcNow.AddDays(-1),
            },
        };

        outsideServiceMock
            .Setup(service => service.GetCompletedLinesAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(completedLines));

        var viewModel = new ViewModel_OutsideService_CompleteHistory(
            outsideServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectedDateRange.Should().Be("All");
        viewModel.FilteredLines.Should().HaveCount(2);
        viewModel.FilteredLines.Should().Contain(line => line.PartId == "PART-OLD");
        viewModel.FilteredLines.Should().Contain(line => line.PartId == "PART-NEW");
    }
}
