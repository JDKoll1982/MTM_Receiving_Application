using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// FR-008: a receiver with several lines for one part appears once, and the quantity received on
/// that row is the sum of those lines.
/// </summary>
public sealed class Dao_InforVisualReceiverMergeTests
    : IClassFixture<InforVisualReceiverTestConnection>
{
    private readonly InforVisualReceiverTestConnection _connection;

    public Dao_InforVisualReceiverMergeTests(InforVisualReceiverTestConnection connection)
    {
        _connection = connection;
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldReturnOneRowPerReceiverAndPart()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);
        _connection.Rows.Should().NotBeEmpty("the configured source produced an empty window");

        var duplicates = _connection
            .Rows.GroupBy(
                row => $"{row.ReceiverNumber}|{row.PartNumber}",
                StringComparer.OrdinalIgnoreCase
            )
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({group.Count()} rows)")
            .ToList();

        duplicates
            .Should()
            .BeEmpty("a receiver and part must collapse to one row: " + string.Join(", ", duplicates));
    }

    [SkippableFact]
    public async Task GetRecentReceiversAsync_ShouldSumTheReceiptLines_ForOneListedRow()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);
        _connection.Rows.Should().NotBeEmpty("the configured source produced an empty window");

        var sample = _connection.Rows[0];

        var recounted = await _connection.GetReceivedQuantityForPartAsync(
            sample.ReceiverNumber,
            sample.PartNumber
        );

        recounted
            .Should()
            .Be(
                sample.QtyReceived,
                "the row for receiver {0} part {1} must be the sum of its receipt lines",
                sample.ReceiverNumber,
                sample.PartNumber
            );
    }
}
