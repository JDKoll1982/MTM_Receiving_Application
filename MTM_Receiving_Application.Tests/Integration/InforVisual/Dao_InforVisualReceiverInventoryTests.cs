using System.Linq;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// FR-006: only receivers where inventory is being added appear, which means the receiving line
/// names a warehouse. The location may be blank, and the row then still shows its warehouse.
/// </summary>
public sealed class Dao_InforVisualReceiverInventoryTests
    : IClassFixture<InforVisualReceiverTestConnection>
{
    private readonly InforVisualReceiverTestConnection _connection;

    public Dao_InforVisualReceiverInventoryTests(InforVisualReceiverTestConnection connection)
    {
        _connection = connection;
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldReturnNoRowWithABlankWarehouse()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);

        _connection
            .Rows.Should()
            .NotBeEmpty("the configured source produced an empty window, so the rule is unproven");

        _connection
            .Rows.Should()
            .OnlyContain(row => !string.IsNullOrWhiteSpace(row.Warehouse));
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldStillNameTheWarehouse_WhenTheLocationIsBlank()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);

        var rowsWithoutLocation = _connection
            .Rows.Where(row => string.IsNullOrWhiteSpace(row.LocationReceivedTo))
            .ToList();

        rowsWithoutLocation.Should().OnlyContain(row => !string.IsNullOrWhiteSpace(row.Warehouse));
    }
}
