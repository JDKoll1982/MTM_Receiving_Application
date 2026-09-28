# Receiving Analytics & Delivery Schedule Tools — Feature Update

- **Last Updated:** 2026-08-31
- **Module:** Ship/Rec Tools (`Module_ShipRec_Tools`)
- **Data Source:** Infor Visual SQL Server (MTMFG) — **read-only**
- **Charting:** LiveCharts2 (WinUI) — `LiveChartsCore.SkiaSharpView.WinUI`

## Overview

Two new Ship/Rec tools recreate the "Receiving Analytics" experience:

1. **Receiving Analytics** — an aggregate chart of receiving **History** (past received
   items) vs **Incoming** (open PO lines due) by date and category (Parts / MMC Coils /
   MMF Flat / Outside Service / Uninventoried), with a History/Incoming view-mode toggle,
   summary stat cards, and an HTML export that embeds a rendered chart image.
2. **Delivery Schedule** — a filterable receiving-schedule grid (per PO line) with a
   compact hotbar (date range + quick search + Search/Clear), a Filters popover (scope,
   delivery-state, and PO-state checkboxes), a real column-header DataGrid, a status-dot
   legend, a result count, and an HTML grid export.

Both read exclusively from the read-only Infor Visual (MTMFG) SQL Server database. No
MySQL writes are involved.

## Data Access

New read-only query scripts under `Database/InforVisualScripts/Queries/`:

| Script | Purpose | Key tables |
| ------ | ------- | ---------- |
| `29_GetDeliveryScheduleLines.sql` | Grid rows with date/search/scope/state filters | `PURCHASE_ORDER`, `PURC_ORDER_LINE`, `VENDOR`, `PART`, `RECEIVER` |
| `30_GetReceivingAnalyticsHistory.sql` | Past-received line counts by date and category | `PURC_ORDER_LINE`, `PURCHASE_ORDER`, `PART` |
| `31_GetReceivingAnalyticsForecast.sql` | Incoming (open PO line) counts by due date and category | `PURC_ORDER_LINE`, `PURCHASE_ORDER`, `PART` |

Category classification (same convention as the rest of the app):

- `PURC_ORDER_LINE.SERVICE_ID IS NOT NULL` → Outside Service
- `PART_ID` NULL or `PART.DETAIL_ONLY = 'Y'` → Uninventoried
- `PART_ID LIKE 'MMC%'` → MMC Coils
- `PART_ID LIKE 'MMF%'` → MMF Flat
- otherwise → Parts

## App Layer

### Module Core (Infor Visual facade)

- `Module_Core/Models/InforVisual/Model_InforVisualDeliveryScheduleLine.cs` — raw grid row.
- `Module_Core/Models/InforVisual/Model_InforVisualDeliveryScheduleFilter.cs` — grid filter/query options.
- `Module_Core/Models/InforVisual/Model_InforVisualReceivingAnalyticsPoint.cs` — raw by-date point
  (used by both History and Forecast datasets).
- `Module_Core/Models/InforVisual/Model_InforVisualReceivingAnalyticsFilter.cs` — analytics
  filter/query options.
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs` — `GetDeliveryScheduleLinesAsync`,
  `GetReceivingAnalyticsHistoryAsync`, and `GetReceivingAnalyticsForecastAsync`
  (READ-ONLY; `ApplicationIntent=ReadOnly` enforced).
- `Module_Core/Contracts/Services/IService_InforVisual.cs` — three new interface methods.
- `Module_Core/Services/Database/Service_InforVisualConnect.cs` — implementations plus
  mock-data fallbacks used in `UseMockData` mode.

### Ship/Rec Tools module

- `Models/Model_Tool_DeliveryScheduleLine.cs` — grid presentation row (formatted date/qty text,
  order date + carrier, status-dot key).
- `Models/Model_Tool_ReceivingAnalyticsPoint.cs` / `Model_Tool_ReceivingAnalyticsStats.cs` /
  `Model_Tool_ReceivingAnalytics.cs` (History + Forecast container).
- `Contracts/IService_Tool_ReceivingAnalytics.cs` (GetAnalyticsAsync + ComputeStatsAsync) /
  `IService_Tool_DeliverySchedule.cs` (SearchAsync).
- `Services/Service_Tool_ReceivingAnalytics.cs` — loads History + Forecast datasets and
  computes stats (total, avg/day, peak day, trend vs previous period).
- `Services/Service_Tool_DeliverySchedule.cs` — maps grid rows to presentation models.
- `ViewModels/ViewModel_Tool_ReceivingAnalytics.cs` — stat cards, History/Incoming view-mode
  toggle, LiveCharts2 series/axes (Line / Bar / Stacked), period (Day / Week / Month),
  category filters, and HTML export with an embedded chart image
  (headless `SKCartesianChart` → base64 PNG).
- `ViewModels/ViewModel_Tool_DeliverySchedule.cs` — quick search, filters, grid + HTML export.
- `Views/View_Tool_ReceivingAnalytics.xaml(.cs)` (chart + toggle) and
  `View_Tool_DeliverySchedule.xaml(.cs)` (compact hotbar + Filters popover + DataGrid).
- `Helpers/Helper_HtmlReportExport.cs` — writes a temp HTML file and opens it in the browser.

## Wiring

- Tool registration in `Service_ShipRecTools_Navigation.RegisterBuiltInTools()`:
  `ReceivingAnalytics` and `DeliverySchedule` (both `Analysis` category).
- Visibility flags and navigation cases in `ViewModel_ShipRecTools_Main`.
- Host `ContentControl`s + activation in `View_ShipRecTools_Main.xaml(.cs)`.
- DI in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  (`AddShipRecToolsModule`).
- LiveCharts2 startup configuration in `App.xaml.cs`:
  `LiveCharts.Configure(settings => settings.AddSkiaSharp());`
- NuGet: `LiveChartsCore.SkiaSharpView.WinUI` 2.0.5 added to `MTM_Receiving_Application.csproj`.

## Queue Behavior (mirrors WIP app `Control_ReceivingAnalytics`)

Reference: `ReceivingAnalytics_SQL_Queue_Sequences.md` (top-level).

- **Analytics default window is `-1 year` to `+6 months`** (History dataset = past received
  items, Incoming/Forecast dataset = open PO lines due). The WIP app always queues the
  analytics with `DateTime.Today.AddYears(-1)` → `AddMonths(6)`, independent of the grid
  date pickers. `ViewModel_Tool_ReceivingAnalytics` defaults `FromDate`/`ToDate` to this
  window so both View Mode datasets have data by default.
- **History query anchors on `PURC_ORDER_LINE.LAST_RECEIVED_DATE`**; Forecast anchors on
  the COALESCE due-date chain (service → desired date, else → promise date). Both exclude
  consignment and internal orders.
- **Queue order is History first, then Forecast** (`Service_Tool_ReceivingAnalytics
  .GetAnalyticsAsync` calls `GetReceivingAnalyticsHistoryAsync` then
  `GetReceivingAnalyticsForecastAsync`), matching the WIP app's single-connection
  Q2 → Q3 sequence (each query here uses a pooled connection).
- **The History/Incoming toggle is client-side**: both datasets are loaded once; toggling
  `SelectedViewModeIndex` re-renders the chart/stats from the cached datasets without
  re-querying Infor Visual.

## Usage

1. Open **Ship/Rec Tools** → choose **Delivery Schedule** to browse the schedule grid.
2. Set the date range and/or type into the **quick search** box (PO, part, supplier,
   carrier at once) and click **Search**. Use **Filters** → **Apply Filters** to toggle
   receiving scope and delivery/PO states.
3. **Scope is inclusion-based**: only the checked categories are shown (Parts / MMC Coils /
   MMF Flat / Outside Service / Uninventoried). Unchecking a category hides it; the
   **Uninventoried (No Part)** toggle controls lines without a part number.
4. **Show Partials Below** (Filters popover, replaces the old Show Partial checkbox) is a toggle:
   when CHECKED it keeps only PARTIAL lines whose received/order percentage is BELOW the entered
   threshold (1-99, default 90); when UNCHECKED it shows only rows with a received qty of 0.
5. The grid shows PO number (with status dot), vendor, due date, part/tag, order/recv/open
   qty, and last receiver; the legend color-codes Closed (blue) / Late / Partial / On Time-Open.
6. Click **Export** to open a table export in the browser.

7. Choose **Receiving Analytics** to see the aggregate chart and stat cards.
7. Use the **View Mode** toggle to switch between **History** (past received items) and
   **Incoming** (open PO lines due). The toggle re-renders instantly from the loaded
   datasets — no re-query is issued.
7. Switch Chart Type (Line / Bar / Stacked) and View Period (Day / Week / Month);
   check **Compare Previous Period** to show the trend stat.
7. Click **Export Chart** to open an HTML export with the rendered chart image.

## Tests

- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Services/`
  - `Service_Tool_ReceivingAnalyticsTests.cs`
  - `Service_Tool_DeliveryScheduleTests.cs`
- `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/`
  - `ViewModel_Tool_ReceivingAnalyticsTests.cs`
  - `ViewModel_Tool_DeliveryScheduleTests.cs`
- `MTM_Receiving_Application.Tests/LiveChartsTestInitializer.cs` — process-wide
  `LiveCharts.Configure(...)` for headless chart-image rendering in tests.
