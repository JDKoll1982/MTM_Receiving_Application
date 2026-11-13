using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// The orchestrator is the only type the page calls to turn a day into findings, so
/// this is where the day-level promises are proved: one finding per loaded row, a
/// summary that reconciles against the rows, a preview that resolves nothing, and a
/// Visual failure that stays inside its own group after one bounded retry
/// (FR-021, FR-038, FR-068, FR-069, SC-003).
/// </summary>
public sealed class Service_ReconciliationPreviewOrchestratorTests
{
    private const string MixedUnitWarningMarker = "more than one unit of measure";

    private static readonly DateOnly TestDay = new(2026, 11, 12);

    private readonly Mock<IService_ReconciliationDayReader> _dayReader = new();
    private readonly Mock<IService_ReconciliationSettings> _settings = new();
    private readonly Mock<IService_LoggingUtility> _logger = new();
    private readonly List<string> _logLines = [];
    private readonly Fake_InforVisual _inforVisual = new();

    public Service_ReconciliationPreviewOrchestratorTests()
    {
        _settings.Setup(settings => settings.GetDiscoveryWindowDaysAsync()).ReturnsAsync(3);
        _settings.Setup(settings => settings.GetStrictWindowDaysAsync()).ReturnsAsync(2);

        _logger
            .Setup(logger => logger.LogInfo(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<string, string?>((message, _) => _logLines.Add(message));
        _logger
            .Setup(logger => logger.LogWarning(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<string, string?>((message, _) => _logLines.Add(message));
        _logger
            .Setup(logger =>
                logger.LogError(It.IsAny<string>(), It.IsAny<Exception?>(), It.IsAny<string?>())
            )
            .Callback<string, Exception?, string?>((message, _, _) => _logLines.Add(message));
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldProduceExactlyOneFindingPerLoadedRow_WhenTheDayHoldsSeveralGroups()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMC0001000", "PO-066868", 5_000, 1),
            Row("MMC0001000", "PO-066868", 2_500, 2),
            Row("MMC0002000", "PO-066914", 1_000, 3),
            Row("MMC0003000", "", 700, 4, isNonPurchaseOrder: true, vendorName: "Vendor C"),
            Row("", "PO-066999", 400, 5)
        );

        StubGroup(
            "MMC0001000",
            [
                Movement("MMC0001000", "PO-066868", "SHIP", 5_000m, 1, 2),
                Movement("MMC0001000", "PO-066868", "V-C0-01", 2_500m, 3, 4),
            ]
        );
        StubGroup("MMC0002000", [Movement("MMC0002000", "PO-066914", "SHIP", 1_000m, 5, 6)]);
        StubGroup("MMC0003000", [Movement("MMC0003000", "", "SHIP", 700m, 7, 8)]);

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed.Groups.Should().HaveCount(3);
        previewed.Findings.Should().HaveCount(previewed.Rows.Count);
        previewed.Rows.Should()
            .OnlyContain(row =>
                previewed.Findings.Count(finding => ReferenceEquals(finding.Row, row)) == 1
            );
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldAccountForEveryLoadedRowInTheSummary_WhenTheDayMixesOutcomes()
    {
        Model_ReconciliationDay day = DayOf(
            [
                .. ReferenceCaseRows(),
                Row("MMC0002000", "PO-066914", 900, 6),
                Row("", "PO-066999", 400, 7),
            ]
        );

        StubGroup("MMC0001000", ReferenceCaseMovements(), onHand: 41_000m);

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed.Findings.Should().HaveCount(previewed.Rows.Count);
        previewed.Summary.Scanned.Should().Be(previewed.Rows.Count);
        previewed.Summary.Written.Should().Be(4);
        previewed.Summary.Unchanged.Should().Be(1);
        previewed.Summary.NotFound.Should().Be(1);
        previewed.Summary.Skipped.Should().Be(1);
        previewed.Summary.Unresolved.Should().Be(0);
        previewed.Summary.Error.Should().Be(0);
        previewed.Summary.TotalAccounted.Should().Be(previewed.Rows.Count);
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldResolveNothing_WhenTheDayHoldsWritableRows()
    {
        Model_ReconciliationDay day = DayOf(ReferenceCaseRows());

        StubGroup("MMC0001000", ReferenceCaseMovements(), onHand: 41_000m);

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed
            .Findings.Where(finding => finding.Outcome is Enum_ReconciliationOutcome.Updated)
            .Should()
            .HaveCount(4)
            .And.OnlyContain(finding =>
                finding.State == Enum_ReconciliationFindingState.Writable
            );
        previewed
            .Findings.Should()
            .NotContain(finding => finding.State == Enum_ReconciliationFindingState.Written);
        previewed.State.Should().Be(Enum_ReconciliationDayState.Previewed);
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldIsolateOneGroupsInforVisualFailure_WhenThatGroupsReadKeepsFailing()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMC0001000", "PO-066868", 5_000, 1),
            Row("MMC0001000", "PO-066868", 2_500, 2),
            Row("MMC0002000", "PO-066914", 1_000, 3)
        );

        _inforVisual.FailingParts.Add("MMC0001000");
        StubGroup("MMC0002000", [Movement("MMC0002000", "PO-066914", "SHIP", 1_000m, 5, 6)]);

        Model_ReconciliationDay previewed = await PreviewAsync(day);
        Guid runId = previewed.Run!.RunId;

        previewed
            .Findings.Where(finding => finding.Row.PartId == "MMC0001000")
            .Should()
            .HaveCount(2)
            .And.OnlyContain(finding => finding.Outcome == Enum_ReconciliationOutcome.Error);
        previewed
            .Findings.Should()
            .ContainSingle(finding => finding.Row.PartId == "MMC0002000")
            .Which.Outcome.Should()
            .Be(Enum_ReconciliationOutcome.Updated);
        previewed.Summary.Error.Should().Be(2);
        previewed.Summary.Written.Should().Be(1);

        _inforVisual.MovementReads.Should().HaveCount(3);
        _inforVisual.MovementReads.Count(part => part == "MMC0001000").Should().Be(2);
        _inforVisual.MovementReads.Count(part => part == "MMC0002000").Should().Be(1);
        _inforVisual.BalanceReads.Should().NotContain("MMC0001000");

        _logLines.Should().NotBeEmpty();
        _logLines.Should()
            .OnlyContain(line => line.Contains(runId.ToString(), StringComparison.Ordinal));
        _logLines
            .Should()
            .Contain(line =>
                line.Contains("retrying once", StringComparison.Ordinal)
                && line.Contains("MMC0001000", StringComparison.Ordinal)
                && line.Contains("PO-066868", StringComparison.Ordinal)
                && line.Contains("2026-11-12", StringComparison.Ordinal)
            );

        Model_ReconciliationFinding failure = previewed.Findings[0];
        failure
            .Reason.Should()
            .Contain("MMC0001000")
            .And.Contain("PO-066868")
            .And.Contain("2026-11-12");
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldDeriveTheReferenceCaseOutcomes_WhenMMC0001000IsPreviewed()
    {
        Model_ReconciliationDay day = DayOf(ReferenceCaseRows());

        StubGroup("MMC0001000", ReferenceCaseMovements(), onHand: 41_000m);

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        foreach ((int quantity, string destination) in new[]
        {
            (5_000, "SHIP"),
            (2_500, "V-C0-01"),
            (500, "R-04"),
            (1_000, "R-05"),
        })
        {
            Model_ReconciliationFinding moved = FindingFor(previewed, quantity);

            moved.Outcome.Should().Be(Enum_ReconciliationOutcome.Updated);
            moved.ProposedLocation.Should().Be(destination);
            moved.State.Should().Be(Enum_ReconciliationFindingState.Writable);
        }

        Model_ReconciliationFinding stayed = FindingFor(previewed, 41_000);

        stayed.Outcome.Should().Be(Enum_ReconciliationOutcome.Unchanged);
        stayed.SavedLocation.Should().Be("RECV");
        stayed.ProposedLocation.Should().Be("RECV");

        previewed.Groups.Should().ContainSingle();
        previewed.Groups[0].Residual!.ExpectedResidual.Should().Be(41_000m);
        previewed.Groups[0].Residual!.BalanceAsOf.Should().Be(41_000m);
        previewed.Summary.Written.Should().Be(4);
        previewed.Summary.Unchanged.Should().Be(1);
        previewed.Summary.Unresolved.Should().Be(0);
        previewed.Summary.Error.Should().Be(0);
        previewed.Summary.TotalAccounted.Should().Be(5);
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldAttributeTheRunToTheDaysDistinctInforVisualUsers()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMC0001000", "PO-066868", 5_000, 1),
            Row("MMC0002000", "PO-066914", 1_000, 2),
            Row("MMC0003000", "PO-066999", 700, 3)
        );

        StubGroup(
            "MMC0001000",
            [
                Movement("MMC0001000", "PO-066868", "SHIP", 5_000m, 1, 2, userId: "vuser"),
                Movement("MMC0001000", "PO-066868", "V-C0-01", 2_500m, 3, 4, userId: "VUSER"),
            ],
            onHand: 42_500m
        );
        StubGroup(
            "MMC0002000",
            [Movement("MMC0002000", "PO-066914", "SHIP", 1_000m, 5, 6, userId: "vuser2")],
            onHand: 0m
        );
        StubGroup(
            "MMC0003000",
            [Movement("MMC0003000", "PO-066999", "SHIP", 700m, 7, 8, userId: "")],
            onHand: 0m
        );

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed.Run!.VisualUserId.Should().Be("vuser, vuser2");
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldWarnOnceAndChangeNoOutcome_WhenOneGroupHoldsTwoUnitsOfMeasure()
    {
        StubGroup(
            "MMC0001000",
            [Movement("MMC0001000", "PO-066868", "SHIP", 7_500m, 1, 2)],
            onHand: 0m
        );

        Model_ReconciliationDay mixedUnits = DayOf(
            Row("MMC0001000", "PO-066868", 5_000, 1, unitOfMeasure: "EA"),
            Row("MMC0001000", "PO-066868", 2_500, 2, unitOfMeasure: "LB")
        );
        Model_ReconciliationDay oneUnit = DayOf(
            Row("MMC0001000", "PO-066868", 5_000, 1),
            Row("MMC0001000", "PO-066868", 2_500, 2)
        );

        Model_ReconciliationDay mixedPreview = await PreviewAsync(mixedUnits);
        string warning = MixedUnitWarnings().Should().ContainSingle().Subject;
        Guid runId = mixedPreview.Run!.RunId;

        warning.Should().Contain(runId.ToString());
        warning.Should().Contain("MMC0001000");
        warning.Should().Contain("2026-11-12");
        warning.Should().Contain("PO-066868");

        _logLines.Clear();

        Model_ReconciliationDay singlePreview = await PreviewAsync(oneUnit);

        MixedUnitWarnings().Should().BeEmpty();
        mixedPreview
            .Findings.Select(finding => finding.Outcome)
            .Should()
            .Equal(singlePreview.Findings.Select(finding => finding.Outcome));
        mixedPreview.Summary.Written.Should().Be(singlePreview.Summary.Written);
        mixedPreview.Summary.TotalAccounted.Should().Be(singlePreview.Summary.TotalAccounted);
    }

    private List<string> MixedUnitWarnings() =>
        _logLines
            .Where(line => line.Contains(MixedUnitWarningMarker, StringComparison.Ordinal))
            .ToList();

    private static Model_ReconciliationFinding FindingFor(
        Model_ReconciliationDay day,
        int quantity
    ) => day.Findings.Single(finding => finding.Row.Quantity == quantity);

    private async Task<Model_ReconciliationDay> PreviewAsync(Model_ReconciliationDay day)
    {
        _dayReader
            .Setup(reader => reader.GetDayAsync(day.Date))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(day));

        Model_Dao_Result<Model_ReconciliationDay> result = await Build().PreviewDayAsync(day.Date);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        return result.Data!;
    }

    private Service_ReconciliationPreviewOrchestrator Build() =>
        new(
            _dayReader.Object,
            new Service_ReconciliationGroupResolver(new Service_ReconciliationNonPurchaseOrderResolver()),
            new Service_ReconciliationMovementSelector(),
            new Service_ReconciliationBucketBuilder(),
            new Service_ReconciliationAllocator(),
            new Service_ReconciliationQuantityProof(),
            _settings.Object,
            _inforVisual,
            _logger.Object
        );

    [Fact]
    public async Task PreviewDayAsync_ShouldReportUnchanged_WhenTheMaterialWasReceivedStraightIntoTheSavedLocation()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMF0008050", "072689", 3_063, 1, savedLocation: "S-00")
        );
        StubGroup("MMF0008050", []);
        _inforVisual.ReceiptsByPart["MMF0008050"] =
        [
            Receipt("MMF0008050", "S-00", 3_063m, "PO-072689"),
        ];

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed.Summary.NotFound.Should().Be(0);
        previewed.Summary.Unchanged.Should().Be(1);
        previewed.Findings.Should()
            .ContainSingle()
            .Which.Outcome.Should()
            .Be(Enum_ReconciliationOutcome.Unchanged);
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldNameTheReceiptLocation_WhenTheSavedLocationIsNotWhereTheMaterialLanded()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMF0008050", "072689", 3_063, 1, savedLocation: "S-99")
        );
        StubGroup("MMF0008050", []);
        _inforVisual.ReceiptsByPart["MMF0008050"] =
        [
            Receipt("MMF0008050", "S-00", 3_063m, "PO-072689"),
        ];

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed.Summary.NotFound.Should().Be(1);
        previewed.Findings.Should().ContainSingle().Which.Reason.Should().Contain("S-00");
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldKeepNotFound_WhenThereIsNeitherMovementNorReceipt()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMF0008050", "072689", 3_063, 1, savedLocation: "S-00")
        );
        StubGroup("MMF0008050", []);

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        previewed.Summary.NotFound.Should().Be(1);
        _inforVisual.ReceiptReads.Should().Contain("MMF0008050");
    }

    [Fact]
    public async Task PreviewDayAsync_ShouldLeaveTheRowsUnchanged_WhenMoreMaterialArrivedAtTheReceivingLocation()
    {
        Model_ReconciliationDay day = DayOf(
            Row("MMC0000652", "071776", 12_640, 1, savedLocation: "S-00"),
            Row("MMC0000652", "071776", 12_680, 2, savedLocation: "S-00"),
            Row("MMC0000652", "071776", 12_670, 3, savedLocation: "S-00")
        );

        StubGroup(
            "MMC0000652",
            [Movement("MMC0000652", "071776", "S-00", 121_950m, 1, 2)],
            onHand: 121_950m
        );

        Model_ReconciliationDay previewed = await PreviewAsync(day);

        // The rows were received into S-00 and nothing moved them out, so the 121,950
        // that arrived there is somebody else's receipt and must not read as surplus.
        previewed.Findings.Should()
            .OnlyContain(finding => finding.Outcome == Enum_ReconciliationOutcome.Unchanged);
        previewed.Summary.Unchanged.Should().Be(3);
        previewed.Summary.Written.Should().Be(0);
    }

    private void StubGroup(
        string partId,
        IReadOnlyList<Model_ReconciliationMovement> movements,
        decimal onHand = 0m
    )
    {
        _inforVisual.MovementsByPart[partId] = [.. movements];
        _inforVisual.BalancesByPart[partId] = [Balance(partId, onHand)];
    }

    private static Model_ReconciliationDay DayOf(params Model_ReconciliationRow[] rows) =>
        DayOf((IReadOnlyList<Model_ReconciliationRow>)rows);

    private static Model_ReconciliationDay DayOf(IReadOnlyList<Model_ReconciliationRow> rows) =>
        new()
        {
            Date = TestDay,
            HasActivity = true,
            Rows = rows,
            State = Enum_ReconciliationDayState.Loaded,
        };

    private static Model_ReconciliationRow[] ReferenceCaseRows() =>
        [
            Row("MMC0001000", "PO-066868", 5_000, 1),
            Row("MMC0001000", "PO-066868", 2_500, 2),
            Row("MMC0001000", "PO-066868", 500, 3),
            Row("MMC0001000", "PO-066868", 1_000, 4),
            Row("MMC0001000", "PO-066868", 41_000, 5),
        ];

    private static Model_ReconciliationMovement[] ReferenceCaseMovements() =>
        [
            Movement("MMC0001000", "PO-066868", "SHIP", 5_000m, 1, 2),
            Movement("MMC0001000", "PO-066868", "V-C0-01", 2_500m, 3, 4),
            Movement("MMC0001000", "PO-066868", "R-04", 500m, 5, 6),
            Movement("MMC0001000", "PO-066868", "R-05", 1_000m, 7, 8),
        ];

    private static Model_ReconciliationRow Row(
        string partId,
        string purchaseOrderNumber,
        int quantity,
        int loadNumber,
        bool isNonPurchaseOrder = false,
        string vendorName = "",
        string unitOfMeasure = "EA",
        string savedLocation = "RECV"
    ) =>
        new()
        {
            PartId = partId,
            PurchaseOrderNumber = purchaseOrderNumber,
            PurchaseOrderLineNumber = "1",
            TransactionDate = TestDay,
            Quantity = quantity,
            SavedLocation = savedLocation,
            UnitOfMeasure = unitOfMeasure,
            LoadNumber = loadNumber,
            IsNonPurchaseOrder = isNonPurchaseOrder,
            VendorName = vendorName,
        };

    private static Model_ReconciliationReceipt Receipt(
        string partId,
        string locationId,
        decimal quantity,
        string purchaseOrderNumber
    ) =>
        new()
        {
            PartId = partId,
            WarehouseId = "002",
            LocationId = locationId,
            Quantity = quantity,
            PurchaseOrderNumber = purchaseOrderNumber,
            PurchaseOrderLineNumber = "1",
            ReceiptDateTime = new DateTime(2026, 11, 12, 7, 24, 40, DateTimeKind.Unspecified),
            ReceiptUserId = "JKOLL",
            TransactionId = 3_870_899,
        };

    private static Model_ReconciliationMovement Movement(
        string partId,
        string purchaseOrderNumber,
        string destination,
        decimal quantity,
        int sourceTransactionId,
        int destinationTransactionId,
        string userId = "vuser"
    ) =>
        new()
        {
            PartId = partId,
            PurchaseOrderNumber = purchaseOrderNumber,
            PurchaseOrderLineNumber = "1",
            SourceWarehouseId = "W1",
            SourceLocationId = "RECV",
            DestinationWarehouseId = "W1",
            DestinationLocationId = destination,
            Quantity = quantity,
            TransactionDate = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
            TransactionUserId = userId,
            SourceTransactionId = sourceTransactionId,
            DestinationTransactionId = destinationTransactionId,
        };

    private static Model_ReconciliationLocationBalance Balance(string partId, decimal onHand) =>
        new()
        {
            PartId = partId,
            WarehouseId = "W1",
            LocationId = "RECV",
            OnHand = onHand,
            MovementAfterWindow = 0m,
        };

    private sealed class Fake_InforVisual : Dao_ReconciliationInforVisual
    {
        public Fake_InforVisual()
            : base("Server=test;Database=test;ApplicationIntent=ReadOnly") { }

        public Dictionary<string, List<Model_ReconciliationMovement>> MovementsByPart { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, List<Model_ReconciliationLocationBalance>> BalancesByPart { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, List<Model_ReconciliationReceipt>> ReceiptsByPart { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public List<string> ReceiptReads { get; } = [];

        public HashSet<string> FailingParts { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> MovementReads { get; } = [];

        public List<string> BalanceReads { get; } = [];

        public override Task<Model_Dao_Result<List<Model_ReconciliationMovement>>>
            GetTransferMovementsAsync(string partId, DateOnly receivedDate, int windowDays)
        {
            MovementReads.Add(partId);

            if (FailingParts.Contains(partId))
            {
                return Task.FromResult(
                    Model_Dao_Result_Factory.Failure<List<Model_ReconciliationMovement>>(
                        "Infor Visual did not answer."
                    )
                );
            }

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    MovementsByPart.TryGetValue(partId, out var movements)
                        ? movements
                        : new List<Model_ReconciliationMovement>()
                )
            );
        }

        public override Task<Model_Dao_Result<List<Model_ReconciliationLocationBalance>>>
            GetLocationBalancesAsync(string partId, DateOnly afterDate)
        {
            BalanceReads.Add(partId);

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    BalancesByPart.TryGetValue(partId, out var balances)
                        ? balances
                        : new List<Model_ReconciliationLocationBalance>()
                )
            );
        }

        public override Task<Model_Dao_Result<List<Model_ReconciliationReceipt>>>
            GetReceivingReceiptsAsync(string partId, DateOnly receivedDate, int windowDays)
        {
            ReceiptReads.Add(partId);

            return Task.FromResult(
                Model_Dao_Result_Factory.Success(
                    ReceiptsByPart.TryGetValue(partId, out var receipts)
                        ? receipts
                        : new List<Model_ReconciliationReceipt>()
                )
            );
        }
    }
}
