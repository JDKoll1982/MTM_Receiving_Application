# Plan: Reprint Labels (new `Module_Reprint`)

**What:** A new dedicated **Reprint Labels** feature — a new top-level module with a sidebar entry (labeled "Reprint Labels", placed **above Scanner**), a **landing page with 3 mode cards** (Receiving / Dunnage / Volvo), and one **sub-page per mode** showing that module's **history only**. Each grid has a **far-left checkbox column**; checked rows are re-queued into that module's **active label queue with `is_reprint=1`** (Receiving's existing pattern) when you hit Save. Rows **already queued render red and have no checkbox**. It recreates Receiving's reprint logic behind dedicated per-module contracts, and **removes the Reprint button from Receiving's Edit Mode**.

## Steps

### Phase 1 — Data layer (Dunnage & Volvo)
- Migration adding `is_reprint` to `dunnage_label_data` + `volvo_generated_label_data`
- New `sp_Dunnage_LabelData_InsertFromHistory` and `sp_Volvo_GeneratedLabelData_InsertFromHistory` (mirror Receiving's, with duplicate guard)
- Per-module reprint-history queries that add an `already_queued` flag
- Matching DAO methods

### Phase 2 — Dedicated reprint contracts
- `IService_Reprint_Receiving` (wraps existing `IService_MySQL_Receiving.InsertFromHistoryAsync`)
- `IService_Reprint_Dunnage`
- `IService_Reprint_Volvo` (uses the **`volvo_generated_label_*` pair**)
- Each exposes history-with-flag + a batch `ReprintAsync` that loops per row and returns `{Queued, AlreadyQueued, Failed}`

### Phase 3 — Module_Reprint UI
- `View_Reprint_Main` landing + 3 sub-pages/ViewModels (x:Bind, per-module filters, checkbox grid, red already-queued rows, Save button, summary feedback)
- Nav wiring: nav item above Scanner + `_navRoutes` entry + search terms + DI registration

### Phase 4 — Remove Receiving Edit Mode reprint
- Strip the button, `ReprintFromHistoryCommand`/`Async`/`CanReprintFromHistory`, and the `IsReprint` row-highlight — but **keep** the SP/DAO/service methods the new module reuses

### Phase 5 — Tests/docs
- Unit tests for landing + 3 sub-pages + batch duplicate/skip logic
- SP integration tests for the two new InsertFromHistory SPs
- DI registration
- Patch notes + a `Module_Reprint/docs/FeatureUpdates` slice

## Key decisions locked from your answers
- Volvo reprint = `volvo_generated_label_history → volvo_generated_label_data` (not the label_data pair)
- Landing page + separate sub-pages per mode; nav label "Reprint Labels" above Scanner
- History-only data source; reuse per-module history filters; new `already_queued` flag
- Batch reprint via checkboxes; per-row duplicate skip; summary + clear checks; no role gating
- Full Receiving Edit Mode removal (UI + command + row highlight), keep shared SPs/DAO
- Unit + SP integration tests

## UI design (locked)
- **Landing page**: 3 mode cards using each module's accent color (Receiving teal / Dunnage olive / Volvo blue); each card = icon + name; click navigates to that mode's sub-page.
- **Sub-pages**: NO header card (the MainWindow header card shows the page title). Each sub-page has an on-page Back button that returns to mode selection.
- **History grid**: compact reprint columns (date, part, qty, reference) per module; far-left checkbox column; checkbox-only selection (row click does not toggle). Already-queued rows render with a RED row background and NO checkbox.
- **Filters** (per sub-page, above the grid):
  - Date range: two DatePickers + preset buttons (Today, Yesterday, Week, Month, Quarter). Default on open = Today. Date range is NOT persisted.
  - Search: "Search By" ComboBox (sets what the text box searches) + a search text box. Default = Part Number.
  - Persisted to `settings_personal` (via `IService_UserPreferences`): Search By only.
  - Search By options:
    - Receiving: Part Number (default), PO Number, Part Description, Vendor, Heat / Lot
    - Dunnage: Part Number (default), PO Number, Type, Quantity
    - Volvo: Part Number (default), Part Description, Quantity
- **Bottom footer** (all three): `Select All` (toggles to `Select None` when all are selected), `Reprint Selected`, `Back`. `Reprint Selected` disabled until ≥1 checkbox checked.
- **Result feedback**: ContentDialog summary ("X queued, Y already queued") with buttons: Close, `Start Over` (resets search, reloads Today), `Mode Selection` (returns to landing).
- **Volvo already-queued / duplicate guard key**: match `volvo_generated_label_history.original_id` to `volvo_generated_label_data.id` where `is_reprint = 1`.

## Further considerations (open but low-risk)
1. Red-row tooltip text ("Already queued for reprint") and disabling Save until ≥1 checkbox is checked.
