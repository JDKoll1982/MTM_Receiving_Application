# Module Reconciliation — Clarification Register

Last Updated: 2026-11-12

**Status: all 66 questions answered.** This register is now the input to the SpecKit spec, not a
questionnaire. No code changes are implied by this document.

Two things remain, and neither blocks spec authoring:

- The eleven conflicts in [Section 13](#13-conflicts-introduced-by-these-answers) need a ruling — nine are
  already resolved inline, two are yours to call.
- The outstanding investigations are **deferred to the SpecKit run** — see
  [Section 14](#14-research-deferred-to-the-speckit-run). Do not perform them before the spec exists; they
  belong in the plan/research phase, where their findings land next to the design they justify.

**Read [Review-Rules.md](Review-Rules.md) first.** It contains the binding rules that settle every
architectural question in this document. Answers that conflict with a rule lose to the rule.

## How To Use This Document

- Every question has a stable ID (`SC`, `AR`, `UI`, `LG`, `DS`, `TS`, `MG`, `NF`).
- `Constraint:` lines cite the rule that narrows the answer.
- `Decided (2026-11-12):` lines are final and are mirrored in the [Decision Record](#12-decision-record).
- [Section 11](#11-resolved-baseline) is the one-page summary of everything settled.
- [Section 13](#13-conflicts-introduced-by-these-answers) lists answers that disagree, with resolutions.
- [Section 14](#14-research-deferred-to-the-speckit-run) lists what still needs investigating and when.

## Contents

1. [Ground truth already in the repo](#1-ground-truth-already-in-the-repo)
2. [Retired questions](#2-retired-questions)
3. [Scope and module boundary (SC)](#3-scope-and-module-boundary-sc)
4. [Scaffolding (AR)](#4-scaffolding-ar)
5. [UI and UX (UI)](#5-ui-and-ux-ui)
6. [Reconciliation logic (LG)](#6-reconciliation-logic-lg)
7. [Data sources and schema (DS)](#7-data-sources-and-schema-ds)
8. [Testing (TS)](#8-testing-ts)
9. [Migration and cleanup (MG)](#9-migration-and-cleanup-mg)
10. [Non-functional (NF)](#10-non-functional-nf)
11. [Resolved baseline](#11-resolved-baseline)
12. [Decision record](#12-decision-record)
13. [Conflicts introduced by these answers](#13-conflicts-introduced-by-these-answers)
14. [Research deferred to the SpecKit run](#14-research-deferred-to-the-speckit-run)

---

## 1. Ground Truth Already In The Repo

Several `(VERIFY)` items in `Review.md` are already answered by the existing implementation, and
several `Review.md` assumptions conflict with what is actually built. Read this before answering
anything.

| Area | What exists today | Consequence for the refactor |
|------|-------------------|------------------------------|
| Service | `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` (~1.2k lines) implements transfer-movement-based allocation | `Review.md`'s algorithm is a *simplification* of what is already built, and the file violates the size rules in R-CODE-01 |
| Contract | `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs` — `GetRecommendedLocationsAsync`, `PreviewLocationsAsync`, `ApplyLocationUpdateAsync`, `ReconcileLocationsAsync` | `GetRecommendedLocationsAsync` is also called by `ViewModel_Receiving_LoadEntry`, so the interface cannot move wholesale |
| Models | `Model_ReceivingLocationReconciliationItem`, `Model_ReceivingLocationReconciliationSummary` in `Module_Receiving/Models` | Move candidates, but they reference `Model_ReceivingLoad` |
| Grouping key | `(PartID, ReceivedDate.Date)` — PO is **not** part of the group key | `Review.md` asks for `(PO, Part)`. Real behavioral conflict (`LG-01`) |
| Date scoping | No user date range. `includeAllHistory` bool: `StartDate = 2000-01-01` or `DateTime.Today`, `EndDate = Today + 1` | One calendar day per run is the agreed target (`UI-02`), so row selection tightens and the old `includeAllHistory` flag disappears |
| Transfer discovery | Query `18` with a **±2 day** window around each group's receipt date (see R-SCOPE-02) | Not a range; an anchor-date window |
| Strict validation | Allocated rows per destination bucket must exactly equal transfer quantity for `receiptDate` and `receiptDate + 1` | Different gate than `Review.md`'s receiver-quantity equality (`LG-04`) |
| Cross-pass merge | Current Labels + History rows for the same `(part, date)` already reconcile together | Preserve per R-SCOPE-02; do not separate them again |
| Visual "receiver" | **No** `RECEIVER` / `RECEIVER_LINE` query exists anywhere in the app | `Review.md` §5.1 has no implementation behind it (`LG-02`) |
| Current-inventory compare | Not used in the preview path. Only `GetRecommendedLocationsAsync` reads evidence query `16` | `Review.md` §5.2 is not implemented in the reconcile flow (`LG-03`) |
| MySQL write path | `UpdateCurrentLabelDataAsync` → `sp_Receiving_LabelData_Update`; `UpdateReceivingLoadsAsync` → `Dao_ReceivingLoad.UpdateLoadsAsync` | SP-only writes already satisfied (R-DATA-03); the new module must **not** reuse these — it gets its own procedures (`MG-05`) |
| Write authorization | Both write methods **enforce Edit Mode ownership**: they throw `InvalidOperationException` when there is no session or when rows belong to another employee/user | A reconciler updating other users' rows fails today (**BLOCKER**, `DS-06`) |
| Settings | `Receiving.BusinessRules.ValidateAllHistoryForLocationReconciliation`, `Receiving.UserPreferences.IgnoredReconciliationLocationsJson`, UI in `Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml` | Settings ownership and value migration question (`AR-04`) |
| UI entry | Mode Selection card → `Enum_ReceivingWorkflowStep.ReconciliationReview` → hosted inside `View_Receiving_Workflow.xaml` | It is a workflow step, not a real dialog |
| Interaction model | One-row-at-a-time wizard (Save / Ignore per row), then a summary page | `Review.md` implies a single "Update Locations" action (`UI-03`, `UI-04`) |
| SQL files | `Database/InforVisualScripts/Queries/16, 17, 18, 18b` | `18a_GetTransferCandidatePairs.sql` is referenced by `docs/prompts/reconciliation-location-fix.md` but **does not exist** (`DS-02`) |
| Precedent | `Module_Reprint` was extracted the same way (nav page, `docs/FeatureUpdates/01-FeatureChange-ReprintLabels.md`) | Strongest template for scaffolding and removal ordering (R-SCOPE-03) |
| Row schema | `receiving_label_data` has `is_reprint`, `label_number`, `part_skid_sequence`, `part_skid_total`, quality-hold flags, `heat`, `initial_location`. **No print timestamp or print audit** | "Was a label printed?" cannot be answered today |
| Transfer sign convention | `INVENTORY_TRANS` transfers are reciprocal positive rows; lower `TRANSACTION_ID` is source, higher is destination | Answers `Review.md` §9.3 partially; preserved by R-SCOPE-02 |

---

## 2. Retired Questions

These were open questions in the first draft of this register. They are settled by the rules and are
no longer decisions. See [Review-Rules.md](Review-Rules.md) §14 for the full mapping.

| Retired | Original question | Settled by |
|---------|-------------------|------------|
| `AR-01` | DI registration shape | R-DI-01, R-DI-02 |
| `AR-02` | Type and contract naming | R-NAM-01 through R-NAM-03 |
| `AR-03` | DAO shape (instance, result type, throwing) | R-DATA-01, R-DATA-02 |
| `AR-06` | Header provider, back action, accent wiring | R-HDR-01 through R-HDR-05 |
| `DS-05` | Is Visual still read-only? | R-DATA-04, R-SQL-07 |
| `LG-08` | Keep the transfer pairing assumption? | R-SCOPE-02 |
| `LG-09` | Keep the chained-move rule? | R-SCOPE-02 |
| `LG-17` | Decimal arithmetic rules | R-SCOPE-05 |
| `TS-01` | Test framework, naming, fixtures | R-TEST-01 through R-TEST-03, R-NAM-06 |
| `TS-08` | Build and test commands | R-TEST-05, R-TEST-06 |
| `NF-03` | Error escalation boundary | R-ERR-01, R-ERR-02 |
| `UI-11` | Manual override for `Ambiguous` rows | Merged into `UI-03` — every line's destination is editable, so a separate override action is redundant |
| `UI-12` | Accessibility baseline | R-MVVM-05 |
| `MG-02` (part) | Must documentation be updated? | R-DOC-01, R-DOC-02 |

---

## 3. Scope And Module Boundary (SC)

### SC-01 — Which full word names the module?

**Constraint:** R-NAM-03 forbids abbreviations in namespaces. `Module_Recon` is therefore not
available, despite `Review.md`.

- A) `Module_Reconciliation`
- B) `Module_ReceivingReconciliation`
- C) `Module_LocationReconciliation`

**Recommendation:** A. Folder, namespace, nav tag, DI extension, and spec title all follow it.

### SC-02 — Does `GetRecommendedLocationsAsync` move?

`ViewModel_Receiving_LoadEntry` calls it during normal receiving entry to suggest a location while
the operator types. It reconciles nothing.

**Constraint:** R-MOD-03 and R-MOD-04 forbid making `Module_Receiving` depend on the new module for a
typing assist.

- A) Split the interface: recommendation stays in `Module_Receiving`; reconcile methods move
- B) Move everything; Receiving references the new module

**Recommendation:** A.

### SC-03 — How does the new module get receiving rows?

**Constraint:** R-MOD-03 forbids reach-through into `Module_Receiving/Data` or its internal models.
R-MOD-01 forbids hoisting `Model_ReceivingLoad` into `Module_Core`.

- A) New module declares a slim row model and maps from an explicit receiving contract
- B) New module owns its own DAO and SPs, no dependency on `Module_Receiving`
- C) New module consumes `IService_MySQL_Receiving` (a facade, not internals)
- D) Move `Model_ReceivingLoad` into `Module_Core`

**Recommendation:** B for writes and reads, A for the row shape. C is acceptable only if a separate
write path (`DS-06`) provides the privileged access, because the current facade throws on foreign rows.
D is excluded by R-MOD-01.

### SC-04 — Is reconciliation still a Receiving workflow step?

Today it is `Enum_ReceivingWorkflowStep.ReconciliationReview = 12`, hosted inside
`View_Receiving_Workflow`.

- A) Remove the enum member and the workflow host entirely
- B) Keep the enum member (value reserved) but never navigate to it
- C) Keep a thin Receiving entry point that opens the new module

**Recommendation:** A, plus deleting the card and command from `ViewModel_Receiving_ModeSelection`.
Confirm no persisted state stores the enum numerically first (`TS-06`).

### SC-05 — Where does it live in navigation?

Existing tags: `ReceivingWorkflowView`, `DunnageLabelPage`, `VolvoShipmentEntry`,
`ReportingMainPage`, `ShipRecToolsPage`, `ReprintLabelsPage`, `ScannerMainPage`,
`AppDocumentation`.

- A) New top-level nav item, placed next to `ReprintLabelsPage`
- B) Under `ShipRecToolsPage`
- C) Under `ReportingMainPage`

**Recommendation:** A. Confirm the exact insertion order and label.

### SC-06 — Does the Mode Selection card get removed?

**Constraint:** R-MOD-04 forbids Receiving referencing the new module to keep a shortcut working.

- A) Remove the card, command, and accessibility name
- B) Keep a shortcut that opens the new module

**Recommendation:** A. Two entry points let permissions, date scope, and telemetry diverge.

### SC-07 — Which artifacts count as "reconcile-specific"?

**Constraint:** R-SCOPE-04 — remove only feature-specific artifacts; never delete shared code.

The candidate list is in [Section 9](#9-migration-and-cleanup-mg). Confirm the boundary, especially
queries `16` and `17`, which back the entry-time recommendation feature rather than reconciliation.

### SC-08 — Receiving only, or Receiving + Dunnage + Volvo?

**Constraint:** R-MOD-06 — do not build a provider framework for sources that do not exist yet.

- A) Receiving only; name the seam, implement nothing extra
- B) Build pluggable per-source providers now

**Recommendation:** A.

### SC-09 — What is the user-facing feature name?

- A) "Reconcile Saved Locations from InforVisual" (current)
- B) "Receiving Reconciliation"
- C) "Location Reconciliation"

**Recommendation:** B in navigation, with "from Infor Visual" as subtitle text.

---

## 4. Scaffolding (AR)

`AR-01`, `AR-02`, `AR-03`, and `AR-06` are retired — see [Section 2](#2-retired-questions).

### AR-04 — Settings key migration

Two settings drive today's behavior, both under `Receiving.*` keys.

**Constraint:** R-SET-01 — new settings go through the settings core facade and CQRS flow.

- A) New `Reconciliation.*` keys with a one-time copy from the `Receiving.*` keys
- B) Keep the `Receiving.*` keys and surface them through the settings core
- C) Keep the keys and keep the settings page where it is

**Recommendation:** A. Duplicated keys with no migration silently reset every user's ignored
locations, changing reconciliation results on day one.

### AR-05 — Which `Module_Settings.*` project hosts the settings page?

**Constraint:** R-SET-03 — settings UI lives under a `Module_Settings.*` project, never inside the
working module's `Views`.

- A) New `Module_Settings.Reconciliation`
- B) Existing `Module_Settings.Receiving`

**Recommendation:** A. With the zero-coupling direction, B would leave the new module's settings inside
Receiving's settings namespace and navigation, which is exactly the entanglement being removed. Add the
project and the settings-manifest entry.

### AR-07 — Is a schema migration in scope?

Consequence of `DS-06` and `DS-07`. If no new columns or tables are approved, this refactor is
migration-free.

**Constraint:** R-DI-03 / R-CODE-06 — schema changes need explicit approval.

**Recommendation:** keep the refactor migration-free; put audit and print-tracking tables behind a
separate approved change.

### AR-08 — Execution model for a single day

`Review.md` implied one large range pass. With the one-day unit of work (`UI-02`) the exposure drops
sharply.

- A) Single `await` per day with an indeterminate banner
- B) Per-day run with progress, plus a stop-between-days control for a sweep
- C) Row cap per day as a safety net

**Recommendation:** B plus C. One day is small, so the old `All history` scan problem disappears — but a
sweep over 90 days must still be stoppable between days, and a pathological day should fail the day
without taking the sweep down with it.

### AR-09 — Where do the Visual SQL files live after the move?

**Constraint:** R-SQL-01 — `Database/` remains the source of truth.

- A) Stay in `Database/InforVisualScripts/Queries/` with existing numbers
- B) Move into `Database/InforVisualScripts/Queries/Reconciliation/` without renumbering
- C) Renumber

**Recommendation:** B. Renumbering breaks every cross-reference in docs and prompts for no gain.

### AR-10 — Logging and run identity

**Constraint:** R-ERR-03 — logs are actionable and scoped to the failed operation.

- A) Existing operation-scoped messages only
- B) Add a correlation/run id to every log line in one reconciliation run

**Recommendation:** B. Multi-row, multi-group runs are otherwise impossible to reconstruct.

### AR-11 — Which accent color does the module use?

**Constraint:** R-HDR-04 fixes the mechanism (brush triple, `GetModuleAccent` mapping, no local hex).
Only the color value is open.

**Recommendation:** pick an unused brand-consistent color and add the full brush triple. Do not reuse
`ReceivingAccentBrush` (`#0B6157`), which would make the module read as part of Receiving.

**Proposed value:** `#00838F` (cyan 800) — unused by any existing module accent, distinguishable from
Receiving `#0B6157`, Volvo `#1E4F7A`, and Reporting `#6A1B9A`, and readable with white title text in both
themes. Confirm or substitute before the theme file is edited.

---

## 5. UI And UX (UI)

### UI-01 — Launcher surface

**Decision (2026-11-12):** a new launcher / mode-select page hosted inside `Module_Reporting`, matching
the existing mode-select pages. Copy the established style and feature set rather than inventing a new
pattern.

- The launcher is the nav destination for the Reporting-group entry (`SC-05` = C, labelled per `SC-09`).
- It reuses the same card/tile layout, `AutomationProperties.Name` coverage, and keyboard behavior as
  `View_Receiving_ModeSelection.xaml` and the `Module_Reprint` landing page.
- Selecting the validation mode opens the working page (`UI-02`, `UI-04`).

**One thing to confirm before the spec is written:** "inside `Module_Reporting`" can mean two different
things, and they differ in coupling cost.

- **A) Nav entry in the Reporting group, page owned by `Module_Reconciliation`.** `MainWindow` adds the nav
  tag under the Reporting section; the launcher view and viewmodel live in `Module_Reconciliation`. No new
  project reference. This is the only shape that keeps R-MOD-03 and R-MOD-07 intact.
- **B) Launcher files physically inside the `Module_Reporting` project.** Then `Module_Reporting` must
  reference `Module_Reconciliation` (or duplicate the page) just to render navigation, which is the same
  reach-through pattern the separation rule forbids.

**Recommendation:** A. The user-visible result is identical — the entry appears in the Reporting group —
but the dependency direction stays one-way.

### UI-02 — Date control: one day at a time

Today: a single boolean. `Review.md` requires a user-supplied date scope.

**Decision (2026-11-12):** the unit of work is **one calendar day**, cycled one day at a time. The module
never reconciles a span in a single pass.

- Primary controls: a date picker, `‹ Previous day`, `Next day ›`, and `Today`.
- Each run covers exactly the selected day, matched on receipt date (`DS-01`).
- A span is still possible as a **sweep**: the user sets a start and end date and the module repeats the
  single-day pass one day at a time, showing per-day progress and allowing a stop between days.
- The sweep reuses the identical single-day code path. There is no separate multi-day algorithm.

Why this is the right shape:

- The allocation group key already carries the receipt date (`LG-01`), so a multi-day pass only ever
  concatenates independent per-day groups.
- Every Infor Visual query is already anchored on one receipt date with a ±2 day discovery window
  (`LG-10`), so a wide range multiplies round-trips without changing a single result.
- One day is small enough that no long-running execution machinery is needed (`AR-08`).

**Decided sub-questions (2026-11-12):**

- Opening day: **Today**.
- Stepper: **skips days with no Receiving App activity** — it advances to the next or previous day that has
  receiving rows, so the user never lands on an empty screen.
- Sweep: **stops on the first day that errors** and leaves the user on that day so the problem is visible.
- Completed days: **carry a visible indicator**, so the user knows where they stopped.

Consequences of "skip empty days":

- The stepper needs a fast path back to Today, or skipping makes it easy to lose position.
- The module must know which days have activity without running a full reconciliation. That needs a cheap
  per-day activity query (a day index), not day-by-day probing from the UI.
- "No activity" means no Receiving App rows for that day, not no Infor Visual transfers.

### UI-03 — Preview, then a per-line review cycle, then apply

**Decision (2026-11-12):** preview → review → apply (option A). The preview never writes; the review step
is a **per-line cycle**; applying writes each accepted line as it is confirmed.

- Only lines whose saved location does not match the Infor Visual-derived location enter the cycle.
- Each mismatched line is presented in turn and asked "where did this go?".
- The destination control is **pre-set to the recommended location**, so the common case is one accept.
- The user can change the destination before accepting, or skip the line.
- Accepting writes that line and advances to the next.
- There is no end-of-run bulk apply; each line is confirmed on its own.

Why this shape:

- Each line is a physical-material judgement, so one "apply all" invites blind approval.
- Pre-setting the recommendation keeps the fast path fast: a run of correct recommendations costs one
  action each.
- The preview still tells the user what the day looks like before they start answering.

Remaining sub-questions:

- Destination control: free text, editable combo seeded with known Visual locations, or search-and-pick?
- Is "skip" permanent for the run, or does it resurface in the end-of-run summary?
- Can the user go back to a previously accepted line, and does that re-open it or create a second change?
- Keyboard: does Enter accept the pre-set recommendation and Escape skip?
- Where do lines with no recommendation go? (`UI-07` — they cannot be pre-filled.)

**Recommendation:** editable combo seeded with known Visual locations plus free-text fallback; skip is
per-run and resurfaces in the summary; once accepted a line leaves the cycle (re-opening a saved line is an
audit/undo concern, not a review concern); Enter accepts, Escape skips.

### UI-04 — Review screen shape: grid plus detail pane

**Constraint:** R-MVVM-07 — larger workflows are pages, not dialogs.

**Decision (2026-11-12):** grid with row selection plus a detail pane (option B). The grid lists the day's
mismatched lines; the detail pane hosts the per-line prompt from `UI-03`.

- The grid is the map of the day's work: which lines exist, and how many are done.
- The detail pane is where the user answers "where did this go?" for the selected line.
- Accepting advances the grid selection to the next unresolved line, which is what makes it feel like a
  cycle rather than a table edit.

- A) Grid plus detail pane (chosen)
- B) Modal window per line
- C) Single-line card with no grid

**Recommendation:** A. It preserves spatial context — the user can see there are 12 lines and 3 remain —
while still giving each line focused attention. B puts a window between the user and the page for every row;
C loses the day-at-a-glance picture.

### UI-05 — Which columns?

**Decision (2026-11-12):** the grid shows three columns — **Part**, **PO #**, and **Quantity**.

Everything else lives in the detail pane: description, PO line, load/skid number, UOM, saved location,
recommended location, resolution, Visual user, transfer time, transfer count, evidence source locations,
and the reason text.

Remaining sub-questions:

- Is row state conveyed by row colour (the `Module_Reprint` already-queued pattern), or by a fourth status
  column?
- Does quantity show its UOM inline, or only in the detail pane?

**Recommendation:** row colour for state, no fourth column; show UOM inline only when it is not the
default unit.

### UI-06 — Filtering and sorting
Applies to the lists, not to the cycle.

- Filter by resolution, part, PO, user, source?
- Default sort?
- Group by part + PO?

**Decided (2026-11-12):** filter by resolution and by part/PO text search; group by part + received date.

The group is deliberately the same key the algorithm uses (`LG-01`), so "why are these rows tied together"
is answered by the layout itself rather than by reading the reason text.

### UI-07 — Lines without a recommendation go to a Needs Attention modal

**Decided (2026-11-12):** option A, surfaced as a **modal window** that is only enabled when unresolved
items exist.

- `Ambiguous`, `NotFound`, `IgnoredBySettings`, `Skipped`, and `Error` lines cannot be pre-filled, so they
  never enter the review cycle (`UI-03`).
- The entry point is a Needs Attention action on the page; it is disabled when the count is zero, so it
  never opens an empty list.
- Opening it shows the unresolved lines for the selected day with their reason and evidence.

**Constraint check (R-MVVM-07):** a modal **window** is the right vehicle for a bounded review list; a
`ContentDialog` is not, because this list is multi-row and may need scrolling and selection. Implement it as
a `Window` with `ModalWindow` behavior, matching `View_Receiving_LabelOverlayWindow`, not as a
`ContentDialog`.

Remaining sub-question: from inside the modal, can the user resolve a line (manual location) or is it
read-only triage that deep-links back to the grid?

**Recommendation:** resolve from inside the modal, writing through the same accepted-line path as the
cycle, with `ManuallySet` as the resolution so audit can distinguish typed values from evidenced ones.

### UI-08 — Progress, cancellation, keyboard
- Cycle progress: "Line N of M" plus accepted and skipped counts?
- Can the user leave and resume the cycle on the same line?
- Day-run progress, and a stop control for a sweep (`AR-08`)?
- Re-run the same day without leaving the page?
- Keyboard: Enter accepts, Escape skips, and a shortcut jumps to the next line?

**Decided (2026-11-12):** show "Line N of M" with accepted and skipped counts; the cycle is resumable within
the session; the day run has its own progress and stop control; Enter accepts, Escape skips (the
accessibility baseline itself is fixed by R-MVVM-05).

### UI-09 — Empty and failure states
Define text for: no rows in range, all rows unchanged, all rows skipped, Visual unreachable, MySQL
unreachable, range too wide. Page-level state or dialog?

**Decided (2026-11-12): use the recommendation.** Page-level empty state with a "step to another day"
action; Visual failure as an inline retry banner rather than the current modal that drops the user back to
Mode Selection.

Required state texts (from the recommendation): no rows for the day, all lines already match, all lines
skipped, Visual unreachable, MySQL unreachable, and the residual-check mismatch from `LG-04` Part 2.

### UI-10 — Summary content (no export)

**Decided (2026-11-12):** use the recommendation, **minus the export action.** No CSV/XLSX export from this
module.

The summary shows: scanned, updated, unchanged, unresolved, skipped, and error counts, run duration, the
day reconciled, Visual user attribution, and a copyable run id (`AR-10`).

**Knock-on:** the quality-of-life `E4` export idea is out of scope for this module; see
[Review-Quality-Of-Life-Suggestions.md](Review-Quality-Of-Life-Suggestions.md). If supervisors need to
triage outside the app later, that becomes its own request rather than a hidden dependency of this spec.

### UI-13 — Label-print findings are **not** part of this module

**Decided (2026-11-12): No.** The module does not surface label-print integrity findings (unprinted
labels, duplicate labels, reprint abuse).

**Knock-on, and it is significant:** family A of
[Review-Quality-Of-Life-Suggestions.md](Review-Quality-Of-Life-Suggestions.md) (label presence and print
integrity, `A1`–`A6`) is out of scope for `Module_Reconciliation`. Those detectors belong with
`Module_Reprint`, which already owns the label queue and reprint path, or with a separate audit module.

The module keeps the location-and-quantity families: row data quality (`C`), Infor Visual cross-checks
(`B`), location sanity (`D`), audit and disposition (`E`), workflow (`F`), automation (`G`), and analytics
(`H`).

---

## 6. Reconciliation Logic (LG)

`LG-08`, `LG-09`, and `LG-17` are retired — see [Section 2](#2-retired-questions).

### LG-01 — What is the grouping key? **BLOCKER**
Current: `(PartID, ReceivedDate.Date)`. `Review.md`: `PO + Part`.

- A) `(PartID, ReceivedDate)` — current
- B) `(PO, PartID)` — `Review.md` literal
- C) `(PO, PartID, ReceivedDate)`
- D) `(PO, PartID, POLine, ReceivedDate)`

**Why it matters:** the group key decides which rows compete for the same transfer quantity. Under C, a
part received on two days from one PO is two groups, which is what the ±2 day window and the
receipt-date-scoped strict check already assume.

**Decided (2026-11-12): C — `(PO, PartID, ReceivedDate)`.** PO isolation as `Review.md` asks, with the
receipt-date scoping the ±2 day discovery window and the date-scoped bucket check already depend on.

Consequence to carry into the spec: two receipts of the same PO + part on the *same* day stay one group
(they do today), and a part received on two days from one PO becomes two independent groups.

### LG-02 — Is there a "receiver record" to query? **BLOCKER**
`Review.md` §5.1 wants the receiver record/transaction. No `RECEIVER` / `RECEIVER_LINE` query exists;
today the MTM saved row *is* the receiver proxy.

**Constraint:** R-DATA-05 — any new Visual query must be researched against the CSV schema reference
first.

- A) MTM rows are the receiver proxy; no Visual receiver query is added
- B) Add a Visual `RECEIVER`/`RECEIVER_LINE` query and reconcile against true ERP receipts
- C) Both: MTM rows drive allocation, ERP receiver quantity is a cross-check

**Decided (2026-11-12): B — add a real Infor Visual `RECEIVER` / `RECEIVER_LINE` query and reconcile
against true ERP receipts.**

This is new data-source work, not a move. Before the spec marks it in scope, the following must be
confirmed against the CSV schema reference (R-DATA-05) and the query-authoring playbook:

- Which `RECEIVER` / `RECEIVER_LINE` columns carry the received quantity, date, PO, PO line, and part.
- Whether `RECEIVER_LINE.RECEIVED_QTY` is already cumulative or per-receipt.
- Whether the receiver number is the "transaction number 594488" referenced in `Review.md`'s example.
- Whether a Visual receiver can be joined to a MTM row by anything better than `(PO, Part, Line, Date)`.

**Consequence for `LG-02` = B:** the module now has a true received-quantity authority, which makes
`LG-04`'s source-residual check meaningful, and unblocks the missing-label and over-receipt detectors in
the quality-of-life document (`A1`, `B3`). Per R-SCOPE-01 this lands as its own reviewed change, not inside
the module move.

### LG-03 — Is a current-inventory comparison required?
`Review.md` §5.2 wants a "current Visual locations" comparison that skips already-correct rows. The
preview path does not do this today.

- A) No — transfer evidence is authoritative (current)
- B) Yes — a first-pass inventory check that short-circuits to `Unchanged`
- C) Yes, but only as a reportable warning

**Decided (2026-11-12): B — a first-pass current-inventory check that short-circuits already-correct lines
to `Unchanged`.**

Implementation notes from the ledger model in
`.github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md`:

- `CR_PART_LOCATION` is the on-hand authority per `(part, warehouse, location)`.
- The ledger is always internally consistent, so a location mismatch is physical-activity, not a lost row.
- This check is also what makes `LG-04`'s source-residual test possible: the leftover in the original
  receiving location is compared against `CR_PART_LOCATION`, not against zero.

**Still to define:** when current inventory and the transfer-derived destination disagree for the same
line, does the discrepancy block the update (leave `Ambiguous`) or allow the transfer-derived value to
win? See `LG-04` for the proposed answer.

### LG-04 — Quantity gate: every destination bucket must reach zero, and the source residual must match live on-hand

**Decision (2026-11-12):** validation is a **two-part** test. It is neither "all transfers must equal the
receipt" nor a receiver-level equality.

**Part 1 — every destination bucket must net to zero.** For each location holding net inbound quantity, the
rows allocated to it must consume that quantity exactly. Bucket remainder must be `0`. A non-zero remainder
means the transfer evidence cannot be fully attributed to physical rows, so the affected rows are
`Ambiguous`.

**Part 2 — the source residual must match live stock.** Whatever remains in the original receiving location
is **legitimate leftover**, not a reconciliation failure. It must equal `CR_PART_LOCATION` on-hand for that
part at that location (`LG-03` = B provides the check).

#### Worked example (from the decision)

| Step | Quantity |
|---|---|
| Received into `RECV` | 50,000 |
| Moved to `SHIP` | 5,000 |
| Moved to `V-C0-01` | 2,500 |
| Moved to `R-04` | 500 |
| Moved to `R-05` | 1,000 |
| Total transferred out of `RECV` | 9,000 |
| **Residual still in `RECV`** | **41,000** |

What the module must produce:

- Destination buckets `SHIP` 5,000, `V-C0-01` 2,500, `R-04` 500, `R-05` 1,000. Each must be driven to `0`
  by the rows allocated to it, or those rows are `Ambiguous`.
- Rows totalling 41,000 keep `RECV` and are reported `Unchanged`, because `RECV` genuinely still holds that
  stock. They are **not** `Ambiguous`.
- `41,000` must equal `CR_PART_LOCATION` at `RECV`; if it does not, the residual line is flagged and the
  discrepancy is reported rather than written.

This deliberately corrects my earlier framing. Requiring the whole receipt to be consumed by transfers
would mark 41,000 of perfectly correct stock as unresolved, which is the single most common real shape: a
receipt partially transferred the same day.

#### Why two parts and not one receiver-level total

A receiver-level equality (`sum(all transfers) == received`) fails the moment any material stays in the
receiving location — which is the normal case, as the example shows. Bucket-level zeroing plus a live
source-residual check expresses both facts the ledger actually records: material that moved is fully
attributed, and material that did not move is still on hand where it was received.

#### Interaction with the ledger model

Per
`.github/instructions/database/infor-visual-inventory-discrepancy-investigation.instructions.md`:

- `INVENTORY_TRANS.QTY` is always positive; direction lives in `TYPE` (`I` in, `O` out).
- Location transfers are matched `I`/`O` pairs via `TRANSFER_TRANS_ID` and net to zero overall, so a
  partially transferred receipt leaves a real, visible residual at the source.
- Chained moves (`RECV → V-STAGE → V-Z9-01`) still resolve to the final net destination, so an intermediate
  staging bucket that fully forwards on nets to `0` and drops out — consistent with Part 1.
- A destination that ran negative indicates consumption ahead of transfer; those lines must be `Ambiguous`,
  never auto-corrected.

**Analytic SQL hook:** the residual and negative checks map to Step 5 and Step 1 of the playbook
(`INVENTORY_TRANS` running balance per location, and `CR_PART_LOCATION` per location).

### LG-05 — Which quantity field represents a row?
`receiving_label_data` has `quantity` (int, rounded) and `weight_quantity` (decimal 18,2). Current
allocation uses `WeightQuantity`.

- A) `WeightQuantity` (current)
- B) `Quantity`
- C) `WeightQuantity`, falling back to `Quantity` when null

**Decided (2026-11-12): B — use `quantity`, the integer column.**

This is the rounded value derived from weight/quantity at save time. Two things to carry into the spec:

- `quantity` is an `INT`, so the quantity gate in `LG-04` compares integers. That is compatible with
  `LG-07` = exact equality, but it means any sub-unit remainder is already gone by the time the module
  sees the row.
- `weight_quantity` (`DECIMAL(18,2)`) is still needed for display and for the missing-label and
  over-receipt detectors in the quality-of-life document, so it stays in the read model even though the
  gate does not use it.

**Risk to watch:** for weight-based parts (`LB`), the label shows a rounded integer while Visual transfers
carry decimals. Exact integer equality will report more `Ambiguous` lines than a decimal comparison would.
`LG-06` = ignore UOM makes this more likely, not less. Confirm after the first live run whether the
`Ambiguous` rate is acceptable.

### LG-06 — Unit-of-measure handling
Should reconciliation refuse to allocate rows whose UOM differs from the transfer evidence, and should a
mixed-UOM group be blocked outright?

**Decided (2026-11-12): unit of measure is ignored in this module.** No UOM blocking, no mixed-UOM guard.

**Consequence, stated plainly so it is a choice and not an accident:** the module will compare an `LB` row
against an `EA` transfer without complaint if that situation arises in the data. Given `LG-05` = integer
`quantity` and `LG-07` = exact equality, a UOM mismatch becomes a silent `Ambiguous` at best and a wrong
location at worst.

Two low-cost mitigations worth keeping in the spec even though UOM is not gated:

- Include UOM in the detail pane so a reviewer can see it when a line looks wrong.
- Log a warning when a reconciliation group contains more than one UOM value, so the condition is visible
  in logs without changing behaviour.

### LG-07 — Exact equality or tolerance?
- A) Exact `decimal` equality (current)
- B) Absolute tolerance (for example ±0.01) for weight rounding
- C) Percentage tolerance

**Decided (2026-11-12): A — exact equality.** No tolerance, no epsilon setting.

Consistent with `LG-04` Part 1 (buckets must net to exactly `0`) and simple to reason about. Accepts the
`Ambiguous` rate that rounded integer quantities (`LG-05` = B) will produce on weight-based parts.

### LG-10 — Date windows
Three windows exist: discovery (receipt date ±2 days), strict validation (receipt date and +1), and row
selection (`2000-01-01` or today → today + 1).

- Row selection: the selected day only, matched on receipt date (`UI-02`, `DS-01`).
- Discovery: unchanged at receipt date ±2 days, because transfers often land the next morning.
- Strict validation: unchanged at receipt date and receipt date + 1.

- Does discovery stay ±2 days?
- Does the strict window stay at +1 day?
- Should both windows become settings keys?

**Decided (2026-11-12):** use the recommendation. Row selection is the selected day only; discovery stays
at receipt date ±2 days; strict validation stays at receipt date and receipt date + 1; both windows become
settings keys. A sweep does not widen them — each day still validates against its own receipt-anchored
windows, which is why one-day-at-a-time is safe.

### LG-11 — Reprint rows (`is_reprint = 1`)
A reprint re-queues a history row into `receiving_label_data` with the same part, PO, quantity, and
location. Including it risks counting one physical skid twice.

- A) Include like any other row
- B) Exclude, but list as informational
- C) Include in a separate group so it cannot consume the original's transfer quantity

**Decided (2026-11-12): exclude reprint rows from label data.** `is_reprint = 1` rows are filtered out of the
candidate set entirely.

A reprint is a label document, not a new physical receipt, so including it would let one skid consume
transfer quantity twice. Because they are excluded rather than reported (`LG-15` = A, logged only), the
exclusion must be logged so the count is recoverable from logs.

### LG-12 — Idempotency and re-runs
Running twice over an overlapping range must not produce a second update.

- A) Already-current rows resolve to `Unchanged` (current)
- B) Also detect rows changed by a previous reconciliation run and skip with a distinct resolution

**Decided (2026-11-12):** use the recommendation — A plus B (`AlreadyReconciled`).

**But `DS-07` = A (logs only, no tables) and `AR-07` (no schema change) make B impossible.** See
[Section 13](#13-conflicts-introduced-by-these-answers). Until an audit table exists, the module can only
implement A: already-current rows report `Unchanged`. `AlreadyReconciled` is specified but deferred with
the `reconciliation_*` tables.

### LG-13 — Non-PO rows are reconciled by a separate service

**Decided (2026-11-12):** non-PO rows are **not skipped**. A **separate service** reconciles them.

This is new scope and needs its own answers before the spec is written. Non-PO rows have no PO to match on,
so the existing transfer-evidence grouping key (`LG-01`) does not apply to them at all.

Open questions for that service:

- What identifies a non-PO receipt for grouping — part + received date + vendor (`vendor_name`), or
  something else?
- Is the destination derived from transfer movements the same way, or from a different source?
- Does it run in the same day pass as PO reconciliation, or is it a separate mode on the launcher?
- What is the resolution name when a non-PO row cannot be matched?

**Recommended shape (to confirm):** `IService_Reconciliation_NonPo` in the new module, same day scope, same
transfer-evidence mechanism, grouped by part + received date + vendor, producing `Ambiguous` /
`NotFound` like the PO path. It must still satisfy `LG-04`'s two-part gate.

### LG-14 — Ignored locations are not used in this module

**Decided (2026-11-12):** the module does **not** consult or apply an ignored-locations list. Every
destination location is eligible.

**Cascades, all of which must be applied together:**

- The `IgnoredBySettings` resolution no longer exists for this module.
- `TS-05` drops the ignored-destination test case.
- `DS-08` becomes not applicable.
- `NF-04` (settings caching) becomes not applicable.
- `AR-04` shrinks: only the discovery/strict window settings remain, and
  `Receiving.UserPreferences.IgnoredReconciliationLocationsJson` is **not** migrated.
- `R-SCOPE-02` in [Review-Rules.md](Review-Rules.md) lists `IgnoredBySettings` as a preserved invariant.
  It is now an **approved removal** rather than a silent regression, and that rule has been amended
  accordingly.

**Consequence to accept:** staging and holding locations will now appear as legitimate destinations. Where
the old behaviour hid them, users will see them. That is the intent of `D2` in the quality-of-life
document, but it may raise the visible line count on day one.

### LG-15 — `NotFound` semantics
Current: no qualifying transfers → treated as "saved location is probably correct" and **not** surfaced.

- A) Silent (current)
- B) Reported but not actionable
- C) Flagged as needs-review

**Decided (2026-11-12): A, but logged.** `NotFound` is not surfaced in the UI; it is written to the log with
part, day, and group so the condition is recoverable without cluttering the review.

Because it is invisible, the run summary should still carry a `NotFound` **count** so a zero-result day is
distinguishable from a day where nothing was found for every line.

### LG-16 — Over-transfer and under-transfer
- Under: current → `Ambiguous`
- Over: current → allocation succeeds if buckets cover rows; surplus is unallocated

**Decided (2026-11-12):** two named resolutions replace the single `Ambiguous` bucket.

| Condition | Resolution | Meaning |
|---|---|---|
| Transferred quantity exceeds what the rows can consume | `ExtraSurplus` ("Extra Surplus") | More moved than the receipt accounts for — usually the movement window caught a neighbouring receipt |
| Transferred quantity is less than the rows require | `NotEnough` ("Not Enough") | Some material did not move, or moved outside the discovery window |

Both still resolve to a non-writable, needs-attention outcome, exactly like `Ambiguous` did, but the name
tells the reviewer which way the numbers diverged. Keep the raw difference in the reason text so the
direction and magnitude are both visible.

### LG-18 — What does "assy" mean in the source notes?
Current analog: the sum of transfer quantities for the group.

**Recommendation:** confirm the intended meaning is "total quantity transferred/consumed". If it means
assembly quantity from a work-order context, that is a different Visual table (`WIP_ISSUE_DETAIL`) and a
materially larger feature. **This answer can change the data source.**

---

## 7. Data Sources And Schema (DS)

`DS-05` is retired — see [Section 2](#2-retired-questions).

### DS-01 — Which date column drives row selection?
Read surface: `GetCurrentLabelDataAsync()` → `sp_Receiving_LabelData_GetAll`, and
`GetAllReceivingLoadsAsync(start, end)` → `Dao_ReceivingLoad.GetAllAsync`.

- Is the selected day inclusive on both bounds? (Today the history call uses `Today` → `Today + 1`.)
- Does history filter on `received_date`, `transaction_date`, or `created_at`? `Review.md` §9.8 asks.
- Should the new module reuse these methods or get new SPs with an explicit date parameter?

**Constraint:** R-DATA-03 / R-SQL-04 — any new SP follows SP-only writes and the module's existing naming.

**Decided (2026-11-12):** investigate both databases and choose the best column pair, rather than assuming
`received_date`.

The investigation must answer, read-only:

1. **MySQL `receiving_label_data` / `receiving_history`** — which date columns exist, how they differ, and
   which is populated for every row: `received_date` (DATETIME), `transaction_date` (DATE), `created_at`
   (TIMESTAMP). Confirm which one matches what the operator perceives as "the day I received this".
2. **Visual `RECEIVER` / `RECEIVER_LINE`** (`LG-02` = B) — which date column represents the receipt event.
3. **Visual `INVENTORY_TRANS`** — `TRANSACTION_DATE` semantics for receipts versus transfers.
4. Whether a MySQL day and a Visual day ever disagree for the same receipt, and which wins.

Deliverable: a short decision note in the spec naming one column per side, with the evidence.

**Credentials rule:** connection details are supplied interactively for the investigation only. They are
never written to a repo file, a script, a log, or a commit (R-SQL-06). The Visual database stays read-only
(R-DATA-04).

### DS-02 — Visual query inventory
Files: `16` (evidence), `17` (transaction history), `18` (transfer movements), `18b` (pair details).
`18a_GetTransferCandidatePairs.sql` is referenced in `docs/prompts/reconciliation-location-fix.md` but is
absent — the candidate-pair CTE lives inside query `18`.

- Complete the 18a/18b split as part of this refactor?
- Are `16` and `17` still used after the split? (`16` backs the recommendation path; `17` appears
  unused — verify.)

**Recommendation:** complete the 18a/18b split (documented as intended and makes the candidate step
testable in SSMS); delete query `17` only after confirming zero callers.

### DS-03 — RECEIVER usage

**Decided (2026-11-12): No.**

> **This directly contradicts `LG-02` = B**, which was answered "add a real Visual `RECEIVER` /
> `RECEIVER_LINE` query". See [Section 13](#13-conflicts-introduced-by-these-answers). One of the two must
> change before the spec is written.

### DS-04 — Performance guardrails on Visual
`INVENTORY_TRANS` is roughly 3.5M rows.

- Per-part loop, or set-based multi-part query for a month-long range?
- Maximum acceptable query time before the module degrades?

**Recommendation:** keep per-part set-based calls (already the shape) and bound each day's row count
(`NF-01`), plus `TOP` protection on the candidate step per R-SQL-02.

### DS-06 — Write authorization model **BLOCKER**
`UpdateReceivingLoadsAsync` and `UpdateCurrentLabelDataAsync` require an active session **and** reject
rows owned by another employee or Windows user. A reconciliation tool fixing history rows created by
other operators throws on the first foreign row.

**Decided (2026-11-12):** this module supersedes the Edit Mode ownership blocker and always makes
corrections so Receiving matches Visual.

What that means concretely:

- The module writes corrected locations regardless of which employee or Windows user created the row.
  The `GetUnauthorizedLoads` guard in `IService_MySQL_Receiving` is bypassed by using the new
  module's own write procedure (`MG-05`), not by weakening the existing Receiving method.
- Receiving's Edit Mode keeps its ownership enforcement untouched.

**Two things that must be settled with this answer:**

1. **Role gate.** "Supersedes the blocker" removes the *ownership* constraint, not the *permission*
   constraint. Decide whether any authenticated user can correct other users' rows or whether it needs a
   supervisor role. `R-DI-03` requires the schema/SP change to be approved either way.
2. **Interaction with the review flow.** "Always makes corrections" must not mean "bypasses the per-line
   accept". `UI-03` = A keeps per-line confirmation, and `LG-04` / `LG-16` still block writes on
   `ExtraSurplus` / `NotEnough` / `Ambiguous` lines. Read together: the module always *proposes* the
   correction for every mismatched line and always *may* write it, but only after the user accepts that
   line and only when the bucket gate passed.

### DS-07 — Do reconciliation runs and changes get persisted?
- A) No persistence; logs only
- B) New run table (user, start/end, range, counts, duration)
- C) B plus a per-row change audit (old location, new location, resolution at write time, evidence ids,
  Visual user, timestamp)

**Recommendation:** C. Without C, "who changed this location and why" is unanswerable, `LG-12` is
impossible, and the quality-of-life disposition features are blocked.

**Sequencing per AR-07:** the refactor answers **A** (logs only, no tables). **C** lands with the separate
approved change that creates `reconciliation_run`, `reconciliation_change`, and `reconciliation_finding`.
Until then, `LG-12`'s `AlreadyReconciled` resolution is unavailable and every run rediscovers everything.

### DS-08 — Ignored-location storage
Stored as a JSON array string under a settings key.

- Keep JSON, or normalize into a table?

**Recommendation:** keep JSON for this refactor; normalize only if per-location metadata (who, when, why)
is required.

---

## 8. Testing (TS)

`TS-01` and `TS-08` are retired — see [Section 2](#2-retired-questions).

### TS-02 — Which existing tests move?
- `Service_ReceivingLocationReconciliationTests.cs` — the `GetRecommendedLocationsAsync` test stays in
  Receiving; all allocation tests move.
- `ViewModel_Receiving_LocationReconciliationReviewTests.cs` — moves.
- `ViewModel_Receiving_LoadEntryTests.cs` — stays, but its mock target changes to the split interface.

**Recommendation:** as above; split the service test file rather than copying it.

### TS-03 — Is there a Visual database test harness?
Current integration tests target MySQL stored procedures; Visual SQL is validated manually.

- A) Visual queries stay manually validated and are covered by unit tests over fakes
- B) Create a read-only Visual integration harness

**Recommendation:** A. Do not add a live ERP dependency to the test suite.

### TS-04 — Which fixtures are golden cases?
`MMC0000650` (five skids, 12000/5000/6350 split) and the acceptance example in
`docs/features/receiving/location-reconciliation-process.md`.

**Recommendation:** keep both as named regression fixtures so a future reader can trace the numbers back
to the business example.

### TS-05 — Required coverage from `Review.md` section 7.10

| `Review.md` requirement | Test |
|---|---|
| Matching locations | `..._ShouldReturnUnchanged_WhenProposedEqualsSaved` |
| Mismatched locations | `..._ShouldProposeDestination_WhenSavedDiffersFromTransfer` |
| Quantity equality | `..._ShouldUpdate_WhenBucketTotalEqualsAllocation` |
| Quantity mismatch | `..._ShouldReturnAmbiguous_WhenBucketTotalDiffers` |
| Multiple destinations | `..._ShouldAllocateAcrossBuckets_WhenReceiptSplits` |
| Date-range boundaries | `..._ShouldExcludeRow_WhenReceiptOutsideRange` |
| Multiple receipts same part | `..._ShouldIsolateGroups_WhenSamePartHasTwoReceiptDates` |

**Recommendation:** add reprint exclusion (`LG-11`), ignored destination (`LG-14`), cross-pass merge
(`LG-12`), UOM conflict (`LG-06`), ownership rejection (`DS-06`), and cancellation (`AR-08`).

### TS-06 — Enum and navigation regression
If `Enum_ReceivingWorkflowStep.ReconciliationReview` is deleted, confirm nothing persists the numeric
value (session restore, saved workflow state).

**Recommendation:** grep for numeric persistence before deleting; if found, keep the member reserved with
a comment.

### TS-07 — ViewModel tests
Should the new module's viewmodel have unit tests (date-range validation, filtering, selection, apply
guard)?

**Recommendation:** yes — model state, not XAML.

---

## 9. Migration And Cleanup (MG)

**Binding direction:** `Module_Reconciliation` and `Module_Receiving` do not talk to each other. No project
reference, no `using`, no injected service, no shared model, enum, constant, DAO, stored procedure, or
settings key. Each module owns its own central code, tables, stored procedures, and UI. See R-MOD-07 and
R-MOD-08 in [Review-Rules.md](Review-Rules.md).

What may still be shared is the repository foundation that every module already uses: `Module_Core`
(`Model_Dao_Result`, logging, error handler, settings core) and `Module_Shared` (`ViewModel_Shared_Base`
and shared presentation assets).

### MG-01 — Artifact disposition table

Confirm each row (`MOVE` / `DELETE` / `EDIT` / `STAYS`). Nothing is co-owned: an artifact either belongs
to the new module, is deleted with the old feature, or is edited to strip the feature out of Receiving.

| Artifact | Proposed | Notes |
|---|---|---|
| `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` | SPLIT, then DELETE reconcile half | `GetRecommendedLocationsAsync` stays in Receiving; the allocation and preview code is deleted once the new module lands |
| `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs` | SPLIT | Only the recommendation contract survives in Receiving; reconcile methods are deleted there |
| `Module_Receiving/Models/Model_ReceivingLocationReconciliationItem.cs` | DELETE | Replaced by new-module models (`MG-06`) |
| `Module_Receiving/Models/Model_ReceivingLocationReconciliationSummary.cs` | DELETE | Replaced by new-module models (`MG-06`) |
| `Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReview.cs` | DELETE | Replaced by new viewmodels (`MG-06`) |
| `Module_Receiving/Views/View_Receiving_Dialog_LocationReconciliationReview.xaml(.cs)` | DELETE | Replaced by new views (`MG-06`) |
| `Module_Receiving/Views/View_Receiving_Workflow.xaml(.cs)` | EDIT | Remove `ReconciliationReviewHost` |
| `Module_Receiving/Views/View_Receiving_ModeSelection.xaml` | EDIT | Remove the reconcile card |
| `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` | EDIT | Remove `ReconcileSavedLocationsCommand` |
| `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs` | EDIT | `SC-04` / `TS-06` |
| `Module_Receiving/Settings/ReceivingSettingsKeys.cs` (2 keys) | DELETE after migration | Values copied into the new key namespace by migration (`AR-04`) |
| `Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml(.cs)` | DELETE | Rebuilt in `Module_Settings.Reconciliation` (`AR-05`) |
| `Database/InforVisualScripts/Queries/18*.sql` | MOVE | Relocate under the new module's query folder (`AR-09`) |
| `Database/InforVisualScripts/Queries/16*.sql` | STAYS | Receiving-owned; backs entry-time recommendation; the new module never reads it |
| `Database/InforVisualScripts/Queries/17*.sql` | DELETE if unused | Verify zero callers |
| Receiving write path (`sp_Receiving_LabelData_Update`, `Dao_ReceivingLoad.UpdateLoadsAsync`) | STAYS untouched | Receiving keeps these for Edit Mode; the new module gets its own SPs (`MG-05`) |
| `...Tests/.../Service_ReceivingLocationReconciliationTests.cs` | SPLIT | Recommendation test stays; allocation tests are rewritten in the new module (`TS-02`) |
| `...Tests/.../ViewModel_Receiving_LocationReconciliationReviewTests.cs` | DELETE | Replaced by new viewmodel tests |
| `docs/features/receiving/location-reconciliation-process.md` | MOVE | See `MG-02` |
| `docs/prompts/reconciliation-location-fix.md` | MOVE to module docs | |
| `Module_Receiving/Proposed_UI_Changes.xaml.md` / `.html` | VERIFY, then DELETE or MOVE | Determine whether reconcile-specific |
| `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` | EDIT | Add the module's registration call per R-DI-01 |
| `MainWindow.xaml` / `MainWindow.xaml.cs` | EDIT | Nav item, route, search terms, accent mapping |

### MG-02 — Move-and-preserve, or rewrite the process doc?
The existing doc describes the *implemented* algorithm, which is richer than `Review.md`. A naive rewrite
loses the chained-move and partial-move rules.

**Constraint:** R-SCOPE-02 and R-DOC-02 — documented invariants must be preserved and docs kept current.

- A) Move the file and add a "selected day" section
- B) Rewrite it from `Review.md`

**Recommendation:** A. Never let the spec restate the algorithm from `Review.md` alone.

### MG-03 — What happens to `.serena/memories`?
Which memory files count as reconcile-specific, and should they be rewritten instead of deleted?

**Recommendation:** rewrite in place where the memory is broader than reconciliation (for example the
Infor Visual discrepancy playbook); delete only memories that name the Receiving reconciliation path
explicitly.

### MG-04 — Removal ordering
Should the new module land before the old UI is removed?

**Constraint:** R-SCOPE-03 — yes.

**Recommendation:** land the module, then remove the old surface in the same PR as the last step, keeping
the solution building between steps.

### MG-05 — Data boundary: shared storage rows, or separate tables? **BLOCKER**

The separation rule forbids module-to-module **code** coupling. It does not by itself decide whether the
new module reads and writes the same physical rows Receiving owns.

- A) Fully separate tables. The module ingests its own copy of receiving data and writes results to its
  own tables. Nothing flows back to Receiving.
- B) Own SPs against the shared receiving tables. The module has its own DAO and its own stored
  procedures, which read `receiving_label_data` / `receiving_history` and write the corrected location
  back. No reconciliation state is persisted.
- C) B plus reconciliation-owned state tables (`reconciliation_run`, `reconciliation_finding`,
  `reconciliation_change`).

**Why it matters:** `Review.md` §3 step 6 and §5.4 require the *Receiving application's saved locations* to
be corrected. Under A that cannot happen — the module produces a report, not a fix. Under B and C the fix
lands where users see it while the two modules still share zero code.

**Constraint:** R-DATA-03 (SP-only writes), R-DATA-04 (Visual read-only), R-DI-03 / R-CODE-06 (schema and
SP changes need approval), R-MOD-08 (shared storage is not shared code).

**Recommendation:** C. The new module owns its three state tables and its own SPs, and performs the
location write-back through exactly one reconciliation-owned procedure (for example
`sp_Reconciliation_ReceivingLocation_Update`). That procedure is the only place the new module touches
Receiving's rows.

**Interaction with AR-07:** `AR-07` is answered "refactor stays migration-free". Read together, the
refactor ships as **B** — own SPs, shared storage rows, no reconciliation state tables — and **C** arrives
with the separate approved change that adds `reconciliation_*`. Answer `DS-07` as **A** for the refactor
and **C** for that follow-up. New procedures are still required either way: R-DATA-03 and R-MOD-08 make
them mandatory, and "migration-free" means no new tables or columns, not no new procedures.

### MG-06 — New UI surface inventory

The new module gets **new** UI. It does not move, reuse, or sub-class Receiving's views and viewmodels.

| Element | Kind | Purpose |
|---|---|---|
| `View_Reconciliation_Launcher` | Page | Mode-select launcher hosted in `Module_Reporting`, copying the existing mode-select card style (`UI-01`) |
| `View_Reconciliation_Main` | Page | Day stepper (`‹ Previous day`, `Next day ›`, Today), run trigger, mismatched-line grid with detail pane (`UI-02`, `UI-04`) |
| `View_Reconciliation_Attention` | Page section | Needs Attention list for lines the module could not answer (`UI-07`) |
| `ViewModel_Reconciliation_Launcher` | ViewModel | Mode list and selection, navigation into the working page |
| `ViewModel_Reconciliation_Main` | ViewModel | Selected day, day activity index, grid rows, detail-pane state, cycle commands, settings load, run orchestration |
| `Converter_ReconciliationResolutionBrush` | Converter | Resolution to row background |
| `Converter_ReconciliationResolutionIcon` | Converter | Resolution to icon |
| Nav tag `ReconciliationPage` | Navigation | `MainWindow` route, nav item, search terms. Per `SC-05` = C the item sits in the Reporting group, labelled "Receiving Validator" (`SC-09`) |
| `IViewModel_HeaderTitleProvider` implementation | Header | Dynamic title, for example "Receiving Validator — 5/7" |
| Reconciliation accent brush triple | Theme | `ModuleAccentBrushes.xaml` + `GetModuleAccent` case (`AR-11`) |
| Settings page in `Module_Settings.Reconciliation` | Settings | Built through the settings core (`R-SET-01`, `R-SET-03`) |
| `ReconciliationSettingsKeys` | Settings | Own key namespace, no `Receiving.*` reuse |

**Open sub-question:** should the day stepper, review card, or attention list be extracted into
`Module_Shared` so a future third consumer can reuse them?

**Recommendation:** no. Build them in the new module now; extract only when a second real consumer exists
(R-MOD-06). `Module_Shared` is for genuinely cross-module presentation, not for one speculative reuse.

---

## 10. Non-Functional (NF)

`NF-03` is retired — see [Section 2](#2-retired-questions).

### NF-01 — Performance targets
Rows scanned per day, maximum runtime for one day, acceptable Infor Visual query latency, and the maximum
row count the grid stays responsive with.

**Recommendation:** state per-day numbers, not range numbers — for example ≤500 rows scanned per day,
≤15 s per day, virtualized grid above 200 rows. A sweep is measured as days × per-day budget, and the
per-day budget is what the Visual query has to meet.

### NF-02 — Error handling and retry
- Retry policy for Visual timeouts?
- Partial failure: fail the run, or mark that group `Error` and continue? (Current: per-group `Error`,
  run continues.)

**Constraint:** R-ERR-01 / R-ERR-02.

**Recommendation:** keep per-group isolation; add one bounded retry per Visual call and a run-level error
summary.

### NF-04 — Settings caching
`IgnoredReconciliationLocationsJson` is read once per run. If it becomes editable inside the module
(`UI-11`), confirm invalidation behavior.

### NF-05 — Privacy
Row data includes employee numbers and Windows usernames. Confirm which may be logged, whether the audit
trail (`DS-07`) may store them, and whether exports are restricted.

**Recommendation:** store employee number in audit, never log Windows usernames at Information level.

### NF-06 — Localized UI text
Are all new UI strings added through the settings text store (the `ReceivingSettingsKeys.UiText`
pattern) or hard-coded?

**Recommendation:** follow the existing `UiText` key pattern for anything user-visible.

---

## 11. Resolved Baseline

Decided items below are settled. Items still open point at their question.

1. Module named `Module_Reconciliation`; feature labelled "Receiving Validator" (`SC-01` A, `SC-09`).
2. Recommendation method stays in `Module_Receiving`; reconcile methods move (`SC-02` A).
3. New module owns its data access with slim row models (`SC-03` B).
4. Reconciliation leaves the Receiving workflow; the Mode Selection card is deleted (`SC-04` A, `SC-06` A).
5. Launcher page inside the Reporting group (`SC-05` C, `UI-01`) with its own accent triple — proposed
   `#00838F` (`AR-11`).
6. Grouping key is `(PO, PartID, ReceivedDate)` (`LG-01` C).
7. **Quantity gate is two-part** (`LG-04`): every destination bucket must net to **0**, and the source
   residual must equal live `CR_PART_LOCATION` on-hand. Material left in the receiving location is a
   legitimate `Unchanged` outcome, not a failure. The `MMC0001000` example (50,000 in, 9,000 transferred,
   41,000 residual) is the reference case.
8. A real Infor Visual `RECEIVER` / `RECEIVER_LINE` query supplies true received quantity (`LG-02` B), and a
   first-pass `CR_PART_LOCATION` check short-circuits already-correct lines to `Unchanged` (`LG-03` B).
   Remaining open: `LG-11` (reprint rows), `LG-15` (`NotFound`), `LG-05`, `LG-06`, `LG-07`, `LG-10`.
9. One calendar day per run: opens on Today, stepper skips days with no Receiving activity, sweep stops on
   first error, completed days marked (`UI-02`, `AR-08` B, `LG-10`, `DS-01`).
10. Preview → per-line review cycle → apply, on a grid with a detail pane; columns are Part, PO #, Quantity
    (`UI-03` A, `UI-04` B, `UI-05`, `UI-06`).
11. Lines with no recommendation go to a Needs Attention **modal window**, enabled only when such items
    exist (`UI-07` A).
12. Summary shows counts, duration, day, user attribution, and a run id. **No export** (`UI-10`, `AR-10` B).
13. **Label-print findings are out of scope** for this module (`UI-13` No). Family A of the quality-of-life
    document belongs with `Module_Reprint` or a separate audit module.
14. Writes use a privileged, role-gated path; new tables deferred, new procedures required (`DS-06`,
    `DS-07`, `AR-07`, `MG-05`).
15. Settings migrate to `Reconciliation.*` keys with a one-time copy; settings UI is built in a new
    `Module_Settings.Reconciliation` project (`AR-04` A, `AR-05` A).
16. Visual SQL moves to `Queries/Reconciliation/` without renumbering (`AR-09` B); the 18a/18b split is
    completed; query `17` is deleted if unused (`DS-02`).
17. The service is split into focused collaborators, not moved as one 1,200-line file (R-CODE-01).
18. `Module_Reconciliation` and `Module_Receiving` share no code, models, enums, stored procedures, or
    settings keys. The new module owns its state tables, its single write-back procedure, and all of its
    own UI (`MG-05`, `MG-06`, R-MOD-07).
19. Visual transaction questions follow the ledger model and are answered read-only against
    `INVENTORY_TRANS` and `CR_PART_LOCATION` (R-DATA-09).

---

## 12. Decision Record

Fill this in, or reply "accept all recommendations" to adopt Section 11. Every ID links back to the
question it settles.

| ID | Question | Decision | Date |
|----|----------|----------|------|
| [SC-01](#sc-01--which-full-word-names-the-module) | Module name | A | 2026-11-12 |
| [SC-02](#sc-02--does-getrecommendedlocationsasync-move) | Recommendation method location | A | 2026-11-12 |
| [SC-03](#sc-03--how-does-the-new-module-get-receiving-rows) | Cross-module data strategy | B | 2026-11-12 |
| [SC-04](#sc-04--is-reconciliation-still-a-receiving-workflow-step) | Workflow enum removal | A | 2026-11-12 |
| [SC-05](#sc-05--where-does-it-live-in-navigation) | Navigation placement | C | 2026-11-12 |
| [SC-06](#sc-06--does-the-mode-selection-card-get-removed) | Mode Selection card removal | A | 2026-11-12 |
| [SC-07](#sc-07--which-artifacts-count-as-reconcile-specific) | Cleanup boundary | Accept Section 9 disposition | 2026-11-12 |
| [SC-08](#sc-08--receiving-only-or-receiving--dunnage--volvo) | Multi-source scope | A | 2026-11-12 |
| [SC-09](#sc-09--what-is-the-user-facing-feature-name) | Feature name | Receiving Validator | 2026-11-12 |
| [AR-04](#ar-04--settings-key-migration) | Settings key migration | A | 2026-11-12 |
| [AR-05](#ar-05--which-module_settings-project-hosts-the-settings-page) | Settings UI project | A | 2026-11-12 |
| [AR-07](#ar-07--is-a-schema-migration-in-scope) | Schema migration in scope | No tables or columns; `reconciliation_*` deferred to a separate approved change | 2026-11-12 |
| [AR-08](#ar-08--execution-model-for-a-single-day) | Execution model | B | 2026-11-12 |
| [AR-09](#ar-09--where-do-the-visual-sql-files-live-after-the-move) | SQL file location | B | 2026-11-12 |
| [AR-10](#ar-10--logging-and-run-identity) | Run identity logging | B | 2026-11-12 |
| [AR-11](#ar-11--which-accent-color-does-the-module-use) | Accent color | New unused brand-consistent triple, not `ReceivingAccentBrush` (proposed `#00838F`) | 2026-11-12 |
| [UI-01](#ui-01--launcher-surface) | Launcher surface | Mode-select launcher inside `Module_Reporting`, copying existing mode-select style and features | 2026-11-12 |
| [UI-02](#ui-02--date-control-one-day-at-a-time) | Date control | One day at a time: opens on Today, stepper skips days with no Receiving activity, sweep stops on first error, completed days marked | 2026-11-12 |
| [UI-03](#ui-03--preview-then-a-per-line-review-cycle-then-apply) | Review flow | A: preview then per-line review cycle then apply; destination pre-set to recommendation; accept writes that line | 2026-11-12 |
| [UI-04](#ui-04--review-screen-shape-grid-plus-detail-pane) | Review screen shape | B: grid with row selection plus detail pane hosting the per-line prompt | 2026-11-12 |
| [UI-05](#ui-05--which-columns) | Columns | Part, PO #, Quantity; everything else in the detail pane | 2026-11-12 |
| [UI-06](#ui-06--filtering-and-sorting) | Filter and group | Filter by resolution and part/PO text; group by part + received date | 2026-11-12 |
| [UI-07](#ui-07--lines-without-a-recommendation-go-to-a-needs-attention-modal) | Lines without a recommendation | A: Needs Attention modal window, enabled only when unresolved items exist | 2026-11-12 |
| [UI-08](#ui-08--progress-cancellation-keyboard) | Progress, cancel, shortcuts | Line N of M with accepted/skipped counts, resumable, day-run progress and stop, Enter accepts, Escape skips | 2026-11-12 |
| [UI-09](#ui-09--empty-and-failure-states) | Empty and failure states | Use recommendation (page-level empty state, inline retry banner) | 2026-11-12 |
| [UI-10](#ui-10--summary-content-no-export) | Summary content | Use recommendation, no export action | 2026-11-12 |
| [UI-13](#ui-13--label-print-findings-are-not-part-of-this-module) | Print-finding placement | No: label-print findings are not part of this module | 2026-11-12 |
| [LG-01](#lg-01--what-is-the-grouping-key-blocker) | Grouping key | C: `(PO, PartID, ReceivedDate)` | 2026-11-12 |
| [LG-02](#lg-02--is-there-a-receiver-record-to-query-blocker) | Receiver data source | B: add a real Visual RECEIVER / RECEIVER_LINE query | 2026-11-12 |
| [LG-03](#lg-03--is-a-current-inventory-comparison-required) | Current-inventory compare | B: first-pass current-inventory check short-circuits already-correct lines to Unchanged | 2026-11-12 |
| [LG-04](#lg-04--quantity-gate-every-destination-bucket-must-reach-zero-and-the-source-residual-must-match-live-on-hand) | Quantity gate | Two-part gate: every destination bucket must net to 0, and the source residual must equal `CR_PART_LOCATION` | 2026-11-12 |
| [LG-05](#lg-05--which-quantity-field-represents-a-row) | Row quantity field | B: `quantity` | 2026-11-12 |
| [LG-06](#lg-06--unit-of-measure-handling) | UOM handling | UOM ignored in this module | 2026-11-12 |
| [LG-07](#lg-07--exact-equality-or-tolerance) | Tolerance | A: exact equality | 2026-11-12 |
| [LG-10](#lg-10--date-windows) | Date windows | Use recommendation (row selection = selected day; discovery +/-2 days; strict +1 day; both to settings keys) | 2026-11-12 |
| [LG-11](#lg-11--reprint-rows-is_reprint--1) | Reprint rows | Ignore reprint rows in label data | 2026-11-12 |
| [LG-12](#lg-12--idempotency-and-re-runs) | Idempotency | Use recommendation (A plus `AlreadyReconciled`) - blocked by DS-07 = A, see Section 13 | 2026-11-12 |
| [LG-13](#lg-13--non-po-rows-are-reconciled-by-a-separate-service) | Non-PO rows | Do not skip; separate service reconciles non-PO rows | 2026-11-12 |
| [LG-14](#lg-14--ignored-locations-are-not-used-in-this-module) | Ignored locations | Do not use ignored locations in this module | 2026-11-12 |
| [LG-15](#lg-15--notfound-semantics) | NotFound semantics | A, but logged | 2026-11-12 |
| [LG-16](#lg-16--over-transfer-and-under-transfer) | Over and under transfer | Over = "Extra Surplus"; Under = "Not Enough" | 2026-11-12 |
| [LG-18](#lg-18--what-does-assy-mean-in-the-source-notes) | "assy" meaning | Use recommendation (total quantity transferred/consumed) | 2026-11-12 |
| [DS-01](#ds-01--which-date-column-drives-row-selection) | Row-selection date column | Investigate both databases and choose the best columns for each | 2026-11-12 |
| [DS-02](#ds-02--visual-query-inventory) | Visual query inventory | Use recommendation (complete 18a/18b split; delete 17 if unused) | 2026-11-12 |
| [DS-03](#ds-03--receiver-usage) | RECEIVER usage | No - conflicts with LG-02 = B, see Section 13 | 2026-11-12 |
| [DS-04](#ds-04--performance-guardrails-on-visual) | Visual performance guardrails | Use recommendation (per-part set-based calls, bounded per day, TOP on candidate step) | 2026-11-12 |
| [DS-06](#ds-06--write-authorization-model-blocker) | Write authorization | Module supersedes the Edit Mode ownership blocker and always corrects receiving so it matches Visual | 2026-11-12 |
| [DS-07](#ds-07--do-reconciliation-runs-and-changes-get-persisted) | Run and change persistence | A: logs only, no tables | 2026-11-12 |
| [DS-08](#ds-08--ignored-location-storage) | Ignored-location storage | Not applicable - ignored locations not used | 2026-11-12 |
| [TS-02](#ts-02--which-existing-tests-move) | Which existing tests move | Use recommendation (split the service tests; delete the old viewmodel tests) | 2026-11-12 |
| [TS-03](#ts-03--is-there-a-visual-database-test-harness) | Visual test harness | A: manual Visual validation, unit tests over fakes | 2026-11-12 |
| [TS-04](#ts-04--which-fixtures-are-golden-cases) | Golden fixtures | Use recommendation (`MMC0000650` and the documented acceptance example) | 2026-11-12 |
| [TS-05](#ts-05--required-coverage-from-reviewmd-section-710) | Required coverage | Use recommendation, but ignored-destination is not tested: this module checks all locations | 2026-11-12 |
| [TS-06](#ts-06--enum-and-navigation-regression) | Enum and navigation regression | Use recommendation (grep for numeric persistence before deleting) | 2026-11-12 |
| [TS-07](#ts-07--viewmodel-tests) | ViewModel tests | Yes | 2026-11-12 |
| [MG-01](#mg-01--artifact-disposition-table) | Artifact disposition | Use recommendation (Section 9 disposition) | 2026-11-12 |
| [MG-02](#mg-02--move-and-preserve-or-rewrite-the-process-doc) | Move versus rewrite the process doc | Use recommendation (move and preserve the algorithm rules) | 2026-11-12 |
| [MG-03](#mg-03--what-happens-to-serenamemories) | Memory files | Use recommendation (rewrite broadly-scoped memories, delete reconcile-specific ones) | 2026-11-12 |
| [MG-04](#mg-04--removal-ordering) | Removal ordering | Use recommendation (module lands first, old surface removed last) | 2026-11-12 |
| [MG-05](#mg-05--data-boundary-shared-storage-rows-or-separate-tables-blocker) | Data boundary | Use recommendation (`MG-05` = C long term, B for the migration-free refactor) | 2026-11-12 |
| [MG-06](#mg-06--new-ui-surface-inventory) | New UI surface | Use recommendation (all-new UI, nothing reused from Receiving) | 2026-11-12 |
| [NF-01](#nf-01--performance-targets) | Performance targets | Use recommendation (`<=500` rows and `<=15 s` per day | 2026-11-12 |
| [NF-02](#nf-02--error-handling-and-retry) | Error handling and retry | Use recommendation (per-group isolation, one bounded retry, run-level summary) | 2026-11-12 |
| [NF-04](#nf-04--settings-caching) | Settings caching | Not applicable - no ignored-location setting | 2026-11-12 |
| [NF-05](#nf-05--privacy) | Privacy | Use recommendation (employee number in audit, no Windows usernames at Information level) | 2026-11-12 |
| [NF-06](#nf-06--localized-ui-text) | Localized UI text | Use recommendation, plus `AutomationProperties.Name` on every new interactive control | 2026-11-12 |

---

## 13. Conflicts Introduced By These Answers

All 66 questions are answered, but eleven answers disagree with another answer or with a repository rule.
Each one below must be resolved before the spec is written. Nothing here is a request to change an answer —
it is a request to pick which of two conflicting answers wins.

### C-01 — **BLOCKER** LG-02 = B versus DS-03 = No

`LG-02` says **add** a real Visual `RECEIVER` / `RECEIVER_LINE` query. `DS-03` says RECEIVER data is
**not** needed.

These cannot both stand. Pick one — **and the research pass in
[Section 14](#14-research-deferred-to-the-speckit-run) decides it**, because the answer depends on whether
`RECEIVER` / `RECEIVER_LINE` actually joins cleanly to a MTM row:

- **If the join is clean:** LG-02 = B wins, DS-03 becomes Yes. The module gains a true ERP received
  quantity, which strengthens `LG-04` Part 2 and unblocks QoL detectors `A1` and `B3`. Cost: a new Visual
  query project, researched against the CSV schema and reviewed separately (R-DATA-05).
- **If the join is not clean** (no reliable PO + part + line + date linkage): DS-03 = No wins, LG-02 becomes
  A. MTM rows remain the receiver proxy. Cheaper, and the module still works, but `LG-04` Part 2 can only
  compare residual against `CR_PART_LOCATION`, and QoL `A1`/`B3` stay unavailable.

**Default if the research is inconclusive:** DS-03 = No, LG-02 = A. Prefer the simpler data surface; do not
build a fragile ERP join to satisfy a research question.

### C-02 — LG-12 (B included) versus DS-07 = A and AR-07

`LG-12` accepts `AlreadyReconciled`, which requires knowing that a row was changed by a previous run.
`DS-07` = A has no persistence, and `AR-07` forbids new tables. There is nowhere to read that fact from.

**Resolution already applied:** `LG-12` is specified but only part A ships. `AlreadyReconciled` lands with
the deferred `reconciliation_*` tables. Confirm that is acceptable, or choose B for `DS-07` and bring the
tables into this refactor.

### C-03 — LG-14 (no ignored locations) contradicts a preserved invariant

`R-SCOPE-02` in [Review-Rules.md](Review-Rules.md) lists `IgnoredBySettings` as behaviour that must not
change. `LG-14` removes it.

**Resolution applied:** the rule has been amended to call this an approved removal. Also cascaded:
`TS-05` drops the ignored-destination test, `DS-08` and `NF-04` become not applicable, and `AR-04` shrinks
because `IgnoredReconciliationLocationsJson` is no longer migrated.

### C-04 — LG-05 = B and LG-07 = A will raise the `Ambiguous` rate

`quantity` is a rounded `INT`; `LG-07` demands exact equality; `LG-06` ignores UOM. On weight-based parts
the rounded label value will rarely equal the decimal transfer total, so more lines land in
`NotEnough` / `ExtraSurplus` than a decimal comparison would produce.

This is not a contradiction, it is a predictable consequence. Decide whether to accept it or switch
`LG-05` to C (`weight_quantity` with fallback). **Recommended watch:** run one live day and measure the
non-writable rate before the spec freezes the choice.

### C-05 — LG-13 adds a second service that has no grouping key

Non-PO rows have no PO, so the `LG-01` key `(PO, PartID, ReceivedDate)` does not apply. The separate
service needs its own key, its own resolution names, and its own tests. See `LG-13` for the proposed
shape — it needs confirmation, not just acknowledgment.

### C-06 — DS-06 "always makes corrections" versus the per-line review

`DS-06` says the module always corrects Receiving to match Visual. `UI-03` = A requires per-line acceptance,
and `LG-04` / `LG-16` block writes on gated lines.

**Resolution applied:** read as "always proposes, always may write, after accept, subject to the gate".
Confirm, and decide whether a role is required to write other users' rows.

### C-07 — LG-15 = A (not surfaced) versus TS-05 / UI-07 expectations

`NotFound` is invisible in the UI, but `TS-05` and `UI-07` describe surfacing unresolved outcomes. `NotFound`
is now logged plus counted in the summary, and is not part of the Needs Attention list. Confirm that the
summary count is enough visibility.

### C-08 — LG-16 renames `Ambiguous`, which other answers still reference

`UI-07`, `TS-05`, `LG-12`, and the QoL document all say `Ambiguous`. The two named resolutions are
`ExtraSurplus` and `NotEnough`. **Resolution applied:** treat `Ambiguous` in those places as the family name
for both. The spec should use the two specific names.

### C-09 — DS-01 is an open investigation, not a decision

`DS-01` defers the date-column choice to a database investigation. Until it completes, `DS-02` (SP shapes),
`LG-10` (windows), and `UI-02` (day boundaries) all assume a column that has not been confirmed.

**Deferred to the SpecKit run** — see [Section 14](#14-research-deferred-to-the-speckit-run). The spec's
research/plan phase resolves it and records the chosen column pair next to the design it justifies,
instead of as a pre-spec prerequisite. The spec must carry the answer as a decision record entry before
implementation tasks begin.

### C-10 — AR-04 has almost nothing left to migrate

With `LG-14` removing ignored locations and `UI-02` replacing `includeAllHistory`, the only settings left
are the discovery and strict windows from `LG-10`. Confirm a `Module_Settings.Reconciliation` project is
still justified, or fold those two values into the module's own settings page.

### C-11 — NF-06 adds an accessibility obligation already covered

`NF-06` asks for `AutomationProperties.Name` on new controls. That is already `R-MVVM-05`. **Resolution
applied:** no conflict, but it is now stated twice, so the spec should cite the rule rather than restate it.

---

## 14. Research Deferred To The SpecKit Run

**Decision (2026-11-12):** no further investigation is performed before the spec exists. Everything below
runs inside the SpecKit research/plan phase, and each finding becomes a decision-record entry in the spec
before implementation tasks begin.

Why this is the right place for it:

- The spec is where the design lives, so evidence recorded there sits next to the thing it justifies.
- A finding that arrives before the spec has nowhere to land except this register, which would become a
  second source of truth.
- Every item below can change a design detail but none can change the shape of the extraction, so nothing
  is blocked while they are open.

### Required before implementation tasks begin

| # | Investigation | Answers | Blocks |
|---|---|---|---|
| R-1 | MySQL `receiving_label_data` / `receiving_history`: compare `received_date`, `transaction_date`, `created_at` for completeness and operator intent | `DS-01` | `DS-02` SP shape, `LG-10` windows, `UI-02` day boundary |
| R-2 | Visual `RECEIVER` / `RECEIVER_LINE` (via the CSV schema reference, then a read-only probe): which columns carry quantity, date, PO, line, part. Does a clean join to a MTM row exist? | `DS-01`, and resolves the `LG-02` vs `DS-03` conflict (`C-01`) | Whether an ERP receipt quantity is available at all |
| R-3 | Visual `INVENTORY_TRANS.TRANSACTION_DATE` semantics for receipts versus transfers | `DS-01` | Day-window anchoring |
| R-4 | Confirm `CR_PART_LOCATION` covers every destination location the module will propose, and that it reflects the same day's movements | `LG-03`, `LG-04` Part 2 | The source-residual gate |

### Confirm against the live system during plan, not before

| # | Investigation | Answers |
|---|---|---|
| R-5 | Measure the actual `NotEnough` / `ExtraSurplus` rate for one day with `quantity` (int) + exact equality + UOM ignored | `C-04`, and whether `LG-05` should become C |
| R-6 | Confirm the 18a/18b split is still the right shape after `LG-02` and `LG-04` changes | `DS-02` |
| R-7 | Confirm query `17` has zero callers before deleting it | `DS-02`, `MG-01` |
| R-8 | Grep for numeric persistence of `Enum_ReceivingWorkflowStep.ReconciliationReview` before deleting the member | `SC-04`, `TS-06` |
| R-9 | Determine whether `Module_Receiving/Proposed_UI_Changes.xaml.md` / `.html` are reconcile-specific | `MG-01` |
| R-10 | Confirm which `.serena/memories` files name the Receiving reconciliation path explicitly | `MG-03` |

### How the findings are recorded

Each completed investigation appends a row to the spec's decision record using the same format as
[Section 12](#12-decision-record): the ID it answers, the evidence found, the choice made, and the date.
An investigation that comes back inconclusive takes the stated default from the relevant question rather
than stalling the plan.

### Rules that still apply during research

- Infor Visual stays read-only (`R-DATA-04`), with `ApplicationIntent=ReadOnly` on the connection.
- Verify every Visual table and column against `docs/InforVisual/DatabaseCSVFiles/` before querying it
  (`R-DATA-05`).
- Credentials are supplied interactively and are never written to a repo file, script, log, or commit
  (`R-SQL-06`).
- Any exploratory write to MySQL targets `mtm_receiving_application_test` and is cleaned up (`R-SQL-06`).
- Research never modifies production data and never stores findings outside repo files.
