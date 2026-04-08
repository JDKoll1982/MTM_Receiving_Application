# AI Implementation Prompt - Location-Based Material Availability Board

Last Updated: 2026-04-08

## Agent Instructions

Before writing any new code:

1. Check `Module_Core` for existing Infor Visual service methods, query loaders, models, and read-only SQL access patterns.
2. Check `Module_Shared` for reusable UI patterns, shared models, and general-purpose view-model behavior.
3. Check `Module_ShipRec_Tools` first for the hosting pattern, because this request is a read-only lookup/analysis tool and the repo already places similar tools there.
4. Only create new code when the needed logic does not already exist.
5. If a new Infor Visual query is required, add it under `Database/InforVisualScripts/Queries/`, wire it through `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`, then expose it through `Module_Core/Services/Database/Service_InforVisualConnect.cs` and `Module_Core/Contracts/Services/IService_InforVisual.cs`.
6. Use `docs/InforVisual/DatabaseCSVFiles/*.csv` as the schema source of truth before changing or adding any Infor Visual SQL.
7. Keep all Infor Visual access read-only. No INSERT, UPDATE, or DELETE against SQL Server.

---

## Goal

Create a new read-only location-based material availability board that lets a user enter a warehouse location and see:

1. One card per unique Part ID currently stored in that location.
2. All other locations where that part currently has quantity greater than zero.
3. Material due to arrive within the next 30 days.
4. Rolled-up PO progress for matching part numbers.
5. Upcoming shipment dates derived from the best available Infor Visual PO date fields.

This is intended for warehouse, planning, and procurement users who need a fast operational view of stock position plus incoming material.

---

## Recommended Feature Placement

Based on the current codebase, this feature should most likely be implemented as a new read-only tool in `Module_ShipRec_Tools`, not inside the guided receiving workflow.

Reasoning:

- The current receiving workflow in `Module_Receiving` is optimized for transactional data entry and label generation.
- The repo already uses `Module_ShipRec_Tools` for read-only warehouse lookup tools such as Outside Service History.
- `Module_ShipRec_Tools` already has the right View, ViewModel, Service, and navigation patterns for a search-driven or analysis-driven tool.

Closest existing pattern to follow:

- `Module_ShipRec_Tools/Contracts/IService_Tool_OutsideServiceHistory.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_OutsideServiceHistory.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_OutsideServiceHistory.cs`
- `Module_ShipRec_Tools/Views/View_Tool_OutsideServiceHistory.xaml`

Related precedent:

- `Module_ShipRec_Tools/docs/CoilInventorySummaryReport_Plan.md`

---

## Existing Code Reuse Confirmed In The Repo

### Infor Visual Service Surface Already Exists

These methods already exist and should be reused before creating anything new:

- `IService_InforVisual.GetPartByIDAsync`
- `IService_InforVisual.GetPOWithPartsAsync`
- `IService_InforVisual.GetPurchaseOrdersByPartAsync`
- `IService_InforVisual.GetReceivingLocationEvidenceAsync`
- `IService_InforVisual.FuzzySearchLocationsAsync`

Primary files:

- `Module_Core/Contracts/Services/IService_InforVisual.cs`
- `Module_Core/Services/Database/Service_InforVisualConnect.cs`
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`

### Existing Models That Matter

- `Model_InforVisualPart` already exposes part-level receiving data such as `PartID`, `QtyOrdered`, `RemainingQuantity`, `DueDate`, and `DefaultLocationId`.
- `Model_InforVisualPO` already exposes `Vendor`, `Status`, `HeaderPromiseDate`, `HeaderDesiredReceiveDate`, and `FreeOnBoard`, with blanket-order detection already built in through `IsBlanketOrder`.
- `Model_InforVisualPartInfo` already exposes `OnHandQty`, `AllocatedQty`, and `AvailableQty` at the DAO/service layer.

Primary files:

- `Module_Core/Models/InforVisual/Model_InforVisualPart.cs`
- `Module_Core/Models/InforVisual/Model_InforVisualPO.cs`
- `Module_Core/Models/InforVisual/Model_InforVisualPartInfo.cs`

### Existing Query Logic Already Relevant

- `01_GetPOWithParts.sql` already returns line-level part rows, selected due dates, header promise dates, header desired receive dates, vendor name, PO status, and blanket-order metadata via `FREE_ON_BOARD`.
- `03_GetPartByNumber.sql` already returns on-hand, allocated, available, and default location data.
- `14_FuzzySearchPOsByPart.sql` already proves the codebase can find purchase orders by part.
- `16_GetReceivingLocationEvidence.sql` already proves the codebase can retrieve current location evidence from Infor Visual inventory.

Primary files:

- `Database/InforVisualScripts/Queries/01_GetPOWithParts.sql`
- `Database/InforVisualScripts/Queries/03_GetPartByNumber.sql`
- `Database/InforVisualScripts/Queries/14_FuzzySearchPOsByPart.sql`
- `Database/InforVisualScripts/Queries/16_GetReceivingLocationEvidence.sql`

### Schema Source Of Truth

Before writing or changing any SQL, validate against:

- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Tables.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_ColumnDetails.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_FKs.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_PKs.csv`
- `docs/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Views.csv`

Important verified facts already known in the repo:

- `CR_PART_LOCATION` is a real SQL Server view and is the right source for bin-level stock.
- `PART_SITE` is site-level only and should not be treated as the primary location breakdown source.
- Infor Visual is read-only from this application.
- Live SQL validation for work-order demand showed that `SITE_ID` and `WAREHOUSE_ID` are not interchangeable in MTM data. Example: live rows used `SITE_ID = MTM2` while `WAREHOUSE_ID = 002`.
- Live SQL validation also confirmed that `REQUIREMENT.PART_ID` can be used to trace a consumed material/component part to the associated parent part on `WORK_ORDER.PART_ID`.

---

## Feature Objective

Build a new read-only tool where the user enters a location and gets a card-based summary for all parts currently in that location.

Each card should represent one unique part found in the entered location and should display:

1. Part ID.
2. Part description.
3. Quantity currently in the entered location.
4. All current locations with quantity greater than zero for that same part.
5. Material due to arrive within the next 30 days.
6. A rolled-up PO progress summary for qualifying incoming POs.
7. Upcoming shipment dates.

---

## User Personas

### Warehouse Manager

- Goal: Track incoming material by location and part number.
- Needs: A quick location-based view showing what parts are on hand, where they are stored, and what is arriving soon.
- Pain Point: Hard to tell which parts are running low and when replenishment is actually expected.

### Planner

- Goal: Schedule material runs more reliably.
- Needs: A sorted and prioritized set of material cards that helps identify what needs attention first.
- Pain Point: Delivery visibility is too fragmented to trust scheduling decisions.

### Procurement Specialist

- Goal: Understand open/firmed PO coverage for a part without manually reading multiple lines.
- Needs: Rolled-up PO progress and upcoming dates in one place.
- Pain Point: Multiple PO lines for the same part create noise and ambiguity.

---

## Functional Requirements

### Input

- User enters a warehouse location.
- The tool validates the location using existing Infor Visual location validation/search patterns where possible.

### Core Output

- System generates one card per unique Part ID currently found in the entered location.
- Each card should include all known locations for that part where quantity is greater than zero.
- Quantities should display as whole numbers in the UI.

### Incoming Material Window

- Show incoming material due within the next 30 days.
- Exclude closed and cancelled POs.
- Include only open or firmed PO states once the exact status-code mapping is confirmed.

### PO Rollup

- If multiple PO lines use the same part number, roll them up into a combined progress summary.
- The combined summary should include:
  - Quantity Received
  - Quantity Ordered
  - Percent Complete

### Shipment Date Guidance

Use the following business guidance when deciding which date to surface as the most useful incoming-material date:

- When a PO is created, header status indicates whether the order has been sent to the supplier.
- Header desired receive date is present at creation time.
- PO lines may carry different `RecvDate` values.
- Once confirmed, the vendor-confirmed date is typically entered at the PO header `Promise Delivery Date` field.
- For blanket orders, the header desired receive date is the final expected completion date, not a release schedule.
- Tracking numbers are out of scope because the business confirmed they are not maintained in a shared log and does not want a new tool or spreadsheet for them.
- For non-blanket orders, start with line-specific dates when those dates come before PO-level dates.
- At the PO/header level, Promise Date should be preferred before Want/Desired Receive Date.
- For blanket orders, use the last received date for the relevant line number and label that value as `Last received on` instead of presenting it as a normal incoming shipment date.

### Sorting Guidance

- Sort cards by earliest incoming date first.

---

## Technical Expectations

### Reuse First

- Reuse `IService_InforVisual` and the existing query-loading pattern before creating any new service surface.
- Reuse `Module_ShipRec_Tools` tool composition patterns for View, ViewModel, and tool-specific service abstractions.
- Reuse existing `Model_Dao_Result` patterns and error handling.

### New Code Likely Required

This feature appears to need new tool-specific code even after reuse:

- New tool model(s) for card rows and aggregated PO/incoming summaries.
- A new tool service in `Module_ShipRec_Tools` that orchestrates the location board.
- A new ViewModel and View under `Module_ShipRec_Tools`.
- At least one new Infor Visual SQL query for "all parts currently at a specific location" and likely one additional aggregation query for incoming PO rollups by part.

### Query Authoring Rules

- Use the CSV schema exports as the source of truth before authoring SQL.
- Prefer indexed joins and restrictive filters.
- Avoid broad scans on large Infor Visual tables.
- Keep SQL Server access read-only.

### UI Pattern Recommendation

- Use card-based display rather than a purely tabular grid.
- Add explicit loading, empty-state, and error-state messaging.
- Favor a ShipRec tool layout similar to existing lookup tools, but adapted for multi-card output.

---

## Suggested Implementation Shape

### Likely Module Placement

- `Module_ShipRec_Tools/Models/`
- `Module_ShipRec_Tools/Contracts/`
- `Module_ShipRec_Tools/Services/`
- `Module_ShipRec_Tools/ViewModels/`
- `Module_ShipRec_Tools/Views/`

### Likely Shared/Core Touch Points

- `Module_Core/Contracts/Services/IService_InforVisual.cs`
- `Module_Core/Services/Database/Service_InforVisualConnect.cs`
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`
- `Database/InforVisualScripts/Queries/`

### Suggested Work Breakdown

1. Confirm the report grain and sort rule.
2. Author or extend the Infor Visual query surface for:

- Parts currently in a specific location.
- All locations with positive quantity for those parts.
- Rolled-up incoming PO data within the next 30 days.

3. Add tool-specific models for the location card and incoming summary.
4. Add a ShipRec tool-specific service that composes the data.
5. Add a ViewModel and View for the new tool.
6. Add the tool entry to the Ship/Rec tool-selection surface.
7. Add tests for aggregation and date-selection logic.

---

## Explicit Non-Goals

- No write-back to Infor Visual.
- No PO editing.
- No receiving workflow changes unless requested separately.
- No carrier tracking number feature.
- No spreadsheet or shared-log tracking workflow.

---

## Assumptions To Clarify

Please confirm or correct these before implementation starts:

1. **Target module**
   Assumption: This should be a new read-only tool in `Module_ShipRec_Tools`, not a new screen inside `Module_Receiving`.

2. **Warehouse scope**
   Assumption: The entered location should be interpreted in warehouse/site `002` unless you want a warehouse selector added.

3. **Card grain**
   Assumption: One card should represent one unique Part ID in the entered location, even if multiple inventory rows or receipt histories contributed to that part being there.

4. **All other locations scope**
   Assumption: “Display all locations with >0 quantity of Part Number” means all current positive-quantity locations for that part in the same warehouse/site, not across all sites.

5. **Quantity formatting**
   Assumption: Quantities should be rounded for display only, while calculation logic should continue using decimals from Infor Visual.

6. **PO status mapping**
   Assumption: “Open or firmed” should include the Infor Visual status codes that the business considers active for incoming material. I need you to confirm whether that means only `F` and `R`, or whether `O` and `P` should also be included.

7. **PO rollup grain**
   Assumption: The rollup should combine all qualifying PO lines for the same part across all qualifying POs in the 30-day window, not create a separate summary per PO.

8. **Upcoming shipment dates output**
   Assumption: Each card should show a list of upcoming distinct dates, not just one single “best” date.

9. **Date precedence**
   Confirmed rule: Start with line-specific dates when those dates come before the PO-specific dates. At the PO/header level, Promise Date takes precedence before Want/Desired Receive Date.

10. **Blanket orders**
    Confirmed rule: Blanket orders should still appear on the card, but instead of using a normal incoming date, use the last received date for the line number and label it `Last received on`.

11. **Closest-to-run sorting**
    Confirmed rule: `Closest to run` means earliest incoming date first.

12. **Navigation entry point**
    Assumption: This tool should appear on the Ship/Rec Tools selection screen alongside the other read-only tools, likely under an Analysis-style category rather than Lookup.

13. **Export requirement**
    Assumption: Export is not required for the first pass unless you explicitly want CSV/XLSX from day one.

14. **Performance target**
    Assumption: If you still want a strict performance target, please define it explicitly. The repo currently does not encode a “closest to run” or location-board SLA, and query shape may change depending on expected record counts.
