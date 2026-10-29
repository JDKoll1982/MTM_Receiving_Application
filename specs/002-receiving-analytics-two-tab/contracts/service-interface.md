# Service Interface: Receiving Analytics App Data Tab

This file is the C# contract a consumer or a test codes against for the App Data tab. It records the
exact type and member names pinned in the spec, so a test or another service can bind to them without
reopening the spec.

Namespaces: `Model_Dao_Result<T>` lives in `MTM_Receiving_Application.Module_Core.Models.Core`. The
filter, dataset, comparison and row models live in `MTM_Receiving_Application.Module_ShipRec_Tools.Models`.
The service, DAO, settings and translation types live under `MTM_Receiving_Application.Module_ShipRec_Tools`.

## IService_Tool_ReceivingAnalyticsApp

The new service reads the app's own records from MySQL for the three app panels and reads Infor Visual
for the comparison panel. It returns typed results and never throws for an expected failure.

| Method | Returns |
| --- | --- |
| `GetAppDataAsync(Model_Tool_ReceivingAnalyticsAppFilter filter)` | `Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppDataset>` |
| `GetComparisonAsync(Model_Tool_ReceivingAnalyticsAppFilter filter)` | `Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppComparison>` |

Both methods are async and take the same filter. The filter carries the range bounds only; period,
unit and supplier measure stay in the view model and are never sent to the service.

```csharp
namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

public interface IService_Tool_ReceivingAnalyticsApp
{
    Task<Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppDataset>> GetAppDataAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    );

    Task<Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppComparison>> GetComparisonAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    );
}
```

`GetAppDataAsync` runs the four DAO reads concurrently and fails the whole dataset if any one of them
fails. `GetComparisonAsync` reads the ERP side through
`IService_InforVisual.GetReceivingAnalyticsHistoryAsync` with the identical bounds and all five
category scopes enabled. Its ERP half can fail on its own, and that failure is reported on the
comparison panel alone. The Infor Visual connection is opened with `ApplicationIntent=ReadOnly`.

## Dao_Tool_ReceivingAnalyticsApp

Instance-based MySQL DAO that takes the connection string in its constructor and reads through
`Helper_Database_StoredProcedure`. Every method is `virtual`, async, and returns a typed
`Model_Dao_Result<T>`. No raw SQL appears in the DAO.

| Method | Returns |
| --- | --- |
| `GetActivityByDayAsync(DateTime startDate, DateTime endDate)` | `Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppActivityRow>>` |
| `GetOnTimeByDayAsync(DateTime startDate, DateTime endDate)` | `Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppOnTimeRow>>` |
| `GetSupplierTotalsAsync(DateTime startDate, DateTime endDate)` | `Model_Dao_Result<List<Model_Tool_ReceivingAnalyticsAppSupplierRow>>` |
| `GetMissingLoadNumberCountAsync(DateTime startDate, DateTime endDate)` | `Model_Dao_Result<int>` |

Each method passes only the two date bounds to its stored procedure. The start bound is inclusive and
the end bound is exclusive; the caller supplies an end that is one instant after the last included
moment, so a range ending today passes the start of tomorrow as the end.

## DAO parameter-name rule

`Helper_Database_StoredProcedure` adds the `p_` prefix to dictionary keys. Callers pass `start_date`
and `end_date` without the `p_` prefix, and the helper resolves them to `@p_start_date` and
`@p_end_date`. A DAO method therefore builds its parameters as `{ "start_date", startDate }` and
`{ "end_date", endDate }`, never `{ "p_start_date", ... }`.

## IService_ShipRecToolsSettings accessor

| Method | Returns |
| --- | --- |
| `GetReceivingAnalyticsOnTimeTargetAsync()` | `Task<decimal?>` |

A null return means no target is configured. The target is stored under the
`ReceivingAnalytics.OnTimeTarget` settings key with a default of 95.

## Translation_ReceivingAnalyticsCategory

A single static type in `Module_ShipRec_Tools/Helpers` that owns the category vocabulary for both
tabs. The App Data procedures pass `part_type` through untranslated; normalisation happens only in
this type.

| List | Values, in order |
| --- | --- |
| ERP category list | `Parts`, `MMC Coils`, `MMF Flat`, `Outside Service`, `Uninventoried` |
| App category list | `Standard`, `Coil`, `Sheet` |

| Method | Returns |
| --- | --- |
| `ToErpCategory(string appCategory)` | `string?` |
| `ToAppCategory(string erpCategory)` | `string?` |

Mapping:

| App category | ERP category |
| --- | --- |
| `Standard` | `Parts` |
| `Coil` | `MMC Coils` |
| `Sheet` | `MMF Flat` |

Null contract:

- `ToAppCategory("Outside Service")` returns null, and `ToAppCategory("Uninventoried")` returns
  null. Those two ERP categories have no app counterpart.
- Every app category maps to an ERP category, so `ToErpCategory` returns null only for an input that
  is not one of the three app category names.

Pinned category rules:

- App category rule: `MMC` for Coil, `MMF` for Sheet, matched by contains.
- ERP category rule: `MMC` for MMC Coils, `MMF` for MMF Flat, matched by starts with.

## ViewModel_Tool_ReceivingAnalyticsApp observable surface

The view model inherits `ViewModel_Tool_Base` and drives the App Data tab with `x:Bind`. It owns the
filter state, the cached per-day rows and the rendered series. A test binds to the following
properties and commands.

### Filters

| Property | Type | Default |
| --- | --- | --- |
| `FromDate` | `DateTimeOffset` | today minus 29 days |
| `ToDate` | `DateTimeOffset` | today |
| `Period` | `string` | `Day` |
| `SelectedPeriodIndex` | `int` | 0 for `Day` |
| `SelectedUnit` | `string` | the dominant unit in range, or blank when two units tie |
| `SupplierMeasure` | `string` | `Labels` |

`Period` holds `Day`, `Week` or `Month`. `SelectedPeriodIndex` stays in sync with `Period`.
`SupplierMeasure` holds `Labels`, `Quantity` or `Loads`.

### Series and axes

| Property | Type | Panel |
| --- | --- | --- |
| `LoadsLabelsSeries` | `IReadOnlyList<ISeries>` | How busy were we, loads and labels chart |
| `LoadsLabelsXAxes` | `IEnumerable<ICartesianAxis>` | How busy were we, loads and labels chart |
| `LoadsLabelsYAxes` | `IEnumerable<ICartesianAxis>` | How busy were we, loads and labels chart |
| `QuantitySeries` | `IReadOnlyList<ISeries>` | How busy were we, quantity stack |
| `QuantityXAxes` | `IEnumerable<ICartesianAxis>` | How busy were we, quantity stack |
| `QuantityYAxes` | `IEnumerable<ICartesianAxis>` | How busy were we, quantity stack |
| `OnTimeSeries` | `IReadOnlyList<ISeries>` | Were we on time |
| `OnTimeXAxes` | `IEnumerable<ICartesianAxis>` | Were we on time |
| `OnTimeYAxes` | `IEnumerable<ICartesianAxis>` | Were we on time |
| `SupplierSeries` | `IReadOnlyList<ISeries>` | Who is filling our dock |
| `SupplierXAxes` | `IEnumerable<ICartesianAxis>` | Who is filling our dock |
| `SupplierYAxes` | `IEnumerable<ICartesianAxis>` | Who is filling our dock |
| `CoverageSeries` | `IReadOnlyList<ISeries>` | How much does our app actually see, two coverage lines |
| `CoverageXAxes` | `IEnumerable<ICartesianAxis>` | How much does our app actually see, two coverage lines |
| `CoverageYAxes` | `IEnumerable<ICartesianAxis>` | How much does our app actually see, two coverage lines |
| `MixAppSeries` | `IReadOnlyList<ISeries>` | How much does our app actually see, app split |
| `MixErpSeries` | `IReadOnlyList<ISeries>` | How much does our app actually see, ERP split |
| `MixXAxes` | `IEnumerable<ICartesianAxis>` | How much does our app actually see, shared category axis |
| `MixYAxes` | `IEnumerable<ICartesianAxis>` | How much does our app actually see, shared category axis |

### Readout text and target

| Property | Type | Meaning |
| --- | --- | --- |
| `MissingLoadNumberText` | `string` | disclosure of how many receipts in range had no load number |
| `QuantityLabel` | `string` | `Quantity (LB)` style label with the unit uppercased |
| `OnTimePercentText` | `string` | the formatted on-time percentage |
| `OnTimeTarget` | `decimal?` | the configured target, null when none |
| `HasTarget` | `bool` | true when a target is configured |
| `IsOnTimeBelowTarget` | `bool` | true when the percentage is below the target, false when no target is set |

### Visibility flags

| Property | Type | Meaning |
| --- | --- | --- |
| `IsBusy` | `bool` | inherited from `ViewModel_Tool_Base`, progress ring while loading |
| `IsEmpty` | `bool` | explicit empty state when the range holds no completed receipts |
| `IsErpUnavailable` | `bool` | true when the comparison ERP side could not be read |

### Commands

| Command | Type | Behaviour |
| --- | --- | --- |
| `Load` | `IAsyncRelayCommand` | runs the four app reads and the comparison |
| `Clear` | `IRelayCommand` | resets the filters to their defaults and clears the panels |
