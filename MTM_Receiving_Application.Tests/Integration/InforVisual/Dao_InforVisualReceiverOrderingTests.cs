using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// FR-004: received date never increases down the list, and rows that share a date run by receiver
/// number descending.
/// </summary>
public sealed class Dao_InforVisualReceiverOrderingTests
    : IClassFixture<InforVisualReceiverTestConnection>
{
    private readonly InforVisualReceiverTestConnection _connection;

    public Dao_InforVisualReceiverOrderingTests(InforVisualReceiverTestConnection connection)
    {
        _connection = connection;
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldNeverIncreaseTheReceivedDateDownTheWindow()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);
        _connection.Rows.Count.Should().BeGreaterThan(1, "ordering needs at least two rows to mean anything");

        var rows = _connection.Rows;

        for (var index = 1; index < rows.Count; index++)
        {
            rows[index]
                .ReceivedDate.Should()
                .BeOnOrBefore(
                    rows[index - 1].ReceivedDate,
                    "row {0} is newer than row {1}, so the list is not newest first",
                    index,
                    index - 1
                );
        }
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldRunEqualDatesByReceiverNumberDescending()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);

        var rows = _connection.Rows;

        for (var index = 1; index < rows.Count; index++)
        {
            if (rows[index].ReceivedDate != rows[index - 1].ReceivedDate)
            {
                continue;
            }

            CompareReceiverNumbers(rows[index].ReceiverNumber, rows[index - 1].ReceiverNumber)
                .Should()
                .BeLessThanOrEqualTo(
                    0,
                    "receiver {0} follows {1} on the same date, so the tie-break is not descending",
                    rows[index].ReceiverNumber,
                    rows[index - 1].ReceiverNumber
                );
        }
    }

    private static int CompareReceiverNumbers(string left, string right)
    {
        // Receiver numbers read as numbers on the shop floor, so compare numerically when both
        // sides parse and fall back to ordinal text otherwise.
        if (
            long.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out var leftValue)
            && long.TryParse(
                right,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var rightValue
            )
        )
        {
            return leftValue.CompareTo(rightValue);
        }

        return string.CompareOrdinal(left, right);
    }
}
