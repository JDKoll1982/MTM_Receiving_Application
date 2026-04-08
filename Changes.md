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
   Assumption: For non-blanket orders, the business-preferred displayed date may need to prioritize the confirmed header promise date over line dates in some cases. I need you to confirm the exact precedence rule you want applied in the card.

10. **Blanket orders**
    Assumption: Blanket orders should still appear on the card, but they should be clearly marked because header desired receive date is not a true shipment schedule.

11. **Closest-to-run sorting**
    Assumption: There is currently no implemented formula in the repo for “closest to run.” I need you to define the rule. Examples:

- lowest available quantity first
- earliest incoming date first
- smallest coverage gap first
- some separate planning signal that is not yet in the codebase

12. **Navigation entry point**
    Assumption: This tool should appear on the Ship/Rec Tools selection screen alongside the other read-only tools, likely under an Analysis-style category rather than Lookup.

13. **Export requirement**
    Assumption: Export is not required for the first pass unless you explicitly want CSV/XLSX from day one.

14. **Performance target**
    Assumption: If you still want a strict performance target, please define it explicitly. The repo currently does not encode a “closest to run” or location-board SLA, and query shape may change depending on expected record counts.

# AI Agent Requirements - MTM Receiving Application

---

## Agent Instructions

> **Before writing any new code**, the AI agent must:
>
> 1. Check `Module_Core` for existing core utilities, data access helpers, and shared business logic.
> 2. Check `Module_Shared` for reusable components, common functions, and shared models.
> 3. Only create new code if equivalent logic does not already exist in those modules.
> 4. If extending or reusing existing logic, prefer modifying the shared module over duplicating code.

---

## User Personas

### Warehouse Manager

- **Goal**: Track incoming material by location and part number.
- **Needs**: A quick, location-based view showing what parts are on hand, where they are stored, and what is arriving soon.
- **Pain Points**: Difficulty knowing which parts are low and when replenishment is arriving.

### Planner

- **Goal**: Schedule and coordinate material runs efficiently.
- **Needs**: Sorted, prioritized material cards showing which parts are closest to being needed for a run.
- **Pain Points**: Lack of visibility into upcoming delivery dates makes scheduling unreliable.

### Procurement Specialist

- **Goal**: Manage PO status and track deliveries.
- **Needs**: Consolidated PO information per part, including open/firmed PO quantities, promised delivery dates, and shipment visibility.
- **Pain Points**: Multiple PO lines for the same part create confusion; needs a single rolled-up view.

## Development Phases

### Phase 1: Data Retrieval & Card Generation

- User enters a Location
- System generates a separate card for each unique Part ID in that Location
- Display all locations with >0 quantity of Part Number (formatted as whole numbers)

**Material Scheduling & Sorting:**

- Cards must be sorted in order of closest to run → furthest away
- Reference CSV files in `docs\InforVisual\DatabaseCSVFiles` for database schema information

**PO & Shipment Information:**

- Material due to come in within the next 30 days
- PO status must be "open" or "firmed" (exclude "closed" or "cancelled")
- For POs with multiple lines using the same part number, combine into a single sum:
  - Qty Received / Qty Ordered / % Complete

**Required Display Information:**

1. Material Due to come in in the next 30 Days
2. For POs with multiple lines that use the part number prescribed by the card, combine them into 1 sum (Qty Receivied / Qty Ordered / % Complete)
3. Dates of Upcoming Shipments

Use the following guidelines on how to get the most accurate shipment date for incoming material:
Guidelines:
When material is ordered: What screens/fields indicate the order has been placed, and what information is available at that stage?
When a PO is created on the header there is a Status field. If that field = Released, that means the PO has been sent to the supplier. At that point you can see our desired delivery date on the header. If the PO has multiple lines with different delivery dates that will be in the body of the PO on each line under the RecvDate column. Once the PO is confirmed, most often we will attach the confirmation to the paper clip at the top of the header. At that point we will also include a date in the Promise Delivery Date field on the header so we know when the order should arrive. Jose will also include a sales order number if available in the Sales Order ID field on the header.
When the vendor confirms a delivery date: Where is the confirmed date entered/updated, and does it update at the PO header level, the line level, or both?
The confirmed delivery date is entered in the Promise Delivery Date field on the header. We typically do not update the dates on the lines within the body of the PO. Note, if there are multiple delivery dates in the body of the PO on each line, we enter the latest date as our Desired Recv Date on the header.
Blanket/Bulk order delivery dates (for example MMC0000364 and MMC0000366): How are delivery dates handled for blanket/bulk orders, and how do we identify what is expected to arrive on a specific date?
Typically in the FOB field on the header, we will put BLANKET so that we know the order is a blanket order, meaning there will be several deliveries off the order before it is closed. The Desired Recv Date field has the final delivery date or the date we plan to have all material consumed by. We do not break out a delivery schedule in the body of the order because we make releases off the planning teams requests. Each release has different quantities and dates that we do not know at the time of PO creation.
Carrier tracking numbers (UPS, USPS, FedEx): I understand VISUAL does not store tracking numbers, but I want to confirm our current process:
Do we already keep a spreadsheet or log of tracking numbers (UPS/USPS/FedEx), possibly on the Expo drive? - NO
If we do, where is it located, and who is responsible for updating it?
If we don’t, would it be useful to have a simple tool or shared spreadsheet where tracking numbers can be entered as soon as they’re received (ideally tied back to PO and line numbers)? - NO
