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

    [Fact]
    public async Task ResetFiltersToDefaults_ShouldClearSearchAndRestoreAllFilters()
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
                SetupVendorName = "Precision Plating Inc.",
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
                SetupVendorName = "Acme Heat Treat",
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

        viewModel.SearchText = "PART-NEW";
        viewModel.SelectedVendorFilter = "Acme Heat Treat";
        viewModel.SelectedDateRange = "Last 30 days";

        viewModel.ResetFiltersToDefaults();

        viewModel.SearchText.Should().BeEmpty();
        viewModel.SelectedVendorFilter.Should().Be("All");
        viewModel.SelectedDateRange.Should().Be("All");
        viewModel.FilteredLines.Should().HaveCount(2);
        viewModel.FilteredLines.Should().Contain(line => line.PartId == "PART-OLD");
        viewModel.FilteredLines.Should().Contain(line => line.PartId == "PART-NEW");
    }

    [Fact]
    public async Task LoadAsync_ShouldRestoreVendorFilterToAll_WhenVendorSelectionIsBlank()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .Setup(service => service.GetCompletedLinesAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceRequestLine>
                    {
                        new()
                        {
                            OutsideServiceRequestLineId = 3,
                            RequestNumber = "OS-1002",
                            LineNumber = 1,
                            PartId = "PART-VENDOR",
                            SetupVendorName = "Acme Heat Treat",
                            LinePhase = Enum_OutsideServiceLinePhase.Complete,
                            CreatedUtc = DateTime.UtcNow.AddDays(-10),
                            CompletedUtc = DateTime.UtcNow.AddDays(-1),
                        },
                    }
                )
            );

        var viewModel = new ViewModel_OutsideService_CompleteHistory(
            outsideServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        )
        {
            SelectedVendorFilter = string.Empty,
        };

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectedVendorFilter.Should().Be("All");
        viewModel.VendorFilterOptions.Should().Contain("All");
    }

    [Fact]
    public async Task LoadAsync_ShouldReplaceVendorFilterOptions_WhenResultsChange()
    {
        var outsideServiceMock = new Mock<IService_OutsideService>();
        outsideServiceMock
            .SetupSequence(service => service.GetCompletedLinesAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceRequestLine>
                    {
                        new()
                        {
                            OutsideServiceRequestLineId = 10,
                            RequestNumber = "OS-1100",
                            LineNumber = 1,
                            PartId = "PART-A",
                            SetupVendorName = "Vendor A",
                            LinePhase = Enum_OutsideServiceLinePhase.Complete,
                            CreatedUtc = DateTime.UtcNow.AddDays(-5),
                            CompletedUtc = DateTime.UtcNow.AddDays(-2),
                        },
                    }
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceRequestLine>
                    {
                        new()
                        {
                            OutsideServiceRequestLineId = 11,
                            RequestNumber = "OS-1101",
                            LineNumber = 1,
                            PartId = "PART-B",
                            SetupVendorName = "Vendor B",
                            LinePhase = Enum_OutsideServiceLinePhase.Complete,
                            CreatedUtc = DateTime.UtcNow.AddDays(-1),
                            CompletedUtc = DateTime.UtcNow,
                        },
                    }
                )
            );

        var viewModel = new ViewModel_OutsideService_CompleteHistory(
            outsideServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.VendorFilterOptions.Should().Contain("All");
        viewModel.VendorFilterOptions.Should().Contain("Vendor B");
        viewModel.VendorFilterOptions.Should().NotContain("Vendor A");
    }
}
