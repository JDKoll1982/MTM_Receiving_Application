using FluentAssertions;
using MTM_Receiving_Application.Module_Volvo.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.Models;

public sealed class VolvoShipmentStatusTests
{
    [Theory]
    [InlineData("pending_po", VolvoShipmentStatus.PendingPo)]
    [InlineData("Pending", VolvoShipmentStatus.PendingPo)]
    [InlineData("Pending PO", VolvoShipmentStatus.PendingPo)]
    [InlineData("Pending PO Number", VolvoShipmentStatus.PendingPo)]
    [InlineData("completed", VolvoShipmentStatus.Completed)]
    [InlineData("Completed", VolvoShipmentStatus.Completed)]
    [InlineData("archived", VolvoShipmentStatus.Archived)]
    [InlineData("All", VolvoShipmentStatus.All)]
    public void NormalizeStorageValue_ShouldReturnCanonicalStatus(string rawStatus, string expected)
    {
        var normalizedStatus = VolvoShipmentStatus.NormalizeStorageValue(rawStatus);

        normalizedStatus.Should().Be(expected);
    }

    [Theory]
    [InlineData("pending_po", VolvoShipmentStatus.PendingPoDisplayName)]
    [InlineData("Pending", VolvoShipmentStatus.PendingPoDisplayName)]
    [InlineData("completed", VolvoShipmentStatus.CompletedDisplayName)]
    [InlineData("archived", VolvoShipmentStatus.ArchivedDisplayName)]
    [InlineData("in_review", "In Review")]
    public void ToDisplayName_ShouldReturnFriendlyLabel(string rawStatus, string expectedDisplay)
    {
        var displayStatus = VolvoShipmentStatus.ToDisplayName(rawStatus);

        displayStatus.Should().Be(expectedDisplay);
    }

    [Fact]
    public void ModelVolvoShipment_ShouldExposeNormalizedStatusDisplay()
    {
        var shipment = new Model_VolvoShipment
        {
            Status = "Pending",
        };

        shipment.Status.Should().Be(VolvoShipmentStatus.PendingPo);
        shipment.StatusDisplay.Should().Be(VolvoShipmentStatus.PendingPoDisplayName);
    }
}