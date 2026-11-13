using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Composes one day: loads it, groups it, selects its movement evidence, builds the
/// destination buckets, allocates the rows, proves the result and derives exactly
/// one outcome per row. It is the only type that composes the other collaborators,
/// it writes nothing, and a failure in one group stays inside that group
/// (FR-021, FR-038, FR-068).
/// </summary>
public sealed class Service_ReconciliationPreviewOrchestrator : IService_ReconciliationPreview
{
    private readonly IService_ReconciliationDayReader _dayReader;
    private readonly IService_ReconciliationGroupResolver _groupResolver;
    private readonly IService_ReconciliationMovementSelector _movementSelector;
    private readonly IService_ReconciliationBucketBuilder _bucketBuilder;
    private readonly IService_ReconciliationAllocator _allocator;
    private readonly IService_ReconciliationQuantityProof _quantityProof;
    private readonly IService_ReconciliationSettings _settings;
    private readonly Dao_ReconciliationInforVisual _inforVisual;
    private readonly IService_LoggingUtility? _logger;

    /// <summary>Creates the orchestrator over its collaborators.</summary>
    public Service_ReconciliationPreviewOrchestrator(
        IService_ReconciliationDayReader dayReader,
        IService_ReconciliationGroupResolver groupResolver,
        IService_ReconciliationMovementSelector movementSelector,
        IService_ReconciliationBucketBuilder bucketBuilder,
        IService_ReconciliationAllocator allocator,
        IService_ReconciliationQuantityProof quantityProof,
        IService_ReconciliationSettings settings,
        Dao_ReconciliationInforVisual inforVisual,
        IService_LoggingUtility? logger = null
    )
    {
        _dayReader = dayReader;
        _groupResolver = groupResolver;
        _movementSelector = movementSelector;
        _bucketBuilder = bucketBuilder;
        _allocator = allocator;
        _quantityProof = quantityProof;
        _settings = settings;
        _inforVisual = inforVisual;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_ReconciliationDay>> PreviewDayAsync(DateOnly date)
    {
        Model_Dao_Result<Model_ReconciliationDay> loaded = await _dayReader.GetDayAsync(date);

        if (loaded.IsSuccess is false || loaded.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<Model_ReconciliationDay>(
                loaded.ErrorMessage,
                loaded.Exception
            );
        }

        Model_ReconciliationDay day = loaded.Data;
        day.State = Enum_ReconciliationDayState.Previewing;

        Guid runId = Guid.NewGuid();
        DateTime startedAt = DateTime.Now;

        int discoveryWindowDays = await _settings.GetDiscoveryWindowDaysAsync();
        int strictWindowDays = await _settings.GetStrictWindowDaysAsync();

        var findings = new List<Model_ReconciliationFinding>();
        var candidates = new List<Model_ReconciliationRow>();

        foreach (Model_ReconciliationRow row in day.Rows)
        {
            string candidacyFailure =
                Service_ReconciliationGroupResolver.GetCandidacyFailureReason(row);

            if (candidacyFailure.Length == 0)
            {
                candidates.Add(row);
                continue;
            }

            findings.Add(CreateFinding(row, Enum_ReconciliationOutcome.Skipped, row.SavedLocation, string.Empty, candidacyFailure, null, null));
        }

        Model_Dao_Result<IReadOnlyList<Model_ReconciliationGroup>> groupsResult =
            _groupResolver.Resolve(candidates);

        if (groupsResult.IsSuccess is false)
        {
            day.State = Enum_ReconciliationDayState.Failed;
            day.FailureReason = $"The day could not be grouped: {groupsResult.ErrorMessage}";
            return Model_Dao_Result_Factory.Success(day);
        }

        IReadOnlyList<Model_ReconciliationGroup> groups = groupsResult.Data ?? [];

        foreach (Model_ReconciliationGroup group in groups)
        {
            findings.AddRange(
                await PreviewGroupAsync(group, date, discoveryWindowDays, strictWindowDays, runId)
            );
        }

        var run = new Model_ReconciliationRun
        {
            RunId = runId,
            Day = date,
            StartedAt = startedAt,
            FinishedAt = DateTime.Now,
            VisualUserId = ResolveVisualUserAttribution(groups),
        };

        day.Groups = groups;
        day.Findings = findings;
        day.Run = run;
        day.Summary = BuildSummary(day, groups, findings);
        day.State = Enum_ReconciliationDayState.Previewed;
        day.FailureReason = string.Empty;

        _logger?.LogInfo(
            $"Reconciliation run {run.RunId} finished day {date:yyyy-MM-dd}: {day.Summary.Scanned} scanned, "
                + $"{day.Summary.Written} writable, {day.Summary.Unchanged} unchanged, "
                + $"{day.Summary.Unresolved} unresolved, {day.Summary.NotFound} not found, "
                + $"{day.Summary.Skipped} skipped, {day.Summary.Error} error."
        );

        return Model_Dao_Result_Factory.Success(day);
    }

    private async Task<IReadOnlyList<Model_ReconciliationFinding>> PreviewGroupAsync(
        Model_ReconciliationGroup group,
        DateOnly date,
        int discoveryWindowDays,
        int strictWindowDays,
        Guid runId
    )
    {
        LogMixedUnitWarning(group, date, runId);

        Model_Dao_Result<List<Model_ReconciliationMovement>> movements =
            await ReadMovementsWithRetryAsync(group.Key, date, discoveryWindowDays, runId);

        if (movements.IsSuccess is false)
        {
            return BuildFailureFindings(
                group,
                Enum_ReconciliationOutcome.Error,
                $"Infor Visual could not be read for {group.Key.DisplayText}: {movements.ErrorMessage}"
            );
        }

        IReadOnlyList<Model_ReconciliationMovement> selected = _movementSelector.Select(
            group,
            movements.Data ?? []
        );

        group.Movements = selected;

        if (selected.Count == 0)
        {
            IReadOnlyList<Model_ReconciliationFinding>? byReceipt =
                await ResolveDirectReceiptAsync(group, date, discoveryWindowDays, runId);

            if (byReceipt is not null)
            {
                return byReceipt;
            }

            _logger?.LogWarning(
                $"Reconciliation run {runId}: no qualifying movement for part {group.Key.PartId} "
                    + $"on {date:yyyy-MM-dd}, group {group.Key.DisplayText}."
            );

            return BuildFailureFindings(
                group,
                Enum_ReconciliationOutcome.NotFound,
                $"No Infor Visual transfer was found for {group.Key.PartId} around {date:yyyy-MM-dd}."
            );
        }

        IReadOnlyList<Model_ReconciliationDestinationBucket> buckets = _bucketBuilder.Build(selected);
        group.Buckets = buckets;

        DateOnly afterStrictWindow = date.AddDays(strictWindowDays + 1);

        Model_Dao_Result<List<Model_ReconciliationLocationBalance>> balances =
            await ReadBalancesWithRetryAsync(group.Key, afterStrictWindow, runId);

        if (balances.IsSuccess is false)
        {
            return BuildFailureFindings(
                group,
                Enum_ReconciliationOutcome.Error,
                $"The Infor Visual balance could not be read for {group.Key.DisplayText}: {balances.ErrorMessage}"
            );
        }

        group.Balances = balances.Data ?? [];

        Model_Dao_Result<IReadOnlyList<Model_ReconciliationAllocation>> allocationsResult =
            _allocator.Allocate(group.Rows, buckets);

        if (allocationsResult.IsSuccess is false || allocationsResult.Data is null)
        {
            return BuildFailureFindings(
                group,
                Enum_ReconciliationOutcome.Error,
                $"The rows of {group.Key.DisplayText} could not be allocated: {allocationsResult.ErrorMessage}"
            );
        }

        Model_Dao_Result<Model_ReconciliationProofResult> proofResult =
            await _quantityProof.VerifyAsync(group, allocationsResult.Data);

        if (proofResult.IsSuccess is false || proofResult.Data is null)
        {
            return BuildFailureFindings(
                group,
                Enum_ReconciliationOutcome.Error,
                $"The quantity proof failed to run for {group.Key.DisplayText}: {proofResult.ErrorMessage}"
            );
        }

        group.Residual = proofResult.Data.Residual;
        group.Allocations = allocationsResult.Data;
        group.Findings = DeriveFindings(group, allocationsResult.Data, proofResult.Data);

        var proofContext = new Model_ReconciliationProofContext
        {
            Key = group.Key,
            ReceivingLocation = group.ReceivingLocation,
            Rows = group.Rows,
            Buckets = group.Buckets,
            Allocations = group.Allocations,
            Balances = group.Balances,
        };

        foreach (Model_ReconciliationFinding finding in group.Findings)
        {
            finding.ProofContext = proofContext;
        }

        return group.Findings;
    }

    private static IReadOnlyList<Model_ReconciliationFinding> DeriveFindings(
        Model_ReconciliationGroup group,
        IReadOnlyList<Model_ReconciliationAllocation> allocations,
        Model_ReconciliationProofResult proof
    )
    {
        decimal largestRemaining = group.Buckets
            .Select(bucket => bucket.RemainingQuantity)
            .DefaultIfEmpty(0m)
            .Max();

        var findings = new List<Model_ReconciliationFinding>();

        foreach (Model_ReconciliationRow row in group.Rows)
        {
            findings.Add(DeriveRowFinding(group, row, allocations, proof, largestRemaining));
        }

        return findings;
    }

    private static Model_ReconciliationFinding DeriveRowFinding(
        Model_ReconciliationGroup group,
        Model_ReconciliationRow row,
        IReadOnlyList<Model_ReconciliationAllocation> allocations,
        Model_ReconciliationProofResult proof,
        decimal largestRemaining
    )
    {
        if (row.Quantity <= 0)
        {
            return CreateFinding(row, Enum_ReconciliationOutcome.Unchanged, row.SavedLocation, string.Empty, "The row carries no quantity to move.", null, null);
        }

        Model_ReconciliationAllocation? allocation = allocations.FirstOrDefault(candidate =>
            ReferenceEquals(candidate.Row, row)
        );

        if (allocation is null)
        {
            return DeriveUnallocatedFinding(group, row, proof, largestRemaining);
        }

        Model_ReconciliationBucketVerdict? verdict = proof.BucketVerdicts.FirstOrDefault(candidate =>
            ReferenceEquals(candidate.Bucket, allocation.Bucket)
        );

        if (verdict is null)
        {
            return CreateFinding(row, Enum_ReconciliationOutcome.Error, row.SavedLocation, string.Empty, "The destination bucket of this row could not be verified.", null, null);
        }

        if (
            verdict.Passed is false
            && verdict.OutcomeWhenFailed is Enum_ReconciliationOutcome.ExtraSurplus
            && Helper_ReconciliationCanonical.LocationsEqual(
                row.SavedLocation,
                group.ReceivingLocation
            )
            && Helper_ReconciliationCanonical.LocationsEqual(
                allocation.Bucket.LocationId,
                group.ReceivingLocation
            )
        )
        {
            // The material was received into the location the evidence names and no
            // movement took it out, so anything extra in that location arrived on
            // somebody else's receipt. Counting it here would ask a person to review a
            // line that is already right.
            return CreateFinding(
                row,
                Enum_ReconciliationOutcome.Unchanged,
                row.SavedLocation,
                allocation.Bucket.LocationId,
                $"The material was received into {row.SavedLocation} and no movement took it out, so the row stays where it is.",
                null,
                allocation.Bucket
            );
        }

        if (verdict.Passed is false)
        {
            return CreateFinding(
                row,
                verdict.OutcomeWhenFailed,
                row.SavedLocation,
                allocation.Bucket.LocationId,
                verdict.FailureReason,
                verdict.Remainder,
                allocation.Bucket
            );
        }

        return Helper_ReconciliationCanonical.LocationsEqual(
            allocation.Bucket.LocationId,
            row.SavedLocation
        )
            ? CreateFinding(row, Enum_ReconciliationOutcome.Unchanged, row.SavedLocation, allocation.Bucket.LocationId, "The saved location is a destination the material reached, so the row stays where it is.", null, allocation.Bucket)
            : CreateWritableFinding(row, allocation.Bucket);
    }

    private static Model_ReconciliationFinding DeriveUnallocatedFinding(
        Model_ReconciliationGroup group,
        Model_ReconciliationRow row,
        Model_ReconciliationProofResult proof,
        decimal largestRemaining
    )
    {
        if (group.Key.PurchaseOrderNumber.Length == 0 && group.Key.VendorKey.Length == 0)
        {
            return CreateFinding(
                row,
                Enum_ReconciliationOutcome.NotEnough,
                row.SavedLocation,
                string.Empty,
                "The receipt carries no purchase order and no vendor, so its movement evidence cannot be identified.",
                row.Quantity - largestRemaining,
                null
            );
        }

        if (
            Helper_ReconciliationCanonical.LocationsEqual(
                row.SavedLocation,
                group.ReceivingLocation
            )
            is false
        )
        {
            decimal shortfall = row.Quantity - largestRemaining;
            string largest = largestRemaining.ToString("0.##", CultureInfo.InvariantCulture);
            string required = row.Quantity.ToString("0.##", CultureInfo.InvariantCulture);

            return CreateFinding(
                row,
                Enum_ReconciliationOutcome.NotEnough,
                row.SavedLocation,
                string.Empty,
                $"No destination could cover this row: it needs {required} but the largest unallocated destination holds {largest}.",
                shortfall,
                null
            );
        }

        if (proof.Residual.Agrees)
        {
            return CreateFinding(
                row,
                Enum_ReconciliationOutcome.Unchanged,
                row.SavedLocation,
                row.SavedLocation,
                "The material stayed in the receiving location and the residual agrees with the balance.",
                null,
                null
            );
        }

        return CreateFinding(
            row,
            Enum_ReconciliationOutcome.NotEnough,
            row.SavedLocation,
            string.Empty,
            proof.Residual.DisagreementReason,
            proof.Residual.Difference ?? 0m,
            null
        );
    }

    private static Model_ReconciliationFinding CreateWritableFinding(
        Model_ReconciliationRow row,
        Model_ReconciliationDestinationBucket bucket
    )
    {
        Model_ReconciliationFinding finding = CreateFinding(
            row,
            Enum_ReconciliationOutcome.Updated,
            row.SavedLocation,
            bucket.LocationId,
            $"The material moved to {bucket.LocationId}. Accept to write that location.",
            null,
            bucket
        );

        finding.State = Enum_ReconciliationFindingState.Writable;

        return finding;
    }

    /// <summary>
    /// Adjudicates a group that has no transfer pair from the day's receipts instead.
    /// A part received straight into its stock location never produces a transfer, so
    /// the pair queries cannot speak for it; the receipt names the location the
    /// material actually reached (FR-015, LG-15).
    /// </summary>
    /// <returns>
    /// One finding per row when the day holds a receipt for the part, otherwise
    /// <see langword="null"/> so the caller keeps its own verdict. A row whose saved
    /// location is a receipt location is unchanged; every other row reports the
    /// receipt location instead of claiming Infor Visual holds no evidence.
    /// </returns>
    private async Task<IReadOnlyList<Model_ReconciliationFinding>?> ResolveDirectReceiptAsync(
        Model_ReconciliationGroup group,
        DateOnly date,
        int discoveryWindowDays,
        Guid runId
    )
    {
        Model_Dao_Result<List<Model_ReconciliationReceipt>> receipts =
            await _inforVisual.GetReceivingReceiptsAsync(
                group.Key.PartId,
                date,
                discoveryWindowDays
            );

        if (receipts.IsSuccess is false)
        {
            _logger?.LogWarning(
                $"Reconciliation run {runId}: Infor Visual receipt read failed for group "
                    + $"{group.Key.DisplayText}. {receipts.ErrorMessage}"
            );
            return null;
        }

        IReadOnlyList<Model_ReconciliationReceipt> found = receipts.Data ?? [];

        if (found.Count == 0)
        {
            return null;
        }

        var receiptLocations = new HashSet<string>(
            found.Select(receipt => Helper_ReconciliationCanonical.Normalize(receipt.LocationId)),
            StringComparer.Ordinal
        );
        receiptLocations.Remove(string.Empty);

        string receivedInto = string.Join(
            ", ",
            found.Select(receipt => receipt.LocationId)
                .Where(location => location.Length != 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );

        var findings = new List<Model_ReconciliationFinding>();

        foreach (Model_ReconciliationRow row in group.Rows)
        {
            bool landedHere = receiptLocations.Contains(
                Helper_ReconciliationCanonical.Normalize(row.SavedLocation)
            );

            findings.Add(
                landedHere
                    ? CreateFinding(
                        row,
                        Enum_ReconciliationOutcome.Unchanged,
                        row.SavedLocation,
                        row.SavedLocation,
                        $"Infor Visual received this material straight into {row.SavedLocation}, so the row already matches.",
                        null,
                        null
                    )
                    : CreateFinding(
                        row,
                        Enum_ReconciliationOutcome.NotFound,
                        row.SavedLocation,
                        string.Empty,
                        $"Infor Visual received this material into {receivedInto}, so the saved location {row.SavedLocation} cannot be confirmed.",
                        null,
                        null
                    )
            );
        }

        group.Findings = findings;

        _logger?.LogInfo(
            $"Reconciliation run {runId}: group {group.Key.DisplayText} has no transfer, "
                + $"resolved from the receipt into {receivedInto}."
        );

        return findings;
    }

    private static IReadOnlyList<Model_ReconciliationFinding> BuildFailureFindings(
        Model_ReconciliationGroup group,
        Enum_ReconciliationOutcome outcome,
        string reason
    )
    {
        var findings = new List<Model_ReconciliationFinding>();

        foreach (Model_ReconciliationRow row in group.Rows)
        {
            findings.Add(
                CreateFinding(row, outcome, row.SavedLocation, string.Empty, reason, null, null)
            );
        }

        group.Findings = findings;

        return findings;
    }

    private static Model_ReconciliationFinding CreateFinding(
        Model_ReconciliationRow row,
        Enum_ReconciliationOutcome outcome,
        string savedLocation,
        string proposedLocation,
        string reason,
        decimal? difference,
        Model_ReconciliationDestinationBucket? bucket
    ) =>
        new(
            row,
            outcome,
            savedLocation,
            proposedLocation,
            Enum_ReconciliationDestinationSource.Derived,
            reason,
            difference,
            bucket
        );



    private static Model_ReconciliationDaySummary BuildSummary(
        Model_ReconciliationDay day,
        IReadOnlyList<Model_ReconciliationGroup> groups,
        IReadOnlyList<Model_ReconciliationFinding> findings
    ) =>
        new()
        {
            Scanned = day.Rows.Count,
            ExcludedReprintCount = day.Summary.ExcludedReprintCount,
            Written = findings.Count(finding =>
                finding.Outcome
                    is Enum_ReconciliationOutcome.Updated
                        or Enum_ReconciliationOutcome.ManuallySet
            ),
            Unchanged = findings.Count(finding =>
                finding.Outcome is Enum_ReconciliationOutcome.Unchanged
            ),
            Unresolved = findings.Count(finding =>
                finding.Outcome
                    is Enum_ReconciliationOutcome.ExtraSurplus
                        or Enum_ReconciliationOutcome.NotEnough
            ),
            Skipped = findings.Count(finding =>
                finding.Outcome is Enum_ReconciliationOutcome.Skipped
            ),
            Error = findings.Count(finding => finding.Outcome is Enum_ReconciliationOutcome.Error),
            NotFound = findings.Count(finding =>
                finding.Outcome is Enum_ReconciliationOutcome.NotFound
            ),
            ResidualDisagreement = groups.Any(group => group.Residual?.Agrees is false),
        };

    /// <summary>
    /// The distinct non-empty Infor Visual users behind this day's movement evidence,
    /// comma-separated, which is the attribution the summary strip shows (FR-049, FR-042).
    /// </summary>
    /// <param name="groups">The day's resolved groups, each holding its selected movements.</param>
    private static string ResolveVisualUserAttribution(
        IReadOnlyList<Model_ReconciliationGroup> groups
    ) =>
        string.Join(
            ", ",
            groups
                .SelectMany(group => group.Movements)
                .Select(movement => movement.TransactionUserId)
                .Where(userId => string.IsNullOrWhiteSpace(userId) is false)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(userId => userId, StringComparer.OrdinalIgnoreCase)
        );

    /// <summary>
    /// Warns once when one group holds more than one unit of measure, so the operator
    /// can see a mixed-unit receipt without the unit ever gating allocation
    /// (FR-028, LG-06). Nothing here changes a verdict: the group is previewed in full.
    /// </summary>
    /// <param name="group">The group about to be previewed.</param>
    /// <param name="date">The day being previewed.</param>
    /// <param name="runId">The run the warning belongs to.</param>
    private void LogMixedUnitWarning(Model_ReconciliationGroup group, DateOnly date, Guid runId)
    {
        List<string> units = group
            .Rows.Select(row => row.UnitOfMeasure)
            .Where(unit => string.IsNullOrWhiteSpace(unit) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (units.Count < 2)
        {
            return;
        }

        _logger?.LogWarning(
            $"Reconciliation run {runId}: part {group.Key.PartId} on {date:yyyy-MM-dd}, group "
                + $"{group.Key.DisplayText}, holds more than one unit of measure "
                + $"({string.Join(", ", units)}). The unit does not gate allocation and the group is previewed in full."
        );
    }

    private async Task<Model_Dao_Result<List<Model_ReconciliationMovement>>>
        ReadMovementsWithRetryAsync(
            Model_ReconciliationGroupKey key,
            DateOnly date,
            int discoveryWindowDays,
            Guid runId
        )
    {
        Model_Dao_Result<List<Model_ReconciliationMovement>> result =
            await _inforVisual.GetTransferMovementsAsync(key.PartId, date, discoveryWindowDays);

        if (result.IsSuccess)
        {
            return result;
        }

        _logger?.LogWarning(
            $"Reconciliation run {runId}: Infor Visual movement read failed for group "
                + $"{key.DisplayText}; retrying once. {result.ErrorMessage}"
        );

        return await _inforVisual.GetTransferMovementsAsync(key.PartId, date, discoveryWindowDays);
    }

    private async Task<Model_Dao_Result<List<Model_ReconciliationLocationBalance>>>
        ReadBalancesWithRetryAsync(Model_ReconciliationGroupKey key, DateOnly afterDate, Guid runId)
    {
        Model_Dao_Result<List<Model_ReconciliationLocationBalance>> result =
            await _inforVisual.GetLocationBalancesAsync(key.PartId, afterDate);

        if (result.IsSuccess)
        {
            return result;
        }

        _logger?.LogWarning(
            $"Reconciliation run {runId}: Infor Visual balance read failed for group "
                + $"{key.DisplayText}; retrying once. {result.ErrorMessage}"
        );

        return await _inforVisual.GetLocationBalancesAsync(key.PartId, afterDate);
    }
}
