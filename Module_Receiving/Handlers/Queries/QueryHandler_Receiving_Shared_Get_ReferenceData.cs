using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for QueryRequest_Receiving_Shared_Get_ReferenceData
/// Retrieves all reference data needed for dropdowns and lookups
/// </summary>
public class QueryHandler_Receiving_Shared_Get_ReferenceData
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_ReferenceData, Model_Dao_Result<Model_Receiving_DataTransferObjects_ReferenceData>>
{
    private readonly Dao_Receiving_Repository_Reference _dao;
    private readonly IService_LoggingUtility _logger;

    public QueryHandler_Receiving_Shared_Get_ReferenceData(
        Dao_Receiving_Repository_Reference dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<Model_Receiving_DataTransferObjects_ReferenceData>> Handle(
        QueryRequest_Receiving_Shared_Get_ReferenceData request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo("Loading reference data");

            var DataTransferObjects = new Model_Receiving_DataTransferObjects_ReferenceData();

            // Load all reference datasets (or only requested ones if specified)
            var loadAll = request.DatasetNames.Count == 0;

            if (loadAll || request.DatasetNames.Contains("PartTypes"))
            {
                var partTypesResult = await _dao.GetPartTypesAsync();
                if (partTypesResult.Success && partTypesResult.Data != null)
                {
                    DataTransferObjects.PartTypes = partTypesResult.Data;
                }
            }

            if (loadAll || request.DatasetNames.Contains("PackageTypes"))
            {
                var packageTypesResult = await _dao.GetPackageTypesAsync();
                if (packageTypesResult.Success && packageTypesResult.Data != null)
                {
                    DataTransferObjects.PackageTypes = packageTypesResult.Data;
                }
            }

            if (loadAll || request.DatasetNames.Contains("Locations"))
            {
                var locationsResult = await _dao.GetLocationsAsync();
                if (locationsResult.Success && locationsResult.Data != null)
                {
                    DataTransferObjects.Locations = locationsResult.Data;
                }
            }

            _logger.LogInfo($"Reference data loaded: {DataTransferObjects.PartTypes.Count} PartTypes, {DataTransferObjects.PackageTypes.Count} PackageTypes, {DataTransferObjects.Locations.Count} Locations");

            return Model_Dao_Result_Factory.Success(DataTransferObjects);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in QueryHandler_Receiving_Shared_Get_ReferenceData: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DataTransferObjects_ReferenceData>(
                $"Error loading reference data: {ex.Message}", ex);
        }
    }
}
