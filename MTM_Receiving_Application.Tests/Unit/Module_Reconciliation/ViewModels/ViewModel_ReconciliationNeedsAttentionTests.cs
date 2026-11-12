using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.ViewModels;

/// <summary>
/// Needs Attention lists only the lines the module could not answer, writes a hand-set
/// location through the accepted-line path, and leaves untouched lines unresolved
/// (FR-046, FR-047).
/// </summary>
public sealed class ViewModel_ReconciliationNeedsAttentionTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly Mock<IService_ReconciliationWriteService> _writeService = new();

    [Fact]
    public void LoadShouldListOnlyUnresolvedLines()
    {
        ViewModel_ReconciliationNeedsAttention viewModel = Build();

        viewModel.Load(
            [
                Finding(Enum_ReconciliationOutcome.NotEnough, 40m),
                Finding(Enum_ReconciliationOutcome.ExtraSurplus, 15m),
                Finding(Enum_ReconciliationOutcome.Unchanged, null),
                Finding(Enum_ReconciliationOutcome.NotFound, null),
            ]
        );

        viewModel.Items.Should().HaveCount(2);
        viewModel.UnresolvedCount.Should().Be(2);
    }

    [Fact]
    public void ItemsShouldCarryTheReasonEvidenceAndRawDifference()
    {
        ViewModel_ReconciliationNeedsAttention viewModel = Build();

        viewModel.Load([Finding(Enum_ReconciliationOutcome.NotEnough, 40m)]);

        Model_ReconciliationAttentionItem item = viewModel.Items[0];
        item.Reason.Should().NotBeNullOrWhiteSpace();
        item.DifferenceText.Should().Be("40");
        item.Evidence.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitShouldWriteThroughTheAcceptedLinePathAndMarkItManuallySet()
    {
        Model_ReconciliationFinding finding = Finding(
            Enum_ReconciliationOutcome.NotEnough,
            40m
        );

        _writeService
            .Setup(service => service.AcceptAsync(finding, "R-04", It.IsAny<string>()))
            .ReturnsAsync(
                (Model_ReconciliationFinding accepted, string _, string _) =>
                {
                    accepted.Outcome = Enum_ReconciliationOutcome.ManuallySet;
                    accepted.State = Enum_ReconciliationFindingState.Written;

                    return Model_Dao_Result_Factory.Success(accepted);
                }
            );

        ViewModel_ReconciliationNeedsAttention viewModel = Build();
        viewModel.Load([finding, Finding(Enum_ReconciliationOutcome.ExtraSurplus, 15m)]);

        viewModel.Items[0].LocationInput = "R-04";
        await viewModel.SubmitAsync(viewModel.Items[0]);

        _writeService.Verify(
            service => service.AcceptAsync(finding, "R-04", It.IsAny<string>()),
            Times.Once
        );
        finding.Outcome.Should().Be(Enum_ReconciliationOutcome.ManuallySet);
        viewModel.UnresolvedCount.Should().Be(1);
    }

    [Fact]
    public async Task SubmitShouldLeaveUntouchedLinesCountedAsUnresolved()
    {
        Model_ReconciliationFinding first = Finding(Enum_ReconciliationOutcome.NotEnough, 40m);
        Model_ReconciliationFinding second = Finding(Enum_ReconciliationOutcome.NotEnough, 20m);

        _writeService
            .Setup(service => service.AcceptAsync(first, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(first));

        ViewModel_ReconciliationNeedsAttention viewModel = Build();
        viewModel.Load([first, second]);

        viewModel.Items[0].LocationInput = "R-04";
        await viewModel.SubmitAsync(viewModel.Items[0]);

        viewModel.Items.Should().ContainSingle().Which.Finding.Should().BeSameAs(second);
        viewModel.UnresolvedCount.Should().Be(1);
        second.Outcome.Should().Be(Enum_ReconciliationOutcome.NotEnough);
    }

    [Fact]
    public async Task ARejectedWriteShouldKeepTheLineAndReportTheReason()
    {
        Model_ReconciliationFinding finding = Finding(Enum_ReconciliationOutcome.NotEnough, 40m);

        _writeService
            .Setup(service =>
                service.AcceptAsync(
                    It.IsAny<Model_ReconciliationFinding>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_ReconciliationFinding>(
                    "The location was rejected."
                )
            );

        ViewModel_ReconciliationNeedsAttention viewModel = Build();
        viewModel.Load([finding]);

        viewModel.Items[0].LocationInput = "R-04";
        await viewModel.SubmitAsync(viewModel.Items[0]);

        viewModel.Items.Should().ContainSingle();
        viewModel.StatusMessage.Should().Be("The location was rejected.");
    }

    private ViewModel_ReconciliationNeedsAttention Build() =>
        new(
            _writeService.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Model_ReconciliationFinding Finding(
        Enum_ReconciliationOutcome outcome,
        decimal? difference
    )
    {
        var row = new Model_ReconciliationRow
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = Today,
            Quantity = 100,
            SavedLocation = "RECV",
            UnitOfMeasure = "EA",
        };

        var bucket = new Model_ReconciliationDestinationBucket
        {
            WarehouseId = "W1",
            LocationId = "SHIP",
            NetQuantity = 100m,
            RemainingQuantity = 0m,
            EvidenceLocations = "RECV → SHIP",
        };

        return new Model_ReconciliationFinding(
            row,
            outcome,
            "RECV",
            string.Empty,
            Enum_ReconciliationDestinationSource.Derived,
            $"Reason for {outcome}.",
            difference,
            outcome is Enum_ReconciliationOutcome.Unchanged ? null : bucket
        );
    }
}
