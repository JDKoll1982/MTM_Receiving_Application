using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to clear the dedicated active Volvo generated-label queue by moving
/// its rows to the generated-label history table.
/// </summary>
public record ClearLabelDataCommand : IRequest<Model_Dao_Result<int>>
{
    /// <summary>
    /// Employee identifier to stamp on all archived records as <c>archived_by</c>.
    /// </summary>
    public string ArchivedBy { get; init; } = string.Empty;
}
