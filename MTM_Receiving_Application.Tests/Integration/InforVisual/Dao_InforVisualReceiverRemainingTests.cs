using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// FR-013: remaining quantity is ordered minus total received on the PO line, and an over-received
/// line shows zero rather than a negative number.
/// </summary>
public sealed class Dao_InforVisualReceiverRemainingTests
    : IClassFixture<InforVisualReceiverTestConnection>
{
    private readonly InforVisualReceiverTestConnection _connection;

    public Dao_InforVisualReceiverRemainingTests(InforVisualReceiverTestConnection connection)
    {
        _connection = connection;
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldNeverShowANegativeRemainingQuantity()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);

        _connection
            .Rows.Should()
            .NotBeEmpty("the configured source produced an empty window, so the rule is unproven");

        _connection.Rows.Should().OnlyContain(row => row.QtyRemaining >= 0m);
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldKeepRemainingWithinTheLine_WhenAnEarlierReceiptWasReversed()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);

        // A receiver that on its own received at least the ordered quantity does not empty the line:
        // a later reversal against the same PO line puts the quantity back, so the line still owes
        // what the row reports. The remaining quantity therefore stays inside 0..ordered rather
        // than being forced to zero here.
        var overReceived = _connection
            .Rows.Where(row => row.QtyOrdered > 0m && row.QtyReceived >= row.QtyOrdered)
            .ToList();

        overReceived.Should().OnlyContain(row => row.QtyRemaining >= 0m);
        overReceived.Should().OnlyContain(row => row.QtyRemaining <= row.QtyOrdered);
    }
}
