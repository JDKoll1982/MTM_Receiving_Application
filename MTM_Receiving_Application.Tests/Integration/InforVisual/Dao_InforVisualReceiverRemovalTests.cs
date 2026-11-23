using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// FR-007: a receiver the source marks for removal or purge never appears, so the operator cannot
/// pick a row that no longer exists.
/// </summary>
public sealed class Dao_InforVisualReceiverRemovalTests
    : IClassFixture<InforVisualReceiverTestConnection>
{
    private readonly InforVisualReceiverTestConnection _connection;

    public Dao_InforVisualReceiverRemovalTests(InforVisualReceiverTestConnection connection)
    {
        _connection = connection;
    }

    [SkippableFact]
    public async Task GetRecentReceiversAsync_ShouldReturnNoReceiverTheSourceMarkedForPurge()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);
        _connection.Rows.Should().NotBeEmpty("the configured source produced an empty window");

        var marked = await _connection.GetMarkedForPurgeReceiverNumbersAsync(
            _connection.Rows.Select(row => row.ReceiverNumber)
        );

        marked
            .Should()
            .BeEmpty(
                "the source marks these receivers for removal: "
                    + string.Join(", ", marked)
            );
    }
}
