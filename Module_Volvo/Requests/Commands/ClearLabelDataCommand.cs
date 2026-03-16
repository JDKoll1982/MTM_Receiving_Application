using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to clear all completed Volvo shipments from the active queue tables
/// (<c>volvo_label_data</c>, <c>volvo_line_data</c>) by moving them to the
/// history archive tables (<c>volvo_label_history</c>, <c>volvo_line_history</c>).
/// </summary>
public record ClearLabelDataCommand : IRequest<Model_Dao_Result<int>>
{
    /// <summary>
    /// Employee identifier to stamp on all archived records as <c>archived_by</c>.
    /// </summary>
    public string ArchivedBy { get; init; } = string.Empty;
}
