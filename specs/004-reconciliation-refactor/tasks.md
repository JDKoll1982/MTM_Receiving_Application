# Tasks: Receiving Validator

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Data model**: [data-model.md](data-model.md) | **Contracts**: [contracts/](contracts/)
**Created**: 2026-11-12
**Size**: oversized

**Scale note**: This list spans about 55 files across five areas: a new working module with its own data access, a new settings project, three stored procedures and three Visual queries, and a removal sweep through Receiving, the shell and the tests. Watch two things while working: the `Module_Reconciliation` boundary is namespace-enforced because the repository compiles every module into one assembly (D-01), and the quantity proof is the only place a line becomes writable.

**One file is deliberately shared.** `Module_Reconciliation/Views/View_ReconciliationMain.xaml` and its code-behind are written in Phase 4 and extended in Phases 5, 6 and 8, because FR-042 pins a single page of grid plus detail pane rather than one page per story. Every other path in this list has exactly one owner phase, and the view model behind that page is split into four partials so each story owns its own file.

**Phase order deviates from priority once.** User Story 3 (the quantity proof) runs before User Story 1, because a day's outcomes cannot be derived and no line can be accepted until the proof exists (FR-032, FR-041). The reason is restated at Phase 3.

---

## Phase 1: Setup

**Files**: the module and settings-project folder trees, the module's test folder tree.

**Wave 1 — independent (different folders):**

- [x] **T001** [P] [US7] Create the `Module_Reconciliation` folder tree (`Contracts/`, `Data/`, `Models/`, `Services/`, `ViewModels/`, `Views/`, `Converters/`, `Settings/`, `Navigation/`) with the `MTM_Receiving_Application.Module_Reconciliation` namespace root and no reference to `Module_Receiving` · `Module_Reconciliation/` — FR-001, FR-004, FR-002, SC-01, R-NAM-03
- [x] **T002** [P] [US7] Create the `Module_Settings.Reconciliation` folder tree (`ViewModels/`, `Views/`) beside the other settings projects · `Module_Settings.Reconciliation/` — FR-059, AR-05, R-SET-03
- [x] **T003** [P] [US1] Create the module's test folder tree (`Services/`, `Data/`, `ViewModels/`) · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/` — TS-02, R-TEST-02

---

## Phase 2: Foundational

**Files**: all module models, enums and contracts; settings keys; the three stored procedures and their deployment script; the three Visual query files; both DAOs; the accent triple; the outcome converter; the header title provider.

Every story below depends on this phase. Nothing in it reaches a user story's behaviour.

**Wave 1 — independent (different files): types, settings and SQL**

- [x] **T004** [P] [US3] `Enum_ReconciliationOutcome` with exactly `Unchanged`, `Updated`, `ExtraSurplus`, `NotEnough`, `NotFound`, `Skipped`, `Error`, `ManuallySet`, and no `Ambiguous` or `IgnoredBySettings` member · `Module_Reconciliation/Models/Enum_ReconciliationOutcome.cs` — FR-032, FR-035, D-12
- [x] **T005** [P] [US1] The four supporting enums: `Enum_ReconciliationRowSource` (`CurrentLabel`, `History`), `Enum_ReconciliationDestinationSource` (`Derived`, `Operator`), `Enum_ReconciliationFindingState`, `Enum_ReconciliationDayState` · `Module_Reconciliation/Models/Enum_ReconciliationRowSource.cs`, `Enum_ReconciliationDestinationSource.cs`, `Enum_ReconciliationFindingState.cs`, `Enum_ReconciliationDayState.cs` — FR-032, data-model §Enum_ReconciliationOutcome
- [x] **T006** [P] [US3] `Model_ReconciliationRow` projecting `quantity` as an `int` and `weight_quantity` as a separate `decimal?`, with `TransactionDate` for scope and `ReceivedAt` for display · `Module_Reconciliation/Models/Model_ReconciliationRow.cs` — FR-027, FR-016, FR-013, D-05, R-1
- [x] **T007** [P] [US1] `Model_ReconciliationGroupKey` as a value object carrying purchase order, part, transaction date and vendor key · `Module_Reconciliation/Models/Model_ReconciliationGroupKey.cs` — FR-015, FR-020, D-06
- [x] **T008** [P] [US3] `Model_ReconciliationMovement` with both legs' warehouses, locations, transaction ids and the inbound leg's quantity, date and user · `Module_Reconciliation/Models/Model_ReconciliationMovement.cs` — FR-030, D-04
- [x] **T009** [P] [US3] `Model_ReconciliationLocationBalance` (`OnHand`, `MovementAfterWindow`) · `Module_Reconciliation/Models/Model_ReconciliationLocationBalance.cs` — FR-023, D-02
- [x] **T010** [P] [US3] `Model_ReconciliationDestinationBucket` with immutable `NetQuantity`, mutable `RemainingQuantity`, `AllocatedQuantity`, `LatestMovementAt`, `MovementCount`, `EvidenceLocations` and `RanNegative` · `Module_Reconciliation/Models/Model_ReconciliationDestinationBucket.cs` — FR-017, FR-022, FR-025
- [x] **T011** [P] [US3] `Model_ReconciliationSourceResidual` with `ExpectedResidual`, `BalanceAsOf`, `Difference`, `Agrees` and `BalanceSource` · `Module_Reconciliation/Models/Model_ReconciliationSourceResidual.cs` — FR-023, FR-024, D-02
- [x] **T012** [P] [US1] Settings keys and defaults: `Reconciliation.BusinessRules.MovementDiscoveryWindowDays` (2) and `Reconciliation.BusinessRules.StrictValidationWindowDays` (1), with no `Receiving.` key and no ignored-locations key · `Module_Reconciliation/Settings/ReconciliationSettingsKeys.cs`, `Module_Reconciliation/Settings/ReconciliationSettingsDefaults.cs` — FR-059, FR-014, FR-035, FR-060
- [x] **T013** [P] [US7] Add the module accent triple (fill, border, highlight) in a colour distinct from `ReceivingAccentBrush`, proposed `#00838F` · `Module_Core/Themes/ModuleAccentBrushes.xaml` — FR-062, AR-11, R-HDR-04, SC-09
- [x] **T014** [P] [US1] `sp_Reconciliation_ReceivingRows_GetDay` returning both row sources for one `transaction_date`, reprints included, with header block, explicit column list and a toggleable test block · `Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/sp_Reconciliation_ReceivingRows_GetDay.sql` — FR-056, FR-013, FR-016, R-SQL-02, R-SQL-03, R-SQL-04, R-SQL-05
- [x] **T015** [P] [US1] `sp_Reconciliation_DayActivity_GetRange` returning distinct `activity_date` values across both tables for a bounded range, with no join and no Visual access · `Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/sp_Reconciliation_DayActivity_GetRange.sql` — FR-009, FR-056, R-SQL-02, R-SQL-03, R-SQL-04, R-SQL-05
- [x] **T016** [P] [US2] `sp_Reconciliation_ReceivingLocation_Update`, the single write procedure: `SET NOCOUNT ON`, explicit `BEGIN`/`COMMIT`, table chosen by `IF`/`ELSEIF`/`SIGNAL`, updates `initial_location` only, rejects blank location and blank operator, no ownership predicate, returns `p_affected_rows` · `Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/sp_Reconciliation_ReceivingLocation_Update.sql` — FR-005, FR-052, FR-055, FR-056, D-11, R-SQL-03, R-SQL-04, R-SQL-05
- [x] **T017** [P] [US3] Move `18a_GetTransferCandidatePairs.sql` under the reconciliation query folder and replace the lower-id-is-source heuristic with direction from `TYPE` (`O` out, `I` in), keeping the window as a parameter · `Database/InforVisualScripts/Queries/Reconciliation/18a_GetTransferCandidatePairs.sql` — FR-030, FR-014, FR-067, D-04, AR-09, R-DATA-09
- [x] **T018** [P] [US3] Move `18b_GetTransferPairDetails.sql` under the reconciliation query folder and label each leg's role from `TYPE`, returning the twelve columns the movement model maps, excluding equal-location and non-positive rows · `Database/InforVisualScripts/Queries/Reconciliation/18b_GetTransferPairDetails.sql` — FR-030, FR-067, D-04, AR-09
- [x] **T019** [P] [US3] Write `18c_GetLocationMovementAfterWindow.sql` returning `WarehouseId`, `LocationId` and signed net movement after `@AfterDate` per part, warehouse and location · `Database/InforVisualScripts/Queries/Reconciliation/18c_GetLocationMovementAfterWindow.sql` — FR-023, FR-057, D-02, R-4, R-SQL-02, R-SQL-07

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — independent (different files): composite types and settings service**

- [x] **T020** [US1] `Model_ReconciliationGroup` holding key, rows, source locations, receiving location, movements, buckets, residual and findings · `Module_Reconciliation/Models/Model_ReconciliationGroup.cs` — FR-015, FR-068
- [x] **T021** [US2] `Model_ReconciliationFinding` with outcome, saved and proposed location, destination source, reason, `Difference`, bucket evidence, state and `IsWritable`; a surplus or shortfall finding without a difference is rejected by a constructor guard · `Module_Reconciliation/Models/Model_ReconciliationFinding.cs` — FR-032, FR-033, FR-036, FR-037
- [x] **T022** [US3] `Model_ReconciliationAllocation`, `Model_ReconciliationBucketVerdict` and `Model_ReconciliationProofResult` · `Module_Reconciliation/Models/Model_ReconciliationAllocation.cs`, `Model_ReconciliationBucketVerdict.cs`, `Model_ReconciliationProofResult.cs` — FR-022, FR-025
- [x] **T023** [US1] `Model_ReconciliationRun` and `Model_ReconciliationDaySummary` with the seven counts, `ExcludedReprintCount`, `ResidualDisagreement` and one `TotalAccounted` projection; no export member · `Module_Reconciliation/Models/Model_ReconciliationRun.cs`, `Module_Reconciliation/Models/Model_ReconciliationDaySummary.cs` — FR-021, FR-049, FR-069, SC-003, SC-012
- [x] **T024** [P] [US1] `IService_ReconciliationSettings` and its implementation reading both windows through the settings core facade under `Reconciliation.`, returning the default when a value is missing · `Module_Reconciliation/Contracts/IService_ReconciliationSettings.cs`, `Module_Reconciliation/Services/Service_ReconciliationSettings.cs` — FR-059, FR-014, R-SET-01

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3 — independent (different files): day model, contracts, data access, converter, header title provider**

- [x] **T025** [US1] `Model_ReconciliationDay` carrying date, `HasActivity`, rows, groups, findings, summary, state and failure reason · `Module_Reconciliation/Models/Model_ReconciliationDay.cs` — FR-007, FR-010, FR-012
- [x] **T026** [P] [US1] Contracts `IService_ReconciliationDayReader`, `IService_ReconciliationPreview` and `IService_ReconciliationWriteService`, every method returning `Model_Dao_Result<T>` with an `Async` suffix · `Module_Reconciliation/Contracts/IService_ReconciliationDayReader.cs`, `IService_ReconciliationPreview.cs`, `IService_ReconciliationWriteService.cs` — FR-038, FR-041, R-DATA-02, R-NAM-04
- [x] **T027** [P] [US3] Contracts `IService_ReconciliationGroupResolver`, `IService_ReconciliationNonPurchaseOrderResolver`, `IService_ReconciliationMovementSelector`, `IService_ReconciliationBucketBuilder`, `IService_ReconciliationAllocator` and `IService_ReconciliationQuantityProof` · `Module_Reconciliation/Contracts/IService_ReconciliationGroupResolver.cs`, `IService_ReconciliationNonPurchaseOrderResolver.cs`, `IService_ReconciliationMovementSelector.cs`, `IService_ReconciliationBucketBuilder.cs`, `IService_ReconciliationAllocator.cs`, `IService_ReconciliationQuantityProof.cs` — FR-017, FR-018, FR-020, FR-022, FR-071
- [x] **T028** [US1] `Dao_ReconciliationReceivingRows`: instance class, connection through constructor injection, `GetDayRowsAsync`, `GetActivityDatesAsync` and `UpdateLocationAsync` over the three reconciliation procedures, mapping `quantity` and `weight_quantity` separately · `Module_Reconciliation/Data/Dao_ReconciliationReceivingRows.cs` — FR-005, FR-056, FR-027, R-DATA-01, R-DATA-02, R-DATA-03
- [x] **T029** [US3] `Dao_ReconciliationInforVisual`: instance class, read-only connection guard repeated, loads `Reconciliation/18a`, `18b` and `18c` through `Helper_SqlQueryLoader`, and reads current on-hand from `CR_PART_LOCATION` · `Module_Reconciliation/Data/Dao_ReconciliationInforVisual.cs` — FR-057, FR-023, D-10, R-DATA-01, R-DATA-02, R-DATA-04
- [x] **T030** [US2] `Converter_ReconciliationOutcomeBrush` mapping the eight outcomes to their row brush and display text, as the only place the display strings exist · `Module_Reconciliation/Converters/Converter_ReconciliationOutcomeBrush.cs` — FR-050, FR-032, R-MVVM-08
- [x] **T031** [P] [US7] `ReconciliationHeaderTitleProvider` implementing `IViewModel_HeaderTitleProvider` with `Receiving Validator` as the title and `from Infor Visual` as the context subtitle, so no view or view model writes a title itself · `Module_Reconciliation/Navigation/ReconciliationHeaderTitleProvider.cs` — FR-062, R-HDR-01

**⟶ Wait for Wave 3 to finish, then:**

- [x] **T032** [US7] Write the deployment script that creates the three procedures idempotently, with a header block, `--whatif` and `--generate-sql` support, a backup before writing, typed confirmation, abort-and-restore on first error, and a `mtm_receiving_application_test` validation step · `Database/Database_Deployment/Sql_Files/Migrations/19_Migration_reconciliation_stored_procedures.sql` — FR-056, R-SQL-06, R-SQL-01

---

## Phase 3: User Story 3 — Only write when the material is fully accounted for (P3)

**Files**: `Module_Reconciliation/Services/Service_ReconciliationMovementSelector.cs`, `Service_ReconciliationBucketBuilder.cs`, `Service_ReconciliationAllocator.cs`, `Service_ReconciliationQuantityProof.cs`; tests under `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/`.

This phase runs before User Story 1 because every outcome in the grid is derived from the proof, and accepting a line re-runs it (FR-032, FR-041). Splitting it out afterwards is not possible.

### Tests

Written first, failing until the implementations below land.

**Wave 1 — independent (different files):**

- [x] **T033** [P] [US3] Tests: movement selector admits a movement reachable by source location or by purchase order, repeats until nothing is admitted, discards a movement whose legs are the same location, and returns a stable order · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationMovementSelectorTests.cs` — FR-017
- [x] **T034** [P] [US3] Tests: bucket builder drops a location that fully forwards, keeps one that retains quantity for that remainder, refuses to make a bucket of a destination that ran negative, and orders buckets deterministically · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationBucketBuilderTests.cs` — FR-017, FR-025
- [x] **T035** [P] [US3] Tests: allocator binds a saved-location row to its own bucket before the general pass, prefers an exact fit, otherwise the smallest sufficient bucket, orders rows by quantity then load number then load id, never gates on unit of measure, and leaves a row unallocated when nothing covers it · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationAllocatorTests.cs` — FR-018, FR-026, FR-028, FR-029, D-03
- [x] **T036** [P] [US3] Tests: quantity proof reproduces the `MMC0001000` reference case (50,000 into `RECV`, 9,000 moved, 41,000 live on-hand, four destination verdicts consumed to zero and the residual agreeing), and fails the residual when the balance differs, fails a negative bucket, and passes an exact match with no tolerance · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationQuantityProofTests.cs` — FR-022, FR-023, FR-024, FR-025, FR-026, FR-031, SC-001, TS-04

### Implementation

**Wave 1 — independent (different files):**

- [x] **T037** [P] [US3] `Service_ReconciliationMovementSelector` seeding on the group's saved locations and purchase order keys, admitting reachable movements in repeat passes, discarding same-location moves and ordering the result · `Module_Reconciliation/Services/Service_ReconciliationMovementSelector.cs` — FR-017, FR-030
- [x] **T038** [P] [US3] `Service_ReconciliationBucketBuilder` computing net per location from the selected movements, dropping zero nets, keeping retained remainders, recording `RanNegative`, and filling the evidence fields · `Module_Reconciliation/Services/Service_ReconciliationBucketBuilder.cs` — FR-017, FR-022, FR-025
- [x] **T039** [P] [US3] `Service_ReconciliationAllocator` running the saved-location pass first and then the deterministic general pass, with no unit gate and exact `decimal` arithmetic · `Module_Reconciliation/Services/Service_ReconciliationAllocator.cs` — FR-018, FR-026, FR-028, FR-029, D-03

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T040** [US3] `Service_ReconciliationQuantityProof` running both parts: bucket verdicts consumed to zero with `Extra Surplus` or `Not Enough` naming the divergence, and the residual checked against `OnHand - MovementAfterWindow` at the receiving location, every figure formatted with `CultureInfo.InvariantCulture` · `Module_Reconciliation/Services/Service_ReconciliationQuantityProof.cs` — FR-022, FR-023, FR-024, FR-025, FR-026, FR-031, FR-033, FR-058, D-02

**Checkpoint**: the reference case passes as a unit test, and no line can be reported writable without a passing proof.

---

## Phase 4: User Story 1 — See what one day holds (P1)

**Files**: `Module_Reconciliation/Services/Service_ReconciliationDayReader.cs`, `Service_ReconciliationGroupResolver.cs`, `Service_ReconciliationPreviewOrchestrator.cs`; `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`; `Module_Reconciliation/Views/View_ReconciliationMain.xaml` and `.xaml.cs`; tests `Service_ReconciliationDayReaderTests.cs`, `Service_ReconciliationGroupResolverTests.cs`, `Service_ReconciliationPreviewOrchestratorTests.cs`, `ViewModel_ReconciliationMainTests.cs`.

### Tests

Written first, failing until the implementations below land.

**Wave 1 — independent (different files):**

- [x] **T041** [P] [US1] Tests: day reader excludes reprint rows and reports the excluded count, returns activity dates from the cheap range query, and answers the nearest active day in each direction without running a preview · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationDayReaderTests.cs` — FR-009, FR-010, FR-016
- [x] **T042** [P] [US1] Tests: group resolver makes one group of one purchase order and part on one day, two groups when the purchase order differs on the same day, two groups when the day differs, reconciles current-label and history rows together, and routes non-purchase-order rows to the injected resolver · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationGroupResolverTests.cs` — FR-015, FR-019, FR-020, D-06
- [x] **T043** [P] [US1] Tests: preview orchestrator produces exactly one finding per loaded row, the summary counts sum to the rows loaded, a preview performs no write, and one group's Visual failure marks only that group's findings as `Error` · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestratorTests.cs` — FR-021, FR-038, FR-068, FR-069, SC-003
- [x] **T044** [P] [US1] Tests: the main view model opens on today, steps only to days that hold rows, keeps Today reachable in one action, filters by outcome and by part or purchase-order text, and reports the empty state for a day with no rows · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainTests.cs` — FR-008, FR-009, FR-043, FR-044, FR-048, TS-07

### Implementation

**Wave 1 — independent (different files):**

- [x] **T045** [P] [US1] `Service_ReconciliationDayReader` loading the day, setting `HasActivity` from the row count, excluding and counting reprints, and answering activity dates and nearest active days from the range procedure · `Module_Reconciliation/Services/Service_ReconciliationDayReader.cs` — FR-007, FR-009, FR-010, FR-016
- [x] **T046** [P] [US1] `Service_ReconciliationGroupResolver` partitioning purchase-order rows on purchase order, part and transaction date, marking candidacy failures as `Skipped`, and handing non-purchase-order rows to `IService_ReconciliationNonPurchaseOrderResolver` · `Module_Reconciliation/Services/Service_ReconciliationGroupResolver.cs` — FR-015, FR-019, FR-020, FR-021, D-06

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T047** [US1] `Service_ReconciliationPreviewOrchestrator` composing day reader, group resolver, movement selector, bucket builder, allocator and proof, applying the outcome derivation order from the data model, logging `NotFound` groups with part, day and group, carrying the run id on every log line, and isolating a failure to one group · `Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestrator.cs` — FR-021, FR-034, FR-037, FR-038, FR-068, FR-069, FR-070, FR-071

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T048** [US1] `ViewModel_ReconciliationMain` as a partial inheriting `ViewModel_Shared_Base`, holding the selected day, both stepper commands, Today, filters, per-line progress, accepted and skipped counts, the summary and the day state · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs` — FR-008, FR-009, FR-012, FR-043, FR-044, R-MVVM-01, R-MVVM-02

**⟶ Wait for Wave 3 to finish, then:**

- [x] **T049** [US1] The working page: day bar with date picker, previous, next and Today, summary strip with the seven counts, run id and duration, a three-column grid of part, purchase order number and quantity grouped by part and received date with row colour carrying the outcome, a detail-pane region, the page-level empty and all-match states, the inline Infor Visual retry banner, virtualization, and `x:Bind` with explicit modes and accessible names throughout · `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `Module_Reconciliation/Views/View_ReconciliationMain.xaml.cs` — FR-042, FR-043, FR-048, FR-049, FR-050, FR-051, FR-070, SC-012, R-MVVM-03, R-MVVM-05

**Checkpoint**: the module lists a day's rows grouped and coloured by outcome, never lands on an empty day while stepping, states the empty state explicitly, and writes nothing on preview.

---

## Phase 5: User Story 2 — Correct one line at a time (P2)

**Files**: `Module_Reconciliation/Services/Service_ReconciliationWriteService.cs`; `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Review.cs`; `Module_Reconciliation/Views/View_ReconciliationMain.xaml` (detail pane); tests `Service_ReconciliationWriteServiceTests.cs`, `ViewModel_ReconciliationMainReviewTests.cs`.

### Tests

Written first, failing until the implementations below land.

**Wave 1 — independent (different files):**

- [x] **T050** [P] [US2] Tests: the write service re-runs the proof at accept time, refuses a finding that is not `Writable` and a proposal that failed the proof, calls the update procedure exactly once, records an operator-supplied destination as `Manually Set`, and leaves a failed write `Unresolved` with the reason · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationWriteServiceTests.cs` — FR-041, FR-052, FR-053, FR-054, DS-06
- [x] **T051** [P] [US2] Tests: the review cycle pre-fills the destination from the proposal, writes on accept and advances to the next unresolved line, counts a skip without writing, resumes at the first unresolved line within the session, and keeps accepted and skipped counts across a reload · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainReviewTests.cs` — FR-039, FR-040, FR-044, FR-045

### Implementation

**Wave 1:**

- [x] **T052** [US2] `Service_ReconciliationWriteService` accepting a finding, re-running the proof for its group, performing one call to the update procedure, setting `Manually Set` for an operator-entered location and `Updated` for an accepted proposal, and refusing every other state with a reason · `Module_Reconciliation/Services/Service_ReconciliationWriteService.cs` — FR-041, FR-047, FR-052, FR-053, FR-054, D-09, D-11

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T053** [US2] `ViewModel_ReconciliationMain.Review.cs`: selected finding, destination field pre-set to the proposal, accept and skip commands, Enter and Escape handling, and the advance-to-next-unresolved rule · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Review.cs` — FR-039, FR-040, FR-041, FR-044, FR-045

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T054** [US2] Fill the detail panes on the working page: the thirteen remaining fields from FR-042, the pre-set destination field, accept and skip affordances, and the manually-set marker, with no confirmation dialog and no bulk action between keystroke and write · `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `Module_Reconciliation/Views/View_ReconciliationMain.xaml.cs` — FR-042, FR-045, FR-047, FR-051

**Checkpoint**: three mismatched lines are cleared with the keyboard alone, a skipped fourth is unchanged, and the cycle resumes where it stopped within the session.

---

## Phase 6: User Story 4 — Triage what the module could not answer (P4)

**Files**: `Module_Reconciliation/ViewModels/ViewModel_ReconciliationNeedsAttention.cs`, `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Attention.cs`, `Module_Reconciliation/Views/View_ReconciliationNeedsAttentionWindow.xaml` and `.xaml.cs`, `View_ReconciliationMain.xaml` (Needs Attention action); test `ViewModel_ReconciliationNeedsAttentionTests.cs`.

### Tests

**Wave 1:**

- [x] **T055** [P] [US4] Tests: the attention view model lists only unresolved lines with reason, evidence and raw difference, writes a hand-set location through the accepted-line path, marks it `Manually Set`, and leaves untouched lines counted as unresolved · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationNeedsAttentionTests.cs` — FR-046, FR-047

### Implementation

**Wave 1:**

- [x] **T056** [US4] `ViewModel_ReconciliationNeedsAttention` exposing the unresolved findings and a location field per line that submits through `IService_ReconciliationWriteService.AcceptAsync` · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationNeedsAttention.cs` — FR-046, FR-047

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T057** [US4] `ViewModel_ReconciliationMain.Attention.cs`: the unresolved count, a command disabled whenever that count is zero, and opening the window with the current day's findings · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Attention.cs` — FR-046

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T058** [US4] The Needs Attention window as a modal `Window`, not a dialog, listing reason, evidence and raw difference per line with a location field per line and accessible names on every control · `Module_Reconciliation/Views/View_ReconciliationNeedsAttentionWindow.xaml`, `Module_Reconciliation/Views/View_ReconciliationNeedsAttentionWindow.xaml.cs` — FR-047, FR-051, R-MVVM-07

**⟶ Wait for Wave 3 to finish, then:**

- [x] **T059** [US4] The Needs Attention action in the day bar, showing the count and disabled while it is zero · `Module_Reconciliation/Views/View_ReconciliationMain.xaml` — FR-046

**Checkpoint**: a surplus day opens the window with the difference stated, one location typed there writes through the same path, and the untouched lines stay unresolved in the summary.

---

## Phase 7: User Story 5 — Non-purchase-order receipts are assessed, not skipped (P5)

**Files**: `Module_Reconciliation/Services/Service_ReconciliationNonPurchaseOrderResolver.cs`; test `Service_ReconciliationNonPurchaseOrderResolverTests.cs`.

### Tests

**Wave 1:**

- [x] **T060** [P] [US5] Tests: non-purchase-order rows group by part, transaction date and vendor, a row with no vendor reaches `Not Enough` with a reason naming the missing vendor and stays in the counts, a null purchase order never skips a row, and the same proof applies · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationNonPurchaseOrderResolverTests.cs` — FR-020, LG-13

### Implementation

**Wave 1:**

- [x] **T061** [US5] `Service_ReconciliationNonPurchaseOrderResolver` producing groups of the same shape as the purchase-order resolver, grouped by part, transaction date and vendor · `Module_Reconciliation/Services/Service_ReconciliationNonPurchaseOrderResolver.cs` — FR-020, LG-13

**Checkpoint**: a day of only non-purchase-order receipts is assessed line by line, and its groups reach the movement selector, allocator and proof without a special case.

---

## Phase 8: User Story 6 — Clear a backlog a day at a time (P6)

**Files**: `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Sweep.cs`, `View_ReconciliationMain.xaml` (sweep controls); test `ViewModel_ReconciliationMainSweepTests.cs`.

### Tests

**Wave 1:**

- [x] **T062** [P] [US6] Tests: a sweep runs each day through the single-day path in order with per-day progress, stops between days on request, stops on the first failing day and leaves the operator there, marks a completed day for the session, and never widens the discovery or strict window · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainSweepTests.cs` — FR-011, FR-012, FR-014, SC-006

### Implementation

**Wave 1:**

- [x] **T063** [US6] `ViewModel_ReconciliationMain.Sweep.cs`: sweep start and end date, start and stop commands, per-day progress, the completed-day markers, the stop-on-first-failure rule and the settings read that keeps both windows fixed · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Sweep.cs` — FR-011, FR-012, FR-014

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T064** [US6] Sweep controls in the day bar: start date, end date, Start, Stop, per-day progress and the completed mark on a visited day · `Module_Reconciliation/Views/View_ReconciliationMain.xaml` — FR-011, FR-012, FR-051

**Checkpoint**: a five-day sweep with one failing day marks the first four, stops on the fifth and states why.

---

## Phase 9: User Story 7 — The feature has its own home and Receiving keeps its own behaviour (P7)

**Files**: `Module_Reconciliation/ViewModels/ViewModel_Reconciliation_Launcher.cs`, `Module_Reconciliation/Views/View_Reconciliation_Launcher.xaml` and `.xaml.cs`; `Module_Settings.Reconciliation/ViewModels/ViewModel_Settings_Reconciliation_Main.cs`, `Module_Settings.Reconciliation/Views/View_Settings_Reconciliation_Main.xaml` and `.xaml.cs`; `MainWindow.xaml`, `MainWindow.xaml.cs`, `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`; then every path in the removal surface.

The new surface is complete before anything is removed (`R-SCOPE-03`, `MG-04`), and the solution builds between removal steps.

**Wave 1 — independent (different files):**

- [x] **T065** [P] [US7] `ViewModel_Reconciliation_Launcher` exposing the module's modes and opening the working page · `Module_Reconciliation/ViewModels/ViewModel_Reconciliation_Launcher.cs` — FR-062, UI-01, D-07
- [x] **T066** [P] [US7] `ViewModel_Settings_Reconciliation_Main` reading and writing both window settings through the settings core facade, with validation that rejects a negative window · `Module_Settings.Reconciliation/ViewModels/ViewModel_Settings_Reconciliation_Main.cs` — FR-059, R-SET-01, R-SET-03
- [x] **T067** [P] [US7] Trim `IService_ReceivingLocationReconciliation` to `GetRecommendedLocationsAsync` and its result type, leaving the signature unchanged · `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs` — FR-006, SC-02
- [x] **T068** [P] [US7] Delete the query 17 plumbing: the interface member, the DAO method and file load, the service pass-through, the model and the SQL file · `Module_Core/Contracts/Services/IService_InforVisual.cs`, `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`, `Module_Core/Services/Database/Service_InforVisualConnect.cs`, `Module_Core/Models/InforVisual/Model_InforVisualLocationTransaction.cs`, `Database/InforVisualScripts/Queries/17_GetReceivingLocationTransactionHistory.sql` — FR-067, R-7, DS-02
- [x] **T069** [P] [US7] Delete the superseded combined transfer query now that 18a and 18b carry its work · `Database/InforVisualScripts/Queries/18_GetReceivingLocationTransferMovements.sql` — FR-067, R-6, DS-02
- [x] **T070** [P] [US7] Delete the old review view-model tests · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReviewTests.cs` — FR-066, TS-02

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — independent (different files):**

- [x] **T071** [US7] The launcher page, copying the mode-select style of the existing mode-select pages, labelled `Receiving Validator` with `from Infor Visual` as supporting text · `Module_Reconciliation/Views/View_Reconciliation_Launcher.xaml`, `Module_Reconciliation/Views/View_Reconciliation_Launcher.xaml.cs` — FR-062, UI-01, D-07, SC-09, R-MVVM-03, R-MVVM-05
- [x] **T072** [US7] The settings page with a field for each window setting and its validation message · `Module_Settings.Reconciliation/Views/View_Settings_Reconciliation_Main.xaml`, `Module_Settings.Reconciliation/Views/View_Settings_Reconciliation_Main.xaml.cs` — FR-059, R-SET-03, R-MVVM-05
- [x] **T073** [US7] Split `Service_ReceivingLocationReconciliation`: keep `GetRecommendedLocationsAsync` and its query 16 call unchanged, delete the reconcile methods and the ignored-locations read, and leave no second reconciliation implementation behind · `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` — FR-006, FR-035, FR-066, SC-009

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3 — independent (different files):**

- [x] **T074** [US7] Register the module's two DAOs, its services and collaborators, its launcher and working view models and views, and the settings view model and view; remove the four Receiving registrations for the reconciliation service, review view model, review dialog and settings view · `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` — FR-061, FR-003, FR-001, R-DI-01, R-DI-02
- [x] **T075** [US7] Shell presentation wiring: the accent map row for `MTM_Receiving_Application.Module_Reconciliation`, the launcher page route, the settings route, and removal of the reconciliation settings catalogue entry · `MainWindow.xaml.cs` — FR-062, FR-061
- [x] **T076** [US7] The single navigation entry in the Reporting footer group, labelled `Receiving Validator` with `from Infor Visual` as supporting text · `MainWindow.xaml` — FR-062, SC-05, SC-09

**⟶ Wait for Wave 3 to finish, then:**

**Wave 4 — independent (different files): deletion of the old surface**

- [x] **T077** [P] [US7] Delete the old review view model · `Module_Receiving/ViewModels/ViewModel_Receiving_LocationReconciliationReview.cs` — FR-066
- [x] **T078** [P] [US7] Delete the old review dialog and its code-behind · `Module_Receiving/Views/View_Receiving_Dialog_LocationReconciliationReview.xaml`, `Module_Receiving/Views/View_Receiving_Dialog_LocationReconciliationReview.xaml.cs` — FR-066
- [x] **T079** [P] [US7] Split the Receiving service tests: keep the recommendation cases, drop the allocation cases that the module now owns · `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingLocationReconciliationTests.cs` — FR-066, TS-02
- [x] **T080** [P] [US7] Delete the two old reconciliation models · `Module_Receiving/Models/Model_ReceivingLocationReconciliationItem.cs`, `Module_Receiving/Models/Model_ReceivingLocationReconciliationSummary.cs` — FR-066
- [x] **T081** [P] [US7] Remove `ReconciliationReviewHost` and its visibility plumbing from the workflow view · `Module_Receiving/Views/View_Receiving_Workflow.xaml`, `Module_Receiving/Views/View_Receiving_Workflow.xaml.cs` — FR-065
- [x] **T082** [P] [US7] Remove the six workflow-step references from the workflow view model · `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` — FR-063
- [x] **T083** [P] [US7] Remove the Mode Selection reconciliation card, its command binding and its accessible name, leaving the rest of the page in place · `Module_Receiving/Views/View_Receiving_ModeSelection.xaml`, `Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs` — FR-064

**⟶ Wait for Wave 4 to finish, then:**

**Wave 5 — independent (different files):**

- [x] **T084** [US7] Re-run the numeric-persistence check for `Enum_ReceivingWorkflowStep` and then delete the `ReconciliationReview = 12` member outright, with no reserved slot · `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs` — FR-063, R-8, TS-06
- [x] **T085** [US7] Remove the three help entries keyed `Receiving.ReconciliationReview` · `Module_Core/Services/Help/Service_Help.cs` — FR-063, R-8
- [x] **T086** [US7] Remove the reconciliation block from the receiving preferences view and view model · `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_UserPreferences.cs`, `Module_Settings.Receiving/Views/View_Settings_Receiving_UserPreferences.xaml` — FR-060
- [x] **T087** [US7] Remove both old key reads from the remaining receiving settings view models · `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_PartFormatting.cs`, `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_BusinessRules.cs` — FR-060
- [x] **T088** [US7] Delete the ignored-locations settings view and remove its card from the receiving settings category hub · `Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml`, `Module_Settings.Receiving/Views/View_Settings_Receiving_Reconciliation.xaml.cs`, `Module_Settings.Receiving/Views/View_Settings_Receiving_CategoryHub.xaml`, `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_CategoryHub.cs` — FR-060
- [x] **T089** [US7] Remove the settings metadata entry that describes the deleted view · `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs` — FR-060

**⟶ Wait for Wave 5 to finish, then:**

- [x] **T090** [US7] Remove the two Receiving settings keys and their defaults: `ValidateAllHistoryForLocationReconciliation` and `IgnoredReconciliationLocationsJson`, with no migration · `Module_Receiving/Settings/ReceivingSettingsKeys.cs`, `Module_Receiving/Settings/ReceivingSettingsDefaults.cs` — FR-060, AR-04, LG-14

**Checkpoint**: the validator is reachable once, from the Reporting group, with its own accent and title; Receiving offers no reconciliation card, step or dialog; and entry-time location suggestion still returns the same recommendation.

---

## Phase 10: Polish

**Files**: the module's `Documentation/` tree, the moved algorithm document, `.github/copilot-instructions.md`, and the validation records.

**Wave 1 — independent (different files): documentation**

- [x] **T091** [P] [US7] Create the `Documentation/` folders and the two Overview files, each with `Last Updated: 2026-11-12` inside its first five lines · `Module_Reconciliation/Documentation/Overview/About-This-Module.md`, `Module_Reconciliation/Documentation/Overview/How-It-Works-at-a-Glance.md` — R-DOC-01, D-08
- [x] **T092** [P] [US7] The two How-To-Guides files and the two End-User-Help files, in plain language · `Module_Reconciliation/Documentation/How-To-Guides/Daily-Tasks.md`, `How-To-Guides/Unusual-Situations.md`, `End-User-Help/Quick-Start.md`, `End-User-Help/FAQ.md` — R-DOC-01
- [x] **T093** [P] [US7] The two Support-and-Fixes files, covering the Infor Visual retry banner, an unreachable application database, and a residual disagreement · `Module_Reconciliation/Documentation/Support-and-Fixes/Common-Issues.md`, `Support-and-Fixes/Checks-and-Health.md` — R-DOC-01
- [x] **T094** [P] [US7] The Change-Log and Decisions files, recording D-01 to D-12, the FR-054 working default of any authenticated user with no supervisor gate, and the removal sequence · `Module_Reconciliation/Documentation/Changes-and-Decisions/Change-Log.md`, `Changes-and-Decisions/Decisions.md` — R-DOC-01, R-DOC-03, D-09
- [x] **T095** [P] [US7] The Refactor-Plan and Impact-Map files, with goals, risks, rollback steps, success criteria and the removed surface · `Module_Reconciliation/Documentation/Big-Changes/Refactor-Plan.md`, `Big-Changes/Impact-Map.md` — R-DOC-01
- [x] **T096** [P] [US7] The Editing-Brief and Guardrails files, naming the namespace boundary, the single write procedure, and the proof as the only write gate · `Module_Reconciliation/Documentation/AI-Handoff/Editing-Brief.md`, `AI-Handoff/Guardrails.md` — R-DOC-01
- [x] **T097** [P] [US7] The Templates placeholder · `Module_Reconciliation/Documentation/Templates/README.md` — R-DOC-01

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2 — independent (different files):**

- [x] **T098** [US7] Move the receiving reconciliation process document into the module's documentation, preserving every algorithm rule, and remove the source · `Module_Reconciliation/Documentation/Overview/Location-Reconciliation-Algorithm.md` (from `docs/features/receiving/location-reconciliation-process.md`) — MG-02, R-DOC-01, R-DOC-02
- [x] **T099** [US7] Add `Module_Reconciliation` to the module map and remove nothing else from it · `.github/copilot-instructions.md` — R-DOC-02
- [ ] **T100** [US3] Validate `18a`, `18b` and `18c` by hand against the live Infor Visual database with the `MMC0001000` reference case and the `MMC0000650` five-row example, and record both results · `Module_Reconciliation/Documentation/Support-and-Fixes/Checks-and-Health.md` — TS-03, R-DATA-05

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T101** [US7] Verify the module boundary in both directions: build with `Module_Reconciliation/**` excluded from compilation, then with `Module_Receiving/**` excluded, restoring `MTM_Receiving_Application.csproj` after each run and recording both results · `MTM_Receiving_Application.csproj` — FR-002, FR-003, SC-008, D-01
- [x] **T102** [US7] Run the full test and lint suites, confirm no location is written for a failed bucket or residual check, no outcome is missing from the counts, the recommendation regression still passes, and the summary offers no export; append the result to the change log · `Module_Reconciliation/Documentation/Changes-and-Decisions/Change-Log.md` — SC-002, SC-003, SC-009, SC-012, R-TEST-06

---

## Dependencies & Execution Order

**Phase dependencies.** Setup → Foundational → User Story 3 → User Story 1 → User Story 2 → User Story 4 → User Story 5 → User Story 6 → User Story 7 → Polish.

Nothing in Foundational is user-visible and no story starts until it is complete. User Story 3 precedes User Story 1 because the outcome derivation and the accept gate both consume the proof. User Story 5 is independent of User Stories 2, 4 and 6 and only has to land before Phase 9 registers the real resolver behind `IService_ReconciliationNonPurchaseOrderResolver`. Every removal task sits in Phase 9, after the new surface exists, and the solution builds between removal waves.

**Per-phase waves.**

- **Phase 1** — one wave, three tasks in different folders.
- **Phase 2** — Wave 1 types, settings and SQL; Wave 2 composite types and the settings service; Wave 3 day model, contracts, both DAOs and the converter; Wave 4 the deployment script.
- **Phase 3** — tests wave; Wave 1 selector, bucket builder and allocator; Wave 2 the proof.
- **Phase 4** — tests wave; Wave 1 day reader and group resolver; Wave 2 preview orchestrator; Wave 3 main view model; Wave 4 the page.
- **Phase 5** — tests wave; Wave 1 write service; Wave 2 review partial; Wave 3 detail panes.
- **Phase 6** — tests wave; Wave 1 attention view model; Wave 2 attention partial; Wave 3 the window; Wave 4 the day-bar action.
- **Phase 7** — tests wave; Wave 1 the non-purchase-order resolver.
- **Phase 8** — tests wave; Wave 1 sweep partial; Wave 2 sweep controls.
- **Phase 9** — Wave 1 new files plus the three standalone deletions; Wave 2 launcher, settings view and the service split; Wave 3 registration, shell wiring and the navigation entry; Wave 4 the seven independent deletions; Wave 5 the enum member, help entries and settings editors; Wave 6 the two settings keys.
- **Phase 10** — Wave 1 the seven documentation tasks; Wave 2 the moved algorithm document, the module map and the manual Visual validation; Wave 3 the boundary build and the suite run.

**Serialization points.** `View_ReconciliationMain.xaml` is the one shared path, ordered Phase 4 → Phase 5 → Phase 6 → Phase 8. `ModuleServicesExtensions.cs` opens with the four Receiving registrations still present in Phase 9 Wave 3 and the old types are deleted afterwards in Wave 4, so every join builds. Within Phase 9, `T067` precedes `T073`, `T073` precedes `T090`, and `T074` precedes every deletion in Wave 4.

## Requirement Coverage

All 71 functional requirements map to at least one task. The structured `coverage` field in `.spec-context.json` is not writable by this version of `write-context.py`, which accepts only `decisions`, `step_summary`, `notes` and `concerns` in a batch, so the map is kept here.

| Requirement | Tasks |
| --- | --- |
| FR-001 | T001, T074 |
| FR-002 | T001, T101 |
| FR-003 | T074, T101 |
| FR-004 | T001 |
| FR-005 | T016, T028 |
| FR-006 | T067, T073 |
| FR-007 | T025, T045 |
| FR-008 | T044, T048 |
| FR-009 | T015, T041, T044, T045, T048 |
| FR-010 | T025, T041, T045 |
| FR-011 | T062, T063, T064 |
| FR-012 | T025, T048, T062, T063, T064 |
| FR-013 | T006, T014 |
| FR-014 | T012, T017, T024, T062, T063 |
| FR-015 | T007, T020, T042, T046 |
| FR-016 | T006, T014, T041, T045 |
| FR-017 | T010, T027, T033, T034, T037, T038 |
| FR-018 | T027, T035, T039 |
| FR-019 | T042, T046 |
| FR-020 | T007, T027, T042, T046, T060, T061 |
| FR-021 | T023, T043, T046, T047 |
| FR-022 | T010, T022, T027, T036, T038, T040 |
| FR-023 | T009, T011, T019, T029, T036, T040 |
| FR-024 | T011, T036, T040 |
| FR-025 | T010, T022, T034, T036, T038, T040 |
| FR-026 | T035, T036, T039, T040 |
| FR-027 | T006, T028 |
| FR-028 | T035, T039 |
| FR-029 | T035, T039 |
| FR-030 | T008, T017, T018, T037 |
| FR-031 | T036, T040 |
| FR-032 | T004, T005, T021, T030 |
| FR-033 | T021, T040 |
| FR-034 | T047 |
| FR-035 | T004, T012, T073 |
| FR-036 | T021 |
| FR-037 | T021, T047 |
| FR-038 | T026, T043, T047 |
| FR-039 | T051, T053 |
| FR-040 | T051, T053 |
| FR-041 | T026, T050, T052, T053 |
| FR-042 | T049, T054 |
| FR-043 | T044, T048, T049 |
| FR-044 | T044, T048, T051, T053 |
| FR-045 | T051, T053, T054 |
| FR-046 | T055, T056, T057, T059 |
| FR-047 | T052, T054, T055, T056, T058 |
| FR-048 | T044, T049 |
| FR-049 | T023, T049 |
| FR-050 | T030, T049 |
| FR-051 | T049, T054, T058, T064 |
| FR-052 | T016, T050, T052 |
| FR-053 | T050, T052 |
| FR-054 | T050, T052, T094 |
| FR-055 | T016 |
| FR-056 | T014, T015, T016, T028, T032 |
| FR-057 | T019, T029 |
| FR-058 | T040 |
| FR-059 | T002, T012, T024, T066, T072 |
| FR-060 | T012, T086, T087, T088, T089, T090 |
| FR-061 | T074, T075 |
| FR-062 | T013, T031, T065, T071, T075, T076 |
| FR-063 | T082, T084, T085 |
| FR-064 | T083 |
| FR-065 | T081 |
| FR-066 | T070, T073, T077, T078, T079, T080 |
| FR-067 | T017, T018, T068, T069 |
| FR-068 | T020, T043, T047 |
| FR-069 | T023, T043, T047 |
| FR-070 | T047, T049 |
| FR-071 | T027, T047 |

---

## Phase 11: Convergence

Appended by `/speckit.converge` on 2026-11-12 after the implementation pass. Both tasks close
one gap: a task marked complete whose deliverable is absent. Nothing above this section was
renumbered, reordered or removed.

- [x] **T103** Write the four preview-orchestrator test cases that T043 specified and never delivered: a day holding several groups produces exactly one finding per loaded row, the summary counts sum to the rows loaded, a preview performs no write, and one group's Infor Visual failure marks only that group's findings as `Error` after one bounded retry, with the run id carried on every log line · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestratorTests.cs` (the file exists and is empty) per T043, FR-021, FR-038, FR-068, FR-069, SC-003 (missing)
- [x] **T104** Extend the `MMC0001000` reference case to the outcome half of SC-001: drive the day through `PreviewDayAsync` with 50,000 received into `RECV` and movements of 5,000 to `SHIP`, 2,500 to `V-C0-01`, 500 to `R-04` and 1,000 to `R-05`, then assert the four destination rows are offered `SHIP`, `V-C0-01`, `R-04` and `R-05` as `Updated` and that the row totalling 41,000 derives `Unchanged` at `RECV` against a 41,000 as-of balance · `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestratorTests.cs` per SC-001, US3/AC1, FR-023, FR-029 (partial)

---

## Phase 12: Convergence

Appended by `/speckit.converge` on 2026-11-12 (pass 2), after Phase 11 closed. Phase 11's two
tasks were re-verified as delivered and behavioural, and the defect those tests exposed is fixed.
Five findings remain against the spec: two the phase list never built, three partial gaps. Nothing
above this section was renumbered, reordered or removed.

**T100 stays open on purpose.** It is a manual validation of `18a`, `18b` and `18c` against the live
Infor Visual database. `TS-03` forbids the suite from reaching live Visual and no `/speckit.implement`
pass can hold the connection, so it is carried as a documented manual residual in
`Support-and-Fixes/Checks-and-Health.md`, not as an implementable task. It is not evidence that the
spec is unbuilt.

- [x] **T105** [US1] Give the day summary its Infor Visual user attribution: aggregate the distinct non-empty movement users of the day's findings into the run, and show the result in the summary strip with an explicit binding mode and an accessible name, so the summary carries the attribution FR-049 names rather than leaving `Model_ReconciliationRun.VisualUserId` permanently empty · `Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestrator.cs`, `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`, `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestratorTests.cs` per FR-049, AR-10 (missing)
- [x] **T106** [US1] Expose the two filters the view model already honours: a part-or-purchase-order text box bound to `TextFilter` and an outcome selector bound to `OutcomeFilter` whose choices take their display text from `Converter_ReconciliationOutcomeText`, placed in the day bar with accessible names and explicit binding modes, so the page itself can filter by outcome and by part or purchase-order text · `Module_Reconciliation/Views/View_ReconciliationMain.xaml` per FR-043, UI-06 (missing)
- [x] **T107** [US7] Add the missing explicit binding mode to the nine `x:Bind` usages that omit one — the group header key and the three item-template columns in the day grid, and the four item texts plus the submit command in the Needs Attention window — so every binding is compiled with a stated mode · `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `Module_Reconciliation/Views/View_ReconciliationNeedsAttentionWindow.xaml` per FR-051, R-MVVM-05 (partial)
- [x] **T108** [US7] Bring the change log back in step with the change it describes: record the run-id and group-identity correction to the orchestrator's failure reasons and retry lines, and correct the recorded reconciliation test count from 63 to 68 · `Module_Reconciliation/Documentation/Changes-and-Decisions/Change-Log.md` per R-DOC-02, R-DOC-03, FR-069 (partial)
- [x] **T109** [US3] Record that the `SC-004` measurement is an outstanding manual check needing the shop floor terminal — a 500-row day inside 15 seconds and the 250-row scroll check — beside the existing note for the live Visual validation, and state that the grid already virtualizes through its `*`-row `ListView` · `Module_Reconciliation/Documentation/Support-and-Fixes/Checks-and-Health.md` per SC-004, FR-070 (partial)

---

## Phase 13: Convergence

Appended by `/speckit.converge` on 2026-11-12 (pass 3), after Phase 12 closed. Phase 12's two HIGH
findings are now closed at the requirement level rather than only at the binding: the run's Infor
Visual attribution is computed from the day's selected movement evidence rather than from the
findings, so a day whose every line is `Unchanged` still reports who moved the material; and the two
filters drive the grouped grid source, not just an unwatched collection, with the outcome selector's
"All outcomes" line clearing back to the whole day. All 77 `x:Bind` occurrences in the module's
three views name an explicit mode. Nothing above this section was renumbered, reordered or removed.

Four findings remain, none of them a Phase 11 or Phase 12 item. The first is the same defect class
Phase 12 half-fixed: that pass taught the failed-day branch to clear the summary strip's identity, but
the grid, the selection and the attention count still survive a failed load.

**The two manual residuals are not tasks, and not evidence that the spec is unbuilt.** The live Infor
Visual validation of `18a`, `18b` and `18c` needs a connection and an operator, and `TS-03 = A`
forbids the suite from reaching live Visual; the `SC-004` grid measurement needs the shop floor
terminal. Both are recorded in `Support-and-Fixes/Checks-and-Health.md`. Each validates an artefact
that exists and is covered against fakes; neither can be closed by any `/speckit.implement` pass.

- [x] **T110** [US1] Reset the page when a day fails to load: the failure branch of `LoadDayAsync` clears the findings, the summary and the run identity but leaves `GroupedFindings` — the grid's actual source — the selected line, the destination field, the empty-state message, the line counters and `UnresolvedCount` holding the previous day's values, so a failed day renders the earlier day's rows under the new day's header, `AcceptCommand` can still write that stale line, and Needs Attention stays enabled over a list the cleared `Findings` reports as empty; clear all of that page state (or route the failure through the same reset `ApplyDay` uses), and extend the failed-day regression test to assert the grid, the selection and the unresolved count rather than only the three run-identity fields · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainTests.cs` per FR-048, FR-046, US1/AC4, US4/AC2 (partial)
- [x] **T111** [US3] Log the mixed-unit warning FR-028 requires: a group holding more than one unit of measure must write a warning carrying the run id, the part, the day and the group, so the group stays ungated on unit (`Model_ReconciliationRow.UnitOfMeasure` is documented as "display and warning only", the detail pane already shows the unit, and no code path issues the warning), with a test that a two-unit group logs once and a single-unit group does not · `Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestrator.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestratorTests.cs` per FR-028, FR-069, LG-06 (missing)
- [x] **T112** [US7] Make the module's header title come through `ReconciliationHeaderTitleProvider` instead of a literal: the provider is registered but never resolved, because the shell's `ResolveHeaderProvider` reads only the page, its `DataContext` and its `ViewModel` property, so the launcher's header falls back to the `"Receiving Validator"` string in the shell's route table and the working page's header is merely retained from the launcher because `SetContentPage` hands it an empty fallback — both are the directly-written title FR-062 forbids; expose the provider from the launcher page so the shell tracks it, and cover it with a test that the page's resolved header provider reports `Receiving Validator` with `from Infor Visual` as its subtitle · `Module_Reconciliation/Navigation/ReconciliationHeaderTitleProvider.cs`, `Module_Reconciliation/Views/View_Reconciliation_Launcher.xaml.cs`, `Module_Reconciliation/ViewModels/ViewModel_Reconciliation_Launcher.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/` per FR-062, R-HDR-01 (partial)
- [x] **T113** [US7] Bring the change log's recorded test count back in step: the convergence entry states "Reconciliation unit tests — 72 passing" while the suite reports 73, and the total will move again when T110 and T111 land, so record whatever the suite reports at the close of this pass in the same entry the fixes above write to · `Module_Reconciliation/Documentation/Changes-and-Decisions/Change-Log.md` per R-DOC-02, R-DOC-03 (partial)

---

## Phase 14: Convergence

Appended by `/speckit.converge` on 2026-11-13 (pass 4), after Phase 13 closed and after the two
entries the module shipped since then — the grid hiding matching lines with the `18d` direct-receipt
adjudication, and the bidirectional reachability walk in the movement selector. Every one of
T001–T113 was re-checked against the code rather than against its own checkbox. The build is clean
(`dotnet build MTM_Receiving_Application.slnx`, 0 warnings, 0 errors), 88 reconciliation tests pass,
and the full suite reports 1,157 passing with the same 8 pre-existing `Module_ShipRec_Tools`
failures the change log has already traced to a clean checkout.

Four findings remained, all of them promises the spec makes that only the page can keep. Three are
things the view model already computes and the page never shows; the fourth was a recorded decision
that contradicted an acceptance test. That fourth one is now closed: the operator decided on
2026-11-13 that matching lines must stay out of the default grid but must be reachable through the
outcome filter, so `T117` implements the filter route rather than restoring the rows, and the spec
records the rule beside the acceptance test it qualifies.

**The two manual residuals are still not tasks, and still not evidence that the spec is unbuilt.**
The live Infor Visual validation of `18a`, `18b`, `18c` and `18d` needs a connection and an
operator, and `TS-03 = A` forbids the suite from reaching live Visual; the `SC-004` grid measurement
needs the shop floor terminal. Both stay recorded in `Support-and-Fixes/Checks-and-Health.md`.

- [ ] **T114** [US1] Put the day's identity in the day summary: FR-049 requires the summary to show "the day reconciled" and "a copyable run id", and the page shows neither — `DayHeaderText` and `RunIdentifier` are computed on every load and bound nowhere, no view offers any copy affordance (the module contains no clipboard call at all), and `RunProvenanceText` documents the run id as "deliberately not shown", which is the opposite of the requirement; bind the day and the run id in the summary strip with an accessible name and a copy action, and cover it with a test that a loaded day exposes both and a failed day clears both · `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainTests.cs` per FR-049, R-DOC-02 (missing)
- [ ] **T115** [US2] Show the review cycle's accepted and skipped counts: FR-044 requires the cycle to state "line N of M together with accepted and skipped counts", and only the first half is on the page — `AcceptedCount` and `SkippedCount` are incremented on every accept and every skip and are bound by no view, so the operator can see neither how many lines this session has written nor how many it has set aside; add both to the cycle progress line (`CyclePositionText` / `RunProvenanceText`) beside the position, and extend the review-cycle test to assert the rendered progress text moves on an accept and on a skip · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Review.cs`, `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`, `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainReviewTests.cs` per FR-044, UI-08 (partial)
- [ ] **T116** [US1] State the two page-level outcomes the page cannot state: FR-048 lists "all lines skipped" and "residual disagreement" as page-level states and neither exists — `UpdateEmptyState` runs only inside `ApplyDay`, so after the operator skips the last reviewable line the page keeps saying nothing at all, and when it does run it folds `Skipped` into the "already matches where the material went" sentence, which is false for a skipped line; and `Summary.ResidualDisagreement` is computed by the orchestrator, copied by the refresh, and rendered nowhere, so a day whose residual does not agree with live on-hand is reported only inside the surviving line reasons while FR-024 requires the disagreement on the page as well as in the summary; recompute the empty state on skip and accept, split the all-skipped sentence out of the all-match one, surface the residual disagreement beside them, and cover all three with tests · `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`, `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.Review.cs`, `Module_Reconciliation/Views/View_ReconciliationMain.xaml`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainTests.cs`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainReviewTests.cs` per FR-048, FR-024, US1/AC4 (missing)
- [x] **T117** [US1] Make the day's matching lines reachable without listing them: the grid hides every `Unchanged` finding and `Model_ReconciliationOutcomeFilterChoice.All` also left `Unchanged` out of the selector, so a day whose receipts all match rendered an empty grid with no way to ask why, and one of the eight outcomes FR-032 names could not be filtered at all (FR-043). Decided with the operator on 2026-11-13: matching lines must not appear in the default grid, because they are for checking rather than working, but they must be reachable through a filter — so offer `Unchanged` as a filter choice and let `ApplyFilters` show exactly the chosen outcome, `Unchanged` included, while the no-filter view keeps hiding it; point the all-match empty state at the filter that reveals those lines and withdraw that sentence once they are on screen; amend decision `D-13` and record the rule in the change log so no decision contradicts the spec (`SC-010`); the filter tests change with it · `Module_Reconciliation/Models/Model_ReconciliationOutcomeFilterChoice.cs`, `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`, `Module_Reconciliation/Documentation/Changes-and-Decisions/Decisions.md`, `Module_Reconciliation/Documentation/Changes-and-Decisions/Change-Log.md`, `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainTests.cs` per US1/AC1, FR-032, FR-036, FR-043, SC-010 (contradiction)

