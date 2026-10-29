# Tasks: Receiving Analytics App Data Tab

**Feature**: `specs/002-receiving-analytics-two-tab`
**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Data model**: [data-model.md](./data-model.md)
**Contracts**: [service-interface.md](./contracts/service-interface.md), [stored-procedures.md](./contracts/stored-procedures.md), [ui-contract.md](./contracts/ui-contract.md)
**Size**: oversized

## Scale note

This change moves roughly thirty-five files across five layers: two views, two view models, a new
service and DAO, four stored procedures, one shared category translation, a settings key and its
manifest entry, DI registration, four test files and three documentation files. A reader should watch
two things. The ERP tab is wrapped in place and must not be rewritten, so any edit inside its existing
markup is a regression; and the App Data panels all render from one view model and one view, so the
story phases extend those two files in priority order and never run as parallel workers.

## Execution model

`[P]` marks a task that touches a file no other task in its wave touches and that no incomplete task
blocks. Tasks inside a wave may be built in any order. A join line means the tasks after it wait for
the whole wave before it. Every task names the file it creates or edits.

**Out of scope, and no task may drift into it**: catalog items 5, 6, 7, 12, 21, 24, 25, 26, 28, 29,
30, 31, 32, 33, 39, 40, 41, 42, 46, 48, 49 and 50; the marking-coverage figure and its target; and any
inclusion of in-progress receipts. No task adds a table, view, function or trigger, and no task
de-duplicates loads with `DISTINCT load_number`.

## Phase 1: Setup (database objects and configuration)

The four stored procedures and the settings key every story reads. No C# type changes here.

**Wave 1 — independent (different files):**

- [x] **T001** [P] Create `sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay`: bucket key `DATE(COALESCE(received_date, transaction_date))`, day totals for load and label joined in from a derived table, `SUM(COALESCE(weight_quantity, quantity))` per day, `part_type` and `unit_of_measure` untranslated, `p_start_date` inclusive and `p_end_date` exclusive, `DELIMITER $$` conventions, SQL exactly as the contract gives it · Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay.sql
- [x] **T002** [P] Create `sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay`: one row per bucket date and status, `CASE` classifying `NoDueDate`, `Early`, `OnTime` (bucket date equals `po_due_date`), `Late`, ordered by `FIELD(status, 'Early', 'OnTime', 'Late', 'NoDueDate')` · Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay.sql
- [x] **T003** [P] Create `sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals`: `COALESCE(NULLIF(TRIM(vendor_name), ''), 'Unknown')` grouped case-insensitively, one row per supplier and unit with label count, distinct load count and quantity total · Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals.sql
- [x] **T004** [P] Create `sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount`: single scalar `COUNT(*)` of in-range rows whose `load_number IS NULL`, no grouping · Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount.sql
- [x] **T005** [P] Add a nested `ReceivingAnalytics` class holding `OnTimeTarget = "ReceivingAnalytics.OnTimeTarget"` beside the existing key groups · Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs
- [x] **T006** [P] Declare the `ReceivingAnalytics.OnTimeTarget` entry under category `ShipRecTools`: `defaultValue` `95`, `dataType` `Decimal` (supported by `Enum_SettingsDataType`), `scope` `System`, `permissionLevel` `Admin`, `isSensitive` false, validation rule stating a percentage between 0 and 100 · Module_Settings.Core/Defaults/settings.manifest.json

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T007** Regenerate the object catalog so the four new procedures are registered: run `Database/Database_Deployment/Build-SqlObjectCatalog.ps1` and commit the regenerated catalog · Database/Database_Deployment/sql-object-catalog.json

## Phase 2: Foundational (shared data path, blocks every story)

Every type the four panels read from. No user-story work begins until this phase compiles.

**Wave 1 — independent (different files):**

- [x] **T008** [P] Create `Enum_ReceivingAnalyticsPeriod` with `Day`, `Week`, `Month`, matching the repo's one-enum-per-file convention · Module_ShipRec_Tools/Enums/Enum_ReceivingAnalyticsPeriod.cs
- [x] **T009** [P] Create `Enum_ReceivingAnalyticsSupplierMeasure` with `Labels`, `Quantity`, `Loads` · Module_ShipRec_Tools/Enums/Enum_ReceivingAnalyticsSupplierMeasure.cs
- [x] **T010** [P] Create `Model_Tool_ReceivingAnalyticsAppFilter` with `StartDate` (today minus 29 days, inclusive), `EndDate` (today plus one day, exclusive), `Period` (`Day`), `UnitOfMeasure` (null meaning one line per unit), `SupplierMeasure` (`Labels`); a `Clear`-style reset returns these defaults · Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsAppFilter.cs
- [x] **T011** [P] Create `Model_Tool_ReceivingAnalyticsAppActivityRow` with `BucketDate`, `PartType`, `UnitOfMeasure`, `QuantityTotal`, `LoadCount`, `LabelCount`, and a comment that the two counts repeat per day and must be read once per day · Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsAppActivityRow.cs
- [x] **T012** [P] Create `Model_Tool_ReceivingAnalyticsAppOnTimeRow` with `BucketDate`, `Status`, `ReceiptCount` · Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsAppOnTimeRow.cs
- [x] **T013** [P] Create `Model_Tool_ReceivingAnalyticsAppSupplierRow` with `VendorName`, `UnitOfMeasure`, `LabelCount`, `LoadCount`, `QuantityTotal` · Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsAppSupplierRow.cs
- [x] **T014** [P] Create `Model_Tool_ReceivingAnalyticsAppDataset` holding the three read-only row lists and `MissingLoadNumberCount`, with an `IsEmpty` derivation from the activity rows · Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsAppDataset.cs
- [x] **T015** [P] Create `Model_Tool_ReceivingAnalyticsAppComparison` with `AppCoverageSeries`, `ErpCoverageSeries`, `AppCategorySplit`, `ErpCategorySplit` (both keyed by the five ERP names) and `ErpAvailable` · Module_ShipRec_Tools/Models/Model_Tool_ReceivingAnalyticsAppComparison.cs
- [x] **T016** [P] Create `Translation_ReceivingAnalyticsCategory`: the five ERP names, the three app names, `ToErpCategory` and `ToAppCategory` with the null contract, and the pinned derivation rules (`MMC` contains for `Coil`, `MMF` contains for `Sheet`) · Module_ShipRec_Tools/Helpers/Translation_ReceivingAnalyticsCategory.cs

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T017** [P] Create `Dao_Tool_ReceivingAnalyticsApp`: instance-based, connection string in the constructor, four `virtual` async reads through `Helper_Database_StoredProcedure.ExecuteListAsync` and `ExecuteSingleAsync`, parameters built as `start_date` and `end_date` without the `p_` prefix, each returning `Model_Dao_Result<T>`, no raw SQL · Module_ShipRec_Tools/Data/Dao_Tool_ReceivingAnalyticsApp.cs
- [x] **T018** [P] Create `IService_Tool_ReceivingAnalyticsApp` with `GetAppDataAsync` and `GetComparisonAsync`, both taking the filter and returning `Model_Dao_Result<T>` · Module_ShipRec_Tools/Contracts/IService_Tool_ReceivingAnalyticsApp.cs
- [x] **T019** [P] Add `GetReceivingAnalyticsOnTimeTargetAsync()` returning `Task<decimal?>`, null when the key is blank or missing, parsed from the string value and never defaulted to a number in code; declare it on the interface and implement it beside the existing accessors · Module_ShipRec_Tools/Contracts/IService_ShipRecToolsSettings.cs, Module_ShipRec_Tools/Services/Service_ShipRecToolsSettings.cs
- [x] **T020** [P] Replace the five ERP category literals with reads from `Translation_ReceivingAnalyticsCategory`, leaving the series, labels, colours and figures exactly as they are · Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalytics.cs
- [x] **T021** [P] Create the translation tests written to fail first: both directions of the three mappings, null for `Outside Service` and `Uninventoried`, null for an unknown app category, and the contains rules for `MMC` and `MMF` · MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Helpers/Translation_ReceivingAnalyticsCategoryTests.cs

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T022** Create `Service_Tool_ReceivingAnalyticsApp`: `GetAppDataAsync` issues the four DAO reads concurrently and fails the dataset as a whole if any one fails; `GetComparisonAsync` calls `IService_InforVisual.GetReceivingAnalyticsHistoryAsync` with the identical bounds and all five scopes on, translates the app categories, and reports `ErpAvailable` false instead of throwing when the ERP half fails; both log through `IService_LoggingUtility` · Module_ShipRec_Tools/Services/Service_Tool_ReceivingAnalyticsApp.cs

**Checkpoint**: the data path compiles, the translation tests pass, and no view exists yet. Nothing user-visible has changed.

## Phase 3: User Story 1 (P1): the dock's own activity by day

**Goal**: App Data is reachable as the second tab and answers how busy the dock was, from the app's own
completed receipts. This is the MVP slice.

**Independent Test**: pick a week of known activity, open App Data, and compare loads, labels and
category totals against a hand count of completed receipts. Then open ERP Data and confirm its charts
and filters behave as before.

**Files**: `View_Tool_ReceivingAnalyticsApp.xaml`, `View_Tool_ReceivingAnalyticsApp.xaml.cs`,
`ViewModel_Tool_ReceivingAnalyticsApp.cs`, `View_Tool_ReceivingAnalytics.xaml`,
`View_Tool_ReceivingAnalytics.xaml.cs`, `ModuleServicesExtensions.cs`

### Tests (write these first, they must fail)

- [x] **T023** [P] [US1] Create the view model tests: default range is the last thirty days, `Load` fills a dataset from a mocked service, `Clear` restores the defaults, an empty range sets `IsEmpty`, a period switch re-buckets without calling the service again, and `QuantityLabel` renders `Quantity (EA)` for a blank unit · MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsAppTests.cs
- [x] **T024** [P] [US1] Create the service tests: `GetAppDataAsync` assembles the four reads into a dataset, one failing read fails the whole dataset with the reason carried, and `GetComparisonAsync` passes the same bounds to the ERP read · MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/Service_Tool_ReceivingAnalyticsAppTests.cs

### Implementation

**Wave 1:**

- [x] **T025** [US1] Create `ViewModel_Tool_ReceivingAnalyticsApp` on `ViewModel_Tool_Base`: filter state and its defaults, `Load` and `Clear` commands, the cached dataset, `IsBusy` and `IsEmpty`, the missing-load-number disclosure text, `QuantityLabel`, day/week/month bucketing (Monday to Sunday weeks, continuous axis with empty buckets present), the loads and labels series, the quantity stack for the selected unit with the dominant unit chosen on load, and `RenderFromLastData` re-rendering from the cache so a period or unit change never re-queries · Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs

**⟶ Wait for Wave 1 to finish, then:**

**Wave 2:**

- [x] **T026** [US1] Create the App Data view as a `UserControl`: the filter bar (range, period, unit selector, Load, Clear) and the 2x2 grid of the four titled cards in the contract's order, each carrying the completed-work statement, with the `How busy were we` card holding the loads and labels chart beside the quantity stack; `x:Bind` only, series replaced wholesale · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml

**⟶ Wait for Wave 2 to finish, then:**

**Wave 3:**

- [x] **T027** [US1] Add the App Data control's code-behind: `InitializeComponent`, an injected view model exposed as a property for `x:Bind`, and nothing else · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml.cs

**⟶ Wait for Wave 3 to finish, then:**

**Wave 4:**

- [x] **T028** [US1] Wrap the page's existing content in a `TabView` without moving it: the current root grid verbatim inside the first `TabViewItem` titled `ERP Data`, a second item titled `App Data` hosting `View_Tool_ReceivingAnalyticsApp`; ERP Data is first and open by default by declaration order, with no `SelectedIndex` binding · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalytics.xaml

**⟶ Wait for Wave 4 to finish, then:**

**Wave 5:**

- [x] **T029** [US1] Inject `ViewModel_Tool_ReceivingAnalyticsApp` into the page and hand it to the hosted control; keep the existing help, filter flyout and export wiring untouched · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalytics.xaml.cs

**⟶ Wait for Wave 5 to finish, then:**

**Wave 6:**

- [x] **T030** [US1] Register the new types beside the existing Receiving Analytics registrations: `Dao_Tool_ReceivingAnalyticsApp` and the service as singletons built from `mySqlConnectionString`, `ViewModel_Tool_ReceivingAnalyticsApp` and both views as transient; nothing added to `App.xaml.cs` · Infrastructure/DependencyInjection/ModuleServicesExtensions.cs

**Checkpoint**: the page has two tabs, App Data loads from the app's own records, and how-busy-were-we is independently testable against a hand count.

## Phase 4: User Story 2 (P2): whether receipts met the PO due date

**Goal**: the same tab shows early, on time and late counts and an on-time percentage coloured against
the configured target.

**Independent Test**: classify a hundred completed receipts by hand and compare the buckets, the
percentage and the colour to the panel.

**Files**: `ViewModel_Tool_ReceivingAnalyticsApp.cs` and `View_Tool_ReceivingAnalyticsApp.xaml`, both
extended in place, plus the view model test file.

### Tests (write these first, they must fail)

- [x] **T031** [US2] Extend the view model tests: the three buckets plus the no-due-date remainder add up to the receipts that had a due date, a receipt completed on its due date lands in `OnTime`, the percentage is early plus on-time over the due-dated total, it is null when the denominator is zero, `HasTarget` is false and `IsOnTimeBelowTarget` false when the settings read returns null, and the remainder is never counted as on time · MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsAppTests.cs

### Implementation

**Wave 1:**

- [x] **T032** [US2] Extend the view model: on-time bucketing from the cached rows on the shared period points, `OnTimePercentage` absent rather than zero or one hundred, the target read through `IService_ShipRecToolsSettings` on load, and `OnTimeTarget`, `HasTarget`, `IsOnTimeBelowTarget`, `OnTimePercentText` and the `NoDueDate` remainder surfaced to the view · Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T033** [US2] Fill the `Were we on time` card: the `Early`, `OnTime` and `Late` stack per period, a readout strip with the percentage beside the target in force, green at or above and red below using the existing `Converter_BoolToColor` with `IsOnTimeBelowTarget`, and the plain, uncoloured percentage shown when `HasTarget` is false using the existing visibility converters · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml

**Checkpoint**: the on-time panel is independently functional and its colouring is trustworthy against a hand count.

## Phase 5: User Story 3 (P3): which suppliers fill the dock

**Goal**: suppliers ranked by labels, quantity or loads, with a cumulative share that reaches one
hundred per cent through a remainder.

**Independent Test**: rank by labels for a month with known supplier activity, compare the order, the
counts and the cumulative share to a hand count, then switch the measure to quantity and confirm the
shares still reach one hundred per cent.

**Files**: `ViewModel_Tool_ReceivingAnalyticsApp.cs` and `View_Tool_ReceivingAnalyticsApp.xaml`, both
extended in place, plus the view model test file.

### Tests (write these first, they must fail)

- [x] **T034** [US3] Extend the view model tests: ranking defaults to labels, switching to quantity sums only the selected unit, suppliers past the leading few collapse into one remainder so the cumulative share reaches one hundred per cent, a blank supplier ranks as `Unknown`, and a single-receipt range still lands on one hundred per cent · MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsAppTests.cs

### Implementation

**Wave 1:**

- [x] **T035** [US3] Extend the view model: build the ranking entries from the cached supplier rows with `Unknown` applied to blanks, rank by the selected measure, compute the running cumulative share, group the tail into one remainder entry, and re-render on a `SupplierMeasure` change without re-querying · Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T036** [US3] Fill the `Who is filling our dock` card: the ranking chart plus a readout strip with the cumulative share of the leading suppliers, and the unit named when the measure is quantity · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml

**Checkpoint**: the supplier panel is independently functional and its shares close to one hundred per cent.

## Phase 6: User Story 4 (P4): how much of the real work the app captured

**Goal**: app and ERP lines over the same dates, with the app's category split beside the ERP's over
the same five names.

**Independent Test**: pick one closed month, take the app's weekly counts from Story 1, take Infor
Visual's counts for the same weeks, and confirm both lines match the underlying records and both splits
name the same five categories.

**Files**: `ViewModel_Tool_ReceivingAnalyticsApp.cs` and `View_Tool_ReceivingAnalyticsApp.xaml`, both
extended in place, plus the view model test file.

### Tests (write these first, they must fail)

- [x] **T037** [US4] Extend the view model tests: both coverage series carry the same dates, both splits name the five ERP names and each sums to one hundred per cent within one decimal place, an ERP-only category appears as its own segment with a zero app share, a failed comparison sets `IsErpUnavailable` and leaves the other panels populated, and the comparison is requested with the same bounds as the app dataset · MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsAppTests.cs

### Implementation

**Wave 1:**

- [x] **T038** [US4] Extend the view model: request the comparison with the loaded filter's bounds, build the two coverage series over the shared date axis, build both category splits over the five ERP names through `Translation_ReceivingAnalyticsCategory`, and set `IsErpUnavailable` when the ERP half fails while the three app panels stay as they are · Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T039** [US4] Fill the `How much does our app actually see` card: the two coverage lines over one axis, the two-sided category mix over the shared five-name axis, and the panel's supporting text stating that the comparison counts app labels against ERP receipt lines on the same dates with the ERP counted as the ERP counts · Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml

**Checkpoint**: all four panels are independently functional and the coverage comparison reads app against ERP.

## Phase 7: Polish (cross-cutting validation, tests and docs)

**Wave 1 — independent (different files):**

- [x] **T040** [P] Create the integration test exercising all four procedures against the test database, extending `StoredProcedureIntegrationTestBase`: a range boundary proves start inclusive and end exclusive, a row with both date columns counts once, a row with only `transaction_date` counts once, a blank supplier groups as `Unknown`, and the missing-load count matches a seeded row · MTM_Receiving_Application.Tests/Integration/Database/StoredProcedures/ReceivingAnalyticsAppStoredProcedureTests.cs
- [x] **T041** [P] Write the module feature update: what the two tabs are, the four panels and what each answers, the alignment rule (one app label against one ERP receipt line), the on-time target key and its 95 default, the four procedures, the known limits inherited from the unchanged ERP query, and the catalog items intentionally left out of this slice · Module_ShipRec_Tools/docs/ReceivingAnalyticsAppDataTab_FeatureUpdate.md
- [x] **T042** [P] Mark delivery status against the analytics catalog so the deferred items stay visibly deferred · Module_ShipRec_Tools/docs/ReceivingAnalytics_TwoTab_AnalyticsCatalog.md
- [x] **T043** [P] Add the changelog entry describing the two tabs, the four App Data panels and the new settings key · CHANGELOG.md

**⟶ Wait for Wave 1 to finish, then:**

- [x] **T044** Tick the four new procedures in the ledger and update its progress count under the existing every-five-tests rule · Database/Database_Deployment/StoredProcedure-Test-Checklist.md

**⟶ Wait for Wave 2 to finish, then:**

- [x] **T045** Run the narrowest slice that can fail this change: `dotnet build MTM_Receiving_Application.slnx`, then `dotnet test MTM_Receiving_Application.Tests` filtered to the ShipRec Tools unit tests and the stored-procedure integration tests; fix what fails · MTM_Receiving_Application.slnx

**⟶ Wait for Wave 3 to finish, then:**

- [x] **T046** Walk the Success Criteria and record the outcome for each: all four panels visible without scrolling at the default 1450 by 900 window, zero mixed-unit totals, both splits naming the five categories and summing to 100 per cent, a thirty day range inside three seconds, the on-time percentage matching a sampled hand count, the same range producing identical figures from either tab, the missing-load number disclosed once, ERP Data unchanged against a pre-change baseline, and ten tab switches with filter changes leaving each tab as last set · specs/002-receiving-analytics-two-tab/tasks.md

## Dependencies & Execution Order

- **Phase 1 (Setup)** → **Phase 2 (Foundational)** → **Phase 3 (US1)** → **Phase 4 (US2)** → **Phase 5 (US3)** → **Phase 6 (US4)** → **Phase 7 (Polish)**.
- Setup: wave 1 (T001 to T006) is six independent files; T007 joins after it because the catalog is generated from the SQL files.
- Foundational: wave 1 (T008 to T016) is nine independent files; wave 2 (T017 to T021) needs those types; T022 joins after the DAO, contract, translation and settings accessor exist.
- US1: tests (T023, T024) run in parallel; T025 builds the view model; T026 then T027 build the control; T028 then T029 wrap the ERP page; T030 registers everything and needs all of them.
- US2 to US4: each phase is its test task, then the view model extension, then the panel in the view. They extend the same two files, so they run in priority order and never as parallel workers.
- Polish: wave 1 (T040 to T043) is four independent files; T044 follows the integration tests; T045 follows everything; T046 follows the build and test run.
