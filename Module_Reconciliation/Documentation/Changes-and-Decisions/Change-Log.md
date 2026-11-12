# Change Log

Last Updated: 2026-11-12

## 2026-11-12 — Convergence close-out: failed-day reset, mixed-unit warning, header provider

**What changed**

- A day that fails to load now clears the whole page: the grouped grid source, the selected line, the destination field, the empty-state message, the line counters and the unresolved count go with the findings, the summary and the run identity. The reset runs through `ApplyFilters`, the same path a loaded day uses, so the flat and the grouped sources cannot drift apart. Before this, a failed day rendered the earlier day's rows under the new day's header, `AcceptCommand` could still write that stale line, and Needs Attention stayed enabled over a list the cleared findings reported as empty.
- The orchestrator logs FR-028's mixed-unit warning. A group holding more than one unit of measure writes one warning carrying the run id, the part, the day and the group identity. The unit still gates nothing and the group is previewed in full, so no verdict moves.
- The module's header title comes through `ReconciliationHeaderTitleProvider`. The launcher's view model is the provider the shell resolves from the page's `DataContext`, and the launcher heading binds to that provider instead of to its own literal. The working page's view model no longer carries a title literal of its own, so neither page writes the title itself.

**Why**

The failed-day branch cleared three of the page's eleven values, which left a screen describing one day while the header named another. The mixed unit was documented as a warning the module never issued. And the title provider existed but nothing could reach it, so the header came from a route-table string.

**Verification**

- `dotnet build MTM_Receiving_Application.slnx` succeeded with no warnings, and so did the test project build.
- Reconciliation unit tests: 76 passing. This entry corrects the convergence entry below, which recorded 72 where the suite already stood at 73. This pass adds three tests, the launcher's resolved header provider, the mixed-unit warning with and without a mixed group, and the empty-state reset after a failed day, and it extends the failed-day regression test to assert the grid source, the selection and the unresolved count as well as the run identity. With the reset reverted, the extended test fails on each of the three in turn.

## 2026-11-12 — Convergence pass: attribution, filters, binding modes and run identity

**What changed**

- The orchestrator attributes each run to the day's distinct non-empty Infor Visual users, so `Model_ReconciliationRun.VisualUserId` carries the summary attribution FR-049 names instead of staying empty. The working page shows it in the summary strip, fed through the view model.
- The page now binds the text and outcome filters the view model already honoured: a part-or-purchase-order box and an outcome selector whose choices take their text from `Converter_ReconciliationOutcomeText`, with an "All outcomes" line that clears the filter.
- Every `x:Bind` in the module's views carries an explicit mode, including the nine that omitted one.
- The orchestrator's retry lines and its failure reasons name the run id, the part, the day and the group rather than the run alone, so one failing group can be told apart from the rest of the day.
- `SC-004` and the live Visual validation are recorded in `Support-and-Fixes/Checks-and-Health.md` as outstanding manual checks that need the shop floor terminal and a live database.

**Why**

Each was a promise the spec makes that only the code could see: the attribution was never assigned, neither filter was reachable from the page, nine bindings were compiled without a stated mode, and a run with several groups could not say which one failed. An operator reads the page, not the view model.

**Verification**

- `dotnet build MTM_Receiving_Application.slnx` — succeeded, no warnings.
- Reconciliation unit tests — 72 passing. This entry also corrects the extraction entry below, which recorded 63: the suite already stood at 68 when the previous convergence pass closed, and this pass added four tests (the filter choices, the filter round trip, the run's attribution, and the view model's display of it).

## 2026-11-12 — Receiving Validator extracted from Receiving

**What changed**

- Added `Module_Reconciliation` with its own models, data access, services, view models, views, converter, settings keys, navigation title provider and accent triple.
- Added `Module_Settings.Reconciliation` with the module's settings page.
- Added three stored procedures: `sp_Reconciliation_ReceivingRows_GetDay`, `sp_Reconciliation_DayActivity_GetRange` and `sp_Reconciliation_ReceivingLocation_Update`. The third is the single write surface between the two modules.
- Added `18c_GetLocationMovementAfterWindow.sql` and moved `18a` and `18b` under `Database/InforVisualScripts/Queries/Reconciliation/`, replacing the transaction-id direction heuristic with direction from `TYPE`.
- Split `Service_ReceivingLocationReconciliation` in Receiving: the entry-time recommendation stays with query 16, and every reconcile method is gone.
- Removed the Receiving reconciliation step, mode-selection card, review view model, review dialog, two models, the workflow host, the ignored-locations settings view, the help entries, and the two old settings keys.
- Deleted query 17 and its plumbing, and the superseded combined query 18.

**Why**

Location reconciliation belongs to its own module, writes only when the material is fully accounted for, and must not share code with Receiving.

**Verification**

- `dotnet build MTM_Receiving_Application.slnx` — succeeded, no warnings.
- Reconciliation unit tests — 68 passing, including the `MMC0001000` reference case. (This line recorded 63; the count was wrong and is corrected here. See the convergence entry above for the current total.)
- Module boundary: no `Module_Reconciliation` file names `Module_Receiving`, and no Receiving or Receiving-settings file names `Module_Reconciliation`.
- Full suite: 1132 passing, 8 failing in `ViewModel_Tool_ReceivingAnalyticsAppTests`. Those failures are in the ShipRec Tools analytics feature, which this change does not touch, and they are not related to it.

## Boundary check result

Both source-level checks pass: no file under `Module_Reconciliation/` names `Module_Receiving`, and no file under `Module_Receiving/` or `Module_Settings.Receiving/` names `Module_Reconciliation`.

The whole-solution exclusion build the plan proposed cannot succeed, and that is a property of the repository rather than a defect in this change. Every module compiles into one assembly, so `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` and `MainWindow.xaml.cs` reference both modules by design, which FR-061 and R-DI-01 require. Building with `Module_Reconciliation/**` excluded therefore fails only inside that shared wiring, and building with `Module_Receiving/**` excluded fails throughout the shell. The attempt was run and reverted; the tree builds clean afterwards.

**Outstanding**

- The live Infor Visual validation of `18a`, `18b` and `18c` against `MMC0001000` and `MMC0000650` has not been run; it needs a live connection.
- The non-writable rate on one live day has not been measured. That measurement decides whether the proof keeps the integer quantity or moves to the decimal weight quantity.

## 2026-11-12 — Review cycle admits only writable lines (FR-046)

**What changed**

- `ViewModel_ReconciliationMain.ReviewableFindings` now selects `Writable` lines only. It previously also admitted `Unresolved`, so a surplus or shortfall line with no proposal entered the review cycle, which FR-046 forbids: such a line cannot be pre-filled and belongs behind the Needs Attention action.
- The write service still accepts `Unresolved` lines with a `NotEnough` or `ExtraSurplus` outcome, because the Needs Attention window writes a hand-set location through that same path. Only the cycle's membership changed; the manual-resolution capability is untouched.

**Why**

An unresolved line carried an empty proposal, so the reviewer was advanced onto a line the module had already declined to answer, and the accept path for it skipped the re-proof that a writable line is subjected to. The Needs Attention window is the intended surface for those lines and is enabled whenever such a line exists, so nothing became unreachable.

**Verification**

- `dotnet build MTM_Receiving_Application.slnx` — succeeded, 0 warnings, 0 errors.
- Reconciliation unit tests — 77 passing, 0 failing.
- `UnresolvedLinesShouldNotEnterTheCycleButShouldStayReachableAsNeedsAttention` fails against the previous predicate and passes against the current one, so the change is pinned by a test rather than by assertion.
- Three existing review tests encoded the old behaviour (`AcceptShouldWriteOnceAndAdvanceToTheNextUnresolvedLine`, the selection assertion in `SkipShouldCountWithoutWritingAnything`, and `CountsShouldSurviveAReload`, which needed a second writable line to exercise accept-then-skip) and were updated to the requirement.

## 2026-11-12 — Working page made readable

**What changed**

- The three date pickers that looked identical are now labelled: `Working day` on the day bar, and `From` / `To` inside a collapsed `Sweep a backlog` expander. The sweep is a once-a-week action and no longer competes with the day.
- The summary is a named sentence instead of a bare number strip: "39 lines · 30 updated · 0 already correct · 0 need attention · 9 not found · 0 skipped · 0 errors". All seven counts FR-049 requires stay visible; they are labelled rather than positional.
- Run provenance moved to one quiet line beneath it: cycle position, run id, Infor Visual user, duration.
- The grid gained Part / PO # / Qty column captions aligned over the rows, and each group header now carries its line count on the right.
- The detail pane was an unlabelled vertical dump of seventeen values. It is now a card of labelled fields in three groups: the part and its move (`Saved` -> `Visual says`), an outcome badge beside the one-sentence reason, then `PO / line`, `Load`, `Unit`, `Difference`, and an `EVIDENCE` block (moved by, moved at, moves, from, source). The action sits last: the destination field, an accent `Accept`, `Skip`, and the `Enter accepts · Escape skips` hint.
- A placeholder ("Select a line to see why it is here.") shows when nothing is selected, instead of a pane of empty values.
- Added `HasSelectedFinding`, `HasNoSelectedFinding`, `CyclePositionText` and `SummaryLineText` to the view model so the page needs no converters for state and no invented captions.

**Why**

The screenshot showed the page as a wall of unlabelled values: three indistinguishable date pickers, a fifteen-number strip, an unheadered grid, and a detail pane whose every line was a bare value. Nothing on it said what any number meant. The requirement content did not change; only its presentation.

**Verification**

- `dotnet build MTM_Receiving_Application.csproj` — 0 warnings, 0 errors.
- Reconciliation unit tests — 77 passing, 0 failing.
- Every binding remains a compiled binding with an explicit mode and every new control carries an accessible name (FR-051). The three apparent misses from a naive regex are nested `{StaticResource ...}` braces and all carry `Mode=OneWay`.

## 2026-11-12 — Fix day picker drifting backwards and a fabricated 00:00

**What changed**

- `SelectedDateOffset`, `SweepStartDateOffset` and `SweepEndDateOffset` now build their `DateTimeOffset` with the machine's local offset. They previously used `TimeSpan.Zero`, so a midnight date landed on the previous calendar day anywhere west of Greenwich: the picker showed the wrong day, and each `Date` push re-entered `DateChanged` and walked the day back another one until the page crashed. Proven by `SelectedDateOffsetShouldRoundTripToTheSameDayInLocalTime`, which fails against the old construction.
- `OnWorkingDayChanged` now ignores a `DateChanged` whose date already equals `SelectedDate`, so a programmatic push cannot re-enter the load. The sweep handlers got the same equality guard.
- The evidence pane's `Moved at` reads the real posting timestamp from `INVENTORY_TRANS.CREATE_DATE` (query `18b` now returns `MovementDateTime`), because `TRANSACTION_DATE` holds no time of day in this database. `TRANSACTION_DATE` still drives all date-window arithmetic, so no outcome changed.
- `MovementTimeText` shows only the date when Visual recorded no time of day, rather than inventing `00:00`.
- `RunDurationText` is suppressed below one second instead of printing `0:00`.
- The run identifier is no longer displayed; it remains in the log. The quiet line now reads cycle position, who moved the material in Infor Visual, and duration, and omits empty parts so no separator dangles.

**Why**

A date picker that silently changes the working day is a correctness problem, not a cosmetic one: every navigation moved backwards. The two time defects both showed a clock reading Visual never recorded.

**Verification**

- `dotnet build MTM_Receiving_Application.slnx` — 0 warnings, 0 errors.
- Reconciliation unit tests — 81 passing, 0 failing (4 added).
- The round-trip and both `MovementTimeText` cases fail against the previous code.
