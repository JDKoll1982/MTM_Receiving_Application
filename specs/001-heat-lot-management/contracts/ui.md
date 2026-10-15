# UI Contract: Heat / Lot Management

The screen contract for the Heat / Lot Management feature: the tool page, the settings page, and every
screen that shows a marking. Layout follows the module's existing tool pattern, and every binding uses
`x:Bind` with an explicit `Mode`.

## Tool Page

- **Display name**: `Heat / Lot Management`
- **Tool key**: `HeatLotManagement`
- **View**: `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml` (+ `.xaml.cs`)
- **View model**: `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`
- **Reached from**: the Ship/Rec Tools selection list, through
  `ViewModel_ShipRecTools_Main.NavigateToTool("HeatLotManagement")`.
- **Access**: any user who already has access to the Ship/Rec Tools module. No new permission is
  introduced. **Satisfies**: FR-022.

### Heat search

- **Inputs**: a heat number typed by the operator. Partial text is accepted and normalized before it is
  sent.
- **Shows**: each matching lot with its heat number, supplier, shape, color, whether the marking was
  chosen automatically or set by hand, and whether it is locked.
- **Empty state**: "No marked lot in the list matches that heat number. The list covers only the parts
  listed below, so a lot whose material is not on that list is not shown." The wording states the scope
  rather than implying the lot carries no marking, because material that is not on the part list is a
  normal reason for no match. **Satisfies**: FR-073.
- **Failure modes**: a query failure shows the shared Infobar error and leaves the previous results
  in place. A no-match result is not an error.
- **Satisfies**: FR-017, SC-009.

### Marking list

- **Inputs**: the optional heat number from the heat search, and the part number chosen in the part
  list. Choosing a part reads the list again, so the list follows the choice rather than waiting for
  the search button. Reloading the part list clears its selection, which happens while the view model
  is busy and is therefore not read as a user choosing nothing.
- **Shows**: one row per lot — heat number, supplier, the part numbers that carry it, shape, color,
  source, locked badge, last changed by, last changed at — newest first.
- **Scope**: the list holds a lot when at least one enabled part carries it, or when no receiving row
  carries its number at all, because such a lot cannot be attributed to a part and dropping it would
  hide a lot whose number was corrected away. Choosing a part narrows the list to that part alone,
  whether or not the part is still enabled, so a removed part's recorded markings stay reachable.
  **Satisfies**: FR-067, FR-068, FR-069, FR-070, FR-071.
- **Duplicate flag**: two rows holding the same shape and color are marked so the collision is visible
  rather than hidden. This is the visible half of the case where the palette is exhausted.
- **Empty state**: "No marked lots for the parts on the list. Choose a part to see its lots."
- **Failure modes**: a load failure shows the Infobar error and an empty list; the tool stays usable.
  A failure to read the enabled part list fails the load rather than showing every lot, because showing
  everything again is the reading this scope exists to stop.
- **Satisfies**: FR-018, FR-019, and the edge case requiring a shared marking to be visible in the tool.

### Override panel

- **Inputs**: the selected lot, a shape chosen from the 10 shapes, a color chosen from the 12 colors,
  and a **required** reason.
- **Behaviour**: Save stays disabled until a reason is non-blank, because the procedure refuses a
  reasonless change.
- **Output**: the row updates in place, `source` becomes the manual value, and the acting user and
  timestamp are shown on the row.
- **Locked rows**: the panel stays available. FR-019 stops the system from changing a locked marking,
  not the operator, and the lock badge stays set after the change.
- **Failure modes**: a rejected save shows the procedure's own message — `A reason is required to
  change a lot marking` or `Lot marking not found` — through the shared Infobar.
- **Satisfies**: FR-018, FR-020.

### Lock control

- **Inputs**: the selected lot and an on/off toggle.
- **Output**: the locked badge appears or clears; the codes do not change.
- **Failure modes**: `Lot marking not found` surfaces through the shared Infobar.
- **Satisfies**: FR-019.

### Palette legend

- **Shows**: the available shapes and the available colors, each with its display name, in the order the
  palette model gives them, honouring `LotMarking.Palette.EnabledShapes` and
  `LotMarking.Palette.EnabledColors`.
- **Behaviour**: a shape is always paired with its color and never encodes meaning through color alone.
- **Empty state**: cannot occur — an unset palette setting falls back to the complete default set.
- **Satisfies**: FR-021.

### Enabled part list

- **Inputs**: Add opens the existing `Module_Core/Dialogs/Dialog_FuzzySearchPicker.xaml`, so a part is
  chosen by searching rather than typed from memory. The selected part is confirmed into the list.
- **Shows**: one row per part number with its on/off state, who added it, and when.
- **Actions**: Remove deactivates a part (it does not delete the row); Re-enable reactivates it. A part
  that is removed stops receiving new markings and keeps every marking already recorded.
- **Empty state**: the list starts empty, which is the state in which the feature is invisible because
  no part receives a marking.
- **Failure modes**: a blank or unknown part number shows the procedure's message; a repeated Add is
  not an error, because the add is idempotent.
- **Satisfies**: FR-009, FR-010, FR-011, FR-012.

## Settings Page

- **View**: `Module_Settings.Core/Views/View_Settings_LotMarking.xaml` (+ `.xaml.cs`)
- **View model**: `Module_Settings.Core/ViewModels/ViewModel_Settings_LotMarking.cs`
- **Reached from**: the Settings drill-down. `View_Settings_CoreNavigationHub` gains a button and a
  handler that calls `NavigateUsingServiceProvider(typeof(View_Settings_LotMarking))`, and
  `View_Settings_CoreWindow.GetPageHeader(Type)` gains a `View_Settings_LotMarking` arm in its
  `pageType.Name switch` so the shell header shows the page title. `MainWindow.xaml.cs` gains the
  matching settings destination entry.

### Controls

- **Feature switch** — `LotMarking.Enabled`, system scope, `Admin` permission, default `true`. Turning
  it off stops new markings and changes nothing already recorded. A user without the permission sees it
  read-only.
- **Collision behaviour** — `LotMarking.CollisionPolicy`, system scope, `User` permission, default
  `Probe`, bound to `Enum_LotMarkingCollisionPolicy`.
- **Enabled shapes** — `LotMarking.Palette.EnabledShapes`, system scope, `User` permission. A
  multi-select over the 10 shapes.
- **Enabled colors** — `LotMarking.Palette.EnabledColors`, system scope, `User` permission. A
  multi-select over the 12 colors.
- **Display order** — `LotMarking.Display.ShapeFirst`, user scope, `User` permission, default `true`.
  Decides whether a marking leads with the shape or the color, everywhere a marking appears.

### Behaviour

- The page carries a plain-language note that changing the palette or switching the feature off never
  changes a marking already recorded, because the physical marking on the floor cannot change.
- Deselecting a shape or a color removes it from future assignments only. Every recorded marking keeps
  the codes it was given, and an old receipt repaints nothing.
- The enabled part list is also maintainable here, using the same search picker and the same
  deactivate-don't-delete rule as the tool page.
- **Failure modes**: a failed save shows the shared Infobar error and leaves the stored value untouched.
  A malformed stored value resolves to its default instead of blocking the page.
- **Satisfies**: FR-004, FR-009, FR-011, FR-012, FR-013, FR-018, FR-021, and SC-004.

## Screens That Show A Marking

A marking is shown as a shape plus a color, laid out according to `LotMarking.Display.ShapeFirst`.
Shape is the primary identifier, so the default leads with the shape.

### `receiving-line`

- **Screen**: the receipt line grid in `View_Receiving_Review.xaml`, bound through
  `ViewModel_Receiving_Review`.
- **Inputs**: each load's `heat_shape_code` and `heat_color_code`, which arrive on the queue row from
  the save.
- **Shows**: the shape and color for that line's lot, beside the heat number.
- **Unmarked lines**: a line with no marking — a blank heat number, or a part that is not on the enabled
  list — shows its normal cells with the marking cell empty. Nothing is hidden, disabled, or garbled.
- **Failure modes**: a blank or unknown code renders as no marking rather than as a broken glyph. The
  grid never blocks on a marking.
- **Satisfies**: FR-008, FR-010, FR-014, SC-001.

### `receiving-header`

- **Screen**: the per-step receipt header card at the top of the same view, so the whole trip can be
  scanned before any line is opened.
- **Inputs**: the distinct markings of the receipt's loads.
- **Shows**: one marking per lot on the receipt. Two lots with similar heat numbers deliberately show
  different shapes and colors, which is the point of the card.
- **Unmarked receipts**: a receipt created before this feature has no recorded marking and shows its
  header exactly as it does today.
- **Failure modes**: a receipt with mixed marked and unmarked lines shows the marked lots and omits the
  rest; nothing appears broken.
- **Satisfies**: FR-014, SC-001, SC-005.

### `reprint-grid`

- **Screen**: the receiving reprint grid in `View_Reprint_Main.xaml`, bound through
  `ViewModel_Reprint_Receiving`.
- **Inputs**: the two codes returned by `sp_Receiving_History_GetForReprint`, carried onto
  `Model_ReprintSelectableRow` and its mapper, which currently discards the `heat` value the procedure
  already returns.
- **Shows**: each history row's marking, identical to what the handler saw on the floor.
- **Failure modes**: a pre-feature history row has no codes and renders without a marking.
- **Satisfies**: FR-016, SC-007.

## Palette

The palette is fixed by the specification and is consumed verbatim by the models, the resolver, and the
settings page.

- Shapes (10), stored as the emoji itself: `⚡`, `💗`, `🌙`, `⭐`, `💎`, `⬛`, `🔺`, `⭕`, `❌`, `➡️`
- Shapes are limited to emoji that Segoe UI Emoji draws, so none can render as a missing-glyph box.
  Cylinder, hexagon and pentagon are not offered because no dependable emoji exists for them.
- Colors (12): `BLUE`, `RED`, `GREEN`, `WHITE`, `YELLOW`, `BLACK`, `PURPLE`, `ORANGE`, `PINK`, `GRAY`,
  `BROWN`, `TEAL`
- 10 shapes x 12 colors = 120 combinations, so two simultaneous lots can collide by chance. The later
  lot is given a free combination instead of sharing one.

## Presentation And Failure Rules

- Every error reaches the operator through `ViewModel_Tool_Base.ShowStatus`, which owns the shared
  Infobar. No `MessageBox.Show` appears outside a `Views` file.
- Every exception a view model catches goes to `IService_ErrorHandler`; a view model never logs and
  swallows silently.
- Every list has a named empty state: no lots marked, no heat search match, no part on the enabled
  list, and every combination taken. No screen shows a blank panel without an explanation.
- Business logic stays out of `.xaml.cs`. The tool's code-behind holds only `XamlRoot`, the help
  invocation, and the fuzzy-picker delegate.
- Shared collections are rebuilt by replacing the instance rather than by `Clear()` followed by an
  `Add` loop.

## Navigation And Registration

Adding the tool mirrors the wiring the Welded Coils tool already uses, at these points:

1. `Service_ShipRecTools_Navigation.RegisterBuiltInTools()` registers a `Model_ToolDefinition` with
   `ToolKey = "HeatLotManagement"`, `Title = "Heat / Lot Management"`, a one-line `Description`,
   `Category = Enum_ToolCategory.Utilities`, and `IsAvailable = true`.
2. `ViewModel_ShipRecTools_Main` gains an `IsHeatLotManagementVisible` observable property and a
   `case "HeatLotManagement":` arm in `NavigateToTool`.
3. `View_Tool_LotMarking` is injected into the `View_ShipRecTools_Main` constructor, null-checked, and
   its `ViewModel` captured into a private field.
4. `View_ShipRecTools_Main.xaml` gains a `ContentPresenter` host for the view, with `Visibility` bound
   to `IsHeatLotManagementVisible` using an explicit `Mode=OneWay`.
5. `View_ShipRecTools_Main.xaml.cs` assigns the view to that host's `Content`, adds
   `nameof(ViewModel_ShipRecTools_Main.IsHeatLotManagementVisible)` to its property-changed
   subscription list, and calls `ActivateView()` on the tool's view model when its flag becomes true.
6. The feature switch is read when `ViewModel_ShipRecTools_Main` activates, and it decides whether the
   `Heat / Lot Management` entry is offered. When the switch is off the tool is not offered and no new
   marking is assigned, while every marking already recorded stays visible on receiving and reprint.
   **Satisfies**: FR-013.
7. `Module_Core/Services/Help/Service_Help.cs` gains a `ShipRecTools.LotMarking` article so the tool's
   help button opens real content rather than a placeholder.
