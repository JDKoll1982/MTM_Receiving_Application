using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.ViewModels;

/// <summary>
/// The review cycle: one line at a time, destination pre-set to the proposal,
/// Enter accepts and Escape skips, and the session resumes where it stopped
/// (FR-039, FR-040, FR-044, FR-045).
/// </summary>
public partial class ViewModel_ReconciliationMain
{
    /// <summary>The destination the operator may change before accepting the line.</summary>
    [ObservableProperty]
    private string _destinationField = string.Empty;

    /// <summary>The authenticated operator recorded on every write.</summary>
    public string OperatorUserId { get; set; } = Environment.UserName;

    /// <summary>Puts the cycle on the first line it still has to resolve.</summary>
    public void SelectFirstReviewableLine() => SelectNextReviewableLine(null);

    /// <summary>Accepts the selected line and advances to the next unresolved one.</summary>
    [RelayCommand]
    private async Task AcceptAsync()
    {
        Model_ReconciliationFinding? finding = SelectedFinding;

        if (finding is null)
        {
            return;
        }

        Model_Dao_Result<Model_ReconciliationFinding> result = await WriteService.AcceptAsync(
            finding,
            DestinationField,
            OperatorUserId
        );

        if (result.IsSuccess is false)
        {
            DestinationField = finding.ProposedLocation;
            RefreshSummaryFromFindings();
            ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
            return;
        }

        AcceptedCount++;
        RefreshSummaryFromFindings();
        ApplyFilters();
        SelectNextReviewableLine(finding);
    }

    /// <summary>Skips the selected line for this run without writing anything.</summary>
    [RelayCommand]
    private void Skip()
    {
        Model_ReconciliationFinding? finding = SelectedFinding;

        if (finding is null || finding.State is Enum_ReconciliationFindingState.Written)
        {
            return;
        }

        finding.Outcome = Enum_ReconciliationOutcome.Skipped;
        finding.State = Enum_ReconciliationFindingState.SkippedForRun;

        SkippedCount++;
        RefreshSummaryFromFindings();
        ApplyFilters();
        SelectNextReviewableLine(finding);
    }

    private void SelectNextReviewableLine(Model_ReconciliationFinding? resolved)
    {
        SelectedFinding =
            ReviewableFindings.FirstOrDefault(candidate =>
                ReferenceEquals(candidate, resolved) is false
            );

        UpdateProgress();
    }

    /// <summary>
    /// Rebuilds the counts from the findings, so a write or a skip moves the summary
    /// strip immediately without re-running the day.
    /// </summary>
    private void RefreshSummaryFromFindings()
    {
        Model_ReconciliationDaySummary current = Summary;
        Summary = new Model_ReconciliationDaySummary
        {
            Scanned = current.Scanned,
            ExcludedReprintCount = current.ExcludedReprintCount,
            ResidualDisagreement = current.ResidualDisagreement,
            Written = Findings.Count(finding =>
                finding.Outcome
                    is Enum_ReconciliationOutcome.Updated
                        or Enum_ReconciliationOutcome.ManuallySet
            ),
            Unchanged = Findings.Count(finding =>
                finding.Outcome is Enum_ReconciliationOutcome.Unchanged
            ),
            Unresolved = Findings.Count(finding =>
                finding.Outcome
                    is Enum_ReconciliationOutcome.ExtraSurplus
                        or Enum_ReconciliationOutcome.NotEnough
            ),
            Skipped = Findings.Count(finding =>
                finding.Outcome is Enum_ReconciliationOutcome.Skipped
            ),
            Error = Findings.Count(finding => finding.Outcome is Enum_ReconciliationOutcome.Error),
            NotFound = Findings.Count(finding =>
                finding.Outcome is Enum_ReconciliationOutcome.NotFound
            ),
        };

        UpdateUnresolvedCount();
        OnPropertyChanged(nameof(SummaryLineText));
    }
}
