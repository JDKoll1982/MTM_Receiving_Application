using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// FR-005: every listed receiver line names a part, because a row without one cannot carry a part
/// through the guided steps.
/// </summary>
public sealed class Dao_InforVisualReceiverPartNumberTests
    : IClassFixture<InforVisualReceiverTestConnection>
{
    private readonly InforVisualReceiverTestConnection _connection;

    public Dao_InforVisualReceiverPartNumberTests(InforVisualReceiverTestConnection connection)
    {
        _connection = connection;
    }

    [SkippableFact]
    public void GetRecentReceiversAsync_ShouldReturnNoRowWithABlankPartNumber()
    {
        Skip.If(!_connection.IsConfigured, _connection.SkipReason);

        _connection
            .Rows.Should()
            .NotBeEmpty("the configured source produced an empty window, so the rule is unproven");

        _connection
            .Rows.Should()
            .OnlyContain(row => !string.IsNullOrWhiteSpace(row.PartNumber));
    }
}
