using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Covers the Material Availability Board location-range presentation: results become one
/// collapsible section per location while the flat card list is kept for printing.
/// </summary>
public sealed class ViewModel_Tool_MaterialAvailabilityBoardTests
{
    private const string WarehouseCode = "002";

    private static Model_Tool_MaterialAvailabilityCard CreateCard(string partId)
    {
        return new Model_Tool_MaterialAvailabilityCard { PartId = partId };
    }

    private static ViewModel_Tool_MaterialAvailabilityBoard CreateViewModel(
        IService_Tool_MaterialAvailabilityBoard service,
        IService_SharedLocationRange locationRange
    )
    {
        return new ViewModel_Tool_MaterialAvailabilityBoard(
            service,
            new Mock<IService_ShipRecToolsSettings>().Object,
            locationRange,
            new Mock<IService_AppSettings>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static Mock<IService_SharedLocationRange> CreateLocationRangeMock(
        params string[] locations
    )
    {
        var locationRange = new Mock<IService_SharedLocationRange>();
        locationRange
            .Setup(service =>
                service.ResolveRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_SharedLocationRangeResult
                    {
                        StartLocation = locations.FirstOrDefault() ?? string.Empty,
                        StopLocation = locations.LastOrDefault() ?? string.Empty,
                        Locations = [.. locations],
                    }
                )
            );

        return locationRange;
    }

    [Fact]
    public async Task LoadLocationRangeAsync_ShouldGroupCardsByLocation_ExpandedByDefault()
    {
        var service = new Mock<IService_Tool_MaterialAvailabilityBoard>();
        service
            .Setup(boards => boards.GetBoardByLocationAsync("V-A0-01", WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_MaterialAvailabilityCard>
                    {
                        CreateCard("MMC0000001"),
                        CreateCard("MMC0000002"),
                    }
                )
            );
        service
            .Setup(boards => boards.GetBoardByLocationAsync("V-A0-02", WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_MaterialAvailabilityCard> { CreateCard("MMC0000003") }
                )
            );

        var viewModel = CreateViewModel(
            service.Object,
            CreateLocationRangeMock("V-A0-01", "V-A0-02").Object
        );
        viewModel.RangeStartLocation = "V-A0-01";
        viewModel.RangeStopLocation = "V-A0-02";

        await viewModel.LoadLocationRangeCommand.ExecuteAsync(null);

        // Grouped, collapsible sections replace the flat list for range results.
        viewModel.ShowGroupedCards.Should().BeTrue();
        viewModel.ShowFlatCards.Should().BeFalse();

        viewModel.CardGroups.Should().HaveCount(2);
        viewModel.CardGroups[0].LocationId.Should().Be("V-A0-01");
        viewModel.CardGroups[0].Cards.Should().HaveCount(2);
        viewModel.CardGroups[0].LocationHeaderText.Should().Contain("V-A0-01");
        viewModel.CardGroups[0].LocationHeaderText.Should().Contain("2 part(s)");
        viewModel.CardGroups[0].IsExpanded.Should().BeTrue();
        viewModel.CardGroups[1].LocationId.Should().Be("V-A0-02");
        viewModel.CardGroups[1].Cards.Should().HaveCount(1);

        // The flat list still carries every card so Print keeps working for a range search.
        viewModel.Cards.Should().HaveCount(3);
        viewModel.HasCards.Should().BeTrue();
    }

    [Fact]
    public async Task LoadLocationRangeAsync_ShouldSkipLocationsWithoutParts()
    {
        var service = new Mock<IService_Tool_MaterialAvailabilityBoard>();
        service
            .Setup(boards => boards.GetBoardByLocationAsync("V-A0-01", WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_MaterialAvailabilityCard> { CreateCard("MMC0000001") }
                )
            );
        service
            .Setup(boards => boards.GetBoardByLocationAsync("V-A0-02", WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_Tool_MaterialAvailabilityCard>())
            );

        var viewModel = CreateViewModel(
            service.Object,
            CreateLocationRangeMock("V-A0-01", "V-A0-02").Object
        );
        viewModel.RangeStartLocation = "V-A0-01";
        viewModel.RangeStopLocation = "V-A0-02";

        await viewModel.LoadLocationRangeCommand.ExecuteAsync(null);

        // An empty location produces no section rather than an empty header.
        viewModel.CardGroups.Should().HaveCount(1);
        viewModel.CardGroups[0].LocationId.Should().Be("V-A0-01");
    }

    [Fact]
    public async Task SetSearchByPartCommand_ShouldClearLocationGroups()
    {
        var service = new Mock<IService_Tool_MaterialAvailabilityBoard>();
        service
            .Setup(boards => boards.GetBoardByLocationAsync(It.IsAny<string>(), WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_MaterialAvailabilityCard> { CreateCard("MMC0000001") }
                )
            );

        var viewModel = CreateViewModel(service.Object, CreateLocationRangeMock("V-A0-01").Object);
        viewModel.RangeStartLocation = "V-A0-01";
        viewModel.RangeStopLocation = "V-A0-01";

        await viewModel.LoadLocationRangeCommand.ExecuteAsync(null);
        viewModel.ShowGroupedCards.Should().BeTrue();

        viewModel.SetSearchByPartCommand.Execute(null);

        viewModel.CardGroups.Should().BeEmpty();
        viewModel.ShowGroupedCards.Should().BeFalse();
        viewModel.ShowFlatCards.Should().BeFalse();
        viewModel.IsLocationRangeEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task LoadLocationRangeAsync_WhenBoardFails_ShouldClearGroupsAndReportStatus()
    {
        var service = new Mock<IService_Tool_MaterialAvailabilityBoard>();
        service
            .Setup(boards => boards.GetBoardByLocationAsync(It.IsAny<string>(), WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_Tool_MaterialAvailabilityCard>>(
                    "Infor Visual is unavailable."
                )
            );

        var viewModel = CreateViewModel(service.Object, CreateLocationRangeMock("V-A0-01").Object);
        viewModel.RangeStartLocation = "V-A0-01";
        viewModel.RangeStopLocation = "V-A0-01";

        await viewModel.LoadLocationRangeCommand.ExecuteAsync(null);

        viewModel.CardGroups.Should().BeEmpty();
        viewModel.Cards.Should().BeEmpty();
        viewModel.IsStatusOpen.Should().BeTrue();
        viewModel.StatusMessage.Should().Contain("Infor Visual is unavailable.");
    }

    [Fact]
    public void ResolveReceiptDateRange_ShouldUseToday_WhenNoRangeWasEntered()
    {
        var viewModel = CreateViewModel(
            new Mock<IService_Tool_MaterialAvailabilityBoard>().Object,
            CreateLocationRangeMock("V-A0-01").Object
        );

        var (start, end) = viewModel.ResolveReceiptDateRange();

        start.Should().Be(DateTime.Today);
        end.Should().Be(DateTime.Today);
        viewModel.HasReceiptDateRange.Should().BeFalse();
        viewModel.ReceiptDateRangeButtonLabel.Should().Be("Date Range");
    }

    [Fact]
    public void ApplyReceiptDateRange_ShouldSwapBounds_WhenTheRangeWasEnteredBackwards()
    {
        var viewModel = CreateViewModel(
            new Mock<IService_Tool_MaterialAvailabilityBoard>().Object,
            CreateLocationRangeMock("V-A0-01").Object
        );
        viewModel.ReceiptsFromDate = new DateTimeOffset(new DateTime(2026, 11, 6));
        viewModel.ReceiptsToDate = new DateTimeOffset(new DateTime(2026, 11, 1));

        viewModel.ApplyReceiptDateRangeCommand.Execute(null);
        var (start, end) = viewModel.ResolveReceiptDateRange();

        start.Should().Be(new DateTime(2026, 11, 1));
        end.Should().Be(new DateTime(2026, 11, 6));
        viewModel.HasReceiptDateRange.Should().BeTrue();
        viewModel.ReceiptDateRangeButtonLabel.Should().Be("Receipts 11/01-11/06");
    }

    [Fact]
    public void ClearReceiptDateRange_ShouldFallBackToToday()
    {
        var viewModel = CreateViewModel(
            new Mock<IService_Tool_MaterialAvailabilityBoard>().Object,
            CreateLocationRangeMock("V-A0-01").Object
        );
        viewModel.ReceiptsFromDate = new DateTimeOffset(new DateTime(2026, 11, 1));
        viewModel.ReceiptsToDate = new DateTimeOffset(new DateTime(2026, 11, 6));

        viewModel.ClearReceiptDateRangeCommand.Execute(null);
        var (start, end) = viewModel.ResolveReceiptDateRange();

        start.Should().Be(DateTime.Today);
        end.Should().Be(DateTime.Today);
        viewModel.HasReceiptDateRange.Should().BeFalse();
    }

    [Fact]
    public async Task PrintAsync_ShouldPassTheReceiptRangeToThePrintableSheet()
    {
        var service = new Mock<IService_Tool_MaterialAvailabilityBoard>();
        service
            .Setup(boards => boards.LocationExistsAsync("RECV", WarehouseCode))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        service
            .Setup(boards => boards.GetBoardByLocationAsync("RECV", WarehouseCode, 30))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_MaterialAvailabilityCard> { CreateCard("T68048") }
                )
            );
        service
            .Setup(boards =>
                boards.FormatBoardForPrintAsync(
                    It.IsAny<IReadOnlyList<Model_Tool_MaterialAvailabilityCard>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    true,
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_FormattedReportDocument
                    {
                        DocumentTitle = "Material Availability Transaction Sheet",
                    }
                )
            );

        var viewModel = CreateViewModel(service.Object, CreateLocationRangeMock().Object);
        viewModel.RequestPrintAsync = _ => Task.FromResult(Model_Dao_Result_Factory.Success(true));
        viewModel.SelectLocationPrintModeAsync = () => Task.FromResult<bool?>(true);
        viewModel.SearchTerm = "RECV";
        await viewModel.SearchCommand.ExecuteAsync(null);
        viewModel.ReceiptsFromDate = new DateTimeOffset(new DateTime(2026, 11, 1));
        viewModel.ReceiptsToDate = new DateTimeOffset(new DateTime(2026, 11, 6));

        await viewModel.PrintCommand.ExecuteAsync(null);

        service.Verify(
            boards =>
                boards.FormatBoardForPrintAsync(
                    It.IsAny<IReadOnlyList<Model_Tool_MaterialAvailabilityCard>>(),
                    It.IsAny<string>(),
                    "RECV",
                    WarehouseCode,
                    "30",
                    true,
                    new DateTime(2026, 11, 1),
                    new DateTime(2026, 11, 6)
                ),
            Times.Once
        );
    }
}
