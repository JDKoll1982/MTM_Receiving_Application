using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_LocationReconciliationReviewTests
{
    [Fact]
    public void Initialize_ShouldHideNotFoundItemsFromVisibleUnresolvedList()
    {
        var viewModel = new ViewModel_Receiving_LocationReconciliationReview(
            new Mock<IService_ReceivingLocationReconciliation>().Object,
            new Mock<IService_ReceivingWorkflow>().Object,
            new Mock<IService_ReceivingSettings>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        var summary = new Model_ReceivingLocationReconciliationSummary();
        summary.UnresolvedItems.Add(
            new Model_ReceivingLocationReconciliationItem
            {
                PartID = "PART-1",
                Resolution = "NotFound",
                Details = "No exact quantity match was found.",
            }
        );
        summary.UnresolvedItems.Add(
            new Model_ReceivingLocationReconciliationItem
            {
                PartID = "PART-2",
                Resolution = "Ambiguous",
                Details = "Multiple exact quantity matches were found.",
            }
        );

        viewModel.Initialize(summary);

        viewModel.NeedsAttentionCount.Should().Be(1);
        viewModel.HasUnresolvedItems.Should().BeTrue();
        viewModel.VisibleUnresolvedItems.Should().ContainSingle();
        viewModel.VisibleUnresolvedItems[0].PartID.Should().Be("PART-2");
        viewModel.SummaryDescription.Should().Contain("1 row(s) that still need manual attention");
    }
}
