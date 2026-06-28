using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Builds a print-ready Customer Pull n' Pack context from the active report or queue state.
/// </summary>
/// <param name="PrintMode"></param>
/// <param name="CustomerId"></param>
/// <param name="CustomerName"></param>
/// <param name="ActiveFiltersSummary"></param>
/// <param name="DemandLines"></param>
/// <param name="WaitlistEntries"></param>
/// <param name="Title"></param>
public sealed record Query_CustomerPullPackPrintContext(
    Enum_CustomerPullPackPrintMode PrintMode,
    string CustomerId,
    string CustomerName,
    string ActiveFiltersSummary,
    IReadOnlyList<Model_CustomerPullPack_DemandLine> DemandLines,
    IReadOnlyList<Model_CustomerPullPack_WaitlistEntry> WaitlistEntries,
    string Title = ""
) : IRequest<Model_Dao_Result<Model_CustomerPullPack_PrintContext>>;
