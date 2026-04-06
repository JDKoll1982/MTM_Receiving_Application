using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Reconciles saved receiving locations using InforVisual receipt history and current inventory.
/// </summary>
public sealed class Service_ReceivingLocationReconciliation
    : IService_ReceivingLocationReconciliation
{
    private static readonly DateTime AllHistoryStartDate = new(2000, 1, 1);
    private static readonly Regex CanonicalPoPattern = new(
        @"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[A-Za-z]?)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private readonly IService_MySQL_Receiving _mySqlReceiving;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ReceivingSettings _receivingSettings;
    private readonly IService_UserSessionManager _sessionManager;

    public Service_ReceivingLocationReconciliation(
        IService_MySQL_Receiving mySqlReceiving,
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger,
        IService_ReceivingSettings receivingSettings,
        IService_UserSessionManager sessionManager
    )
    {
        _mySqlReceiving = mySqlReceiving;
        _inforVisual = inforVisual;
        _logger = logger;
        _receivingSettings = receivingSettings;
        _sessionManager = sessionManager;
    }

    public async Task<
        Model_Dao_Result<Model_ReceivingLocationReconciliationSummary>
    > PreviewLocationsAsync(bool includeAllHistory)
    {
        try
        {
            var summary = new Model_ReceivingLocationReconciliationSummary
            {
                IncludeAllHistory = includeAllHistory,
            };

            var currentLabelsResult = await _mySqlReceiving.GetCurrentLabelDataAsync();
            if (!currentLabelsResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationSummary>(
                    currentLabelsResult.ErrorMessage
                );
            }

            var historyStartDate = includeAllHistory ? AllHistoryStartDate : DateTime.Today;
            var historyEndDate = DateTime.Today.AddDays(1);
            var historyResult = await _mySqlReceiving.GetAllReceivingLoadsAsync(
                historyStartDate,
                historyEndDate
            );
            if (!historyResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationSummary>(
                    historyResult.ErrorMessage
                );
            }

            var currentLabelRows = currentLabelsResult.Data ?? [];
            var historyRows = historyResult.Data ?? [];
            var ignoredLocations = await GetIgnoredLocationsAsync();

            summary.CurrentLabelRowsScanned = currentLabelRows.Count;
            summary.HistoryRowsScanned = historyRows.Count;

            var currentLabelItems = await ReconcileRowsAsync(
                currentLabelRows,
                "Current Labels",
                ignoredLocations
            );
            foreach (var item in currentLabelItems)
            {
                RecordPreviewDecision(item, summary);
            }

            var historyItems = await ReconcileRowsAsync(historyRows, "History", ignoredLocations);
            foreach (var item in historyItems)
            {
                RecordPreviewDecision(item, summary);
            }

            return Model_Dao_Result_Factory.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Receiving location reconciliation preview failed: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationSummary>(
                $"Receiving location reconciliation preview failed: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<Model_ReceivingLocationReconciliationItem>> ApplyLocationUpdateAsync(
        Model_ReceivingLocationReconciliationItem item
    )
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!string.Equals(item.Resolution, "Updated", StringComparison.OrdinalIgnoreCase))
        {
            return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationItem>(
                "Only rows with an Updated reconciliation result can be saved."
            );
        }

        if (item.SourceLoad == null)
        {
            return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationItem>(
                "The reconciliation row is missing its source load data."
            );
        }

        try
        {
            item.SourceLoad.InitialLocation = item.ProposedLocation;

            if (string.Equals(item.DataSource, "History", StringComparison.OrdinalIgnoreCase))
            {
                await _mySqlReceiving.UpdateReceivingLoadsAsync([item.SourceLoad]);
            }
            else
            {
                await _mySqlReceiving.UpdateCurrentLabelDataAsync([item.SourceLoad]);
            }

            return Model_Dao_Result_Factory.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to apply location reconciliation update for part {item.PartID}, PO {item.PONumber}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationItem>(
                $"Failed to save the reconciled location update: {ex.Message}",
                ex
            );
        }
    }

    public async Task<
        Model_Dao_Result<Model_ReceivingLocationReconciliationSummary>
    > ReconcileLocationsAsync(bool includeAllHistory)
    {
        try
        {
            var previewResult = await PreviewLocationsAsync(includeAllHistory);
            if (!previewResult.IsSuccess || previewResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationSummary>(
                    previewResult.ErrorMessage
                );
            }

            var summary = previewResult.Data;

            foreach (var item in summary.UpdatedItems)
            {
                var applyResult = await ApplyLocationUpdateAsync(item);
                if (!applyResult.IsSuccess)
                {
                    summary.ErrorCount++;
                    item.Details = applyResult.ErrorMessage;
                    summary.UnresolvedItems.Add(item);
                    continue;
                }

                if (string.Equals(item.DataSource, "History", StringComparison.OrdinalIgnoreCase))
                {
                    summary.HistoryRowsUpdated++;
                }
                else
                {
                    summary.CurrentLabelRowsUpdated++;
                }
            }

            _logger.LogInfo(
                $"Receiving location reconciliation complete. Updated {summary.TotalRowsUpdated} of {summary.TotalRowsScanned} scanned rows."
            );

            return Model_Dao_Result_Factory.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Receiving location reconciliation failed: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationSummary>(
                $"Receiving location reconciliation failed: {ex.Message}",
                ex
            );
        }
    }

    private async Task<List<Model_ReceivingLocationReconciliationItem>> ReconcileRowsAsync(
        IEnumerable<Model_ReceivingLoad> loads,
        string dataSource,
        HashSet<string> ignoredLocations
    )
    {
        var items = new List<Model_ReceivingLocationReconciliationItem>();
        var validLoads = new List<Model_ReceivingLoad>();

        foreach (var load in loads)
        {
            var item = CreateBaseItem(load, dataSource);

            if (load.IsNonPOItem)
            {
                item.Resolution = "Skipped";
                item.Details =
                    "Non-PO rows cannot be reconciled against InforVisual purchase-order history.";
                items.Add(item);
                continue;
            }

            if (string.IsNullOrWhiteSpace(load.PartID) || string.IsNullOrWhiteSpace(load.PoNumber))
            {
                item.Resolution = "Skipped";
                item.Details = "Part ID and PO number are both required for location reconciliation.";
                items.Add(item);
                continue;
            }

            var inforVisualPoNumber = NormalizeInforVisualPoNumber(load.PoNumber);
            if (string.IsNullOrWhiteSpace(inforVisualPoNumber))
            {
                item.Resolution = "Skipped";
                item.Details = "PO number could not be normalized to InforVisual format.";
                items.Add(item);
                continue;
            }

            validLoads.Add(load);
        }

        foreach (
            var receiptGroup in validLoads.GroupBy(load =>
                new ReconciliationGroupKey(
                    NormalizeInforVisualPoNumber(load.PoNumber),
                    Normalize(load.PartID),
                    Normalize(load.PoLineNumber),
                    load.ReceivedDate.Date
                ))
        )
        {
            var templateLoad = receiptGroup.First();
            var evidenceResult = await _inforVisual.GetReceivingLocationEvidenceAsync(
                receiptGroup.Key.PoNumber,
                templateLoad.PartID.Trim(),
                string.IsNullOrWhiteSpace(templateLoad.PoLineNumber)
                    ? null
                    : templateLoad.PoLineNumber.Trim(),
                templateLoad.ReceivedDate
            );

            if (!evidenceResult.IsSuccess)
            {
                items.AddRange(
                    receiptGroup.Select(load =>
                    {
                        var item = CreateBaseItem(load, dataSource);
                        item.Resolution = "Error";
                        item.Details = evidenceResult.ErrorMessage;
                        return item;
                    })
                );
                continue;
            }

            var historyResult = await _inforVisual.GetReceivingLocationTransactionHistoryAsync(
                receiptGroup.Key.PoNumber,
                templateLoad.PartID.Trim(),
                string.IsNullOrWhiteSpace(templateLoad.PoLineNumber)
                    ? null
                    : templateLoad.PoLineNumber.Trim(),
                templateLoad.ReceivedDate
            );

            var transactionHistory = historyResult.IsSuccess
                ? historyResult.Data ?? []
                : new List<Model_InforVisualLocationTransaction>();

            items.AddRange(
                ResolveLocationDecisions(
                    receiptGroup.ToList(),
                    dataSource,
                    evidenceResult.Data ?? [],
                    transactionHistory,
                    ignoredLocations
                )
            );
        }

        return items;
    }

    private static Model_ReceivingLocationReconciliationItem CreateBaseItem(
        Model_ReceivingLoad load,
        string dataSource
    )
    {
        return new Model_ReceivingLocationReconciliationItem
        {
            DataSource = dataSource,
            LoadId = load.LoadID,
            LabelDataRecordId = load.LabelDataRecordID,
            HistoryRecordId = load.HistoryRecordID,
            PartID = load.PartID,
            PONumber = load.PoNumber ?? string.Empty,
            POLineNumber = load.PoLineNumber,
            ReceivedDate = load.ReceivedDate,
            ExistingLocation = load.InitialLocation ?? string.Empty,
            SavedRowQuantity = load.WeightQuantity,
            QuantityMoved = 0,
            MatchedLocationQuantity = 0,
            QuantityUnitOfMeasure = load.UnitOfMeasure,
            SourceLoad = load,
        };
    }

    private static List<Model_ReceivingLocationReconciliationItem> ResolveLocationDecisions(
        List<Model_ReceivingLoad> loads,
        string dataSource,
        List<Model_InforVisualLocationEvidence> evidenceRows,
        List<Model_InforVisualLocationTransaction> transactionHistoryRows,
        HashSet<string> ignoredLocations
    )
    {
        var items = loads
            .Select(load => CreateBaseItem(load, dataSource))
            .OrderByDescending(item => item.SavedRowQuantity)
            .ThenBy(item => item.SourceLoad?.LoadNumber ?? int.MaxValue)
            .ThenBy(item => item.LoadId)
            .ToList();

        var allBuckets = BuildCandidateBuckets(evidenceRows);
        var historyTracker = new ReconciliationHistoryTracker(transactionHistoryRows);
        var availableBuckets = allBuckets
            .Where(bucket => !ignoredLocations.Contains(bucket.NormalizedLocation))
            .ToList();
        var receiptCount = evidenceRows.Select(row => row.ReceiptCount).DefaultIfEmpty(0).Max();

        if (allBuckets.Count == 0)
        {
            foreach (var item in items)
            {
                item.Resolution = "NotFound";
                item.Details =
                    receiptCount > 0
                        ? "Receipt history exists, but no current on-hand inventory location could be identified for the part."
                        : "No matching InforVisual receipt history or current inventory was found for the row.";
            }

            return items;
        }

        if (availableBuckets.Count == 0)
        {
            var ignoredLocationList = string.Join(
                ", ",
                allBuckets.Select(bucket => bucket.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase)
            );

            foreach (var item in items)
            {
                item.Resolution = "IgnoredBySettings";
                item.Details =
                    $"All candidate destination locations are currently ignored by user preference: {ignoredLocationList}.";
            }

            return items;
        }

        foreach (var item in items)
        {
            var selection = SelectBestBucket(item, availableBuckets, historyTracker);
            if (selection.Bucket is null)
            {
                if (
                    TryResolvePendingVisualReceipt(
                        item,
                        loads,
                        allBuckets,
                        evidenceRows,
                        historyTracker,
                        out var pendingDetails
                    )
                )
                {
                    item.Resolution = "PendingVisualReceipt";
                    item.Details = pendingDetails;
                    continue;
                }

                item.Resolution = selection.Resolution;
                item.Details = selection.Details;
                continue;
            }

            var selectedBucket = selection.Bucket;
            var quantityBeforeAllocation = selectedBucket.RemainingQuantity;

            item.ProposedLocation = selectedBucket.LocationId;
            item.MatchedLocationQuantity = quantityBeforeAllocation;
            item.QuantityMoved = quantityBeforeAllocation;

            if (
                selection.UsesExactHistoryQuantity
                && historyTracker.TryConsumeExact(
                    selectedBucket.LocationId,
                    item.SavedRowQuantity,
                    out var matchedTransaction
                )
            )
            {
                item.MovedByUserId = matchedTransaction?.UserId ?? string.Empty;
                item.MovedAt = matchedTransaction?.TransactionDate;
            }
            else
            {
                item.MovedByUserId = selectedBucket.MovedByUserId;
                item.MovedAt = selectedBucket.MovedAt;
            }

            item.AllocationMethod = selection.AllocationMethod;
            item.QuantityDifference = Math.Abs(quantityBeforeAllocation - item.SavedRowQuantity);
            item.Resolution = selection.Resolution;

            item.Details = string.Create(
                CultureInfo.InvariantCulture,
                $"{selection.Details} Remaining destination quantity before allocation: {quantityBeforeAllocation:0.##} {item.QuantityUnitOfMeasure}."
            );

            selectedBucket.RemainingQuantity = Math.Max(
                0,
                selectedBucket.RemainingQuantity - item.SavedRowQuantity
            );
        }

        return items;
    }

    private static List<ReconciliationLocationBucket> BuildCandidateBuckets(
        IEnumerable<Model_InforVisualLocationEvidence> evidenceRows
    )
    {
        return evidenceRows
            .Where(row => !string.IsNullOrWhiteSpace(row.CurrentLocationId))
            .GroupBy(row => new
            {
                Warehouse = Normalize(row.CurrentWarehouseId),
                Location = Normalize(row.CurrentLocationId),
            })
            .Select(group =>
            {
                var evidence = group
                    .OrderByDescending(row => row.MatchedTransactionDate ?? row.LatestTransactionDate)
                    .ThenByDescending(row => row.CurrentQuantity)
                    .First();

                var matchedTransactionQuantity = group.Sum(row => row.MatchedTransactionQuantity);
                var currentQuantity = group.Max(row => row.CurrentQuantity);
                var availableQuantity = matchedTransactionQuantity > 0
                    ? matchedTransactionQuantity
                    : currentQuantity;

                return new ReconciliationLocationBucket
                {
                    WarehouseId = evidence.CurrentWarehouseId.Trim(),
                    LocationId = evidence.CurrentLocationId.Trim(),
                    CurrentQuantity = currentQuantity,
                    EvidenceQuantity = availableQuantity,
                    RemainingQuantity = availableQuantity,
                    AllocationBasis = matchedTransactionQuantity > 0
                        ? "Matched PO transfer quantity"
                        : "Current on-hand inventory quantity",
                    MovedByUserId = !string.IsNullOrWhiteSpace(evidence.MatchedTransactionUserId)
                        ? evidence.MatchedTransactionUserId.Trim()
                        : evidence.LatestTransactionUserId.Trim(),
                    MovedAt = evidence.MatchedTransactionDate
                        ?? evidence.LatestTransactionDate
                        ?? evidence.LatestReceiptEvidenceDate,
                };
            })
            .Where(bucket => bucket.RemainingQuantity > 0)
            .OrderByDescending(bucket => bucket.RemainingQuantity)
            .ThenBy(bucket => bucket.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static BucketSelectionResult SelectBestBucket(
        Model_ReceivingLocationReconciliationItem item,
        IReadOnlyList<ReconciliationLocationBucket> buckets,
        ReconciliationHistoryTracker historyTracker
    )
    {
        var candidates = buckets.Where(bucket => bucket.RemainingQuantity > 0).ToList();
        if (candidates.Count == 0)
        {
            return new BucketSelectionResult(
                null,
                "Skipped",
                string.Empty,
                "No remaining destination quantity was available after earlier rows in the same receipt group were allocated.",
                false
            );
        }

        var sameLocationHistoryCandidates = candidates
            .Where(bucket =>
                LocationsEqual(bucket.LocationId, item.ExistingLocation)
                && bucket.RemainingQuantity >= item.SavedRowQuantity
                && historyTracker.HasExact(bucket.LocationId, item.SavedRowQuantity)
            )
            .ToList();

        if (sameLocationHistoryCandidates.Count == 1)
        {
            return new BucketSelectionResult(
                sameLocationHistoryCandidates[0],
                "Unchanged",
                "Same location and quantity history match",
                "The current location already has an exact matching movement for this load quantity.",
                true
            );
        }

        var exactHistoryCandidates = candidates
            .Where(bucket =>
                bucket.RemainingQuantity >= item.SavedRowQuantity
                && historyTracker.HasExact(bucket.LocationId, item.SavedRowQuantity)
            )
            .ToList();

        if (exactHistoryCandidates.Count == 1)
        {
            var resolution = LocationsEqual(exactHistoryCandidates[0].LocationId, item.ExistingLocation)
                ? "Unchanged"
                : "Updated";

            return new BucketSelectionResult(
                exactHistoryCandidates[0],
                resolution,
                "Exact quantity transaction-history match",
                $"Transaction history contains one exact movement match for this load quantity in {exactHistoryCandidates[0].DisplayName}.",
                true
            );
        }

        var newestExactHistoryCandidate = SelectUniqueNewestHistoryCandidate(
            exactHistoryCandidates,
            item.SavedRowQuantity,
            historyTracker
        );
        if (newestExactHistoryCandidate is not null)
        {
            var resolution = LocationsEqual(newestExactHistoryCandidate.LocationId, item.ExistingLocation)
                ? "Unchanged"
                : "Updated";

            return new BucketSelectionResult(
                newestExactHistoryCandidate,
                resolution,
                "Newest exact quantity transaction-history match",
                $"Transaction history shows the newest exact movement for this load quantity at {newestExactHistoryCandidate.DisplayName}.",
                true
            );
        }

        var sameLocationExactByTotal = candidates.FirstOrDefault(bucket =>
            LocationsEqual(bucket.LocationId, item.ExistingLocation)
            && bucket.RemainingQuantity == item.SavedRowQuantity
        );
        if (sameLocationExactByTotal is not null)
        {
            return new BucketSelectionResult(
                sameLocationExactByTotal,
                "Unchanged",
                "Same location and exact total match",
                "The current location total exactly matches this saved load quantity.",
                false
            );
        }

        var exactMatches = candidates
            .Where(bucket => bucket.RemainingQuantity == item.SavedRowQuantity)
            .ToList();
        if (exactMatches.Count == 1)
        {
            var resolution = LocationsEqual(exactMatches[0].LocationId, item.ExistingLocation)
                ? "Unchanged"
                : "Updated";

            return new BucketSelectionResult(
                exactMatches[0],
                resolution,
                "Exact quantity match",
                $"Exact quantity match selected {exactMatches[0].DisplayName} using {exactMatches[0].AllocationBasis.ToLowerInvariant()}.",
                false
            );
        }

        if (exactMatches.Count > 1)
        {
            var sameLocationExactMatch = exactMatches.FirstOrDefault(bucket =>
                LocationsEqual(bucket.LocationId, item.ExistingLocation)
            );
            if (sameLocationExactMatch is not null)
            {
                return new BucketSelectionResult(
                    sameLocationExactMatch,
                    "Unchanged",
                    "Same location exact total match",
                    $"The current location {sameLocationExactMatch.DisplayName} is one of the exact quantity matches, so the load can stay where it is.",
                    false
                );
            }

            var newestExactBucket = SelectUniqueNewestBucket(exactMatches);
            if (newestExactBucket is not null)
            {
                return new BucketSelectionResult(
                    newestExactBucket,
                    "Updated",
                    "Newest exact quantity evidence",
                    $"Multiple exact quantity totals existed, so the newest evidence location {newestExactBucket.DisplayName} was selected.",
                    false
                );
            }

            return new BucketSelectionResult(
                null,
                "Ambiguous",
                string.Empty,
                $"Multiple destination locations have the same exact remaining quantity for this row: {string.Join(", ", exactMatches.Select(bucket => bucket.DisplayName))}.",
                false
            );
        }

        var sameLocationCapacityMatch = candidates.FirstOrDefault(bucket =>
            LocationsEqual(bucket.LocationId, item.ExistingLocation)
            && bucket.RemainingQuantity >= item.SavedRowQuantity
            && candidates.Count(candidate => candidate.RemainingQuantity >= item.SavedRowQuantity) == 1
        );
        if (sameLocationCapacityMatch is not null)
        {
            return new BucketSelectionResult(
                sameLocationCapacityMatch,
                "Unchanged",
                "Only current location has enough remaining quantity",
                $"The current location {sameLocationCapacityMatch.DisplayName} is the only remaining location that can still fit this load quantity.",
                false
            );
        }

        var bucketsWithCapacity = candidates
            .Where(bucket => bucket.RemainingQuantity >= item.SavedRowQuantity)
            .OrderBy(bucket => bucket.RemainingQuantity - item.SavedRowQuantity)
            .ThenBy(bucket => bucket.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (bucketsWithCapacity.Count > 0)
        {
            var smallestDifference = bucketsWithCapacity[0].RemainingQuantity - item.SavedRowQuantity;
            var closestFits = bucketsWithCapacity
                .Where(bucket => bucket.RemainingQuantity - item.SavedRowQuantity == smallestDifference)
                .ToList();
            if (closestFits.Count == 1)
            {
                return new BucketSelectionResult(
                    closestFits[0],
                    "Updated",
                    "Closest fit without over-allocation",
                    $"Closest fit without over-allocation selected {closestFits[0].DisplayName} using {closestFits[0].AllocationBasis.ToLowerInvariant()}.",
                    false
                );
            }

            var newestClosestBucket = SelectUniqueNewestBucket(closestFits);
            if (newestClosestBucket is not null)
            {
                return new BucketSelectionResult(
                    newestClosestBucket,
                    LocationsEqual(newestClosestBucket.LocationId, item.ExistingLocation)
                        ? "Unchanged"
                        : "Updated",
                    "Newest closest-fit evidence",
                    $"Multiple closest-fit totals existed, so the newest evidence location {newestClosestBucket.DisplayName} was selected.",
                    false
                );
            }

            return new BucketSelectionResult(
                null,
                "Ambiguous",
                string.Empty,
                $"Multiple destination locations are equally close quantity fits for this row: {string.Join(", ", closestFits.Select(bucket => bucket.DisplayName))}.",
                false
            );
        }

        return new BucketSelectionResult(
            null,
            "NotFound",
            string.Empty,
            "No current destination location has enough remaining quantity to fit this load. This can happen when InforVisual totals are still behind MTM or when the load has no clear posted movement yet.",
            false
        );
    }

    private static ReconciliationLocationBucket? SelectUniqueNewestHistoryCandidate(
        IEnumerable<ReconciliationLocationBucket> candidates,
        decimal quantity,
        ReconciliationHistoryTracker historyTracker
    )
    {
        var datedCandidates = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                LatestDate = historyTracker.GetLatestExactDate(candidate.LocationId, quantity),
            })
            .Where(entry => entry.LatestDate.HasValue)
            .OrderByDescending(entry => entry.LatestDate)
            .ToList();

        if (datedCandidates.Count < 2)
        {
            return datedCandidates.FirstOrDefault()?.Candidate;
        }

        return datedCandidates[0].LatestDate > datedCandidates[1].LatestDate
            ? datedCandidates[0].Candidate
            : null;
    }

    private static ReconciliationLocationBucket? SelectUniqueNewestBucket(
        IEnumerable<ReconciliationLocationBucket> candidates
    )
    {
        var orderedCandidates = candidates
            .Where(candidate => candidate.MovedAt.HasValue)
            .OrderByDescending(candidate => candidate.MovedAt)
            .ToList();

        if (orderedCandidates.Count == 0)
        {
            return null;
        }

        if (orderedCandidates.Count == 1)
        {
            return orderedCandidates[0];
        }

        return orderedCandidates[0].MovedAt > orderedCandidates[1].MovedAt
            ? orderedCandidates[0]
            : null;
    }

    private static bool TryResolvePendingVisualReceipt(
        Model_ReceivingLocationReconciliationItem item,
        IReadOnlyList<Model_ReceivingLoad> loads,
        IReadOnlyList<ReconciliationLocationBucket> buckets,
        IReadOnlyList<Model_InforVisualLocationEvidence> evidenceRows,
        ReconciliationHistoryTracker historyTracker,
        out string details
    )
    {
        details = string.Empty;

        var totalSavedQuantity = loads.Sum(load => load.WeightQuantity);
        var totalVisibleQuantity = buckets.Sum(bucket => bucket.RemainingQuantity);
        var missingQuantity = totalSavedQuantity - totalVisibleQuantity;
        if (missingQuantity <= 0)
        {
            return false;
        }

        if (historyTracker.HasExact(item.ExistingLocation, item.SavedRowQuantity))
        {
            return false;
        }

        var latestReceiptLocation = evidenceRows
            .Select(row => row.LatestReceiptLocationId)
            .FirstOrDefault(location => !string.IsNullOrWhiteSpace(location));
        var looksLikeReceiptSide = LocationsEqual(item.ExistingLocation, latestReceiptLocation)
            || Normalize(item.ExistingLocation).StartsWith("RECV", StringComparison.OrdinalIgnoreCase);
        if (!looksLikeReceiptSide)
        {
            return false;
        }

        details = string.Create(
            CultureInfo.InvariantCulture,
            $"MTM currently shows {totalSavedQuantity:0.##} {item.QuantityUnitOfMeasure} for this PO and part, while InforVisual currently shows {totalVisibleQuantity:0.##}. The missing {missingQuantity:0.##} may indicate that the PO receipt has not posted into InforVisual yet."
        );
        return true;
    }

    private static string NormalizeInforVisualPoNumber(string? poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return string.Empty;
        }

        var trimmed = poNumber.Trim().ToUpperInvariant();
        var match = CanonicalPoPattern.Match(trimmed);
        if (!match.Success)
        {
            return trimmed;
        }

        var digits = match.Groups["digits"].Value;
        var suffix = match.Groups["suffix"].Value.ToUpperInvariant();
        return string.Create(CultureInfo.InvariantCulture, $"PO-{digits.PadLeft(6, '0')}{suffix}");
    }

    private static void RecordPreviewDecision(
        Model_ReceivingLocationReconciliationItem item,
        Model_ReceivingLocationReconciliationSummary summary
    )
    {
        switch (item.Resolution)
        {
            case "Updated":
                summary.UpdatedItems.Add(item);
                break;
            case "Unchanged":
                summary.UnchangedCount++;
                break;
            case "Ambiguous":
                summary.AmbiguousCount++;
                summary.UnresolvedItems.Add(item);
                break;
            case "NotFound":
                summary.NotFoundCount++;
                summary.UnresolvedItems.Add(item);
                break;
            case "Skipped":
                summary.SkippedCount++;
                summary.UnresolvedItems.Add(item);
                break;
            case "IgnoredBySettings":
                summary.SkippedCount++;
                break;
            case "PendingVisualReceipt":
                summary.SkippedCount++;
                summary.UnresolvedItems.Add(item);
                break;
            default:
                summary.ErrorCount++;
                summary.UnresolvedItems.Add(item);
                break;
        }
    }

    private async Task<HashSet<string>> GetIgnoredLocationsAsync()
    {
        var currentUserId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        var stored = await _receivingSettings.GetStringAsync(
            ReceivingSettingsKeys.UserPreferences.IgnoredReconciliationLocationsJson,
            currentUserId
        );

        if (string.IsNullOrWhiteSpace(stored))
        {
            stored = ReceivingSettingsDefaults.StringDefaults[
                ReceivingSettingsKeys.UserPreferences.IgnoredReconciliationLocationsJson
            ];
        }

        try
        {
            var locations = System.Text.Json.JsonSerializer.Deserialize<string[]>(stored) ?? [];
            return new HashSet<string>(locations.Select(Normalize), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            var locations = stored.Split(
                new[] { '\r', '\n', ',', ';' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            return new HashSet<string>(locations.Select(Normalize), StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool LocationsEqual(string? left, string? right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
    }

    private sealed record ReconciliationGroupKey(
        string PoNumber,
        string PartId,
        string PoLineNumber,
        DateTime ReceivedDate
    );

    private sealed class ReconciliationLocationBucket
    {
        public string WarehouseId { get; init; } = string.Empty;

        public string LocationId { get; init; } = string.Empty;

        public decimal CurrentQuantity { get; init; }

        public decimal EvidenceQuantity { get; init; }

        public decimal RemainingQuantity { get; set; }

        public string AllocationBasis { get; init; } = string.Empty;

        public string MovedByUserId { get; init; } = string.Empty;

        public DateTime? MovedAt { get; init; }

        public string NormalizedLocation => Normalize(LocationId);

        public string DisplayName => string.IsNullOrWhiteSpace(WarehouseId)
            ? LocationId
            : $"{WarehouseId}/{LocationId}";
    }

    private sealed record BucketSelectionResult(
        ReconciliationLocationBucket? Bucket,
        string Resolution,
        string AllocationMethod,
        string Details,
        bool UsesExactHistoryQuantity
    );

    private sealed class ReconciliationHistoryTracker
    {
        private readonly Dictionary<string, Queue<Model_InforVisualLocationTransaction>> _exactHistoryByLocationAndQuantity;

        public ReconciliationHistoryTracker(
            IEnumerable<Model_InforVisualLocationTransaction> transactionHistoryRows
        )
        {
            _exactHistoryByLocationAndQuantity = transactionHistoryRows
                .Where(row => !string.IsNullOrWhiteSpace(row.LocationId) && row.Quantity > 0)
                .GroupBy(row => BuildKey(row.LocationId, row.Quantity))
                .ToDictionary(
                    group => group.Key,
                    group => new Queue<Model_InforVisualLocationTransaction>(
                        group.OrderByDescending(row => row.TransactionDate)
                    ),
                    StringComparer.OrdinalIgnoreCase
                );
        }

        public bool HasExact(string locationId, decimal quantity)
        {
            return _exactHistoryByLocationAndQuantity.TryGetValue(
                    BuildKey(locationId, quantity),
                    out var historyRows
                )
                && historyRows.Count > 0;
        }

        public DateTime? GetLatestExactDate(string locationId, decimal quantity)
        {
            return _exactHistoryByLocationAndQuantity.TryGetValue(
                    BuildKey(locationId, quantity),
                    out var historyRows
                )
                && historyRows.Count > 0
                ? historyRows.Peek().TransactionDate
                : null;
        }

        public bool TryConsumeExact(
            string locationId,
            decimal quantity,
            out Model_InforVisualLocationTransaction? transaction
        )
        {
            transaction = null;
            if (
                !_exactHistoryByLocationAndQuantity.TryGetValue(
                    BuildKey(locationId, quantity),
                    out var historyRows
                )
                || historyRows.Count == 0
            )
            {
                return false;
            }

            transaction = historyRows.Dequeue();
            return true;
        }

        private static string BuildKey(string locationId, decimal quantity)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{Normalize(locationId)}|{quantity:0.########}"
            );
        }
    }
}
