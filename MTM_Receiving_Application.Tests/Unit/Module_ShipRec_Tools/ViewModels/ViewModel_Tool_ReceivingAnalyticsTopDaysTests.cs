using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Covers the Record Days tab: the range it asks for, that each item type gets its own ranking by
/// loads, and that every ranking is cut to the selected top count.
/// </summary>
public sealed class ViewModel_Tool_ReceivingAnalyticsTopDaysTests
{
    private static ViewModel_Tool_ReceivingAnalyticsTopDays CreateViewModel(
        Mock<IService_Tool_ReceivingAnalyticsApp> serviceMock
    ) =>
        new(
            serviceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Mock<IService_Tool_ReceivingAnalyticsApp> CreateServiceMock(
        params Model_Tool_ReceivingAnalyticsAppTopDayRow[] rows
    )
    {
        var mock = new Mock<IService_Tool_ReceivingAnalyticsApp>();
        mock.Setup(service =>
                service.GetTopDaysByItemTypeAsync(
                    It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_Tool_ReceivingAnalyticsAppTopDayRow>>(
                    [.. rows]
                )
            );
        mock.Setup(service => service.GetBusiestDaysAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<
                    List<Model_Tool_ReceivingAnalyticsAppBusiestDayRow>
                >([])
            );
        return mock;
    }

    private static Model_Tool_ReceivingAnalyticsAppTopDayRow Row(
        DateTime day,
        string itemType,
        int loads
    ) => new() { DayDate = day, ItemType = itemType, LoadCount = loads, LabelCount = loads * 2 };

    [Fact]
    public async Task LoadAsync_ShouldRankEachItemTypeByLoads()
    {
        var viewModel = CreateViewModel(
            CreateServiceMock(
                Row(new DateTime(2026, 9, 1), "Coil", 4),
                Row(new DateTime(2026, 9, 2), "Coil", 9),
                Row(new DateTime(2026, 9, 3), "Sheet", 5),
                Row(new DateTime(2026, 9, 4), "Standard", 7)
            )
        );

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.CoilDays.Should().HaveCount(2);
        viewModel.CoilDays[0].LoadCount.Should().Be(9);
        viewModel.CoilDays[0].Rank.Should().Be(1);
        viewModel.CoilDays[1].LoadCount.Should().Be(4);
        viewModel.CoilDays[1].Rank.Should().Be(2);

        viewModel.FlatStockDays.Should().ContainSingle().Which.DayDate.Should().Be(new DateTime(2026, 9, 3));
        viewModel.OtherDays.Should().ContainSingle().Which.LoadCount.Should().Be(7);
    }

    [Fact]
    public async Task LoadAsync_ShouldCutEachRankingToTheSelectedTopCount()
    {
        var rows = new List<Model_Tool_ReceivingAnalyticsAppTopDayRow>();
        for (var day = 1; day <= 8; day++)
        {
            rows.Add(Row(new DateTime(2026, 9, day), "Coil", day));
        }

        var viewModel = CreateViewModel(CreateServiceMock([.. rows]));

        // Index 0 is "Top 5".
        viewModel.SelectedTopCountIndex = 0;

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.CoilDays.Should().HaveCount(5);
        viewModel.CoilDays[0].LoadCount.Should().Be(8);
        viewModel.CoilDays[4].LoadCount.Should().Be(4);
    }

    [Fact]
    public async Task LoadAsync_ShouldRankTheBusiestDaysAcrossAllItemTypes()
    {
        var serviceMock = CreateServiceMock(Row(new DateTime(2026, 9, 1), "Coil", 4));
        serviceMock
            .Setup(service => service.GetBusiestDaysAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_ReceivingAnalyticsAppBusiestDayRow>
                    {
                        new() { DayDate = new DateTime(2026, 9, 2), LoadCount = 12, LabelCount = 14 },
                        new() { DayDate = new DateTime(2026, 9, 1), LoadCount = 30, LabelCount = 33 },
                    }
                )
            );

        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.BusiestDays.Should().HaveCount(2);
        viewModel.BusiestDays[0].DayDate.Should().Be(new DateTime(2026, 9, 1));
        viewModel.BusiestDays[0].LoadCount.Should().Be(30);
        viewModel.BusiestDays[0].Rank.Should().Be(1);
        viewModel.BusiestDays[1].LoadCount.Should().Be(12);
        viewModel.BusiestDays[1].Rank.Should().Be(2);
    }

    [Fact]
    public async Task LoadAsync_ShouldKeepTheItemTypeRankings_WhenTheBusiestDayReadFails()
    {
        var serviceMock = CreateServiceMock(Row(new DateTime(2026, 9, 1), "Coil", 4));
        serviceMock
            .Setup(service => service.GetBusiestDaysAsync(It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<
                    List<Model_Tool_ReceivingAnalyticsAppBusiestDayRow>
                >("activity unavailable")
            );

        var viewModel = CreateViewModel(serviceMock);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.CoilDays.Should().ContainSingle();
        viewModel.BusiestDays.Should().BeEmpty();
        viewModel.HasBusiestDays.Should().BeFalse();
        viewModel.StatusMessage.Should().Contain("activity unavailable");
    }

    [Fact]
    public async Task LoadAsync_ShouldRequestTheRangeWithTheLastDayIncluded()
    {
        var serviceMock = CreateServiceMock();
        Model_Tool_ReceivingAnalyticsAppFilter? captured = null;
        serviceMock
            .Setup(service =>
                service.GetTopDaysByItemTypeAsync(
                    It.IsAny<Model_Tool_ReceivingAnalyticsAppFilter>(),
                    It.IsAny<int>()
                )
            )
            .Callback<Model_Tool_ReceivingAnalyticsAppFilter, int>((filter, _) => captured = filter)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_Tool_ReceivingAnalyticsAppTopDayRow>>([])
            );

        var viewModel = CreateViewModel(serviceMock);
        viewModel.FromDate = new DateTimeOffset(new DateTime(2026, 8, 1));
        viewModel.ToDate = new DateTimeOffset(new DateTime(2026, 8, 31));

        await viewModel.LoadCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.StartDate.Should().Be(new DateTime(2026, 8, 1));
        captured.EndDate.Should().Be(new DateTime(2026, 9, 1));
    }
}
