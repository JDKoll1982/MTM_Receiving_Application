namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// The counts the working page shows. Each count maps to exactly one outcome, so
/// the totals reconcile against the rows loaded (FR-021, SC-003). There is no
/// export member (FR-049, SC-012).
/// </summary>
public sealed class Model_ReconciliationDaySummary
{
    /// <summary>The rows loaded for the day.</summary>
    public int Scanned { get; set; }

    /// <summary>The lines written: updated plus manually set.</summary>
    public int Written { get; set; }

    /// <summary>The lines whose saved location already matched.</summary>
    public int Unchanged { get; set; }

    /// <summary>The lines carrying extra surplus or not enough.</summary>
    public int Unresolved { get; set; }

    /// <summary>The lines excluded from candidacy or skipped for this session.</summary>
    public int Skipped { get; set; }

    /// <summary>The lines whose Visual call or write failed.</summary>
    public int Error { get; set; }

    /// <summary>The lines whose group found no qualifying movement.</summary>
    public int NotFound { get; set; }

    /// <summary>The reprint rows kept out of the candidate set (FR-016).</summary>
    public int ExcludedReprintCount { get; set; }

    /// <summary>True when any residual check failed (FR-024).</summary>
    public bool ResidualDisagreement { get; set; }

    /// <summary>
    /// The sum of every counted bucket. A test asserts this equals
    /// <see cref="Scanned"/> so a missing outcome cannot hide in the summary.
    /// </summary>
    public int TotalAccounted => Written + Unchanged + Unresolved + Skipped + Error + NotFound;
}
