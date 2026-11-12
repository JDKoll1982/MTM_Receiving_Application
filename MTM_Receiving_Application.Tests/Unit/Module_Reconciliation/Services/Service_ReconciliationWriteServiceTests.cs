using System;
using System.Collections.Generic;
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
/// Accepting a line re-proves it, writes once, and refuses everything that is not
/// writable (FR-041, FR-052, FR-053, FR-054, DS-06).
/// </summary>
public sealed class Service_ReconciliationWriteServiceTests
{
    private const string DummyConnectionString =
        "Server=localhost;Database=test;Uid=test;Pwd=test;Connection Timeout=1;";

    private const string VisualConnectionString =
        "Data Source=localhost;Initial Catalog=Test;Integrated Security=True;ApplicationIntent=ReadOnly;";

    private static readonly DateOnly WorkingDay = new(2026, 11, 12);

    private readonly Mock<Dao_ReconciliationReceivingRows> _receivingRows = new(
        DummyConnectionString,
        Mock.Of<IService_LoggingUtility>()
    );

    private readonly Mock<Dao_ReconciliationInforVisual> _inforVisual = new(
        VisualConnectionString,
        Mock.Of<IService_LoggingUtility>()
    );

    private readonly Mock<IService_ReconciliationSettings> _settings = new();

    public Service_ReconciliationWriteServiceTests()
    {
        _settings.Setup(service => service.GetStrictWindowDaysAsync()).ReturnsAsync(1);
        _inforVisual
            .Setup(dao => dao.GetLocationBalancesAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_ReconciliationLocationBalance>
                    {
                        new()
                        {
                            PartId = "MMC0001000",
                            WarehouseId = "W1",
                            LocationId = "RECV",
                            OnHand = 0m,
                            MovementAfterWindow = 0m,
                        },
                    }
                )
            );
    }

    [Fact]
    public async Task AcceptShouldReproveTheLineAndWriteExactlyOnce()
    {
        _receivingRows
            .Setup(dao =>
                dao.UpdateLocationAsync(
                    It.IsAny<Enum_ReconciliationRowSource>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(1, 1));

        (Model_ReconciliationFinding finding, Model_ReconciliationGroup group) = WritableFinding();

        Model_Dao_Result<Model_ReconciliationFinding> result = await Service().AcceptAsync(
            finding,
            "SHIP",
            "operator.one"
        );

        result.IsSuccess.Should().BeTrue();
        finding.State.Should().Be(Enum_ReconciliationFindingState.Written);
        finding.Outcome.Should().Be(Enum_ReconciliationOutcome.Updated);
        _inforVisual.Verify(
            dao => dao.GetLocationBalancesAsync(group.Key.PartId, It.IsAny<DateOnly>()),
            Times.Once
        );
        _receivingRows.Verify(
            dao =>
                dao.UpdateLocationAsync(
                    finding.Row.RowSource,
                    finding.Row.RecordId,
                    "SHIP",
                    "operator.one"
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AcceptShouldRefuseAFindingThatIsNotWritable()
    {
        (Model_ReconciliationFinding finding, _) = WritableFinding();
        finding.State = Enum_ReconciliationFindingState.Proposed;

        Model_Dao_Result<Model_ReconciliationFinding> result = await Service().AcceptAsync(
            finding,
            "SHIP",
            "operator.one"
        );

        result.IsSuccess.Should().BeFalse();
        _receivingRows.Verify(
            dao =>
                dao.UpdateLocationAsync(
                    It.IsAny<Enum_ReconciliationRowSource>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task AcceptShouldRefuseAProposalWhoseBucketNoLongerPassesTheProof()
    {
        (Model_ReconciliationFinding finding, Model_ReconciliationGroup group) = WritableFinding();
        finding.ProofContext!.Allocations = [];
        group.Allocations = [];

        Model_Dao_Result<Model_ReconciliationFinding> result = await Service().AcceptAsync(
            finding,
            "SHIP",
            "operator.one"
        );

        result.IsSuccess.Should().BeFalse();
        finding.State.Should().Be(Enum_ReconciliationFindingState.Unresolved);
        _receivingRows.Verify(
            dao =>
                dao.UpdateLocationAsync(
                    It.IsAny<Enum_ReconciliationRowSource>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task AcceptShouldRecordAnOperatorDestinationAsManuallySet()
    {
        _receivingRows
            .Setup(dao =>
                dao.UpdateLocationAsync(
                    It.IsAny<Enum_ReconciliationRowSource>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(1, 1));

        Model_ReconciliationFinding finding = UnresolvedFinding();

        Model_Dao_Result<Model_ReconciliationFinding> result = await Service().AcceptAsync(
            finding,
            "R-04",
            "operator.one"
        );

        result.IsSuccess.Should().BeTrue();
        finding.Outcome.Should().Be(Enum_ReconciliationOutcome.ManuallySet);
        finding.DestinationSource.Should().Be(Enum_ReconciliationDestinationSource.Operator);
    }

    [Fact]
    public async Task AcceptShouldLeaveAFailedWriteUnresolvedWithTheReason()
    {
        _receivingRows
            .Setup(dao =>
                dao.UpdateLocationAsync(
                    It.IsAny<Enum_ReconciliationRowSource>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(0, 0));

        Model_ReconciliationFinding finding = UnresolvedFinding();

        Model_Dao_Result<Model_ReconciliationFinding> result = await Service().AcceptAsync(
            finding,
            "R-04",
            "operator.one"
        );

        result.IsSuccess.Should().BeFalse();
        finding.State.Should().Be(Enum_ReconciliationFindingState.Unresolved);
        finding.Reason.Should().NotBeNullOrWhiteSpace();
    }

    private Service_ReconciliationWriteService Service() =>
        new(
            _receivingRows.Object,
            _inforVisual.Object,
            new Service_ReconciliationQuantityProof(),
            _settings.Object
        );

    private static (Model_ReconciliationFinding Finding, Model_ReconciliationGroup Group)
        WritableFinding()
    {
        Model_ReconciliationRow row = Row(100);
        var bucket = new Model_ReconciliationDestinationBucket
        {
            WarehouseId = "W1",
            LocationId = "SHIP",
            NetQuantity = 100m,
            RemainingQuantity = 0m,
            AllocatedQuantity = 100m,
            LatestMovementAt = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Unspecified),
            TransactionUserId = "vuser",
        };

        var group = new Model_ReconciliationGroup
        {
            Key = new Model_ReconciliationGroupKey("PO-066868", "MMC0001000", WorkingDay, string.Empty),
            Rows = [row],
            SourceLocations = new HashSet<string>(StringComparer.Ordinal) { "RECV" },
            ReceivingLocation = "RECV",
            Buckets = [bucket],
            Balances = [],
        };

        group.Allocations =
        [
            new Model_ReconciliationAllocation
            {
                Row = row,
                Bucket = bucket,
                Quantity = 100m,
                MatchedExactly = true,
            },
        ];

        var proofContext = new Model_ReconciliationProofContext
        {
            Key = group.Key,
            ReceivingLocation = group.ReceivingLocation,
            Rows = group.Rows,
            Buckets = group.Buckets,
            Allocations = group.Allocations,
            Balances = group.Balances,
        };

        var finding = new Model_ReconciliationFinding(
            row,
            Enum_ReconciliationOutcome.Updated,
            "RECV",
            "SHIP",
            Enum_ReconciliationDestinationSource.Derived,
            "The material moved to SHIP.",
            null,
            bucket
        )
        {
            State = Enum_ReconciliationFindingState.Writable,
            ProofContext = proofContext,
        };

        return (finding, group);
    }

    private static Model_ReconciliationFinding UnresolvedFinding() =>
        new(
            Row(50),
            Enum_ReconciliationOutcome.NotEnough,
            "RECV",
            string.Empty,
            Enum_ReconciliationDestinationSource.Derived,
            "No destination could cover this row.",
            50m,
            null
        );

    private static Model_ReconciliationRow Row(int quantity) =>
        new()
        {
            RowSource = Enum_ReconciliationRowSource.CurrentLabel,
            RecordId = 11,
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = WorkingDay,
            Quantity = quantity,
            SavedLocation = "RECV",
            UnitOfMeasure = "EA",
        };
}
