using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack.Queries;

/// <summary>
/// Handles report-demand retrieval for the Customer Pull n' Pack feature.
/// </summary>
public class Query_CustomerPullPackReportHandler
    : IRequestHandler<
        Query_CustomerPullPackReport,
        Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>
    >
{
    private readonly Dao_CustomerPullPackDemand _demandDao;
    private readonly IService_LoggingUtility _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Query_CustomerPullPackReportHandler"/> class.
    /// </summary>
    /// <param name="demandDao"></param>
    /// <param name="logger"></param>
    public Query_CustomerPullPackReportHandler(
        Dao_CustomerPullPackDemand demandDao,
        IService_LoggingUtility logger
    )
    {
        _demandDao = demandDao;
        _logger = logger;
    }

    /// <summary>
    /// Implements Workflow 1.1 by loading the filtered demand report for one selected customer.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async Task<Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>> Handle(
        Query_CustomerPullPackReport request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Filter.CustomerId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                "Customer ID is required."
            );
        }

        _logger.LogInfo(
            $"Loading Customer Pull n' Pack report for customer '{request.Filter.CustomerId}'."
        );
        return await _demandDao.GetDemandAsync(request.Filter);
    }
}
