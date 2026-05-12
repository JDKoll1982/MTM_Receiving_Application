using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_LocationReconciliationReviewTests
{
    [Fact]
    public void Initialize_ShouldExcludeNotFoundItemsFromNeedsAttentionList()
    {
        var viewModel = CreateViewModel();
        var summary = new Model_ReceivingLocationReconciliationSummary();

        summary.UnresolvedItems.Add(
            new Model_ReceivingLocationReconciliationItem
            {
                PartID = "PART-1",
                Resolution = "NotFound",
                Details = "No transfer evidence was found.",
            }
        );
        summary.UnresolvedItems.Add(
            new Model_ReceivingLocationReconciliationItem
            {
                PartID = "PART-2",
                Resolution = "Ambiguous",
                Details = "A leftover quantity requires manual review.",
            }
        );

        viewModel.Initialize(summary);

        viewModel.NeedsAttentionCount.Should().Be(1);
        viewModel.VisibleUnresolvedItems.Should().HaveCount(1);
        viewModel.VisibleUnresolvedItems[0].PartID.Should().Be("PART-2");
        viewModel.SummaryDescription.Should().Contain("1 row(s) that still need manual transfer review");
    }

    [Fact]
    public void CurrentEvidenceSummaryText_ShouldDescribeTransferEvidenceAndUnknownSources()
    {
        var viewModel = CreateViewModel();
        var summary = new Model_ReceivingLocationReconciliationSummary();

        summary.UpdatedItems.Add(
            new Model_ReceivingLocationReconciliationItem
            {
                PartID = "PART-1",
                Resolution = "Updated",
                TransferMovementCount = 2,
                EvidenceSourceLocations = string.Empty,
                ProposedLocation = "V-A0-01",
            }
        );

        viewModel.Initialize(summary);

        viewModel.CurrentEvidenceSummaryText.Should().Be(
            "2 transfer(s) considered. Source locations: Unknown"
        );
    }

    private static ViewModel_Receiving_LocationReconciliationReview CreateViewModel()
    {
        return new ViewModel_Receiving_LocationReconciliationReview(
            new Mock<IService_ReceivingLocationReconciliation>().Object,
            new Mock<IService_ReceivingWorkflow>().Object,
            new Mock<IService_ReceivingSettings>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}