using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.ViewModels;

/// <summary>
/// The working page's Needs Attention action. It is disabled whenever no line is
/// unresolved, and opening it hands the current day's unresolved lines to the modal
/// window (FR-046).
/// </summary>
public partial class ViewModel_ReconciliationMain
{
    /// <summary>How many lines are still unresolved.</summary>
    [ObservableProperty]
    private int _unresolvedCount;

    /// <summary>Raised when the operator asks for the Needs Attention window.</summary>
    public event EventHandler? NeedsAttentionRequested;

    /// <summary>The current day's unresolved lines, which the window lists.</summary>
    public IReadOnlyList<Model_ReconciliationFinding> UnresolvedFindings =>
        Findings
            .Where(finding =>
                finding.Outcome
                    is Enum_ReconciliationOutcome.ExtraSurplus
                        or Enum_ReconciliationOutcome.NotEnough
            )
            .ToList();

    /// <summary>Opens the Needs Attention window with the current day's findings.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenNeedsAttention))]
    private void OpenNeedsAttention() => NeedsAttentionRequested?.Invoke(this, EventArgs.Empty);

    private bool CanOpenNeedsAttention() => UnresolvedCount > 0;

    /// <summary>Recomputes how many lines still need attention.</summary>
    private void UpdateUnresolvedCount()
    {
        UnresolvedCount = UnresolvedFindings.Count;
    }

    partial void OnUnresolvedCountChanged(int value) =>
        OpenNeedsAttentionCommand.NotifyCanExecuteChanged();
}
