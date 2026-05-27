# Changes Needed

## Task A: Separate Mock Data and Visual Data

### User-Confirmed Decisions

The following Task A decisions were explicitly confirmed with the user and should be treated as fixed requirements for the later implementation pass:

- Scope covers all Customer Pull n' Pack screens, not just the report and queue.
- Mock parity must cover both:
	- the same feature projection contract exposed to the UI, including filtering, sorting, selection, and status behavior
	- the underlying raw data shape and fields expected from the live Infor Visual path
- The module-specific mock-data split must use dedicated static and dedicated runtime files.
- Data-source selection must happen at module or view activation through a resolver-style service, not inside individual DAOs or feature services.

### Current Repo Findings

The current codebase already shows a partial mock-data path for Customer Pull n' Pack, but the selection of mock versus live data is still scattered and not isolated at a service-boundary seam.

#### Confirmed Current Toggle Source

- `IService_AppSettings.GetUseInforVisualMockData()` is the current feature toggle used to enable Infor Visual mock mode.
- `Service_AppSettings` reads that toggle from `AppSettings:UseInforVisualMockData`.

#### Confirmed Current Coupling Points

- `Dao_CustomerPullPackDemand` contains an internal `UseMockData` branch and directly switches between:
	- the live SQL Server Infor Visual query path
	- the JSON-backed mock catalog path
- `Dao_CustomerPullPackWaitlist` also contains an internal `UseMockData` branch and switches between:
	- MTM-managed MySQL stored procedure persistence
	- a mock queue synthesized from the shared Infor Visual mock catalog
- `ViewModel_Tool_CustomerPullPackReport` also reads the same mock toggle directly for customer discovery and default-customer behavior.

Current conclusion: the mock/live decision is duplicated across viewmodel and DAO layers instead of being resolved once at a module-owned service boundary.

#### Confirmed Current Host and Screen Ownership

- `View_ShipRecTools_Main` hosts the currently wired Customer Pull n' Pack views.
- The currently DI-hosted main Customer Pull n' Pack surfaces are:
	- `View_Tool_CustomerPullPackReport`
	- `View_Tool_CustomerPullPackQueue`
- Additional Customer Pull n' Pack screens present in the module and included in Task A scope are:
	- `View_Tool_CustomerPullPackPullList`
	- `View_Tool_CustomerPullPackFloorCopy`
	- `View_Tool_CustomerPullPackDefaults`

Current conclusion: source resolution should be established before these Customer Pull n' Pack screens begin using feature data, and all screens in this workflow should consume the same resolved source mode.

#### Confirmed Current Mock Data Storage

- The shared Infor Visual mock catalog service is `Service_InforVisualMockDataCatalog`.
- It currently loads from shared files under `Module_Settings.Core/Defaults/`:
	- `inforvisual-mock-data.json`
	- `inforvisual.mock-runtime.json`
- Customer Pull n' Pack mock rows are currently embedded inside that shared catalog model via:
	- `CustomerPullPackDemandRows`
	- `CustomerPullPackLocationRows`

Current conclusion: Customer Pull n' Pack mock data is currently co-located with unrelated Infor Visual mock datasets instead of being owned by a Customer Pull n' Pack-specific mock-data asset.

### Task A Requirements for the Later Implementation Pass

#### 1. Introduce a Resolved Source Boundary

The later implementation must create separate live-data and mock-data service paths for Customer Pull n' Pack and resolve the active path when the Customer Pull n' Pack workflow becomes active.

Required behavior:

- When the Customer Pull n' Pack workflow opens, the application must determine once whether the workflow is operating in mock mode or live Visual mode.
- That result must be applied through a resolver-style service boundary.
- Downstream Customer Pull n' Pack screens must consume the resolved service set rather than each DAO or viewmodel re-checking `GetUseInforVisualMockData()`.
- The mock/live decision must not remain embedded inside `Dao_CustomerPullPackDemand` and `Dao_CustomerPullPackWaitlist` after the refactor is complete.

#### 2. Separate Mock and Live Implementations Completely

The later implementation must split the current mixed behavior into distinct mock and live implementations.

Required outcome:

- One service path must own live Infor Visual demand retrieval and any live-backed location or status projection behavior.
- A separate service path must own mock demand retrieval and mock-backed location or status projection behavior.
- Waitlist behavior must also follow the same separation principle so the active workflow does not mix live demand rules with mock queue rules or vice versa.
- The screen layer should depend on a common feature contract, while the resolver chooses which implementation fulfills that contract.

#### 3. Enforce Mock-to-Live Parity

The mock-data implementation and mock JSON content must match what the live Visual path would provide.

This parity requirement includes both levels:

- UI-facing contract parity:
	- same feature models
	- same required fields
	- same filtering and sorting expectations
	- same selection and waitlist-state behavior
	- same status-note and location-option behavior used by Customer Pull n' Pack screens
- Source-shape parity:
	- mock records must preserve the same field meanings and shape assumptions that the live query path relies on
	- mock data must not omit or rename fields that are required by the live projection path
	- if live logic derives fields such as waitlist displays, selectable locations, or quantity/status summaries, the mock path must support the same derivation rules

Current implication for future implementation: mock JSON should be treated as a contract-backed research fixture, not as ad hoc demo data.

#### 4. Move Customer Pull n' Pack Mock Data Out of the Shared Catalog Files

Customer Pull n' Pack mock data must be split out of the shared Infor Visual mock JSON files into module-owned files.

Required outcome:

- Create dedicated static mock-data files for Customer Pull n' Pack.
- Create dedicated runtime mock-data files for Customer Pull n' Pack.
- Stop storing Customer Pull n' Pack demand and location rows inside the shared `inforvisual-mock-data.json` and `inforvisual.mock-runtime.json` files once the feature-specific files exist.
- Keep the new files scoped to this workflow so future edits do not require navigating the large shared Infor Visual catalog.

#### 5. Keep the Feature Contract Stable Across All Customer Pull n' Pack Screens

Because Task A scope includes all Customer Pull n' Pack screens, the later implementation must ensure the resolved source boundary is shared consistently across:

- report loading
- waitlist queue loading
- pull list workflows
- floor copy workflows
- defaults workflows where feature data shape influences screen behavior

The later implementation should not allow one screen to resolve mock mode independently from another screen in the same Customer Pull n' Pack session.

### Relevant Code Anchors for the Later Implementation Pass

The following code anchors were confirmed during research and should be treated as the starting points for the eventual refactor:

- `Module_Core/Contracts/Services/IService_AppSettings.cs`
- `Module_Core/Services/Service_AppSettings.cs`
- `Module_Core/Contracts/Services/IService_InforVisualMockDataCatalog.cs`
- `Module_Core/Services/Database/Service_InforVisualMockDataCatalog.cs`
- `Module_Core/Models/InforVisual/Model_InforVisualMockDataCatalog.cs`
- `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackDemand.cs`
- `Module_ShipRec_Tools/Data/CustomerPullPack/Dao_CustomerPullPackWaitlist.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueueHandler.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`
- `Module_Settings.Core/Defaults/inforvisual-mock-data.json`
- `Module_Settings.Core/Defaults/inforvisual.mock-runtime.json`

### Task A Acceptance Criteria for the Later Implementation Pass

- Customer Pull n' Pack resolves its active data source once when the workflow becomes active.
- Mock and live paths are implemented as separate services behind a shared feature contract.
- The active Customer Pull n' Pack screens do not branch individually on `GetUseInforVisualMockData()`.
- Customer Pull n' Pack mock JSON lives in module-owned static and runtime files rather than the shared Infor Visual catalog files.
- Mock data preserves the same field shape and feature behavior expectations as the live Visual path.
- All Customer Pull n' Pack screens in scope consume the same resolved data-source mode during a session.

## Goal

Update Customer Pull n' Pack so the CO table supports selecting multiple customer order lines and so line fulfillment status can be derived from available finished goods inventory in a consistent, oldest-first allocation pass.

## Research and Validation Task

Determine whether Infor Visual already exposes a usable concept of completed or partially fulfilled CO lines by tracing customer order lines to related work orders and verifying whether work order completion status can be reused instead of introducing MTM-only logic.

Use the following schema reference files as the required discovery inputs before implementation:

```text
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_CheckConstraints.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_ColumnDetails.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_DefaultConstraints.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_FKs.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Indexes.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_PKs.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_TableRowCounts.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Tables.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Triggers.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_UniqueConstraints.csv
docs/development/InforVisual/DatabaseCSVFiles/MTMFG_Schema_Views.csv
```

The research output must answer these questions before implementation is finalized:

- Which Visual table or tables represent CO lines, related work orders, and work order status.
- Which foreign key path connects a CO line to one or more work orders.
- Whether Visual already has a native status or quantity field that matches the required Complete or Partially Filled behavior.
- Which date or sequencing field should be treated as the authoritative definition of Oldest Added.
- Whether the required status logic should reuse Visual data, derive a transient UI status, or be stored in MTM-managed data.

Do not invent table names, columns, or relationships. Confirm them from the CSV files before any query or code design is considered complete.

## Research Findings

### Confirmed Source Coverage

The following findings are grounded in the CSV exports listed above, with the strongest evidence coming from `MTMFG_Schema_Tables.csv`, `MTMFG_Schema_FKs.csv`, `MTMFG_Schema_PKs.csv`, `MTMFG_Schema_Indexes.csv`, `MTMFG_Schema_TableRowCounts.csv`, `MTMFG_Schema_CheckConstraints.csv`, `MTMFG_Schema_DefaultConstraints.csv`, `MTMFG_Schema_ColumnDetails.csv`, and `MTMFG_Schema_Views.csv`.

### Confirmed Table Mapping

- `CUSTOMER_ORDER` is the customer order header table.
	- Confirmed key: `ID`
	- Confirmed date/status fields relevant to this change: `ORDER_DATE`, `CREATE_DATE`, `DESIRED_SHIP_DATE`, `STATUS`, `STATUS_EFF_DATE`, `LAST_SHIPPED_DATE`
- `CUST_ORDER_LINE` is the customer order line table.
	- Confirmed composite key: `CUST_ORDER_ID`, `LINE_NO`
	- Confirmed quantity/status fields relevant to this change: `ORDER_QTY`, `USER_ORDER_QTY`, `ALLOCATED_QTY`, `FULFILLED_QTY`, `TOTAL_SHIPPED_QTY`, `LINE_STATUS`, `SHIP_ORDER_STATUS`, `STATUS_EFF_DATE`, `DESIRED_SHIP_DATE`, `PROMISE_DATE`
- `WORK_ORDER` is the work order table.
	- Confirmed composite key: `TYPE`, `BASE_ID`, `LOT_ID`, `SPLIT_ID`, `SUB_ID`
	- Confirmed quantity/status fields relevant to this change: `STATUS`, `STATUS_EFF_DATE`, `CREATE_DATE`, `CLOSE_DATE`, `DESIRED_QTY`, `RECEIVED_QTY`, `ALLOCATED_QTY`, `FULFILLED_QTY`, `PERCENT_COMPL`, `MAX_QTY_COMPLETE`

### Confirmed Relationship Evidence

- `CUST_ORDER_LINE.CUST_ORDER_ID -> CUSTOMER_ORDER.ID` is a declared foreign key.
- `CO_PRODUCT` has a declared foreign key path to `WORK_ORDER` using `WORKORDER_TYPE`, `WORKORDER_BASE_ID`, `WORKORDER_LOT_ID`, `WORKORDER_SPLIT_ID`, and `WORKORDER_SUB_ID`.
- `CO_PRODUCT` does not contain `CUST_ORDER_ID` or `LINE_NO`, so it does not provide a direct, line-level bridge back to customer order lines.
- `CUST_ORDER_ALLOC` contains both demand-side fields and supply-side fields:
	- Demand-side: `ORDER_ID`, `ORDER_LINE_NO`, `ORDER_DEL_NO`, `DEMAND_KEY`, `DEMAND_TYPE`
	- Supply-side: `SUPPLY_KEY`, `SUPPLY_TYPE`, `BASE_ID`, `LOT_ID`, `SPLIT_ID`, `FINISH_DATE`
- No declared foreign key rows were found for `CUST_ORDER_ALLOC` in `MTMFG_Schema_FKs.csv`.
- `CUST_ORDER_ALLOC` does not contain `WORK_ORDER.TYPE` or `WORK_ORDER.SUB_ID`, so a full FK-backed join from `CUST_ORDER_ALLOC` to `WORK_ORDER` cannot be proven from the CSV exports alone.
- `WORK_ORDER` contains `WBS_CUST_ORDER_ID`, which suggests a customer-order-level reference, but no line-level foreign key path from `WORK_ORDER` back to `CUST_ORDER_LINE` was confirmed from the CSV exports.

### Candidate Bridge Assessment

- `CUST_ORDER_ALLOC` is the strongest candidate for a CO-line-to-supply allocation bridge because it stores both order-side identifiers and work-order-like supply identifiers in the same row set.
- `CO_PRODUCT` is not currently a strong primary bridge candidate for this feature because the row-count export shows only `1` row in the current dataset snapshot and the table carries no customer order identifiers.
- The target objects were not found in `MTMFG_Schema_Views.csv`, so this investigation is dealing with base tables rather than SQL Server views.

### Native Quantity and Status Signals Already Present in Visual

The schema already exposes quantity-oriented fulfillment fields that may be reusable as inputs to the final UX:

- `CUST_ORDER_LINE.ALLOCATED_QTY`
- `CUST_ORDER_LINE.FULFILLED_QTY`
- `CUST_ORDER_LINE.TOTAL_SHIPPED_QTY`
- `CUST_ORDER_LINE.SHIP_ORDER_STATUS`
- `CO_PRODUCT.ALLOCATED_QTY`
- `CO_PRODUCT.FULFILLED_QTY`
- `CO_PRODUCT.RECEIVED_QTY`
- `CO_PRODUCT.LINE_STATUS`
- `WORK_ORDER.ALLOCATED_QTY`
- `WORK_ORDER.FULFILLED_QTY`
- `WORK_ORDER.RECEIVED_QTY`
- `WORK_ORDER.STATUS`
- `WORK_ORDER.PERCENT_COMPL`
- `WORK_ORDER.MAX_QTY_COMPLETE`

These fields prove that Visual already tracks quantity and status concepts related to fulfillment. However, the CSV exports do not prove that any single field exactly matches the requested UI meanings of `Complete` and `Partially Filled` for this tool.

### Status-Semantics Limitation

- `MTMFG_Schema_CheckConstraints.csv` shows constraint rows for `CUST_ORDER_LINE` and `WORK_ORDER`, but the definitions are `NULL`.
- `MTMFG_Schema_DefaultConstraints.csv` confirms defaults exist for fields such as `CUST_ORDER_LINE.LINE_STATUS`, `CUST_ORDER_LINE.FULFILLED_QTY`, `CO_PRODUCT.LINE_STATUS`, and `WORK_ORDER` fulfillment-related fields, but the default expressions are also `NULL` in the export.
- Because those definitions are unavailable in the CSV exports, the allowed enum values and exact meaning of `LINE_STATUS`, `SHIP_ORDER_STATUS`, and `WORK_ORDER.STATUS` cannot be decoded from this research set alone.

### Oldest Added Research Result

No clearly named line-level `CREATE_DATE` or `ADDED_DATE` field was found on `CUST_ORDER_LINE`.

The closest candidate fields are:

- Header-level chronology candidates:
	- `CUSTOMER_ORDER.CREATE_DATE`
	- `CUSTOMER_ORDER.ORDER_DATE`
- Line-level scheduling candidates:
	- `CUST_ORDER_LINE.DESIRED_SHIP_DATE`
	- `CUST_ORDER_LINE.PROMISE_DATE`
	- `CUST_ORDER_LINE.STATUS_EFF_DATE`
- Allocation-level sequencing candidates:
	- `CUST_ORDER_ALLOC.WANT_DATE`
	- `CUST_ORDER_ALLOC.FINISH_DATE`

Current conclusion: the CSV exports do not identify a single authoritative column that unquestionably means `Oldest Added` at the CO-line level. That definition remains a business decision unless the existing application query layer already standardizes it elsewhere.

### Query and Scale Implications

- Row counts in the current export snapshot:
	- `CUST_ORDER_LINE`: `190021`
	- `WORK_ORDER`: `107740`
	- `CUSTOMER_ORDER`: `89111`
	- `CUST_ORDER_ALLOC`: `85501`
	- `CO_PRODUCT`: `1`
- Index evidence relevant to future query design:
	- `CUST_ORDER_LINE` is indexed by primary key `CUST_ORDER_ID, LINE_NO` and also has a nonclustered index including `PART_ID`, `LINE_STATUS`, `DESIRED_SHIP_DATE`.
	- `CUST_ORDER_ALLOC` has nonclustered indexes including `ORDER_ID`, `ORDER_LINE_NO`, `BASE_ID`, `LOT_ID`, `SPLIT_ID`, and related schedule/demand fields.
	- `WORK_ORDER` has a composite primary key and a nonclustered index on `STATUS`.

### Research Answers to the Required Questions

- Which Visual table or tables represent CO lines, related work orders, and work order status.
	- Confirmed CO line table: `CUST_ORDER_LINE`
	- Confirmed CO header table: `CUSTOMER_ORDER`
	- Confirmed work order table: `WORK_ORDER`
	- Confirmed candidate bridge/allocation tables: `CUST_ORDER_ALLOC`, `CO_PRODUCT`
- Which foreign key path connects a CO line to one or more work orders.
	- No single declared FK-backed path from `CUST_ORDER_LINE` to `WORK_ORDER` was confirmed from the CSV exports.
	- The strongest candidate derived path is `CUST_ORDER_LINE (CUST_ORDER_ID, LINE_NO)` -> `CUST_ORDER_ALLOC (ORDER_ID, ORDER_LINE_NO)` -> supply-side work-order-like fields in `CUST_ORDER_ALLOC`, but this is not fully proven because the CSV exports show no declared FK for `CUST_ORDER_ALLOC` and the table lacks `WORK_ORDER.TYPE` and `WORK_ORDER.SUB_ID`.
	- `CO_PRODUCT` is FK-backed to `WORK_ORDER`, but it is not FK-backed to `CUST_ORDER_LINE` and does not carry customer order identifiers.
- Whether Visual already has a native status or quantity field that matches the required Complete or Partially Filled behavior.
	- Visual clearly has native quantity and status fields related to allocation, fulfillment, shipment, and work-order completion.
	- The CSV exports do not prove that any single native field exactly equals the required Customer Pull n' Pack statuses `Complete` or `Partially Filled`.
- Which date or sequencing field should be treated as the authoritative definition of Oldest Added.
	- Not yet proven from the CSV exports.
	- The strongest header-level candidates are `CUSTOMER_ORDER.CREATE_DATE` and `CUSTOMER_ORDER.ORDER_DATE`.
	- The strongest line-level scheduling candidates are `CUST_ORDER_LINE.DESIRED_SHIP_DATE` and `CUST_ORDER_LINE.PROMISE_DATE`.
	- The strongest allocation-level candidate is `CUST_ORDER_ALLOC.WANT_DATE`.
- Whether the required status logic should reuse Visual data, derive a transient UI status, or be stored in MTM-managed data.
	- Current evidence supports reusing Visual quantity/status fields as inputs.
	- Current evidence does not support treating a Visual field as a fully proven replacement for the requested UI state machine.
	- Until the join path and enum meanings are proven, `Partially Filled` should be treated as MTM-derived UI behavior, not a Visual-native persisted status.

## Functional Change Summary

### 1. CO Table Selection

- Allow the user to select multiple CO lines in the CO table.
- Preserve the current single-line workflow behavior for actions that still require exactly one line, but allow batch-aware workflows to operate on the selected set.

### 2. Automatic Fulfillment Allocation

Use FG On Hand Total to automatically determine which CO lines can be satisfied.

Apply the allocation in this exact order:

1. Sort eligible CO lines by Oldest Added first.
2. Start with the full FG On Hand Total.
3. For each CO line in order, compare the remaining FG On Hand Total to that line's SHIP QTY.
4. If the remaining FG On Hand Total is greater than or equal to the line's SHIP QTY, mark that line as Complete and subtract the full SHIP QTY from the remaining FG On Hand Total.
5. Continue processing lines until the remaining FG On Hand Total reaches zero or until the next full subtraction would make it negative.
6. If inventory remains but is not enough to fully satisfy the next oldest line, mark that next line as Partially Filled and set its satisfied quantity to the remaining FG On Hand Total.
7. After marking one line as Partially Filled, stop the allocation pass because the remaining FG On Hand Total will be zero.

## Required Data and Status Changes

- Add a new CO line status: Partially Filled.
- Add a Qty Satisfied field or column on the CO line projection used by this tool.
- Set Qty Satisfied to the amount allocated from FG On Hand Total.
- For fully satisfied lines, Qty Satisfied must equal SHIP QTY.
- For a partially satisfied line, Qty Satisfied must be greater than zero and less than SHIP QTY.
- For untouched lines, Qty Satisfied must remain zero or null based on the final data design.

## Business Rules

- Never allow the FG On Hand Total calculation to go negative.
- Only one line may become Partially Filled in a single allocation pass.
- Complete and Partially Filled must be driven by quantity allocation, not by user guesswork.
- If Visual already provides an authoritative completion signal that matches the requirement, document whether that signal replaces or supplements the MTM allocation logic.
- If Visual does not provide a reusable status, treat this as MTM-managed derived behavior and document where that status lives.
- Based on current CSV research, reuse Visual fulfillment quantities as evidence where helpful, but keep the new `Partially Filled` result as MTM-derived behavior unless later research proves a one-to-one Visual equivalent.

## Acceptance Criteria

- Users can select multiple CO lines in the CO table.
- The system evaluates fulfillment in Oldest Added order.
- The system marks fully covered lines as Complete.
- The system marks the first partially covered next-oldest line as Partially Filled when applicable.
- The system records Qty Satisfied for each evaluated line.
- The system never subtracts inventory below zero.
- The implementation documentation identifies whether the final status logic is Visual-derived, MTM-derived, or a combination of both.

## Implementation Notes for the Next Pass

- Treat this file as a requirements handoff, not as final schema proof.
- The CSV-based relationship research is partially complete and should be treated as the current baseline.
- Use `CUST_ORDER_LINE` and `WORK_ORDER` as the core confirmed entities.
- Treat `CUST_ORDER_ALLOC` as the leading candidate bridge for additional query proof, but do not represent it as a fully proven FK-backed relationship.
- Do not use `CO_PRODUCT` as the sole basis for the design without additional proof; it is FK-backed to work orders but currently appears too sparse and does not contain CO identifiers.
- Do not claim a final `Oldest Added` field until the business chooses between order chronology and ship-sequence chronology or until an existing query proves the intended source.
- Document the final join path, chosen `Oldest Added` field, and final status-mapping rule before writing queries or changing the UI behavior.