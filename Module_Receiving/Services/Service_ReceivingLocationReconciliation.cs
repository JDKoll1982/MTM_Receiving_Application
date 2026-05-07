using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
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
/// Reconciles saved receiving locations against same-day Infor Visual transfer movements.
/// </summary>
public sealed class Service_ReceivingLocationReconciliation
    : IService_ReceivingLocationReconciliation
{
    private static readonly DateTime AllHistoryStartDate = new(2000, 1, 1);
    private static readonly Regex CanonicalPoPattern = new(
        @"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[A-Za-z]?)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private const string DeterministicAllocationMethod =
        "Transfer-based exact-fit then best-fit allocation";

    private readonly IService_MySQL_Receiving _mySqlReceiving;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ReceivingSettings _receivingSettings;

    public Service_ReceivingLocationReconciliation(
        IService_MySQL_Receiving mySqlReceiving,
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger,
        IService_ReceivingSettings receivingSettings
    )
    {
        _mySqlReceiving = mySqlReceiving;
        _inforVisual = inforVisual;
        _logger = logger;
        _receivingSettings = receivingSettings;
    }

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

        var ignoredLocations = await GetIgnoredLocationsAsync();

        var recommendations = evidenceResult
            .Data.Where(row =>
                string.IsNullOrWhiteSpace(row.CurrentLocationId) is false
                && row.CurrentQuantity > 0
                && !ignoredLocations.Contains(Normalize(row.CurrentLocationId))
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

            var ignoredLocations = await GetIgnoredLocationsAsync();
            var currentLabelRows = currentLabelsResult.Data ?? [];
            var historyRows = historyResult.Data ?? [];

            summary.CurrentLabelRowsScanned = currentLabelRows.Count;
            summary.HistoryRowsScanned = historyRows.Count;

            foreach (
                var item in await ReconcileRowsAsync(currentLabelRows, "Current Labels", ignoredLocations)
            )
            {
                RecordPreviewDecision(item, summary);
            }

            foreach (var item in await ReconcileRowsAsync(historyRows, "History", ignoredLocations))
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

    public async Task<
        Model_Dao_Result<Model_ReceivingLocationReconciliationItem>
    > ApplyLocationUpdateAsync(Model_ReceivingLocationReconciliationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!string.Equals(item.Resolution, "Updated", StringComparison.OrdinalIgnoreCase))
        {
            return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationItem>(
                "Only rows with an Updated reconciliation result can be saved."
            );
        }

        if (item.SourceLoad is null)
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
            if (!previewResult.IsSuccess || previewResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<Model_ReceivingLocationReconciliationSummary>(
                    previewResult.ErrorMessage
                );
            }

            var summary = previewResult.Data;

            foreach (var item in summary.UpdatedItems.ToList())
            {
                var applyResult = await ApplyLocationUpdateAsync(item);
                if (!applyResult.IsSuccess)
                {
                    item.Resolution = "Error";
                    item.Details = applyResult.ErrorMessage;
                    summary.ErrorCount++;
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
                item.Details = "Non-PO rows cannot be reconciled against Infor Visual transfer evidence.";
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

            if (string.IsNullOrWhiteSpace(NormalizeInforVisualPoNumber(load.PoNumber)))
            {
                item.Resolution = "Skipped";
                item.Details = "PO number could not be normalized to Infor Visual format.";
                items.Add(item);
                continue;
            }

            validLoads.Add(load);
        }

        foreach (
            var receiptGroup in validLoads.GroupBy(load =>
                new ReconciliationGroupKey(Normalize(load.PartID), load.ReceivedDate.Date)
            )
        )
        {
            var groupLoads = receiptGroup.ToList();
            var templateLoad = groupLoads[0];
            var movementsResult = await _inforVisual.GetReceivingLocationTransferMovementsAsync(
                templateLoad.PartID.Trim(),
                templateLoad.ReceivedDate.Date
            );

            if (!movementsResult.IsSuccess)
            {
                items.AddRange(
                    groupLoads.Select(load =>
                    {
                        var item = CreateBaseItem(load, dataSource);
                        item.Resolution = "Error";
                        item.Details = movementsResult.ErrorMessage;
                        return item;
                    })
                );
                continue;
            }

            items.AddRange(
                ResolveTransferBasedGroup(
                    groupLoads,
                    dataSource,
                    movementsResult.Data ?? [],
                    ignoredLocations
                )
            );
        }

        return items;
    }

    private List<Model_ReceivingLocationReconciliationItem> ResolveTransferBasedGroup(
        List<Model_ReceivingLoad> loads,
        string dataSource,
        List<Model_InforVisualLocationTransferMovement> allMovements,
        HashSet<string> ignoredLocations
    )
    {
        var items = loads
            .Select(load => CreateBaseItem(load, dataSource))
            .OrderByDescending(item => item.SavedRowQuantity)
            .ThenBy(item => item.SourceLoad?.LoadNumber ?? int.MaxValue)
            .ThenBy(item => item.LoadId)
            .ToList();

        var relevantMovements = SelectRelevantMovements(loads, allMovements);
        if (relevantMovements.Count == 0)
        {
            foreach (var item in items)
            {
                item.Resolution = "NotFound";
                item.Details =
                    "No same-day Infor Visual transfer movements were found for this material that could explain a saved location change.";
            }

            return items;
        }

        var allBuckets = BuildTransferBuckets(relevantMovements);
        if (allBuckets.Count == 0)
        {
            foreach (var item in items)
            {
                item.Resolution = "Ambiguous";
                item.Details =
                    "Transfer movements were found, but they did not yield any net destination quantity after chained moves were resolved.";
            }

            return items;
        }

        var candidateBuckets = allBuckets
            .Where(bucket => !ignoredLocations.Contains(bucket.NormalizedLocation))
            .ToList();

        if (candidateBuckets.Count == 0)
        {
            var ignoredLocationList = string.Join(
                ", ",
                allBuckets.Select(bucket => bucket.DisplayLocation).Distinct(StringComparer.OrdinalIgnoreCase)
            );

            foreach (var item in items)
            {
                item.Resolution = "IgnoredBySettings";
                item.Details =
                    $"Transfer evidence points only to destination locations currently ignored by user preference: {ignoredLocationList}.";
            }

            return items;
        }

        foreach (var item in items)
        {
            var selectedBucket = candidateBuckets
                .Where(bucket => bucket.RemainingQuantity >= item.SavedRowQuantity)
                .OrderBy(bucket => bucket.RemainingQuantity - item.SavedRowQuantity)
                .ThenByDescending(bucket => bucket.LatestTransactionDate)
                .ThenBy(bucket => bucket.DisplayLocation, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (selectedBucket is null)
            {
                var largestRemainingBucket = candidateBuckets
                    .OrderByDescending(bucket => bucket.RemainingQuantity)
                    .ThenBy(bucket => bucket.DisplayLocation, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                item.Resolution = "Ambiguous";
                item.Details = largestRemainingBucket is null
                    ? "Transfer evidence was found, but no destination quantity remained after allocating earlier rows in the same part-and-day reconciliation group."
                    : string.Create(
                        CultureInfo.InvariantCulture,
                        $"Transfer evidence remains, but the largest unallocated destination bucket is only {largestRemainingBucket.RemainingQuantity:0.##} {item.QuantityUnitOfMeasure} at {largestRemainingBucket.DisplayLocation}. This row requires {item.SavedRowQuantity:0.##} {item.QuantityUnitOfMeasure}."
                    );
                continue;
            }

            item.ProposedLocation = selectedBucket.LocationId;
            item.MatchedLocationQuantity = selectedBucket.RemainingQuantity;
            item.QuantityMoved = item.SavedRowQuantity;
            item.QuantityDifference = Math.Abs(selectedBucket.RemainingQuantity - item.SavedRowQuantity);
            item.AllocationMethod = DeterministicAllocationMethod;
            item.MovedAt = selectedBucket.LatestTransactionDate;
            item.MovedByUserId = selectedBucket.TransactionUserId;
            item.TransferMovementCount = selectedBucket.TransferCount;
            item.EvidenceSourceLocations = selectedBucket.SourceLocationsText;
            item.Resolution = LocationsEqual(item.ExistingLocation, item.ProposedLocation)
                ? "Unchanged"
                : "Updated";
            item.Details = string.Create(
                CultureInfo.InvariantCulture,
                $"Allocated by transfer-based exact-fit then best-fit allocation using {selectedBucket.TransferCount} transfer(s) with a net confirmed quantity of {selectedBucket.NetQuantity:0.##} {item.QuantityUnitOfMeasure} ending at {selectedBucket.DisplayLocation}. Sources: {selectedBucket.SourceLocationsText}. Latest confirmed move: {selectedBucket.LatestTransactionDate:M/d/yyyy h:mm tt}."
            );

            selectedBucket.RemainingQuantity -= item.SavedRowQuantity;
        }

        return items;
    }

    private static List<Model_InforVisualLocationTransferMovement> SelectRelevantMovements(
        List<Model_ReceivingLoad> loads,
        List<Model_InforVisualLocationTransferMovement> allMovements
    )
    {
        var pendingMovements = allMovements
            .Where(movement =>
                string.IsNullOrWhiteSpace(movement.SourceLocationId) is false
                && string.IsNullOrWhiteSpace(movement.DestinationLocationId) is false
                && !LocationsEqual(movement.SourceLocationId, movement.DestinationLocationId)
            )
            .OrderBy(movement => movement.TransactionDate)
            .ThenBy(movement => movement.DestinationTransactionId)
            .ThenBy(movement => movement.SourceTransactionId)
            .ThenBy(movement => movement.SourceLocationId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(movement => movement.DestinationLocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var seedLocations = new HashSet<string>(
            loads.Select(load => Normalize(load.InitialLocation))
                .Where(location => string.IsNullOrWhiteSpace(location) is false),
            StringComparer.OrdinalIgnoreCase
        );
        var allowedPoKeys = new HashSet<string>(
            loads.Select(load => BuildPoKey(load.PoNumber, load.PoLineNumber))
                .Where(key => string.IsNullOrWhiteSpace(key) is false),
            StringComparer.OrdinalIgnoreCase
        );

        var relevantMovements = new List<Model_InforVisualLocationTransferMovement>();
        var reachedLocations = new HashSet<string>(seedLocations, StringComparer.OrdinalIgnoreCase);
        var iteration = 0;
        var maxIterations = Math.Max(1, pendingMovements.Count * pendingMovements.Count);
        var madeProgress = true;

        while (madeProgress && pendingMovements.Count > 0 && iteration < maxIterations)
        {
            madeProgress = false;
            iteration++;

            for (var index = pendingMovements.Count - 1; index >= 0; index--)
            {
                var movement = pendingMovements[index];
                var sourceLocation = Normalize(movement.SourceLocationId);
                var destinationLocation = Normalize(movement.DestinationLocationId);
                var poKey = BuildPoKey(movement.PONumber, movement.POLineNumber);

                if (!reachedLocations.Contains(sourceLocation) && !allowedPoKeys.Contains(poKey))
                {
                    continue;
                }

                relevantMovements.Add(movement);
                pendingMovements.RemoveAt(index);
                reachedLocations.Add(sourceLocation);
                reachedLocations.Add(destinationLocation);
                madeProgress = true;
            }
        }

        return relevantMovements
            .OrderBy(movement => movement.TransactionDate)
            .ThenBy(movement => movement.DestinationTransactionId)
            .ThenBy(movement => movement.SourceTransactionId)
            .ToList();
    }

    private static List<TransferBucket> BuildTransferBuckets(
        IReadOnlyCollection<Model_InforVisualLocationTransferMovement> movements
    )
    {
        var inboundByLocation = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var outboundByLocation = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var inboundEvidence = new Dictionary<
            string,
            List<Model_InforVisualLocationTransferMovement>
        >(StringComparer.OrdinalIgnoreCase);

        foreach (var movement in movements)
        {
            var sourceLocation = Normalize(movement.SourceLocationId);
            var destinationLocation = Normalize(movement.DestinationLocationId);

            if (!outboundByLocation.TryAdd(sourceLocation, movement.TransferQuantity))
            {
                outboundByLocation[sourceLocation] += movement.TransferQuantity;
            }

            if (!inboundByLocation.TryAdd(destinationLocation, movement.TransferQuantity))
            {
                inboundByLocation[destinationLocation] += movement.TransferQuantity;
            }

            if (!inboundEvidence.TryGetValue(destinationLocation, out var evidenceList))
            {
                evidenceList = [];
                inboundEvidence[destinationLocation] = evidenceList;
            }

            evidenceList.Add(movement);
        }

        var buckets = new List<TransferBucket>();
        foreach (var location in inboundByLocation.Keys)
        {
            var netQuantity = inboundByLocation[location]
                - outboundByLocation.GetValueOrDefault(location, 0);
            if (netQuantity <= 0)
            {
                continue;
            }

            var evidenceRows = inboundEvidence[location];
            var latestInbound = evidenceRows
                .OrderByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.DestinationTransactionId)
                .First();

            buckets.Add(
                new TransferBucket
                {
                    WarehouseId = latestInbound.DestinationWarehouseId,
                    LocationId = latestInbound.DestinationLocationId,
                    NormalizedLocation = location,
                    NetQuantity = netQuantity,
                    RemainingQuantity = netQuantity,
                    LatestTransactionDate = latestInbound.TransactionDate,
                    TransactionUserId = latestInbound.TransactionUserId,
                    TransferCount = evidenceRows.Count,
                    SourceLocationsText = string.Join(
                        ", ",
                        evidenceRows
                            .Select(row => row.SourceLocationId.Trim())
                            .Where(source => string.IsNullOrWhiteSpace(source) is false)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(source => source, StringComparer.OrdinalIgnoreCase)
                    ),
                }
            );
        }

        return buckets
            .OrderByDescending(bucket => bucket.NetQuantity)
            .ThenByDescending(bucket => bucket.LatestTransactionDate)
            .ThenBy(bucket => bucket.DisplayLocation, StringComparer.OrdinalIgnoreCase)
            .ToList();
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
            EvidenceSourceLocations = string.Empty,
        };
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
            case "IgnoredBySettings":
            case "Ambiguous":
                summary.AmbiguousCount++;
                summary.UnresolvedItems.Add(item);
                break;
            case "NotFound":
                summary.NotFoundCount++;
                summary.UnresolvedItems.Add(item);
                break;
            case "Error":
                summary.ErrorCount++;
                summary.UnresolvedItems.Add(item);
                break;
            default:
                summary.SkippedCount++;
                break;
        }
    }

    private async Task<HashSet<string>> GetIgnoredLocationsAsync()
    {
        try
        {
            var ignoredLocationsJson = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.UserPreferences.IgnoredReconciliationLocationsJson
            );
            if (string.IsNullOrWhiteSpace(ignoredLocationsJson))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var ignoredLocations = JsonSerializer.Deserialize<List<string>>(ignoredLocationsJson) ?? [];
            return new HashSet<string>(
                ignoredLocations.Select(Normalize).Where(location => string.IsNullOrWhiteSpace(location) is false),
                StringComparer.OrdinalIgnoreCase
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"Failed to load ignored reconciliation locations. Continuing without exclusions. {ex.Message}"
            );
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string NormalizeInforVisualPoNumber(string? poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return string.Empty;
        }

        var match = CanonicalPoPattern.Match(poNumber.Trim());
        if (!match.Success)
        {
            return string.Empty;
        }

        var digits = match.Groups["digits"].Value.PadLeft(6, '0');
        var suffix = match.Groups["suffix"].Value.ToUpperInvariant();
        return $"PO-{digits}{suffix}";
    }

    private static string BuildPoKey(string? poNumber, string? poLineNumber)
    {
        var canonicalPoNumber = NormalizeInforVisualPoNumber(poNumber);
        if (string.IsNullOrWhiteSpace(canonicalPoNumber))
        {
            return string.Empty;
        }

        return $"{canonicalPoNumber}|{Normalize(poLineNumber)}";
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
    }

    private static bool LocationsEqual(string? left, string? right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ReconciliationGroupKey(string PartId, DateTime ReceivedDate);

    private sealed class TransferBucket
    {
        public string WarehouseId { get; init; } = string.Empty;

        public string LocationId { get; init; } = string.Empty;

        public string NormalizedLocation { get; init; } = string.Empty;

        public decimal NetQuantity { get; init; }

        public decimal RemainingQuantity { get; set; }

        public DateTime LatestTransactionDate { get; init; }

        public string TransactionUserId { get; init; } = string.Empty;

        public int TransferCount { get; init; }

        public string SourceLocationsText { get; init; } = string.Empty;

        public string DisplayLocation => string.IsNullOrWhiteSpace(WarehouseId)
            ? LocationId
            : $"{WarehouseId} / {LocationId}";
    }
}