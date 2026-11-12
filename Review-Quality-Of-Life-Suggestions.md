# Module_Recon Quality-Of-Life Suggestions

Last Updated: 2026-11-12

Companion to `Review.md`, `Review-Rules.md`, and `Review-Clarifications.md`. Where the clarification
register asks "how should the move work", this document asks "what else should the module catch".
Everything here is optional. Nothing here is a code change request by itself.

**Binding constraint:** nothing in this document may override [Review-Rules.md](Review-Rules.md).
Every suggestion still has to satisfy the MVVM flow, SP-only MySQL writes, read-only Infor Visual access,
x:Bind, the DI location rules, and the object-calisthenics size rules. A suggestion that needs an
exception to a rule is not a suggestion — it is a rule change, and rule changes are out of scope here.

## Framing

> **Scope note (2026-11-12).** Two decisions narrow this document:
> `UI-13` = **No** — label-print findings are not part of `Module_Reconciliation`, so family A below moves to
> `Module_Reprint` or a separate audit module. `UI-10` = no export, so `E4` moves out of the module spec.
> Everything else stands.

The current feature answers one question: *"did the saved location drift from where Infor Visual
actually put the material?"*

Most real receiving exceptions are not location drift. They are:

- a label that was never printed,
- a label printed twice,
- two labels for the same physical skid,
- a part received without a label at all,
- a quantity entered differently from what the ERP recorded,
- a heat lot that should exist and does not,
- a row saved against a closed or wrong PO line.

A reconciliation module that only fixes locations leaves those on the floor. The suggestion below
is to grow the module from a *location repair tool* into a *receiving integrity auditor* whose
output is a prioritized exception list, of which "wrong location" is one exception type among
several.

### Design guardrail

Every detector below must follow the same contract as location reconciliation:

1. **Detect** – produce evidence-backed findings.
2. **Never silently write** – anything that changes data goes through an explicit user action.
3. **Fail visible** – unresolved findings are surfaced, never swallowed.
4. **Stay read-only against Infor Visual.**
5. **Explain itself** – every finding carries a human-readable reason and its evidence.

Reusing the existing finding model (existing location, proposed location, resolution, details,
evidence) means most detectors can reuse the same review card, lists, filters, and export.

---

## Contents

1. [A. Label presence and print integrity](#1-a-label-presence-and-print-integrity)
2. [B. Cross-check against Infor Visual receipts](#2-b-cross-check-against-infor-visual-receipts)
3. [C. Row-level data quality](#3-c-row-level-data-quality)
4. [D. Location integrity beyond drift](#4-d-location-integrity-beyond-drift)
5. [E. Traceability, audit, and disposition](#5-e-traceability-audit-and-disposition)
6. [F. Workflow quality of life](#6-f-workflow-quality-of-life)
7. [G. Automation and alerting](#7-g-automation-and-alerting)
8. [H. Analytics and reporting](#8-h-analytics-and-reporting)
9. [Prioritized roadmap](#9-prioritized-roadmap)
10. [Data prerequisites](#10-data-prerequisites)
11. [Explicitly out of scope](#11-explicitly-out-of-scope)

---

## 1. A. Label Presence And Print Integrity

> **Out of scope for `Module_Reconciliation`** per decision `UI-13` (2026-11-12). Retained here because the
> detection logic is still worth building — in `Module_Reprint`, which already owns the label queue and the
> reprint path, or in a separate receiving audit module.

These are the "did the paperwork match the material" checks. None of them exist today.

### A1 — Missing label (received but never labeled)

**Catches:** material was received in Infor Visual but the operator never created a label row, or
created fewer labels than the receipt requires.

**Method:** compare ERP receipt quantity against the sum of MTM label quantities for the same
`(PO, part, line, receipt window)`:

```text
erp_received_qty  vs  sum(mtm_label.weight_quantity)  for the same PO + part + line
```

Outcome buckets: `FullyLabeled`, `UnderLabeled` (short by N), `NotLabeled` (no rows at all),
`OverLabeled` (see A3).

**Data needed:** ERP receipt quantity (`RECEIVER_LINE` / `PURC_ORDER_LINE` received quantity), or,
without ERP access, `qty_ordered` minus `remaining_quantity` as a weaker proxy. The proxy is
weaker because partial receipts and multiple receipts against one line are indistinguishable.

**Effort:** M (query work). **Value:** high — this is the failure mode that produces unlabeled
floor stock. **Risk:** medium — false positives if ERP quantity is not scoped to the same receipt.

**Depends on:** `LG-02` (receiver data source). If the answer is "MTM rows are the receiver proxy",
this detector cannot be built and must be dropped.

### A2 — Label never printed

**Catches:** a label row exists in `receiving_label_data` but no label was ever sent to the
printer, so a skid sits on the floor with no tag.

**Blocker:** `receiving_label_data` has **no** print timestamp and no print audit table. This
cannot be inferred reliably today.

**Options:**

- **A2a** Add `printed_at DATETIME NULL` and `print_count INT` to `receiving_label_data` plus a
  write in the print path. Simple, exact going forward, no history.
- **A2b** Add a separate `receiving_label_print_log` table (row id, printed_at, employee, printer,
  is_reprint). Preserves history and supports A5/A6.
- **A2c** Infer from the label-overlay print service call only at print time (no persistence),
  which gives a one-session "not yet printed" badge but no audit.

**Recommendation:** A2b. A2a is the cheapest but loses the ability to answer "printed twice?".
A2c is a UI nicety, not an integrity check.

**Effort:** M (schema + print path + detector). **Value:** high. **Risk:** low.

### A3 — Duplicate or double-counted labels

**Catches:** the same physical skid represented by more than one row, so quantity is overstated.

Signals already available:

- `is_reprint = 1` rows sitting in `receiving_label_data` alongside the original history row.
- Identical `(part_id, po_number, po_line_number, weight_quantity, initial_location)` rows created
  within a short time window.
- `load_id` duplicated across rows that should be distinct skids.

**Method:** flag groups where the combined quantity exceeds the transfer or receipt evidence, and
flag explicit reprint rows that share an identity with a history row.

**Effort:** S. **Value:** high — this is a silent quantity-inflation bug class.
**Risk:** low, but the reason text must distinguish "possible reprint duplicate" from
"possible double entry" or operators will distrust it.

**Note:** this detector is what makes `LG-11` (reprint rows in reconciliation) safe either way.

### A4 — Label split integrity

**Catches:** the "N of M" label arithmetic is wrong.

Available columns: `label_number`, `part_skid_sequence`, `part_skid_total`,
`packages_per_load`, `weight_per_package`, `weight_quantity`.

Checks:

- `part_skid_sequence` values are `1..part_skid_total` with no gaps or duplicates.
- `sum(weight_quantity)` for a save batch equals the batch total recorded at save time.
- `weight_per_package * packages_per_load` equals `weight_quantity` (within the rounding epsilon).
- `label_number` restarts correctly per load.

**Effort:** S. **Value:** medium (wrong labels get reprinted less often).
**Risk:** low.

### A5 — Reprint abuse / unplanned reprint volume

**Catches:** the same skid reprinted repeatedly, or reprints created outside the Reprint module.

**Data needed:** the print log from A2. Reports reprint count per row, per employee, per day.

**Effort:** S once A2b exists. **Value:** medium.

### A6 — Quality hold not acknowledged

**Catches:** `is_quality_hold_required = 1` with `is_quality_hold_acknowledged = 0` on a row that
was already archived to history.

**Effort:** S. **Value:** medium-high (compliance). **Risk:** low.

---

## 2. B. Cross-Check Against Infor Visual Receipts

These checks compare the MTM row to ERP truth. All are read-only Visual queries.

### B1 — PO line does not exist, or belongs to another part

`Review.md` §7.6 already requires matching on PO + part. This extends it: check that the part is
actually on the PO line recorded on the row (`PURC_ORDER_LINE`).

**Catches:** typos in PO or line, part substituted at the dock, blanket-order suffix confusion
(the `PO-061555B` case in `docs/prompts/reconciliation-location-fix.md` Fix 1).

**Effort:** S (one query). **Value:** high — cheap, and catches data-entry errors that currently
survive forever.

### B2 — PO line closed, canceled, or fully received

**Catches:** labels created against a PO line that was already closed, which usually means the
receipt was mis-keyed or duplicated.

**Effort:** S. **Value:** medium.

### B3 — Over-receipt against the PO line

**Catches:** total labeled quantity exceeds `qty_ordered - already_received` for the line.

**Effort:** M (needs correct ERP received-quantity semantics). **Value:** high (over-receipt is a
payables problem, not just a labeling problem). **Risk:** medium (false positives without partial
receipt handling).

### B4 — Received date outside the PO due-date window

**Catches:** labels dated far outside the PO's expected window, which usually means a wrong PO was
selected.

**Effort:** S. **Value:** medium. **Risk:** low if the threshold is generous and configurable.

### B5 — Unit-of-measure mismatch

**Catches:** MTM row says `LB` while the Visual part is `EA` (or vice versa).

**Effort:** S. **Value:** high — a UOM mismatch silently invalidates every quantity comparison,
including location reconciliation (`LG-06`).

### B6 — Part not found in Infor Visual

**Catches:** non-PO or obsolete part numbers that can never be reconciled.

**Effort:** S. **Value:** low-medium. Overlaps the existing `IsNonPOItem` handling.

---

## 3. C. Row-Level Data Quality

Cheap field checks over rows already loaded. All are `S` effort.

| ID | Check | Catches | Value |
|----|-------|---------|-------|
| C1 | `initial_location` blank | Rows that can never be reconciled or traced | High |
| C2 | `initial_location` not a real Visual location | Typos, retired locations, wrong warehouse | High |
| C3 | `po_line_number` blank on a PO row | Rows that fall out of every PO-based join | Medium |
| C4 | `heat` blank on a part that requires a heat lot | Traceability gap on heat-controlled material | High |
| C5 | `unit_of_measure` blank or not in the allowed set | Corrupt quantity semantics | Medium |
| C6 | `employee_number = 0` or `user_id` blank | Unattributable rows | Medium |
| C7 | `received_date` in the future or absurdly in the past | Clock or data-entry error | Low |
| C8 | `weight_quantity` zero or negative | Zero-quantity labels | Medium |
| C9 | `qty_ordered` present but `remaining_quantity` stale | Local mirror drift from ERP | Low |
| C10 | Non-PO row missing `vendor_name` | Incomplete non-PO records | Low |

**Note on C4:** the repo already has `lot_markings`, `lot_marking_part_scope`, and
`dunnage_no_label_parts`, so "does this part require a heat lot" is answerable locally without
touching Visual.

---

## 4. D. Location Integrity Beyond Drift

The current module only asks "did the location change?". These ask "is the location sane?".

### D1 — Material still in a staging or receiving location after N days

**Catches:** skids that arrived, were staged, and were never put away. This is the most common
real-world location problem and it is invisible to a drift-only check because the location never
changed.

**Confirmed by `LG-04`:** the source-residual rule means the module already computes exactly how much
material is legitimately left in the receiving location. This detector is therefore nearly free — it
compares that residual against elapsed days instead of against zero.

**Method:** flag rows whose current location is `RECV`, a `V-STAGE`-style location, or any location
in a configurable "transient" list, where `received_date` is older than a configurable threshold.

**Effort:** S. **Value:** very high — arguably higher than drift detection.
**Risk:** low.

### D2 — Destination is an ignored or transient location

The ignored-locations setting exists. Extend its use: if transfer evidence points into an ignored
location, that currently yields `IgnoredBySettings`. Surface the reason in the exception list
instead of counting it as unresolved noise.

**Effort:** S. **Value:** medium.

### D3 — Location in the wrong warehouse or plant

**Catches:** transfer evidence that resolves to a location outside the expected warehouse prefix
for the receiving context.

**Effort:** S. **Value:** medium. **Risk:** medium (needs a confirmed warehouse-prefix rule).

### D4 — Current on-hand quantity does not match the labeled quantity at that location

**Catches:** the material is at the right location but in the wrong amount — usually part of the
skid was consumed without a transaction.

**Method:** compare `CR_PART_LOCATION` quantity for `(part, location)` against the sum of MTM row
quantities placed there.

**Effort:** M. **Value:** high. **Risk:** medium-high — current on-hand includes material from other
receipts, so this detects *aggregate* mismatch, not per-skid mismatch. Must be presented as
informational, never as a location correction.

### D5 — Ignored-location list hygiene

Report ignored locations that no longer exist in Visual or have had no activity for N days, so the
list does not silently suppress real findings.

**Effort:** S. **Value:** low-medium.

### D6 — More destination locations than skids

**Catches:** transfer evidence that spreads one receipt across more locations than there are
physical skids, which usually means the movement window pulled in a neighboring receipt.

**Effort:** S once allocation runs. **Value:** medium. Related to the surplus case in `LG-16`.

---

## 5. E. Traceability, Audit, And Disposition

Every detector above produces findings. Findings need a lifecycle or they become noise nobody
reads.

### E1 — Reconciliation run history

Persist each run: user, timestamp, day reconciled, rows scanned, counts per outcome, duration, run id.

**Why:** answers "did anyone run this this week?" and gives `AR-10` something to correlate with.

**Effort:** M (new table + SPs). **Value:** high.

### E2 — Per-row change audit

For every written location change: old location, new location, resolution at write time, evidence
(transfer ids, Visual user, transfer time), acting user, timestamp.

**Why:** without this, no one can defend a location change to an auditor, and `LG-12`
(idempotency) is impossible.

**Effort:** M. **Value:** very high. **Depends on:** `DS-07`.

### E3 — Undo a run

Reverse the location changes from one run, gated to the original operator or an admin, and only
when no later change touched the same row.

**Effort:** M. **Value:** high (it is what makes a fast per-line review cycle safe to trust).
**Risk:** medium — needs the E2 audit to be trustworthy first.

### E4 — Export exceptions

> **Deferred** per decision `UI-10` (2026-11-12): the module ships without an export action. Recorded here
> for a later request.

Export the current finding set to CSV or XLSX with reason, evidence, and proposed action.

**Effort:** S. **Value:** high — most supervisors will triage in Excel before acting.

### E5 — Finding disposition

Per finding: `Open`, `Acknowledged`, `Ignored with reason`, `Resolved`. Persisted so a finding
does not reappear every run.

**Why:** today every run rediscovers everything, which trains users to ignore the list.

**Effort:** M. **Value:** very high. **Depends on:** `E1`, `E2`.

### E6 — Assign a finding to a user

Lightweight ownership ("you own this one") for teamwork.

**Effort:** S once E5 exists. **Value:** medium.

---

## 6. F. Workflow Quality Of Life

### F1 — Day stepping and remembered day

Remember the last reconciled day per user, and open on it instead of always landing on Today — a
supervisor who works yesterday's receipts every morning should not have to step back a day. Optional
`Start` / `End` sweep so a backlog can be cleared one day at a time without 30 manual clicks
(see `UI-02`).

**Effort:** S. **Value:** high (daily users work the same day, then step forward).

### F2 — Exception dashboard card

A count badge per exception family on the landing page ("12 need location review, 3 unlabeled,
1 UOM mismatch"), so users know whether running it is worth their time.

**Effort:** S. **Value:** high.

### F3 — Filter, group, and column chooser

Filter by family, resolution, part, PO, employee, source. Group by part/PO. Show/hide columns like
`Module_Reprint`'s column chooser.

**Effort:** S-M. **Value:** medium-high. **Depends on:** `UI-05`, `UI-06`.

### F4 — Accept-remaining shortcut after a spot check

The review is per-line confirmation by design (`UI-03`), so there is no blind bulk apply. Instead, once the
user has worked the first few lines, offer an "accept all remaining recommendations" shortcut that shows
the remaining count and the distinct destination locations before it runs.

**Effort:** S. **Value:** medium-high — keeps a 40-line day from becoming 40 clicks without turning the
module into a rubber stamp.

### F5 — Dry-run indicator

A persistent, unmissable "Preview only — nothing written" vs "Applying" state.

**Effort:** S. **Value:** medium (prevents the worst failure mode: silent bulk writes).

### F6 — Keyboard-first operation

Next/previous finding, apply, skip, re-run, toggle filter. Warehouse users work fast and resent
mouse-only screens.

**Effort:** S. **Value:** medium-high.

### F7 — Deep links to the owning surface

From a finding, jump to the row in Receiving Edit Mode, or to `Module_Reprint` for a reprint, or to
the Visual transaction in a read-only view.

**Effort:** M. **Value:** high — turns the exception list into an actionable queue instead of a
report.

### F8 — "Why is this row here?" explanation panel

Every finding must be explainable in one sentence derived from its evidence, without the user
reading code. The current `Details` strings already do this; make it a first-class pane.

**Effort:** S. **Value:** high.

### F9 — Sticky run scope

Show the active day and filter in the header at all times, so a screenshot of a finding is
self-documenting.

**Effort:** S. **Value:** medium.

---

## 7. G. Automation And Alerting

The repo already has `reporting_scheduled_reports`, so scheduling infrastructure exists.

### G1 — Scheduled nightly scan

Run the read-only detectors nightly over the previous day + unresolved findings, persist results
via `E1`, notify on new findings.

**Effort:** M (detectors must be read-only and idempotent first). **Value:** very high.

### G2 — Threshold alerts

Notify (email or Teams) when new exceptions exceed a threshold, for example "more than 5 unlabeled
receipts today".

**Effort:** S once G1 exists. **Value:** high.

### G3 — Post-archive trigger

Offer a scan after `sp_Receiving_LabelData_ClearToHistory` completes for a load, so location drift
is caught the same day instead of whenever someone remembers to look.

**Effort:** M. **Value:** high. **Risk:** medium — must not block the archive path; run
asynchronously and never fail the parent operation.

### G4 — Workstation idle scan

Run a read-only scan when the app has been idle, surfacing a badge. Lower value; listed for
completeness.

---

## 8. H. Analytics And Reporting

### H1 — Reconciliation accuracy trend

Per run: changes proposed vs changes accepted vs changes ignored. Trending acceptance shows whether
the algorithm is trusted.

**Effort:** S once `E1` exists. **Value:** high (this is the metric that justifies the module).

### H2 — Time-to-final-location

Median elapsed time from `received_date` to the transfer that moved material out of the receiving
location. A directly useful operational KPI.

**Effort:** M. **Value:** high.

### H3 — Drift by location, employee, or shift

Which locations and which operators generate the most drift or the most unlabeled receipts. Use
for training, not punishment — state that in the UI copy.

**Effort:** S once `E1`/`E2` exist. **Value:** medium-high. **Risk:** low, but governance-sensitive.

### H4 — Unresolved finding aging

How long findings stay open. Exposes whether the exception list is actually being worked.

**Effort:** S once `E5` exists. **Value:** high.

### H5 — Reprint volume trend

Reprints per 100 receipts, per employee, per part. A spike usually means a printer, template, or
trainer problem.

**Effort:** S once `A2b` exists. **Value:** medium.

---

## 9. Prioritized Roadmap

Ordering assumes the module extraction happens first with current behavior preserved.

### P0 — Ship with the refactor (no new schema)

| ID | Feature | Why P0 |
|----|---------|--------|
| C1, C2 | Blank / invalid initial location | Free checks over already-loaded rows |
| C4 | Missing heat lot where required | Uses existing `lot_markings` tables |
| B1 | PO line missing or wrong part | One read-only query, catches real entry errors |
| B5 | UOM mismatch | Protects every quantity comparison in the module |
| D1 | Stuck in staging/receiving | Highest real-world value, no schema change |
| LG-11 support | Reprint-row exclusion with reporting | Correctness prerequisite |
| F2 | Exception dashboard counts | Makes the module worth opening |
| F8 | Explanation panel | Makes findings trustworthy |
| E4 | Export | Lets supervisors triage immediately |

### P1 — First follow-up release (needs approval for schema)

| ID | Feature | Dependency |
|----|---------|------------|
| E1, E2 | Run history + per-row audit | Schema |
| E5 | Finding disposition | E1, E2 |
| A1 | Missing label (needs ERP receipt quantity) | `LG-02` |
| A2b, A3 | Print log + duplicate detection | Schema |
| F3, F4, F6 | Review-card ergonomics, accept-remaining, keyboard | UI decisions |
| H2 | Time-to-final-location | E1 |

### P2 — Later

| ID | Feature | Note |
|----|---------|------|
| E3 | Undo a run | Only after E2 is trusted |
| G1, G2 | Scheduled scan + alerts | Detectors must be read-only first |
| A4, A5, A6 | Split integrity, reprint volume, quality hold | Cheap once print log exists |
| B2, B3, B4 | PO state, over-receipt, date window | Needs ERP receipt semantics |
| D4 | On-hand vs labeled at location | Present as informational only |
| H1, H3, H4, H5 | Analytics | Reporting module surface |
| G3 | Post-archive trigger | Async, non-blocking |
| F7 | Deep links | Touches other modules |

---

## 10. Data Prerequisites

Grouped by approval cost.

### No schema change

- All of section C.
- B1, B5 (read-only Visual queries only).
- D1, D2, D3, D6 (use existing locations + transfer evidence).
- E4, F1, F2, F5, F6, F8, F9.
- H1/H2/H3 can be computed from logs but are much weaker without `E1`.

### New columns or tables (requires explicit approval)

| Prerequisite | Enables | Why it cannot be avoided |
|---|---|---|
| `receiving_label_print_log` (or `printed_at` + `print_count`) | A2, A5, H5 | There is no print audit today |
| `reconciliation_run` table + SPs | E1, G1, G2, H1, H3 | Run history must outlive a session |
| `reconciliation_change` audit table + SPs | E2, E3, H4 | Location changes need provenance |
| `reconciliation_finding` (disposition) table + SPs | E5, E6, H4 | Otherwise every run rediscovers everything |

### Blocked on business data decisions

| Prerequisite | Enables | Blocked by |
|---|---|---|
| True ERP receipt quantity per PO line | A1, B3 | `LG-02` (receiver data source) |
| Confirmed "transient location" list | D1 | Business input on which locations count as staging |
| Confirmed warehouse-prefix rule | D3 | Business input |
| Confirmed "assy" meaning | `LG-18` | Business input — may add `WIP_ISSUE_DETAIL` as a source |

---

## 11. Explicitly Out Of Scope

Listed so the spec author does not quietly expand the refactor.

- Writing anything to Infor Visual. This includes any "mark as audited" or "flag" column in ERP.
- Automatic printing or automatic reprinting. The module detects; the user acts.
- Automatic bulk location writes without human review.
- Editing label data (part, quantity, PO) from this module. Edits belong to Receiving Edit Mode.
- Dunnage and Volvo label integrity. Design the seam (`SC-08`) but do not implement.
- Replacing `Module_Reprint`. Reprint actions deep-link into it.
- Machine-vision or physical verification of what is on a skid.
- Any change to stored procedure signatures that other screens depend on, unless called out and
  reviewed individually.
