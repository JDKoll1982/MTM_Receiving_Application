# MTM Receiving Application — Module_Dunnage & Cross-Module Multi-Fix One-Shot

## Pre-Flight: Required Reading (Do First, Every Time)

Before writing a single line of code, you MUST:

1. Read `.serena/memories/architectural_patterns.md`
2. Read `.serena/memories/forbidden_practices.md`
3. Read `.serena/memories/mvvm_guide.md`
4. Read `.serena/memories/dao_best_practices.md`
5. Read `.serena/memories/xaml_binding_patterns.md`
6. Read `.serena/memories/error_handling_guide.md`
7. Read `.github/instructions/sql-schema-generation.instructions.md`
8. Read `.github/instructions/sql-sp-generation.instructions.md`
9. Read `.github/instructions/csharp.instructions.md`
10. Read `.github/instructions/copilotforms-ui-change-logic-change.instructions.md`
11. Read `.github/instructions/copilotforms-new-feature-request.instructions.md`
12. Read `.github/instructions/copilotforms-feature-removal-request.instructions.md`
13. Read `.github/instructions/update-docs-on-code-change.instructions.md`
14. Read `.github/instructions/module-doc-maintenance.instructions.md`

All work MUST comply with the MVVM layer flow:  
**View (XAML) → ViewModel → Service → DAO → Database**  
All DAOs must be instance-based, injected via constructor, and return `Model_Dao_Result<T>`.  
All XAML bindings must use `x:Bind` with explicit `Mode`. No `{Binding}` allowed.  
No business logic in `.xaml.cs` code-behind files. All async methods end in `Async`.  
MySQL operations use stored procedures only. SQL Server / Infor Visual is READ ONLY.

---

## ⚠️ ASSUMPTION PROTOCOL — APPLIES TO THIS ENTIRE REQUEST

Whenever you are about to make a **major assumption** (missing schema detail, ambiguous behavior, unclear scope, inferred stored procedure parameters, etc.), you MUST create an assumption file at:

`.github/assumptions/MMDDYYYY-HHMMam/pm-Assumptions.md`

containing: numbered list of assumptions, why each is needed, potential impact if wrong, alternatives considered, and an explicit request for user confirmation before proceeding.

**Item 2 (LoadType) carries an additional constraint — see Section 2 below.**

---

## Section 1 — spec_json Table: Include Full Image Path in JSON Collection

**Files Affected:**

- `Module_Dunnage/Data/Dao_DunnageSpec.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`
- The stored procedure(s) that insert/update spec_json (currently called via `sp_dunnage_specs_insert` and `sp_dunnage_specs_update`)
- `Module_Dunnage/Models/Model_DunnagePart.cs` — `ImagePath` property already exists
- `Module_Dunnage/Models/Model_DunnageType.cs` — `ImagePath` property already exists

**Requirement:**  
When saving spec values to the `spec_json` / `spec_value` column in the MySQL database, the full image path for the part or type must be included as a key in the JSON collection (e.g., `"image_path": "\\\\ServerName\\ShareName\\Images\\dunnage\\mypart.png"`).

**Critical Path Rule:**  
The image path stored in the database **MUST use UNC path format** (`\\ServerName\ShareName\...`) rather than a drive-letter-mapped path (e.g., `Z:\Images\...`). This ensures that users connecting to the same network share via different drive letter mappings can still resolve the path correctly.

Implementation steps:

1. In the service layer (`Service_MySQL_Dunnage.cs`), before building the JSON spec payload that is passed to the DAO, inject the `ImagePath` value into the spec dictionary using the key `"image_path"`. If `ImagePath` is null or empty, do not include the key.
2. Ensure the path written is in UNC format. If the current value is a drive-letter path (e.g., starts with a letter followed by `:\`), convert it to UNC via the `Service_DunnageImageStorage` helper or a dedicated path-normalization utility. Do not resolve the path at the DAO layer.
3. The stored procedures `sp_dunnage_specs_insert` and `sp_dunnage_specs_update` receive the full serialized JSON string as `@p_spec_value`. No stored procedure change is needed unless the column size must be verified; confirm and document.
4. On read-back (when `Model_DunnagePart.SpecValues` is deserialized in `DeserializeSpecValues()`), the `image_path` key must be extracted and used to populate `Model_DunnagePart.ImagePath` if `ImagePath` is currently null/empty.

---

## Section 2 — LoadType Per-Part Variable (Dunnage Parts)

> **⚠️ MANDATORY INTERACTION REQUIREMENT FOR THIS ENTIRE SECTION**  
> Per the project owner's explicit instruction (requirement 2.4), you are **NOT ALLOWED to make any assumptions** about this feature. For every question you have — regarding schema design, FK cascade behavior, UI interaction detail, migration safety, stored procedure parameter names, enum vs. lookup table approach, or anything else — you MUST ask in the GitHub Copilot chat window **before writing any code for this section**. Do not proceed with implementing Section 2 until every question has been answered and confirmed by the user.

**Background:**  
`Module_Receiving` has a LoadType concept (refer to the existing implementation in `Module_Receiving` for the UI pattern). `Module_Dunnage` needs an equivalent feature, but LoadType is **per Dunnage Part**, not per Dunnage Type.

**Default Load Type values (in this order):**

- Skid
- Barrel
- Crate
- Vendor Supplied Container
- None

**Sub-requirements to clarify with the user before implementing (ask all of these):**

**Section 2.1 — UI placement:**

- The Load Type `ComboBox` must be added to both the Edit (`View_Dunnage_EditPartDialog.xaml`) and Add (`View_Dunnage_QuickAddPartDialog.xaml`) Dunnage Part dialogs.
- Ask: Where exactly within the dialog layout should the Load Type ComboBox appear? (Which card, which row relative to existing fields such as Part ID, Inventory Type, Home Location?)

**Section 2.2 — Manage Dunnage Load Types (+ and - buttons):**

- Add a `+` button (tooltip: "Add a new Dunnage Load Type") and a `-` button (tooltip: "Remove the selected Dunnage Load Type") to the right side of the Load Type ComboBox.
- The `-` button removes the currently selected Dunnage Load Type from the lookup table.
- The Load Type values must be stored in a new MySQL lookup table (e.g., `dunnage_load_types`) with a FK relationship to the `dunnage_parts` table column that stores a part's load type.
- The new `dunnage_load_types` table must be structured so that all currently existing `dunnage_parts` rows default to the "None" load type after migration.
- Ask all ambiguous questions about this sub-feature before proceeding.

**Section 2.3 — Migration Safety:**

- All new tables, stored procedures, and views must be designed so that data exported (via mysqldump or similar) **before** the database migration can be re-integrated **without modifying the exported SQL file**.
- This means: new columns must have `DEFAULT` values, new FK constraints must permit NULL or have a safe default, and new tables must be created with `IF NOT EXISTS` guards.
- Ask any questions about migration scope before proceeding.

**Section 2.4 — No Assumptions Allowed (see above note)**

---

## Section 3 — QuickAddTypeDialog PNG Image Card Redesign

**Files Affected:**

- `Module_Dunnage/Views/View_Dunnage_QuickAddTypeDialog.xaml`

**Problem:**  
The "Type Image (PNG)" card in the Add (and Edit) Dunnage Type dialog uses a `StackPanel` for the image/button layout, causing the Clear button to clip off the right side of the card.

**Required Layout Change:**  
Replace the `StackPanel` layout inside the "Upload a PNG image to represent this dunnage type" `TextBlock`'s parent card with a **3-column, 2-row `Grid`**, laid out as follows:

```
Col 0         Col 1              Col 2
+-------------+------------------+------------------+
| [PNG Image] | Choose PNG Btn   | Rotate 90° Btn   |  Row 0
| (RowSpan 2) |                  |                  |
| (centered   +------------------+------------------+
| H + V)      | Clear Button (ColSpan 2, centered H+V) | Row 1
+-------------+----------------------------------------+
```

- **Row 0 / Col 0:** The PNG preview `Image` control, `RowSpan="2"`, `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`
- **Row 0 / Col 1:** Choose PNG `Button`
- **Row 0 / Col 2:** Rotate 90° `Button`
- **Row 1 / Col 1:** Clear `Button`, `Grid.ColumnSpan="2"`, `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`

Insert this Grid immediately after the `TextBlock` that reads "Upload a PNG image to represent this dunnage type".  
All `x:Bind` bindings and `Click` handlers must be preserved from the original layout.  
Do not use `{Binding}`. Use `x:Bind` with explicit `Mode`.

---

## Section 4 — JPG Support for Dunnage Part Numbers and Dunnage Types

**Files Affected:**

- `Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml` and `.cs`
- `Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml` and `.cs`
- `Module_Dunnage/Views/View_Dunnage_QuickAddTypeDialog.xaml` and `.cs`
- `Module_Dunnage/Services/Service_DunnageImageStorage.cs`
- `Module_Dunnage/Contracts/IService_DunnageImageStorage.cs`
- `Module_Dunnage/Helpers/Helper_DunnageImagePaths.cs` (if it exists)
- Any `FileOpenPicker` filter configurations in `.xaml.cs` code-behind

**Requirement:**  
All image-upload and image-display functionality currently limited to PNG must be extended to also support JPG/JPEG.

Specific changes:

1. **FileOpenPicker filters:** Anywhere a `FileOpenPicker` is configured with `SuggestedStartLocation` and `FileTypeFilter` entries of `".png"`, add `".jpg"` and `".jpeg"` to the filter list.
2. **Label text:** Any `TextBlock` or button content referencing "PNG" in the context of uploading images must be updated to read "PNG / JPG" (e.g., "Choose PNG" → "Choose Image (PNG/JPG)", "Part Image (PNG)" → "Part Image (PNG/JPG)").
3. **Image loading:** The `Helper_DunnageImagePaths.CreateImageSource()` method (or equivalent) must work with both `.png` and `.jpg`/`.jpeg` extensions. Verify it does not perform any extension-specific logic; if it does, update it.
4. **Storage service:** `Service_DunnageImageStorage.cs` must accept and correctly store JPG files. If it performs any extension-based validation, update to allow `.jpg` and `.jpeg` in addition to `.png`.
5. **No database schema change is required** for this item — image paths are stored as strings and the column length is sufficient for both formats.

---

## Section 5 — Required Field Validation (Add/Edit Part Numbers & Guided Workflow)

**Files Affected:**

- `Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml` and `.cs`
- `Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml` and `.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ManualEntryViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_PartSelectionViewModel.cs`
- Any ViewModel or service that gates the Save/Submit action

**Problem:**  
Required fields are not being enforced in two places:

1. When adding or editing Dunnage Part Numbers (via `View_Dunnage_QuickAddPartDialog` / `View_Dunnage_EditPartDialog`)
2. During the guided workflow at each step transition

**Requirement:**

For Add/Edit Dunnage Part dialogs:

- The primary button (Save/Confirm/OK) must be disabled when any required field is empty or invalid.
- Required fields include at minimum: Part ID, Dunnage Type (TypeId), Inventory Type.
- If the primary button must remain enabled for UX reasons, validate on click and surface a `ValidationMessage` visible in the UI (following the existing pattern seen in `View_Dunnage_QuickAddTypeDialog.xaml` where `ViewModel.ValidationMessage` is bound to a `TextBlock`).
- Validation logic must live in the ViewModel, NOT in `.xaml.cs` code-behind.

For the guided workflow:

- Each step's "Next" / "Continue" command (in ViewModels such as `ViewModel_Dunnage_WorkFlowViewModel`, `ViewModel_Dunnage_PartSelectionViewModel`, `ViewModel_Dunnage_DetailsEntryViewModel`, `ViewModel_Dunnage_QuantityEntryViewModel`) must gate advancement when the step's required input is absent.
- Use `[RelayCommand(CanExecute = nameof(CanProceedToNextStep))]` or equivalent `CanExecute` guard rather than showing errors after the fact.
- Do not duplicate validation logic between ViewModel and code-behind.

---

## Section 6 — Mode Selection: Image-Based Dunnage Part Search Feature

**Files Affected:**

- `Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml` and `.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs`
- `Module_Dunnage/Services/Service_MySQL_Dunnage.cs` and `IService_MySQL_Dunnage.cs`
- `Module_Dunnage/Data/Dao_DunnagePart.cs`
- A new modal view (Dialog) for displaying Part ID details
- Reference: `Module_Receiving`'s Reconciliation button for styling

**Requirement:**  
Add a Search button to the Mode Selection view (`View_Dunnage_ModeSelectionView.xaml`), styled identically to the Reconciliation button in `Module_Receiving`'s Mode Selection view (locate and copy the style/template exactly).

**Search Feature Behavior:**

1. Clicking the Search button opens a search modal (new `ContentDialog`, named `View_Dunnage_Dialog_ImagePartSearch.xaml`).
2. The modal allows the user to search for Dunnage Part Numbers by image.
3. On open (or on clicking a "Search" / "Load" button within the modal), the service layer queries the MySQL database for all Dunnage Part Numbers (`dunnage_parts` table) that have a non-null, non-empty `image_path` value.
4. Results are displayed in a **3×3 grid layout** (same layout used in the Select Dunnage Type view — `View_Dunnage_TypeSelectionView.xaml` — use it as the visual reference).
5. Each cell in the grid shows:
   - The part's image
   - The Part Name / Part ID displayed beneath the image
6. Clicking an image cell opens a secondary modal window (`View_Dunnage_Dialog_PartInfoModal.xaml`) displaying **all information for that Part ID** (Part ID, Dunnage Type, Home Location, Inventory Type, Load Type if Section 2 is complete, Image Path, Spec Values, Created By, Created Date, Modified By, Modified Date).
7. Data flow MUST follow MVVM: ViewModel calls Service, Service calls DAO, DAO returns `Model_Dao_Result<List<Model_DunnagePart>>`.
8. If no parts have images, show an appropriate empty-state message.

---

## Section 7 — Material Availability Board: Inventory HTML Output

**Files Affected:**  
Locate the HTML generation service/helper for the Material Availability Board Inventory Sheet (search in `Module_Reporting`, `Module_Shared`, or `Module_ShipRec_Tools` for HTML template/generation logic).

**Changes Required:**

**7a — Add Quantity to Part ID Card:**  
The card that currently contains the Part ID in the HTML output must also show the quantity of material for that location. Place the quantity in the same card. Remove the "Taken From" section from that card — it is already covered in the header of the HTML output. Make the Location text in the header **bold**.

**7b — Page Footer with Page Number:**  
Each printed page can contain up to 7 rows of Part IDs. Add a footer to the document showing: `Page X of Y`, right-aligned. This footer must increment correctly across all pages.

**7c — Remove from HTML Output:**  
Remove both the "Warehouse Scope" section and the "Look Ahead" text from the HTML output entirely.

**7d — Center the First-Column Card:**  
The card in the 1st column of the layout must be centered both horizontally and vertically within its cell using appropriate CSS (`display: flex; align-items: center; justify-content: center;` or equivalent table-cell CSS).

---

## Section 8 — Material Availability Board: Work Order Details Modal

**Files Affected:**  
Locate the Work Order Details Modal (search in `Module_Reporting` or `Module_ShipRec_Tools`). Also read `docs/` or any `InforVisualGuide.md` file that describes Work Order Status normalization.

**Changes Required:**

**8a — Remove Next Due-Date Source:**  
Remove the "Next Due-Date Source" field from the Work Order Details Modal entirely.

**8b — Normalize Work Order Status Output:**  
Read `InforVisualGuide.md` (located in `docs/` or root) and apply the normalized Work Order Status display values described there. The raw Infor Visual status codes must be mapped to human-readable normalized strings before display.

**8c — Required Parts and Estimated Coil Use in Own Card:**  
Move "Required Parts" and "Estimated Coil Use" into their own card within the modal. Format quantities using comma separators for values ≥ 1,000. Round up all Estimated Coil Use values (use `Math.Ceiling`).

**8d — Material Availability Work Order Fields Settings Page Fixes:**  
Locate the settings page for "Material Availability Work Order Fields."

- Add a "Select All / Deselect All" toggle button. Clicking once selects all checkboxes; clicking again deselects all.
- Fix the alignment issue: checkbox headers are currently offset to the right relative to their checkboxes — align them correctly (same horizontal left edge).
- Fix the Save button: settings are not persisting. They must save using the same mechanism as `Module_Settings.Receiving` saves its settings (locate that save pattern and replicate it exactly).

---

## Section 9 — Fuzzy Search Modal: Fix Narrow Results Filter Scope

**Files Affected:**  
Locate the Fuzzy Search Modal (likely in `Module_Shared`, `Module_Core`, or referenced from `Module_Dunnage` or `Module_Receiving`). Search for the "Narrow Results" `TextBox` binding and the underlying `ObservableCollection` or `ICollectionView` that backs the displayed results.

**Problem:**  
The Narrow Results textbox only filters the initial 50 results shown. If the full dataset contains results beyond the first page (e.g., MMC0000928), typing "928" into Narrow Results does not find it because only the first 50 were loaded into the filterable collection.

**Fix:**  
The full result set returned from the database query must be loaded into the filterable collection **before** the initial display is trimmed for visual performance. The display may show the first 50 as a default view, but the Narrow Results filter must operate against the **complete result set**, not the truncated display list.

Implementation approach:

- Store all results in a backing `List<T>` or full `ObservableCollection<T>`.
- The Narrow Results filter must apply a predicate against this full list and replace the displayed collection using a collection-instance replacement (per project rule: prefer replacing the `ObservableCollection` instance over `Clear()` + repeated `Add()` to avoid excessive UI notifications).
- Do not re-query the database on each keystroke; filter client-side.

---

## Section 10 — Module_Volvo: Fix "Show Email" Button Crash

**Files Affected:**  
`Module_Volvo/` — locate the view or ViewModel containing the "Show Email" button and its command.

**Problem:**  
Clicking the "Show Email" button in `Module_Volvo` causes an application crash.

**Fix:**

1. Identify the root cause (null reference, unhandled exception in command, missing service registration, etc.).
2. Apply the appropriate fix following MVVM patterns.
3. Wrap the at-risk operation in a try/catch at the ViewModel level, returning a user-friendly error via `_errorHandler.ShowUserErrorAsync(...)` rather than letting the exception propagate and crash the app.
4. No `MessageBox.Show` — use the established error handler pattern per `.serena/memories/error_handling_guide.md`.

---

## Section 11 — Module_Settings.Volvo: Remove Unimplemented Settings Pages

**Files Affected:**  
`Module_Settings.Volvo/` — all Views, ViewModels, navigation buttons.

**Changes Required:**

1. Remove the following settings pages entirely (Views, ViewModels, navigation buttons, and any DI registrations):
   - Overview
   - Database Settings
   - Connection Settings
   - File Paths
   - UI Configuration
   - Hardcoded to Externalize
2. Remove any navigation buttons in the Volvo settings navigation page that route to these removed pages.
3. **11a:** Convert the current `Module_Volvo` settings navigation page into a placeholder page for future settings. It should display a brief message such as "Additional Volvo settings will appear here in a future release." — no navigation items, no broken links.

---

## Section 12 — Module_OutsideService: Disable and Document for Reinstatement

**Files Affected:**

- `Module_OutsideService/` — entire folder
- `MainWindow.xaml` and `MainWindow.xaml.cs` — navigation button
- Any DI registrations in `Infrastructure/DependencyInjection/` that reference `Module_OutsideService`
- `docs/modules/module_outsideservice/` — create reinstatement doc

**Changes Required:**

1. **Remove** `Module_OutsideService` from the project (exclude from the `.csproj` — do not delete the source files, but comment out or exclude the `<Compile>` / `<Page>` items).
2. **Comment out** any code in `MainWindow.xaml.cs` or other shared files that is used exclusively for `Module_OutsideService` navigation or initialization. Annotate with `// MODULE_OUTSIDESERVICE_DISABLED`.
3. **Delete** (or hide) the navigation button in `MainWindow.xaml` that routes to `Module_OutsideService`.
4. **Create** `docs/modules/module_outsideservice/reinstatement.md` containing:
   - A description of what the module does
   - Step-by-step instructions for re-enabling it (restore `.csproj` includes, uncomment MainWindow code, re-add navigation button, restore DI registrations)
   - A list of every file that was modified/excluded, with the specific line numbers or regions that were commented out

---

## Section 13 — Module_Settings.Core: Remove Navigation Pages

**Files Affected:**  
`Module_Settings.Core/` — navigation page view, and the following settings page Views + ViewModels.

**Changes Required:**  
Remove the following pages and their corresponding navigation buttons from `Module_Settings.Core`:

1. **Logging**
2. **Database**
3. **System**

For each:

- Remove the navigation button from the `Module_Settings.Core` navigation page
- Remove (or exclude from project) the View (`.xaml` + `.xaml.cs`) and ViewModel (`.cs`) files
- Remove any DI registrations for those pages' ViewModels in `Infrastructure/DependencyInjection/`
- Ensure no remaining code references these removed types

---

## Section 14 — Module_Settings.Core: UI Theme Toggle (Dark / Light Mode)

**Files Affected:**

- `Module_Settings.Core/Views/` — the UI Theme settings page
- `Module_Settings.Core/ViewModels/` — the UI Theme settings ViewModel
- `Module_Shared/` or `Module_Core/` — the theme-switching service (locate the existing theme service or create one)
- MySQL database — user settings table (locate the existing user settings pattern used by `Module_Settings.Receiving`)

**Requirement:**  
Implement a dark/light mode toggle on the UI Theme settings page.

1. Place the toggle in a stylized card (Border with `LayerFillColorDefaultBrush` background, `CardStrokeColorDefaultBrush` border, `CornerRadius="6"`, matching the card style used throughout the application).
2. Use a `ToggleSwitch` (or equivalent styled toggle) with `OnContent="Light Mode"` and `OffContent="Dark Mode"` (or vice versa — match the WinUI 3 `ElementTheme` convention: `Dark` = Off, `Light` = On, or ask the user if uncertain).
3. Changing the toggle must immediately apply the theme to the running app by setting `((FrameworkElement)App.MainWindow.Content).RequestedTheme` to `ElementTheme.Light` or `ElementTheme.Dark`.
4. The selected theme must be **persisted as a user setting in the MySQL database**, using the same save pattern used by `Module_Settings.Receiving` for its user settings. Locate that pattern and replicate it. The setting key should be named `ui_theme` (or whatever convention the settings table uses).
5. On app startup, the saved theme preference must be loaded and applied before the main content is shown.
6. Follow the MVVM layer: ViewModel → Service (theme + settings) → DAO → MySQL.

---

## Section 15 — Module_Settings.Core: Material Availability Description Truncation

**Files Affected:**  
`Module_Settings.Core/Views/` — Material Availability settings navigation item or page header.

**Requirement:**  
The description text on the Material Availability settings item/page clips. Shorten the description text to fit within the available space without clipping. Set `TextTrimming="CharacterEllipsis"` or `TextWrapping="Wrap"` as appropriate. If using `TextTrimming`, also set `ToolTipService.ToolTip` to the full description text for accessibility.

---

## Section 16 — Module_Settings.Core: Users & Privileges Full Restyle + Privilege Level Change

**Files Affected:**

- `Module_Settings.Core/Views/` — Users & Privileges page View
- `Module_Settings.Core/ViewModels/` — Users & Privileges ViewModel
- `Module_Settings.Core/Services/` (if a user management service exists)
- Reference: `Module_Receiving` styling — use its exact card/grid style as the visual baseline

**Changes Required:**

**Restyle:**  
Fully restylize the Users & Privileges page to match the styling conventions of `Module_Receiving`. This includes:

- Card-based layout (Border with `LayerFillColorDefaultBrush`, `CardStrokeColorDefaultBrush`, `CornerRadius="6"`)
- Consistent typography (SubtitleTextBlockStyle, BodyStrongTextBlockStyle, CaptionTextBlockStyle)
- Consistent button styling
- Responsive grid layout matching `Module_Receiving`'s column structure

**Add Privilege Level Change Capability:**  
Allow Developer and Admin users to change another user's privilege level, subject to the following rules enforced in the ViewModel and/or Service:

| Acting User | Can Grant        | Cannot Grant |
| ----------- | ---------------- | ------------ |
| Developer   | Developer, Admin | N/A          |
| Admin       | Admin only       | Developer    |

Implementation:

1. In the Users list, add a privilege-level `ComboBox` (or inline selector) per user row.
2. The options shown in the ComboBox must be filtered based on the **current logged-in user's privilege level** (apply the table above).
3. A "Save Changes" button (or per-row save) must persist the change via the Service → DAO → MySQL stored procedure pattern.
4. Attempting to grant a privilege level the acting user is not authorized for must be blocked in the ViewModel with a user-facing error message (via the established error handler, not `MessageBox.Show`).
5. All XAML must use `x:Bind` with explicit `Mode`. ViewModel must be a `partial class` inheriting `ViewModel_Shared_Base`.

---

## Post-Implementation Checklist

After all changes are complete:

- [ ] No `Dao_*` references in any ViewModel file
- [ ] No `Helper_Database_*` calls in any ViewModel or Service file
- [ ] All new DAOs are instance-based and registered in `Infrastructure/DependencyInjection/`
- [ ] All new MySQL operations use stored procedures only
- [ ] All XAML uses `x:Bind` with explicit `Mode` — no `{Binding}`
- [ ] All new async methods end in `Async`
- [ ] No business logic in `.xaml.cs` files
- [ ] All new error handling uses `_errorHandler.ShowUserErrorAsync(...)`, not `MessageBox.Show`
- [ ] All new `ObservableCollection` rebuilds replace the instance rather than using `Clear()` + `Add()`
- [ ] CopilotForms metadata reviewed and updated for every edited module
- [ ] `docs/` module documentation updated per `.github/instructions/module-doc-maintenance.instructions.md`
- [ ] `CHANGELOG.md` updated
- [ ] Assumption files created for any major assumptions made (`.github/assumptions/`)
- [ ] Section 2 (LoadType) NOT implemented without explicit user Q&A confirmation
