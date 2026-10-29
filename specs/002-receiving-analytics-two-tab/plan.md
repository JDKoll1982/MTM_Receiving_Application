# Implementation Plan: Receiving Analytics App Data Tab

**Feature Branch**: `002-receiving-analytics-two-tab`
**Spec**: [spec.md](./spec.md)
**Size**: oversized

## Scale note

This plan moves roughly thirty files across five layers: one page, two view models, a new service
and DAO, four stored procedures, a shared category translation, a settings key, DI registration and
four test files. Watch two things. The ERP Data tab is not rewritten, only wrapped, so any change
inside its existing markup is a regression waiting to happen; and the App Data numbers are only as
honest as the load-versus-label definition in the data model, because in today's data one completed
receipt row is one load and one label at the same time.

## Summary

The Receiving Analytics page becomes two tabs. ERP Data is the page as it stands today, wrapped in a
tab and otherwise untouched: the same filters, the same reading of Infor Visual, the same chart. App
Data is new and reports the application's own completed receiving work from MySQL across four
panels: activity by period, on-time against the PO due date, supplier ranking, and a comparison of
the app's coverage and category mix against Infor Visual's.

No charting library is added. The page already draws with LiveCharts through the same
`CartesianChart` control, and every new panel uses it. Every App Data read of MySQL goes through a
new stored procedure, so no application code issues SQL, and nothing is written to either database.

Two business numbers are settled here rather than left open, because the spec only carried a working
default for each. The on-time target is 95 per cent, held as an administrator-editable setting in the
existing settings store rather than in code, and it marks green at or above and red below. Coverage
counts one app label against one ERP receipt line: both sides are grouped by received date and by the
app's normalised category, counts are compared, and an ERP row with no app equivalent (Outside
Service, Uninventoried) counts toward the ERP line only.

## Constitution Check

| Principle | Assessment |
| --- | --- |
| I. MVVM and View Purity | PASS. Both tabs bind with `x:Bind` only. The new control's code-behind does nothing but `InitializeComponent`. All tab state, bucketing and formatting live in the view model. |
| II. Data Access Integrity | PASS. All four MySQL reads go through `Helper_Database_StoredProcedure` into stored procedures. `Dao_Tool_ReceivingAnalyticsApp` is instance-based, takes the connection string, is async, and returns `Model_Dao_Result<T>`. Infor Visual is read through the existing DAO whose constructor enforces `ApplicationIntent=ReadOnly`. No writes anywhere. |
| III. CQRS + Mediator First | **Justified violation**, see Complexity Tracking. |
| IV. Dependency Injection and Modular Boundaries | PASS. DAO singleton, service singleton, view model and view transient, all registered in `ModuleServicesExtensions` beside the existing Receiving Analytics registrations. Nothing added to `App.xaml.cs`. |
| V. Validation, Errors, and Structured Logging | PASS. Both service methods return typed results and log failures; the view model routes exceptions through `IService_ErrorHandler` and reports through `ShowStatus`. |
| VI. Security and Session Discipline | PASS. No person name or employee number is read, so FR-030 holds by construction. No connection string is embedded. The Infor Visual read-only guard is reused, not bypassed. |
| VII. Library-First Reuse | PASS. LiveCharts is already the page's chart library and no package is added. Mapping stays manual, matching every existing module; the constitution's Mapster preference is not exercised anywhere in this repo today. |

### Complexity Tracking

| Violation | Why needed | Simpler alternative rejected |
| --- | --- | --- |
| III. CQRS + Mediator First | `Module_ShipRec_Tools` has no MediatR at all. Its nine services, including this page's existing one, are constructor-injected and called directly by their view models. Adding a handler and a query pipeline for one tab would give a single page two dispatch mechanisms, and would put the App Data path somewhere no other tool in the module lives. | Introducing `IRequestHandler` for the panel query was rejected: it adds MediatR plumbing, a handler, a validator and a pipeline registration to a module that has never used them, for one query that the neighbouring ERP tab performs through a service call. |

Re-checked after Phase 1 with [data-model.md](./data-model.md) and [contracts/](./contracts/) on
disk: unchanged. The design kept every read behind `Helper_Database_StoredProcedure`, added no
register outside `ModuleServicesExtensions`, and put no state outside a view model.

## Project Structure

```text
Module_ShipRec_Tools/
  Views/
    View_Tool_ReceivingAnalytics.xaml          edited  ERP content wrapped in TabView, App tab hosted
    View_Tool_ReceivingAnalytics.xaml.cs       edited  second view model injected, App control bound
    View_Tool_ReceivingAnalyticsApp.xaml       new     the four App Data panels and their filter bar
    View_Tool_ReceivingAnalyticsApp.xaml.cs    new     InitializeComponent only
  ViewModels/
    ViewModel_Tool_ReceivingAnalytics.cs       edited  ERP category literals read from the translation
    ViewModel_Tool_ReceivingAnalyticsApp.cs    new     App Data filters, bucketing, panel series
  Contracts/
    IService_Tool_ReceivingAnalyticsApp.cs     new
    IService_ShipRecToolsSettings.cs           edited  on-time target accessor
  Services/
    Service_Tool_ReceivingAnalyticsApp.cs      new     app dataset plus ERP comparison
    Service_ShipRecToolsSettings.cs            edited  reads the target through Settings.Core
  Data/
    Dao_Tool_ReceivingAnalyticsApp.cs          new     four stored-procedure reads
  Helpers/
    Translation_ReceivingAnalyticsCategory.cs  new     the one app-to-ERP category translation
  Models/
    Model_Tool_ReceivingAnalyticsAppFilter.cs          new
    Model_Tool_ReceivingAnalyticsAppDataset.cs         new
    Model_Tool_ReceivingAnalyticsAppComparison.cs      new
    Model_Tool_ReceivingAnalyticsAppActivityRow.cs     new
    Model_Tool_ReceivingAnalyticsAppOnTimeRow.cs       new
    Model_Tool_ReceivingAnalyticsAppSupplierRow.cs     new
  Settings/
    ShipRecToolsSettingsKeys.cs                edited  ReceivingAnalytics.OnTimeTarget

Module_Settings.Core/Defaults/settings.manifest.json   edited  the target, admin scope, default 95

Infrastructure/DependencyInjection/ModuleServicesExtensions.cs   edited  DAO, service, VM, view

Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/
  sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay.sql          new
  sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay.sql            new
  sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals.sql         new
  sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount.sql new
Database/Database_Deployment/sql-object-catalog.json               regenerated

MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/
  Services/Service_Tool_ReceivingAnalyticsAppTests.cs              new
  ViewModels/ViewModel_Tool_ReceivingAnalyticsAppTests.cs          new
  Helpers/Translation_ReceivingAnalyticsCategoryTests.cs           new
MTM_Receiving_Application.Tests/Integration/Database/StoredProcedures/
  ReceivingAnalyticsAppStoredProcedureTests.cs                     new
```

**Structure Decision**: everything stays inside `Module_ShipRec_Tools`, which already owns both the
page and two MySQL DAOs under `Data/`. The ERP tab is wrapped in place rather than extracted into its
own control, so its markup is not moved and cannot drift. The App Data tab is a separate control with
its own view model, which makes cross-tab filter leakage structurally impossible rather than a thing
the code has to remember.

## Calls

```text
Load button (App Data)
  → ViewModel_Tool_ReceivingAnalyticsApp.LoadAsync
    → IService_Tool_ReceivingAnalyticsApp.GetAppDataAsync(filter)
      → Dao_Tool_ReceivingAnalyticsApp.GetActivityByDayAsync
          → sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay
      → Dao_Tool_ReceivingAnalyticsApp.GetOnTimeByDayAsync
          → sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay
      → Dao_Tool_ReceivingAnalyticsApp.GetSupplierTotalsAsync
          → sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals
      → Dao_Tool_ReceivingAnalyticsApp.GetMissingLoadNumberCountAsync
          → sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount
    → IService_Tool_ReceivingAnalyticsApp.GetComparisonAsync(filter)
      → IService_InforVisual.GetReceivingAnalyticsHistoryAsync (same bounds, all five scopes)
      → Translation_ReceivingAnalyticsCategory.ToErpCategory
    → RenderFromLastData (bucket, translate, build series)

Period or supplier-measure change
  → ViewModel_Tool_ReceivingAnalyticsApp.RenderFromLastData
      (no re-query: the cached per-day rows are re-bucketed and re-rendered)
```

The four app reads run concurrently inside the service. The comparison is a separate service method
because its ERP half can fail on its own, and a failure there must leave the other three panels
working.

## Code pins

`COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR)))` is the load count. One completed
receipt row is one load in this app, because `label_number` is written from the same session counter
as `load_number`. The load and label figures are therefore equal today, and the missing-load-number
count is what tells a reader that. Counting `DISTINCT load_number` instead would merge unrelated
loads across sessions and badly undercount.

`COALESCE(received_date, transaction_date)` is the bucket key everywhere. A row carrying both fields
counts once, on the received date; a row with only the date-only column counts once, on that day.

`COALESCE(weight_quantity, quantity)` is the operator's own number, unrounded when the row has it.
`quantity` is that same entry rounded, so the two are never compared with each other, only used in
that order of preference.

## States

Idle: the tab has not been opened yet, no query has run. shows app-data

Loading: filters accepted, four app reads and the ERP comparison in flight, panels hold their previous
figures behind a progress ring. shows app-data

Loaded: all four panels carry figures for the range. shows app-data

Empty: the range holds no completed receipts, so the panels state that rather than drawing a flat
chart of zeros. The filter bar stays usable. shows app-data

ErpUnavailable: the app panels are complete, the comparison panel reports that the ERP side could not
be read. shows app-data

NoTarget: an administrator has blanked the on-time target, so the percentage is shown plainly with no
pass or fail colour. shows app-data

## Screens

```screen erp-data Existing
title: ERP Data
text: the page exactly as it is today, first tab, open by default
```

```screen app-data New
title: App Data
text: its own filter bar over a 2x2 panel grid (1)
row:
  title: How busy were we
  text: loads and labels per period beside a quantity stack named by its unit (2)
row:
  title: Were we on time
  text: early, on time and late stacked, percentage against the target (3)
row:
  title: Who is filling our dock
  text: supplier ranking with cumulative share and one remainder
row:
  title: How much does our app actually see
  text: app and ERP lines over the same dates beside the five-category split (4)
```

1: range, period, unit and supplier measure, with Load and Clear.
2: two series on one axis, loads and labels, with the stacked quantity chart beside it.
3: the readout carries the target in force, green at or above it and red below.
4: the mix panel normalises both sides to the same five category names.
