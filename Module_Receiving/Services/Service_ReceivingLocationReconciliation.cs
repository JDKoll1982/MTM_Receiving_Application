using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Suggests where a receipt should be put, from Infor Visual location evidence.
/// The reconciliation half of this service moved to the Receiving Validator module,
/// so nothing here reconciles or writes a location (FR-006, FR-066, SC-009).
/// </summary>
public sealed class Service_ReceivingLocationReconciliation
    : IService_ReceivingLocationReconciliation
{
    private readonly IService_InforVisual _inforVisual;

    /// <summary>Creates the recommendation service over Infor Visual.</summary>
    /// <param name="inforVisual">The Infor Visual read service.</param>
    public Service_ReceivingLocationReconciliation(IService_InforVisual inforVisual)
    {
        _inforVisual = inforVisual;
    }

    /// <inheritdoc />
    public async Task<
        Model_Dao_Result<List<Model_ReceivingRecommendedLocation>>
    > GetRecommendedLocationsAsync(
        string poNumber,
        string partId,
        string? poLineNumber,
        DateTime? receivedDate
    )
    {
        if (string.IsNullOrWhiteSpace(poNumber) || string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Success(new List<Model_ReceivingRecommendedLocation>());
        }

        var evidenceResult = await _inforVisual.GetReceivingLocationEvidenceAsync(
            poNumber,
            partId,
            poLineNumber,
            receivedDate
        );
        if (!evidenceResult.IsSuccess || evidenceResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingRecommendedLocation>>(
                evidenceResult.ErrorMessage,
                evidenceResult.Exception
            );
        }

        var recommendations = evidenceResult
            .Data.Where(row =>
                string.IsNullOrWhiteSpace(row.CurrentLocationId) is false
                && row.CurrentQuantity > 0
            )
            .GroupBy(
                row => $"{Normalize(row.CurrentWarehouseId)}|{Normalize(row.CurrentLocationId)}",
                StringComparer.OrdinalIgnoreCase
            )
            .Select(group =>
            {
                var sample = group.First();
                var latestActivityDate = group
                    .Select(row => row.MatchedTransactionDate ?? row.LatestTransactionDate)
                    .Where(date => date.HasValue)
                    .OrderByDescending(date => date)
                    .FirstOrDefault();

                return new Model_ReceivingRecommendedLocation
                {
                    WarehouseId = sample.CurrentWarehouseId?.Trim() ?? string.Empty,
                    LocationId = sample.CurrentLocationId?.Trim() ?? string.Empty,
                    QuantityOnHand = group.Sum(row => row.CurrentQuantity),
                    ReasonText = latestActivityDate.HasValue
                        ? $"latest activity {latestActivityDate.Value:MM/dd/yyyy}"
                        : "current stock evidence",
                };
            })
            .OrderByDescending(location => location.QuantityOnHand)
            .ThenBy(location => location.DisplayLocation, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Model_Dao_Result_Factory.Success(recommendations);
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
}
