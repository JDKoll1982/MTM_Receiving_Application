using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Covers the enabled-part list, which is the gate on the whole feature: no part on the list means
/// no lot is ever marked. Adding a part opens the gate and removing one closes it again, but a part
/// already marked keeps every marking it has, because the shape and color are on the receipt and on
/// the floor rather than in this list.
/// </summary>
public sealed class ViewModel_Tool_LotMarkingTests
{
    private const string TestPartId = "TEST-21-28841";

    private static Model_Tool_LotMarkingScope CreatePartRow(
        int id,
        string partId,
        bool isEnabled = true
    ) => new() { Id = id, PartId = partId, IsEnabled = isEnabled };

    private static ViewModel_Tool_LotMarking CreateViewModel(
        IService_Tool_LotMarking service,
        IService_InforVisual? inforVisual = null,
        IService_ShipRecToolsSettings? settings = null
    ) =>
        new(
            service,
            settings ?? CreateSettings().Object,
            inforVisual ?? new Mock<IService_InforVisual>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object,
            new Mock<IService_Help>().Object
        );

    /// <summary>
    /// Settings that answer the way a first run does: the full palette, no saved date range and every
    /// column shown. Without this a bare mock would answer nothing and the list would fall back to
    /// defaults silently rather than by design.
    /// </summary>
    private static Mock<IService_ShipRecToolsSettings> CreateSettings()
    {
        var settings = new Mock<IService_ShipRecToolsSettings>();

        settings.Setup(s => s.GetLotMarkingEnabledAsync()).ReturnsAsync(true);
        settings.Setup(s => s.GetLotMarkingShapeFirstAsync()).ReturnsAsync(true);
        settings
            .Setup(s => s.GetLotMarkingEnabledShapesAsync())
            .ReturnsAsync(Model_LotMarkingPalette.Shapes.Select(shape => shape.Glyph).ToList());
        settings
            .Setup(s => s.GetLotMarkingEnabledColorsAsync())
            .ReturnsAsync(Model_LotMarkingPalette.Colors.Select(color => color.Code).ToList());
        settings
            .Setup(s => s.GetLotMarkingListFilterAsync())
            .ReturnsAsync(Model_Tool_LotMarkingFilter.Default);
        settings
            .Setup(s => s.GetLotMarkingListVisibleColumnKeysAsync())
            .ReturnsAsync(
                new HashSet<string>(
                    Model_Tool_LotMarkingColumnCatalog.DefaultVisibleKeys,
                    StringComparer.OrdinalIgnoreCase
                )
            );
        settings
            .Setup(s => s.SaveLotMarkingListFilterAsync(It.IsAny<Model_Tool_LotMarkingFilter>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        settings
            .Setup(s =>
                s.SaveLotMarkingListVisibleColumnKeysAsync(It.IsAny<IReadOnlyCollection<string>>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        return settings;
    }

    private static Mock<IService_Tool_LotMarking> CreateServiceReturningParts(
        params Model_Tool_LotMarkingScope[] parts
    )
    {
        var service = new Mock<IService_Tool_LotMarking>();
        service
            .Setup(s => s.GetEnabledPartsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(parts.ToList()));
        return service;
    }

    [Fact]
    public async Task SearchPartsCommand_ShouldLeaveThePartListEmpty_WhenNoPartIsEnabled()
    {
        var service = CreateServiceReturningParts();
        var viewModel = CreateViewModel(service.Object);

        await viewModel.SearchPartsCommand.ExecuteAsync(null);

        viewModel.Parts.Should().BeEmpty();
        viewModel.IsPartListEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task AddPartCommand_ShouldAddTheConfirmedPart_WhenExactlyOneCandidateMatches()
    {
        var service = CreateServiceReturningParts();
        service
            .Setup(s => s.AddPartAsync(TestPartId, It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        service
            .Setup(s => s.GetEnabledPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_LotMarkingScope>
                    {
                        CreatePartRow(1, TestPartId),
                    }
                )
            );

        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(s => s.FuzzySearchPartsAsync(TestPartId))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = TestPartId, Label = TestPartId },
                    }
                )
            );

        var viewModel = CreateViewModel(service.Object, inforVisual.Object);
        viewModel.PartSearchText = TestPartId;

        await viewModel.AddPartCommand.ExecuteAsync(null);

        service.Verify(s => s.AddPartAsync(TestPartId, It.IsAny<string>()), Times.Once);
        viewModel
            .Parts.Should()
            .ContainSingle(part => part.PartId == TestPartId && part.IsEnabled);
        viewModel.PartSearchText.Should().BeEmpty();
    }

    [Fact]
    public async Task RemovePartRowAsync_ShouldDeactivateTheRowAndKeepTheRecordedMarkings_WhenTheRowIsRemoved()
    {
        var row = CreatePartRow(7, TestPartId);
        var recorded = new Model_Tool_LotMarking
        {
            Id = 3,
            HeatNumber = "TEST-HEAT-1001",
            HeatNumberNormalized = "TESTHEAT1001",
            ShapeCode = "⚡",
            ColorCode = "BLUE",
        };

        var service = CreateServiceReturningParts(row);
        service
            .Setup(s => s.GetLedgerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Model_Tool_LotMarkingFilter?>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking> { recorded }));
        service
            .Setup(s => s.SetPartEnabledAsync(row.Id, false))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(service.Object);
        await viewModel.SearchPartsCommand.ExecuteAsync(null);
        await viewModel.SearchMarkingsCommand.ExecuteAsync(null);

        await viewModel.RemovePartRowAsync(row);

        service.Verify(s => s.SetPartEnabledAsync(row.Id, false), Times.Once);
        service.Verify(s => s.RemovePartAsync(It.IsAny<int>()), Times.Never);
        viewModel
            .Markings.Should()
            .ContainSingle(marking => marking.Id == recorded.Id && marking.ShapeCode == "⚡");
    }

    [Fact]
    public async Task SelectedPart_ShouldReadThatPartsLots_WhenTheListHasNoPartChosenToBeginWith()
    {
        var row = CreatePartRow(9, TestPartId);
        var service = CreateServiceReturningParts(row);
        var askedFor = new TaskCompletionSource<string?>();

        // The read is started from the property change rather than awaited there, so the test waits
        // for the call itself instead of sleeping and hoping.
        service
            .Setup(s => s.GetLedgerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Model_Tool_LotMarkingFilter?>()))
            .Callback((string? _, string? partId, Model_Tool_LotMarkingFilter? _) => askedFor.TrySetResult(partId))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking>()));

        var viewModel = CreateViewModel(service.Object);
        await viewModel.SearchPartsCommand.ExecuteAsync(null);

        viewModel.SelectedPart = row;

        var chosen = await askedFor.Task.WaitAsync(TimeSpan.FromSeconds(5));
        chosen.Should().Be(TestPartId);
    }

    [Fact]
    public async Task SelectedPart_ShouldAskForEveryPart_WhenNoPartIsChosen()
    {
        var row = CreatePartRow(11, TestPartId);
        var service = CreateServiceReturningParts(row);
        var askedFor = new TaskCompletionSource<string?>();
        service
            .Setup(s => s.GetLedgerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Model_Tool_LotMarkingFilter?>()))
            .Callback((string? _, string? partId, Model_Tool_LotMarkingFilter? _) => askedFor.TrySetResult(partId))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking>()));

        var viewModel = CreateViewModel(service.Object);

        await viewModel.SearchMarkingsCommand.ExecuteAsync(null);

        var chosen = await askedFor.Task.WaitAsync(TimeSpan.FromSeconds(5));
        chosen.Should().BeNull();
    }

    [Fact]
    public async Task ApplyFiltersAsync_ShouldSaveTheChosenRangeAndColumns_WhenTheFilterIsApplied()
    {
        var service = CreateServiceReturningParts();
        service
            .Setup(s =>
                s.GetLedgerAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Model_Tool_LotMarkingFilter?>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking>()));

        Model_Tool_LotMarkingFilter? savedFilter = null;
        IReadOnlyCollection<string>? savedColumns = null;

        var settings = CreateSettings();
        settings
            .Setup(s => s.SaveLotMarkingListFilterAsync(It.IsAny<Model_Tool_LotMarkingFilter>()))
            .Callback((Model_Tool_LotMarkingFilter filter) => savedFilter = filter)
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        settings
            .Setup(s =>
                s.SaveLotMarkingListVisibleColumnKeysAsync(It.IsAny<IReadOnlyCollection<string>>())
            )
            .Callback((IReadOnlyCollection<string> keys) => savedColumns = keys)
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(service.Object, settings: settings.Object);

        viewModel.FromDate = new DateTimeOffset(new DateTime(2026, 8, 1));
        viewModel.ToDate = new DateTimeOffset(new DateTime(2026, 8, 31));
        viewModel.SelectedSortOption = Model_Tool_LotMarkingSortOption.FromField(
            Enum_LotMarkingSortField.Supplier
        );

        var heatColumn = viewModel.ColumnOptions.Single(option =>
            option.Key == Model_Tool_LotMarkingColumnCatalog.Heat
        );
        heatColumn.IsVisible = false;

        await viewModel.ApplyFiltersAsync();

        savedFilter.Should().NotBeNull();
        savedFilter!.FromDate.Should().Be(new DateTime(2026, 8, 1));
        savedFilter.ToDate.Should().Be(new DateTime(2026, 8, 31));
        savedFilter.SortField.Should().Be(Enum_LotMarkingSortField.Supplier);

        savedColumns.Should().NotBeNull();
        savedColumns.Should().NotContain(Model_Tool_LotMarkingColumnCatalog.Heat);
        savedColumns.Should().Contain(Model_Tool_LotMarkingColumnCatalog.Skids);

        viewModel
            .ShowHeatColumn.Should()
            .BeFalse("a column turned off belongs to neither the grid nor the printed sheet");
    }

    [Fact]
    public async Task ApplyFiltersAsync_ShouldNotSaveOrRead_WhenTheRangeRunsBackwards()
    {
        var service = CreateServiceReturningParts();
        var readTheList = false;
        service
            .Setup(s =>
                s.GetLedgerAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Model_Tool_LotMarkingFilter?>()
                )
            )
            .Callback(() => readTheList = true)
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_Tool_LotMarking>()));

        var settings = CreateSettings();
        var savedAnything = false;
        settings
            .Setup(s => s.SaveLotMarkingListFilterAsync(It.IsAny<Model_Tool_LotMarkingFilter>()))
            .Callback(() => savedAnything = true)
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var viewModel = CreateViewModel(service.Object, settings: settings.Object);

        viewModel.FromDate = new DateTimeOffset(new DateTime(2026, 9, 30));
        viewModel.ToDate = new DateTimeOffset(new DateTime(2026, 8, 1));

        await viewModel.ApplyFiltersAsync();

        savedAnything.Should().BeFalse("a typo should not become the saved preference");
        readTheList.Should().BeFalse("an impossible range cannot match, so nothing is read");
    }

    [Fact]
    public void ResetFilters_ShouldShowEveryColumnAndClearTheRange_WhenItIsUsed()
    {
        var service = CreateServiceReturningParts();
        var viewModel = CreateViewModel(service.Object);

        viewModel.FromDate = new DateTimeOffset(new DateTime(2026, 8, 1));
        viewModel
            .ColumnOptions.Single(option => option.Key == Model_Tool_LotMarkingColumnCatalog.Heat)
            .IsVisible = false;

        viewModel.ResetFilters();

        viewModel.FromDate.Should().BeNull();
        viewModel.ToDate.Should().BeNull();
        viewModel.HasActiveDateRange.Should().BeFalse();
        viewModel.ShowHeatColumn.Should().BeTrue();
    }

    [Fact]
    public async Task PrintMarkingsAsync_ShouldPrintOnlyTheChosenColumns_WhenAColumnIsHidden()
    {
        var service = CreateServiceReturningParts();
        service
            .Setup(s =>
                s.GetLedgerAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Model_Tool_LotMarkingFilter?>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_LotMarking>
                    {
                        new()
                        {
                            Id = 1,
                            HeatNumber = "TEST-HEAT-9701",
                            SupplierId = "TEST-SUP-A",
                            ShapeCode = "⚡",
                            ColorCode = "TEAL",
                            PartsText = "TEST-PART-1",
                            LineCount = 4,
                            LastLineDate = new DateTime(2026, 5, 6),
                        },
                    }
                )
            );

        var viewModel = CreateViewModel(service.Object);
        Model_FormattedReportDocument? printed = null;
        viewModel.RequestPrintAsync = document =>
        {
            printed = document;
            return Task.FromResult(Model_Dao_Result_Factory.Success(true));
        };

        viewModel.FromDate = new DateTimeOffset(new DateTime(2026, 5, 1));
        viewModel.ToDate = new DateTimeOffset(new DateTime(2026, 5, 31));
        viewModel
            .ColumnOptions.Single(option => option.Key == Model_Tool_LotMarkingColumnCatalog.Skids)
            .IsVisible = false;

        await viewModel.SearchMarkingsCommand.ExecuteAsync(null);
        await viewModel.PrintMarkingsAsync();

        printed.Should().NotBeNull();
        printed!.HtmlFragment.Should().Contain("TEST-HEAT-9701");
        printed.HtmlFragment.Should().Contain("Last received");
        printed.HtmlFragment.Should().Contain("2026-05-01 to 2026-05-31");
        printed.HtmlFragment.Should().NotContain(">Skids<");
    }
}
