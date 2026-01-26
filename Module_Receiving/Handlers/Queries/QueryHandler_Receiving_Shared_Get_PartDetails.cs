using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.DTOs;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for GetPartDetailsQuery
/// Retrieves enriched part details by combining preferences and part type data
/// </summary>
public class QueryHandler_Receiving_Shared_Get_PartDetails
    : IRequestHandler<QueryRequest_Receiving_Shared_Get_PartDetails, Model_Dao_Result<Model_Receiving_DataTransferObjects_PartDetails>>
{
    private readonly Dao_Receiving_Repository_PartPreference _partPrefDao;
    private readonly Dao_Receiving_Repository_Reference _referenceDao;
    private readonly IService_LoggingUtility _logger;

    public QueryHandler_Receiving_Shared_Get_PartDetails(
        Dao_Receiving_Repository_PartPreference partPrefDao,
        Dao_Receiving_Repository_Reference referenceDao,
        IService_LoggingUtility logger)
    {
        _partPrefDao = partPrefDao;
        _referenceDao = referenceDao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<Model_Receiving_DataTransferObjects_PartDetails>> Handle(
        QueryRequest_Receiving_Shared_Get_PartDetails request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Retrieving part details for: {request.PartNumber}");

            var dto = new Model_Receiving_DataTransferObjects_PartDetails
            {
                PartNumber = request.PartNumber
            };

            // Step 1: Try to get User-scoped preferences first, then fall back to System
            var preferenceResult = await _partPrefDao.SelectByPartAsync(
                request.PartNumber,
                request.Scope,
                request.ScopeUserId);

            if (!preferenceResult.Success && request.Scope == "User")
            {
                // Fallback to System scope
                preferenceResult = await _partPrefDao.SelectByPartAsync(
                    request.PartNumber,
                    "System",
                    null);
            }

            // Step 2: Populate from preferences if found
            if (preferenceResult.Success && preferenceResult.Data != null)
            {
                var pref = preferenceResult.Data;
                dto.PartTypeId = pref.PartTypeId;
                dto.PartTypeName = pref.PartTypeName;
                dto.PartTypeCode = pref.PartTypeCode;
                dto.DefaultReceivingLocation = pref.DefaultReceivingLocation;
                dto.DefaultPackageType = pref.DefaultPackageType;
                dto.DefaultPackagesPerLoad = pref.DefaultPackagesPerLoad;
                dto.RequiresQualityHold = pref.RequiresQualityHold;
                dto.QualityHoldProcedure = pref.QualityHoldProcedure;
            }

            // Step 3: Get part type details if PartTypeId is available
            if (dto.PartTypeId.HasValue)
            {
                var partTypesResult = await _referenceDao.GetPartTypesAsync();
                if (partTypesResult.Success && partTypesResult.Data != null)
                {
                    var partType = partTypesResult.Data.FirstOrDefault(pt => pt.PartTypeId == dto.PartTypeId.Value);
                    if (partType != null)
                    {
                        dto.PartTypeName = partType.PartTypeName;
                        dto.PartTypeCode = partType.PartTypeCode;
                        dto.RequiresDiameter = partType.RequiresDiameter;
                        dto.RequiresWidth = partType.RequiresWidth;
                        dto.RequiresLength = partType.RequiresLength;
                        dto.RequiresThickness = partType.RequiresThickness;
                        dto.RequiresWeight = partType.RequiresWeight;
                    }
                }
            }

            _logger.LogInfo($"Part details retrieved for {request.PartNumber}");

            return Model_Dao_Result_Factory.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetPartDetailsQueryHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DataTransferObjects_PartDetails>(
                $"Error retrieving part details: {ex.Message}", ex);
        }
    }
}
