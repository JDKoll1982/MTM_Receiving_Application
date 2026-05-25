using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Query to retrieve Customer Pull n' Pack demand lines for one selected customer context.
/// </summary>
/// <param name="Filter">Demand filter and customer context for the report.</param>
public sealed record Query_CustomerPullPackReport(Model_CustomerPullPack_DemandFilter Filter)
    : IRequest<Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>>;
