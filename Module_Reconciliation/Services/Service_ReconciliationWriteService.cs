using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Accepts one line. It re-runs the quantity proof over the live balance, performs
/// exactly one call to the single write procedure, and refuses every state that is
/// not writable (FR-041, FR-052, FR-053, FR-054, FR-047).
/// </summary>
public sealed class Service_ReconciliationWriteService : IService_ReconciliationWriteService
{
    private readonly Dao_ReconciliationReceivingRows _receivingRows;
    private readonly Dao_ReconciliationInforVisual _inforVisual;
    private readonly IService_ReconciliationQuantityProof _quantityProof;
    private readonly IService_ReconciliationSettings _settings;
    private readonly IService_LoggingUtility? _logger;

    /// <summary>Creates the write service over its data access and the proof.</summary>
    public Service_ReconciliationWriteService(
        Dao_ReconciliationReceivingRows receivingRows,
        Dao_ReconciliationInforVisual inforVisual,
        IService_ReconciliationQuantityProof quantityProof,
        IService_ReconciliationSettings settings,
        IService_LoggingUtility? logger = null
    )
    {
        _receivingRows = receivingRows;
        _inforVisual = inforVisual;
        _quantityProof = quantityProof;
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_ReconciliationFinding>> AcceptAsync(
        Model_ReconciliationFinding finding,
        string destinationLocation,
        string operatorUserId
    )
    {
        ArgumentNullException.ThrowIfNull(finding);

        string refusal = GetRefusalReason(finding, destinationLocation);

        if (refusal.Length != 0)
        {
            return Model_Dao_Result_Factory.Failure<Model_ReconciliationFinding>(refusal);
        }

        bool isOperatorDestination =
            Helper_ReconciliationCanonical.LocationsEqual(
                destinationLocation,
                finding.ProposedLocation
            )
            is false
            || finding.DestinationSource is Enum_ReconciliationDestinationSource.Operator;

        if (finding.State is Enum_ReconciliationFindingState.Writable)
        {
            string? proofRefusal = await ReproveAsync(finding, operatorUserId);

            if (proofRefusal is not null)
            {
                return Model_Dao_Result_Factory.Failure<Model_ReconciliationFinding>(proofRefusal);
            }
        }

        Model_Dao_Result<int> writeResult = await _receivingRows.UpdateLocationAsync(
            finding.Row.RowSource,
            finding.Row.RecordId,
            destinationLocation.Trim(),
            operatorUserId
        );

        if (writeResult.IsSuccess is false || writeResult.Data is not > 0)
        {
            finding.State = Enum_ReconciliationFindingState.Unresolved;
            finding.Reason =
                writeResult.IsSuccess is false
                    ? writeResult.ErrorMessage
                    : "The write procedure did not change the row, so the location is unchanged.";

            _logger?.LogError(
                $"Reconciliation write failed for {finding.Row.RowSource} row {finding.Row.RecordId}: {finding.Reason}"
            );

            return Model_Dao_Result_Factory.Failure<Model_ReconciliationFinding>(finding.Reason);
        }

        finding.ProposedLocation = destinationLocation.Trim();
        finding.DestinationSource =
            isOperatorDestination
                ? Enum_ReconciliationDestinationSource.Operator
                : Enum_ReconciliationDestinationSource.Derived;
        finding.Outcome =
            isOperatorDestination
                ? Enum_ReconciliationOutcome.ManuallySet
                : Enum_ReconciliationOutcome.Updated;
        finding.State = Enum_ReconciliationFindingState.Written;
        finding.Reason = isOperatorDestination
            ? $"The destination {finding.ProposedLocation} was set by hand and written."
            : $"The destination {finding.ProposedLocation} was accepted and written.";

        _logger?.LogInfo(
            $"Reconciliation wrote {finding.ProposedLocation} to {finding.Row.RowSource} row "
                + $"{finding.Row.RecordId} for part {finding.Row.PartId} as {operatorUserId}."
        );

        return Model_Dao_Result_Factory.Success(finding, writeResult.Data);
    }

    private static string GetRefusalReason(
        Model_ReconciliationFinding finding,
        string destinationLocation
    )
    {
        if (string.IsNullOrWhiteSpace(destinationLocation))
        {
            return "A destination location is required before a line can be accepted.";
        }

        if (
            finding.State
                is not (
                    Enum_ReconciliationFindingState.Writable
                    or Enum_ReconciliationFindingState.Unresolved
                )
        )
        {
            return $"A line in state {finding.State} cannot be accepted.";
        }

        if (
            finding.State is Enum_ReconciliationFindingState.Unresolved
            && finding.Outcome
                is not (
                    Enum_ReconciliationOutcome.NotEnough
                    or Enum_ReconciliationOutcome.ExtraSurplus
                )
        )
        {
            return $"A line with outcome {finding.Outcome} can only be resolved through Needs Attention.";
        }

        return string.Empty;
    }

    /// <summary>
    /// Re-runs the proof over the live balance. Returns a refusal sentence, or null
    /// when the finding's bucket still passes (FR-041).
    /// </summary>
    private async Task<string?> ReproveAsync(
        Model_ReconciliationFinding finding,
        string operatorUserId
    )
    {
        if (finding.ProofContext is null || finding.Bucket is null)
        {
            return "The line carries no movement evidence, so it cannot be proven at accept time.";
        }

        Model_ReconciliationProofContext proofContext = finding.ProofContext;

        int strictWindowDays = await _settings.GetStrictWindowDaysAsync();
        DateOnly afterStrictWindow = proofContext.Key.TransactionDate.AddDays(strictWindowDays + 1);

        Model_Dao_Result<List<Model_ReconciliationLocationBalance>> balances =
            await _inforVisual.GetLocationBalancesAsync(proofContext.Key.PartId, afterStrictWindow);

        if (balances.IsSuccess is false)
        {
            return $"The balance could not be re-read, so the line was not written: {balances.ErrorMessage}";
        }

        proofContext.Balances = balances.Data ?? [];

        Model_ReconciliationGroup group = proofContext.ToGroup();

        Model_Dao_Result<Model_ReconciliationProofResult> proof = await _quantityProof.VerifyAsync(
            group,
            group.Allocations
        );

        if (proof.IsSuccess is false || proof.Data is null)
        {
            return $"The quantity proof could not be re-run: {proof.ErrorMessage}";
        }

        Model_ReconciliationBucketVerdict? verdict = proof.Data.BucketVerdicts.FirstOrDefault(
            candidate => ReferenceEquals(candidate.Bucket, finding.Bucket)
        );

        if (verdict is null)
        {
            return "The line's destination bucket is no longer part of the proof, so the line was not written.";
        }

        if (verdict.Passed)
        {
            return null;
        }

        finding.State = Enum_ReconciliationFindingState.Unresolved;
        finding.Reason = verdict.FailureReason;

        _logger?.LogWarning(
            $"Reconciliation refused {finding.Row.PartId} row {finding.Row.RecordId} for {operatorUserId}: {verdict.FailureReason}"
        );

        return verdict.FailureReason;
    }
}
