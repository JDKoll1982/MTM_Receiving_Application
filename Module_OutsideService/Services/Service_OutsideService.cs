using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_OutsideService.Contracts;
using MTM_Receiving_Application.Module_OutsideService.Data;
using MTM_Receiving_Application.Module_OutsideService.Models;

namespace MTM_Receiving_Application.Module_OutsideService.Services;

/// <summary>
/// Business service for the Outside Service module.
/// </summary>
public class Service_OutsideService : IService_OutsideService
{
    private readonly Dao_OutsideServiceRequest _requestDao;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;
    private readonly bool _useInforVisualMockData;

    /// <summary>
    /// Initializes a new service instance.
    /// </summary>
    public Service_OutsideService(
        Dao_OutsideServiceRequest requestDao,
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger,
        IOptions<InforVisualSettings> inforVisualSettings
    )
    {
        _requestDao = requestDao ?? throw new ArgumentNullException(nameof(requestDao));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(inforVisualSettings);
        _useInforVisualMockData = inforVisualSettings.Value.UseMockData;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_OutsideServiceRequest>> CreateRequestAsync(
        Model_OutsideServiceRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        foreach (var line in request.Lines)
        {
            var partValidation = await _inforVisual.PartExistsAsync(line.PartId);
            if (!partValidation.IsSuccess)
            {
                if (_useInforVisualMockData)
                {
                    _logger.LogWarning(
                        $"Infor Visual part validation failed for '{line.PartId}'. Using Outside Service mock validation because mock data mode is enabled.",
                        nameof(Service_OutsideService)
                    );
                    continue;
                }

                return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                    $"Part '{line.PartId}' is not a valid Infor Visual part.",
                    partValidation.Exception
                );
            }

            if (!partValidation.Data)
            {
                return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                    $"Part '{line.PartId}' is not a valid Infor Visual part."
                );
            }
        }

        _logger.LogInfo($"Creating Outside Service request with {request.Lines.Count} line(s).");
        return await _requestDao.CreateRequestAsync(request);
    }

    /// <inheritdoc />
    public Task<Model_Dao_Result<List<Model_OutsideServiceRequestLine>>> GetOpenLinesAsync()
    {
        return _requestDao.GetOpenLinesAsync();
    }

    /// <inheritdoc />
    public Task<Model_Dao_Result<List<Model_OutsideServiceRequestLine>>> GetCompletedLinesAsync()
    {
        return _requestDao.GetCompletedLinesAsync();
    }

    /// <inheritdoc />
    public Task<Model_Dao_Result<bool>> ValidatePartAsync(string partId)
    {
        return ValidatePartInternalAsync(partId);
    }

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_OutsideServicePartMatchSuggestion>>
    > GetPartSuggestionsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServicePartMatchSuggestion>>(
                "Search term cannot be empty."
            );
        }

        var result = await _inforVisual.FuzzySearchPartsAsync(searchTerm.Trim());
        if (!result.IsSuccess || result.Data is null || result.Data.Count == 0)
        {
            if (_useInforVisualMockData)
            {
                _logger.LogWarning(
                    $"Infor Visual part suggestion lookup failed for '{searchTerm}'. Using Outside Service mock suggestions because mock data mode is enabled.",
                    nameof(Service_OutsideService)
                );

                return Model_Dao_Result_Factory.Success(CreateMockPartSuggestions(searchTerm));
            }

            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServicePartMatchSuggestion>>(
                result.ErrorMessage,
                result.Exception
            );
        }

        var suggestions = result
            .Data.Take(10)
            .Select(
                (match, index) =>
                    new Model_OutsideServicePartMatchSuggestion
                    {
                        PartId = match.Key,
                        Description = match.Detail ?? string.Empty,
                        MatchReason =
                            index == 0
                                ? "Best match based on the typed value"
                                : "Similar part found in Infor Visual",
                    }
            )
            .ToList();

        return Model_Dao_Result_Factory.Success(suggestions);
    }

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_OutsideServiceVendorSuggestion>>
    > GetVendorSuggestionsAsync(string partId)
    {
        if (string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceVendorSuggestion>>(
                "Part ID cannot be empty."
            );
        }

        var history = await _inforVisual.GetOutsideServiceHistoryByPartAsync(partId.Trim());
        if (!history.IsSuccess || history.Data is null || history.Data.Count == 0)
        {
            if (_useInforVisualMockData)
            {
                _logger.LogWarning(
                    $"Infor Visual vendor history lookup failed for '{partId}'. Using Outside Service mock vendor suggestions because mock data mode is enabled.",
                    nameof(Service_OutsideService)
                );

                return Model_Dao_Result_Factory.Success(CreateMockVendorSuggestions(partId));
            }

            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceVendorSuggestion>>(
                history.ErrorMessage,
                history.Exception
            );
        }

        var suggestions = history
            .Data.Where(record =>
                !string.IsNullOrWhiteSpace(record.VendorID)
                && !string.IsNullOrWhiteSpace(record.VendorName)
            )
            .GroupBy(record => new
            {
                record.VendorID,
                record.VendorName,
                record.VendorCity,
                record.VendorState,
            })
            .Select(group => new Model_OutsideServiceVendorSuggestion
            {
                VendorId = group.Key.VendorID ?? string.Empty,
                VendorName = group.Key.VendorName ?? string.Empty,
                LocationDetail = string.Join(
                    ", ",
                    new[] { group.Key.VendorCity, group.Key.VendorState }.Where(value =>
                        !string.IsNullOrWhiteSpace(value)
                    )
                ),
                LastDispatchDate = group.Max(record => record.DispatchDate),
                DispatchCount = group.Count(),
            })
            .OrderByDescending(suggestion => suggestion.LastDispatchDate)
            .ThenBy(suggestion => suggestion.VendorName)
            .ToList();

        return Model_Dao_Result_Factory.Success(suggestions);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> SaveSetupAsync(Model_OutsideServiceRequestLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        if (line.OutsideServiceRequestLineId <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Line identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(line.SetupVendorName))
        {
            return Model_Dao_Result_Factory.Failure("Vendor name is required.");
        }

        if (string.IsNullOrWhiteSpace(line.BOLNumber))
        {
            return Model_Dao_Result_Factory.Failure("BOL number is required.");
        }

        _logger.LogInfo(
            $"Saving Outside Service setup for line {line.OutsideServiceRequestLineId}."
        );
        return await _requestDao.SaveSetupAsync(line);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> MarkCompleteAsync(int lineId, string? completionNotes)
    {
        if (lineId <= 0)
        {
            return Model_Dao_Result_Factory.Failure("Line identifier is required.");
        }

        _logger.LogInfo($"Marking Outside Service line {lineId} complete.");
        return await _requestDao.MarkCompleteAsync(lineId, completionNotes);
    }

    private async Task<Model_Dao_Result<bool>> ValidatePartInternalAsync(string partId)
    {
        var result = await _inforVisual.PartExistsAsync(partId);
        if (!result.IsSuccess && _useInforVisualMockData)
        {
            _logger.LogWarning(
                $"Infor Visual exact part validation failed for '{partId}'. Using Outside Service mock validation because mock data mode is enabled.",
                nameof(Service_OutsideService)
            );
            return Model_Dao_Result_Factory.Success(true);
        }

        return result;
    }

    private static List<Model_OutsideServicePartMatchSuggestion> CreateMockPartSuggestions(
        string searchTerm
    )
    {
        var normalizedTerm = searchTerm.Trim().ToUpperInvariant();
        return
        [
            new Model_OutsideServicePartMatchSuggestion
            {
                PartId = $"MOCK-{normalizedTerm}-001",
                Description = "Mock Part - Outside Service heat treat candidate",
                MatchReason = "Mock data mode best match",
            },
            new Model_OutsideServicePartMatchSuggestion
            {
                PartId = $"MOCK-{normalizedTerm}-002",
                Description = "Mock Part - Outside Service plating candidate",
                MatchReason = "Mock data mode similar match",
            },
        ];
    }

    private static List<Model_OutsideServiceVendorSuggestion> CreateMockVendorSuggestions(
        string partId
    )
    {
        var normalizedPartId = partId.Trim().ToUpperInvariant();
        return
        [
            new Model_OutsideServiceVendorSuggestion
            {
                VendorId = "MOCK-VENDOR-001",
                VendorName = "Acme Heat Treating Co.",
                LocationDetail = $"Detroit, MI | {normalizedPartId}",
                LastDispatchDate = DateTime.Today.AddDays(-7),
                DispatchCount = 3,
            },
            new Model_OutsideServiceVendorSuggestion
            {
                VendorId = "MOCK-VENDOR-002",
                VendorName = "Precision Plating Inc.",
                LocationDetail = $"Grand Rapids, MI | {normalizedPartId}",
                LastDispatchDate = DateTime.Today.AddDays(-21),
                DispatchCount = 2,
            },
        ];
    }

    private static Model_Dao_Result<Model_OutsideServiceRequest> ValidateRequest(
        Model_OutsideServiceRequest request
    )
    {
        if (request.Lines.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                "At least one line is required before the request can be saved."
            );
        }

        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.PartId))
            {
                return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                    $"Line {line.LineNumber} is missing a part ID."
                );
            }

            if (line.PackageCount <= 0)
            {
                return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                    $"Line {line.LineNumber} must have at least one package."
                );
            }

            if (line.Packages.Count != line.PackageCount)
            {
                return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                    $"Line {line.LineNumber} package rows do not match the package count."
                );
            }

            if (line.Packages.Any(package => package.PackageQuantity <= 0))
            {
                return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                    $"Line {line.LineNumber} contains an invalid package quantity."
                );
            }
        }

        return Model_Dao_Result_Factory.Success(request);
    }
}
