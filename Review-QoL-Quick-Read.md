# Module_Recon Quality-Of-Life — Quick Read

Last Updated: 2026-11-12

One-page digest of [Review-Quality-Of-Life-Suggestions.md](Review-Quality-Of-Life-Suggestions.md).
Read this first; open the full document only for the areas you care about.

## The One Idea

Stop treating this module as a *location repair tool* and treat it as a **receiving integrity
auditor**. Wrong location is just one exception type among several. The other, more common ones
are: labels never printed, labels printed twice, parts received with no label at all, quantities
that disagree with Infor Visual, and material that arrived but never left the staging area.

Every detector follows the same contract: detect with evidence, never write silently, fail
visibly, stay read-only against Infor Visual, explain itself in one sentence.

## Top 5 Highest-Value Ideas

| # | Idea | Why it matters | Full detail |
|---|------|----------------|-------------|
| 1 | **Stuck in staging** — flag rows still sitting in `RECV`/staging past N days | The most common real location problem, and drift detection cannot see it because nothing moved. Confirmed by `LG-04`: residual in the receiving location is legitimate and must be tracked, not forced to zero | [Section 4 (D1)](Review-Quality-Of-Life-Suggestions.md#4-d-location-integrity-beyond-drift) |
| 2 | **Duplicate / reprint double-count** — same physical skid represented twice | Silent quantity inflation; detectable today with no schema change | [Section 1 (A3)](Review-Quality-Of-Life-Suggestions.md#1-a-label-presence-and-print-integrity) |
| 3 | **Missing labels** — ERP receipt quantity vs sum of MTM label quantities | Produces unlabeled floor stock; needs an ERP receipt-number answer first | [Section 1 (A1)](Review-Quality-Of-Life-Suggestions.md#1-a-label-presence-and-print-integrity) |
| 4 | **UOM mismatch** — MTM row says LB while the Visual part is EA | Invalidates every quantity comparison in the module | [Section 2 (B5)](Review-Quality-Of-Life-Suggestions.md#2-b-cross-check-against-infor-visual-receipts) |
| 5 | **Finding disposition** — acknowledge / ignore-with-reason / resolved, persisted | Without it every run rediscovers everything, which trains users to ignore the list | [Section 5 (E5)](Review-Quality-Of-Life-Suggestions.md#5-e-traceability-audit-and-disposition) |

## Eight Families At A Glance

| Family | Answers | Example findings | Detail |
|--------|---------|------------------|--------|
| **A. Label presence and print** | Was a label created, printed, and only once? | Missing label, unprinted label, duplicate skid, "N of M" gaps, quality hold not acknowledged — **not this module** (`UI-13`; belongs with `Module_Reprint`) | [Section 1](Review-Quality-Of-Life-Suggestions.md#1-a-label-presence-and-print-integrity) |
| **B. ERP cross-check** | Does the row match Infor Visual? | PO line missing, wrong part on line, PO closed, over-receipt, UOM mismatch | [Section 2](Review-Quality-Of-Life-Suggestions.md#2-b-cross-check-against-infor-visual-receipts) |
| **C. Row data quality** | Are the fields even usable? | Blank or invalid location, missing heat lot, blank PO line, zero quantity, employee 0 | [Section 3](Review-Quality-Of-Life-Suggestions.md#3-c-row-level-data-quality) |
| **D. Location sanity** | Is the current location reasonable? | Stuck in staging, wrong warehouse, ignored location hiding real findings, on-hand mismatch | [Section 4](Review-Quality-Of-Life-Suggestions.md#4-d-location-integrity-beyond-drift) |
| **E. Audit and disposition** | Who changed what, and did anyone act on it? | Run history, per-row change audit, undo, CSV export, acknowledged/ignored state | [Section 5](Review-Quality-Of-Life-Suggestions.md#5-e-traceability-audit-and-disposition) |
| **F. Workflow ergonomics** | Is it fast to use daily? | Day stepper, exception count badge, filters, accept-remaining shortcut, keyboard, deep links | [Section 6](Review-Quality-Of-Life-Suggestions.md#6-f-workflow-quality-of-life) |
| **G. Automation** | Does anyone have to remember to run it? | Nightly scan, threshold alerts, post-archive trigger | [Section 7](Review-Quality-Of-Life-Suggestions.md#7-g-automation-and-alerting) |
| **H. Analytics** | Is it working? | Accuracy trend, time-to-final-location, drift by location/employee, aging | [Section 8](Review-Quality-Of-Life-Suggestions.md#8-h-analytics-and-reporting) |

## Roadmap In Three Lines

- **P0 — with the refactor, no schema change:** blank/invalid location, missing heat lot, PO line
  checks, UOM mismatch, stuck-in-staging, reprint-row exclusion, exception badge, explanation pane,
  CSV export.
- **P1 — first follow-up, needs schema approval:** run history + per-row audit, finding
  disposition, missing-label detection, print log + duplicate detection, review-card ergonomics.
- **P2 — later:** undo a run, scheduled scans and alerts, split integrity, over-receipt, on-hand
  mismatch, analytics dashboards, cross-module deep links.

Full tables: [Section 9 — Prioritized Roadmap](Review-Quality-Of-Life-Suggestions.md#9-prioritized-roadmap).

## What Needs Approval Before Anything Is Built

| Prerequisite | Unlocks | Why it is unavoidable |
|---|---|---|
| `receiving_label_print_log` (or `printed_at` + `print_count`) | Unprinted label, reprint volume | There is no print audit today at all |
| `reconciliation_run` table | Run history, alerts, trends | Run history must outlive a session |
| `reconciliation_change` audit table | Undo, provenance, idempotency | Location changes need defensible provenance |
| `reconciliation_finding` table | Disposition, aging | Otherwise every run starts from zero |
| ERP receipt quantity per PO line | Missing label, over-receipt | Blocked on the "what is a receiver" answer in `Review-Clarifications.md` (`LG-02`) |
| Confirmed staging-location list | Stuck-in-staging | Business input on which locations count as transient |

Full breakdown: [Section 10 — Data Prerequisites](Review-Quality-Of-Life-Suggestions.md#10-data-prerequisites).

## Not Happening

No writes to Infor Visual. No automatic printing or reprinting. No silent bulk location writes.
No editing label data from this module. No Dunnage/Volvo implementation yet.

Full list: [Section 11 — Explicitly Out Of Scope](Review-Quality-Of-Life-Suggestions.md#11-explicitly-out-of-scope).

## Related Documents

- [Review.md](Review.md) — original spec and algorithm notes
- [Review-Rules.md](Review-Rules.md) — binding rules that any implementation must satisfy
- [Review-Clarifications.md](Review-Clarifications.md) — open decisions before the spec is written
- [Review-Quality-Of-Life-Suggestions.md](Review-Quality-Of-Life-Suggestions.md) — full detail
