# CHANGELOG

All notable changes to the MTM Receiving Application are documented in this file.
Categories follow Keep a Changelog conventions: Added, Changed, Fixed, Removed, Security.

## Unreleased

### Changed

#### The Review page offers the Shape and Color cards only for parts on the lot marking list (2026-11-13)

- **What changed:** on the receiving Review page, the **Shape** and **Color** cards are left off the entry form
  unless the entry's part number is on the `lot_marking_part_scope` list (enabled rows). A part that is not on the
  list never receives a marking, so the two cards no longer sit there blank.
- **How:** `ViewModel_Receiving_Review.ShowsHeatMarkingCards` answers from the enabled list the page already reads
  for the overlay reminder, matched case- and whitespace-insensitively by `Helper_ReceivingLotMarkingNeeds.NeedsMarking`.
  The two cards use `x:Load`, so a hidden card takes no cell and the form's three columns reflow.
- **Unread list:** if the list cannot be read, the cards stay hidden — the same fail-closed choice the overlay
  reminder already makes, since guessing would offer a marking the part does not get.
- **Files:** `Module_Receiving/Views/View_Receiving_Review.xaml`,
  `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`,
  `Module_Receiving/Helpers/Helper_ReceivingLotMarkingNeeds.cs`

#### The Current Total reminder is a card whose border pulses, not pulsing text (2026-11-13)

- **What changed:** the Weight / Quantity step's "Remember to validate Current Total to you Packing Slip!"
  reminder is now a small card that fits its text, with the message in the normal theme text colour. The animation
  sits on the card's 1px border, cycling black → red → black on a 2-second loop.
- **Files:** `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml`, `View_Receiving_WeightQuantity.xaml.cs`

#### Part ids in the receiving grids stay readable in the dark theme (2026-11-13)

- **What changed:** the quality-hold text converters used to hand back hard-coded black for every part that is not
  on quality hold, which rendered the **Part ID** column black-on-dark in Enter PO, Manual Entry and Edit Mode. They
  now return the theme's own text colour for the ordinary case and keep red for MMFSR/MMCSR.
- **How:** a shared `Helper_UI_ThemeTextBrush.PrimaryText` reads the window content's `ActualTheme`, which is the
  theme the theme manager applies, rather than the application's requested theme, which only follows Windows.
- **Files:** `Module_Core/Helpers/UI/Helper_UI_ThemeTextBrush.cs`,
  `Module_Core/Converters/Converter_QualityHoldToTextColor.cs`,
  `Module_Core/Converters/Converter_PartIDToQualityHoldTextColor.cs`

#### An accepted Reconciliation line leaves the list, and its group header leaves with it (2026-11-13)

- **What changed:** once a line is accepted — or set by hand from **Needs Attention** — it is removed from the grid
  instead of staying there in its written state. The grid's groups are rebuilt from the rows that remain, so a
  part's header disappears with its last line. A day whose lines are all written now says so ("Every line saved for
  … is done.") instead of showing an unexplained empty grid.
- **Unchanged:** skipped lines still stay, the summary strip still counts written lines, and the day is unaffected —
  only what the grid shows changes.
- **Files:** `Module_Reconciliation/ViewModels/ViewModel_ReconciliationMain.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/ViewModels/ViewModel_ReconciliationMainReviewTests.cs`

#### Extra material in the receiving location no longer reads as this group's surplus (2026-11-13)

- **What changed:** a receiving line whose saved location is the group's own receiving location now reports
  **Unchanged** when the destination bucket at that same location received more than the rows account for. The
  rows were received into that location and no movement took them out, so the extra arrived on another receipt.
- **Still failing as before:** a bucket that ran below zero, and a bucket that received *less* than its rows need,
  keep their **Not Enough** verdict — those are about this material, not somebody else's.
- **Why:** `MMC0000652` (PO 071776) showed three `S-00` rows totalling 37,990 as **Extra Surplus** because 121,950
  of another transfer had been booked into `S-00` that morning. The rows never left `S-00`; they were already right.
- **Files:** `Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestrator.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestratorTests.cs`

#### Reconciliation's detail pane names the move for every line that has evidence (2026-11-13)

- **What changed:** **Moved by** and **Moved at** now fill in for surplus and shortfall lines. The finding model
  copies the movement time and Visual user off its evidence bucket, so the two fields no longer depend on which
  code path built the finding — only `Updated` lines used to receive them.
- **Why:** the `MMC0000667` pane showed **Moves 1** and the `RECV → V-C0-01` trail with both move fields blank.
- **Files:** `Module_Reconciliation/Models/Model_ReconciliationFinding.cs`,
  `Module_Reconciliation/Services/Service_ReconciliationPreviewOrchestrator.cs`

#### Reconciliation counts only transfers booked after the material was received (2026-11-13)

- **What changed:** the transfer evidence for a receiving line is now limited to Infor Visual movements posted at or
  after the line's own receipt time. Evidence comes from the earliest receipt stamp across the group's rows, so a
  transfer another user booked into the same destination earlier in the day no longer counts against these rows.
- **Why:** a `MMC0000089` line reported **Extra Surplus** because the destination bucket counted 12,020 received
  against 5,970 of rows. The surplus was `MLAURIN`'s earlier transfer into the same location, not this material.
- **Date-only records:** a movement whose stamp carries no time of day is kept. That is the date-only fallback used
  when Infor Visual has no create date, and dropping it would lose evidence for the wrong reason.
- **Text clipping:** the detail pane wraps its reason sentence inside the pane, and **Source** now has its own
  full-width row so "Derived from Infor Visual" is readable.
- **Files:** `Module_Reconciliation/Services/Service_ReconciliationMovementSelector.cs`,
  `Module_Reconciliation/Views/View_ReconciliationMain.xaml`,
  `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/Services/Service_ReconciliationMovementSelectorTests.cs`

#### The Receiving Validator launcher is one full-width card (2026-11-13)

- **What changed:** the launcher page now leads with its title and then the **Work a Receiving Day** card, which
  stretches the full page width instead of sitting in a narrow left column beside an empty right column.
- **Text removed:** the "from Infor Visual" line under the title and the "Compare a day of saved receiving
  locations …" paragraph on the right are gone, along with the now-unused `DescriptionText` property. The module
  title still comes from `ReconciliationHeaderTitleProvider`, so the shell header and the navigation entry keep
  reading **Receiving Validator / from Infor Visual**.
- **Files:** `Module_Reconciliation/Views/View_Reconciliation_Launcher.xaml`,
  `Module_Reconciliation/ViewModels/ViewModel_Reconciliation_Launcher.cs`

#### The scanner footer is one row, and card edges read in both themes (2026-11-13)

- **What changed:** the Scanner Workbench footer card is now a single row. The **"Was the transaction saved in
  Infor Visual?"** Yes/No prompt sits centred over the same bar as the batch buttons instead of in its own row
  underneath, so the card is one toolbar tall. The left and right button groups keep their exact positions.
- **Card edges:** the card stroke override in `App.xaml` is now 30% black in the light theme and 20% **white** in
  the dark theme. The previous dark value was a black overlay, which disappears against a dark card, so cards had
  no visible edge in the dark theme.
- **Where to tune it:** the two colours still live side by side in `App.xaml`; that remains the only place to
  change the strength of a card edge.
- **Files:** `App.xaml`, `Module_Scanner/Views/View_Scanner_Workbench.xaml`

#### Card borders are darker everywhere, and Label Overlay spans the mode chooser (2026-11-13)

- **What changed:** every card edge in the app is drawn with a darker stroke, and the **Label Overlay** button
  on the receiving mode chooser now runs the full width of the window instead of sitting under the third card.
- **How the borders changed:** `CardStrokeColorDefaultBrush` is overridden once, in `App.xaml`'s theme
  dictionaries — 20% black in the light theme (WinUI's default is 6%) and 30% black in the dark theme — so
  every card border that uses it deepens together, with no per-screen colour edits. The seven settings panels
  that drew their edge with `ControlStrokeColorDefaultBrush` now use the card stroke too, so no card is left
  pale, while text boxes and buttons keep their own control strokes.
- **Where to tune it:** the two colours live side by side in `App.xaml`; that is the only place to change the
  strength of a card edge.
- **Files:** `App.xaml`, `Module_Receiving/Views/View_Receiving_ModeSelection.xaml`,
  `Module_Settings.Receiving/Views/View_Settings_Receiving_PartFormatting.xaml`,
  `Module_Settings.Receiving/Views/View_Settings_Receiving_UserPreferences.xaml`

### Fixed

#### The lot list names the supplier instead of its Infor Visual id (2026-11-13)

- **What changed:** the **Supplier** column in Heat / Lot Management, the selected-lot summary, the printed
  **Heat / Lot Markings** sheet, and the **Update all lots** confirmation list now read the supplier name the
  receipt carried — e.g. **MST STEEL CORPORATION** — instead of the supplier group the lot is keyed on, which
  is the Infor Visual vendor id for most vendors.
- **What was actually saved:** nothing changed on the write side, and nothing needed to. A marking is keyed on
  `supplier_id`, which by design holds the official vendor id when the vendor lookup matches exactly one
  vendor, and `supplier_name` holds the text the receipt carried. Only the display was reading the key.
- **Rows recorded without a name:** a marking whose stored name is empty now takes the spelling its receiving
  lines carry when the list is read, so older id-keyed rows name the vendor too. A lot with no supplier still
  reads `NONPO`.
- **Decided:** the key itself stays as it is. Display only — no re-keying of stored markings, and no change to
  which value `supplier_id` holds.
- **Files:** `Module_ShipRec_Tools/Models/Model_Tool_LotMarking.cs` (`SupplierText`),
  `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingRealign.cs`,
  `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`,
  `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`,
  `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`,
  `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingRealignConfirm.xaml.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/Models/Model_Tool_LotMarking_SupplierTextTests.cs`

#### The overlay list leaves out tags that have no marking to print (2026-11-13)

- **What changed:** the **Overlays** tab now lists only the tags whose part carries a lot marking. A tag whose
  part gets no marking — every part except the ones enabled in the Heat / Lot Management tool — has nothing
  to overlay, so it is no longer offered with a tick that would be skipped at print time.
- **Why it mattered:** the list read every row in the label queue, so a receipt that saved one marked part
  still opened on a queue of unmarked parts. Only the marked tag the receipt just saved is ticked now, and
  the count above the list reports the tags that can actually be printed.
- **Also:** a tag from the receipt that somehow carries no marking is not ticked, so the count of ticked
  labels and the Print button always agree.
- **Files:** `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`,
  `Module_Receiving/Models/Model_ReceivingLabelOverlayItem.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlayPreselectionTests.cs`

#### The Print button wakes up when the receipt's labels are ticked for you (2026-11-05)

- **What changed:** opening the Label Overlay from a receipt now leaves **Print the marking** usable as soon
  as the window has loaded, instead of staying greyed out until a checkbox is touched.
- **Cause:** the labels from the receipt are ticked while the queue is still loading, and `CanPrint` reads
  `IsBusy`. The counts were last read before the load ended, so the button's `CanClickPrint` never heard
  the final answer. The load now re-reads the counts once it has finished.
- **Also:** clearing the printer or moving the marking off the page now reaches the button, because the
  printer and geometry properties raise `CanClickPrint` as well as `CanPrint`.
- **Files:** `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`,
  `MTM_Receiving_Application.Tests/Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlayPreselectionTests.cs`

### Changed

#### The transaction sheet shows how many skids or packages of the part came in (2026-11-06)

- **What changed:** on the printed **Material Availability Transaction Sheet**, the small `LOCATION` row
  above the quantity table is now a **Skids / Pkgs** count for that part, so the operator knows how many
  skids or packages to expect before writing any quantities. The page subtitle still names the warehouse
  location, so nothing about the move destination is lost.
- **Where the number comes from:** MySQL `receiving_history` through `sp_Receiving_History_Get`, counting
  the receiving labels (one per skid or package) whose `transaction_date` falls inside the chosen window.
  Infor Visual still supplies the part card itself.
- **Picking the window:** a new **Date Range** flyout on the board takes **From** and **To** dates. Leaving
  both empty counts the current date only; entering one date counts that day; entering them backwards swaps
  them. The button caption shows the active window, and **Use Today** clears it.
- **If the lookup fails** the row prints `n/a` and the sheet still prints; a part with no receipts in the
  window prints `0`.
- **Files:** `Module_ShipRec_Tools/Data/Dao_Tool_MaterialAvailabilityReceipts.cs` (new),
  `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`,
  `Module_ShipRec_Tools/Contracts/IService_Tool_MaterialAvailabilityBoard.cs`,
  `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs`,
  `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml(.cs)`,
  `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`,
  `Module_ShipRec_Tools/docs/MaterialAvailabilityTransactionSheet.md`

#### The colored symbol card wears a DevWinUI glowing border (2026-11-05)

- **What changed:** the card on the Review and finish pages is now wrapped in DevWinUI's `GlowingBorder`
  in **Beam** mode — a faint amber ring with a bright segment travelling around it, drawn with Win2D — so
  the reminder reads as live rather than as a static yellow banner.
- **Its padding was wrong at the edge:** the InfoBar template leaves no right padding on `Content`, so the
  caption ran to the card's edge with only a sliver below the button. The content now carries
  `Margin="0,4,16,10"` for a right and bottom inset that matches the rest of the card.
- **Hidden with the card:** the border takes the same `HasLotMarkingNeeds` visibility, so an empty glowing
  ring is never left behind on a receipt that needs no overlay.
- **Files:** `Module_Receiving/Views/View_Receiving_Review.xaml`, `Module_Receiving/Views/View_Receiving_Workflow.xaml`

#### The overlay screen is tidier, and its advanced setup moved behind a button (2026-11-05)

- **Advanced is now a window of its own.** The "where the marking lands" panel was an expander at the bottom
  of the tag list; it is now a modal window opened by an **Advanced** button on the footer, which sits left
  of Copies, Close and Print. Nothing is reloaded when it closes — the fields bind to the same view model.
- **The footer reads left to right:** Advanced, the busy ring, `Copies:` with the number, Close, then Print.
  The number box lost its header and its spin buttons (`SpinButtonPlacementMode="Hidden"`), so no arrows
  appear on hover or focus.
- **Print says Print.** The button used to say "Print the marking" or "Print the card sheet"; it now says
  **Print**, and it carries the accent fill because it is the primary action on the screen.
- **The tag list got its toolbar back:** Select All, Clear and Reload sit directly above the table with the
  new names, and the running "N tag(s) selected" total moved below the table on both tabs.
- **"Direction" is gone** from both sort toggle switches — the on and off captions already say it.
- **The printer settings page no longer overlaps itself.** Its grid declared four rows but the card-sheet
  note sat in a fifth, which WinUI folds into the last row: the note drew over the Paper tray picker and
  swallowed its clicks. The note is removed as asked, so the tray combo is clear and usable again.
- **Room to breathe:** the tab content and footer keep 40px at each side instead of 24, and the window is
  1060 wide instead of 900. The "eight cards to a page, 4 across and 2 down" line is gone from the bottom
  of the settings tab.
- **Files:** `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml(.cs)`,
  `Module_Receiving/Views/View_Receiving_LabelOverlayAdvancedDialog.xaml(.cs)` (new),
  `Module_Receiving/Views/View_Receiving_LabelOverlayAdvancedWindow.cs` (new),
  `Module_Receiving/Views/View_Receiving_LabelOverlayWindow.cs`,
  `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`

#### Retrieving your settings no longer walks the database two hundred and eighty times (2026-11-04)

- **What changed:** the startup settings pass now reads the settings tables **once per category** and
  resolves every key from memory, instead of asking the database for one key at a time. On this manifest
  that is roughly a dozen reads rather than 280, and it is the step the splash shows as
  *Preparing your settings (n of 280)*.
- **The resolving still happens in one place.** A new `ISettingsRowSnapshot` holds only the **raw** rows
  after a bulk read; `GetSettingQueryHandler` still does the decryption, the default-writing and the
  validation. Duplicating that logic to save queries would have risked handing callers ciphertext.
- **A primed snapshot is authoritative:** a row it does not hold is genuinely missing, so the handler
  writes the default as it always did, rather than going back to the database to check.
- **The warning toast about `Receiving.LabelOverlay.Overlay.ListFilter` was a real bug, and mine.** The
  four `Receiving.LabelOverlay.*` settings are `Json`-typed but were given an **empty** default. The
  handler validates a JSON value by parsing it, so `''` was rejected, "reset to default" wrote `''` back,
  and the same warning and two writes repeated on every single launch. Their defaults are now `{}`, which
  parses and repairs the stored `''` once on the next start.
- **Guarded against a repeat:** `SettingsManifestDefaultsTests` now fails the build if any manifest
  default does not parse as its declared type — the class of bug that turns a bad default into a
  permanent per-launch reset loop.
- **Files:**
  - `Module_Settings.Core/Interfaces/ISettingsRowSnapshot.cs`,
    `Module_Settings.Core/Services/Service_SettingsRowSnapshot.cs` (new)
  - `Module_Settings.Core/Services/GetSettingQueryHandler.cs` — resolves from the snapshot when primed
  - `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs` — primes before resolving
  - `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  - `Module_Settings.Core/Defaults/settings.manifest.json` — the four JSON defaults
  - `MTM_Receiving_Application.Tests/Unit/Module_Settings.Core/Services/SettingsManifestDefaultsTests.cs` (new)
  - `MTM_Receiving_Application.Tests/Unit/Module_Settings.Core/Service_SettingsCoreFacadeSeedTests.cs`

### Changed

#### Startup stops paying for every setting twice (2026-11-04)

- **What the log showed.** Every launch is timed now. In the last saved session the figures were
  `Startup timing` gaps of ~18 s spent on **Creating user session** and ~11 s of blank main window
  afterwards, against 0.6–1.8 s and ~0.2 s on a healthy launch the same morning. The same launch logged
  four `Failed to show error dialog … COMException … Window.get_Content()` failures, and wrote nothing at
  all to the Serilog file.
- **Root cause of the blank window:** the wording every screen shows is stored as **system-scoped**
  settings — 231 of the 280 in the manifest. The earlier fix for "stuck on creating user session" narrowed
  the startup seeding pass to the 49 user-scoped definitions, which made the splash quick but left all of
  the system-scoped values **cold**. Each screen then read its text one awaited key at a time, so the main
  window painted only after a long chain of database round trips. `InitializeDefaultsAsync` now resolves
  **both scopes** again, 12 at a time and while the splash is still up, so the cache is warm before the
  window appears and the "Preparing your settings (n of 280)" step shows honest progress.
- **The four error dialogs were a second, unrelated failure.** `Service_Window.GetXamlRoot()` read
  `App.MainWindow.Content`, which throws a `COMException` until the window has come up — exactly when
  startup is reporting. Every error raised during the splash therefore turned into a second failure.
  It now returns `null` when there is no root yet, and `ShowErrorDialogAsync` already handles that by
  logging one warning.
- **Timing is logged per section** (`MarkStartupSection`): main window shell, Dunnage image cache, user
  account check, workstation detection, user session and settings cache, version check, and the splash
  hand-off — each with its own milliseconds and a running total. A slow launch is now a number in the log
  rather than "a long pause".
- **The splash no longer moves.** The step row keeps a fixed height whether or not a step is running, and
  the status line keeps a fixed two-line height, so the main progress bar stays where it is for the whole
  launch instead of jumping as messages appear and clear.
- **Files:**
  - `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`
  - `Module_Core/Services/Service_Window.cs`
  - `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
  - `Module_Shared/Views/View_Shared_SplashScreenWindow.xaml`
  - `MTM_Receiving_Application.Tests/Unit/Module_Settings.Core/Service_SettingsCoreFacadeSeedTests.cs` (new)

### Changed

#### Every marking shape is drawn as a black silhouette (2026-11-04)

- **What changed:** the heat / lot marking shapes are emoji codepoints, so Windows rendered them in the
  emoji's own colours — a red heart, a gold star, a purple diamond — on the grids, in the reprint list, on
  the palette legend, in the change-marking dialog, and on the printed overlay and cut-out cards. A marking
  is a black shape on a colored circle, so all of them are now drawn as flat **black** glyphs.
- **The screens now ask for the monochrome outline.** WinUI paints colour-font glyphs in colour by default;
  `IsColorFontEnabled="False"` makes it use the font's single-colour outline instead. Applied to the Shape
  column of the Heat / Lot grid, the palette legend, the change-marking dialog, the reprint list, the
  receiving Review screen, and the receiving Edit-mode grid — where a `DataGridTextColumn` cannot carry the
  property, so that column became a `DataGridTemplateColumn`.
- **The print ink rule was wrong for two colours.** It used a per-channel average with a 0.35 cut-off,
  which rated `PURPLE` as dark and printed a *white* glyph on a purple circle. The rule is now the fill's
  WCAG relative luminance: black ink unless the fill is dark enough to swallow it, which in the palette
  means only `BLACK` takes a white glyph. Verified by rendering all twelve circles at 300 DPI — eleven now
  produce a true `(0,0,0)` glyph and `BLACK` alone produces white.
- **The printed Heat / Lot report** opened in a browser would paint the colour layers. Its shape column is
  now forced to black with `filter: brightness(0)`, which drops every channel and keeps the outline.
- **Not changed:** how a marking is chosen, stored, or worded. Only how it is drawn.
- **Files:**
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs` — `GlyphInkFor` is public and uses WCAG
    relative luminance; new `RelativeLuminance` and `MinFillLuminanceForBlackInk`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml`,
    `Module_ShipRec_Tools/Views/View_Tool_LotMarkingChangeDialog.xaml`,
    `Module_ShipRec_Tools/Dialogs/Dialog_LotMarkingPalette.xaml`
  - `Module_Reprint/Views/View_Reprint_ModulePage.xaml`,
    `Module_Receiving/Views/View_Receiving_Review.xaml`,
    `Module_Receiving/Views/View_Receiving_EditMode.xaml`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs` — report CSS
  - `Module_ShipRec_Tools/Models/Model_LotMarkingPalette.cs` — remark on monochrome rendering
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Helpers/Helper_LabelOverlayPrinterGlyphInkTests.cs`
    (new)
  - `specs/001-heat-lot-management/heat-lot-management.spec.md`,
    `specs/001-heat-lot-management/contracts/ui.md`

### Changed

#### The Label Overlay dialog is three tabs, each with its own filter (2026-11-04)

- **What changed:** the single scrolling page is now a **TabView** with three tabs — **Overlays**,
  **Cut-Out Cards**, and **Printer Settings** — so the thing being printed and the thing being set up are
  never on screen at the same time by accident.
- **Overlays tab** carries the tag queue, its own date range, its own sort and its own columns. The
  "Where the marking lands" panel is now an **Advanced settings** expander, collapsed by default.
- **Cut-Out Cards tab** first asks **which part number** to work on, listing only parts that actually carry
  a marking, then shows that part's lots using the same lot-list filter the Ship/Rec tool uses. Ticking a
  lot there works on a copy of the row, so a tick on the card sheet cannot arm a tag on the overlay tab.
- **Printer Settings tab** sets up one output at a time, chosen at the top. Overlay and cut-out keep
  separate printers, orientations, paper sizes, margins and page counts, and printer's own defaults are
  still never written.
- **Every filter and every sort is saved per user** in the settings database, per tab, so the two lists
  reopen the way they were left. A filter uses the same `CalendarDatePicker` range and sort list as the
  Heat / Lot Management tool.
- **The lists are now real columned lists** whose headings follow the column chooser: part number,
  heat / lot, shape, color, received, and description. Shape and color are separate columns, so a marking
  reads as its two halves rather than one combined phrase.
- **Removed:** the JSON print-settings store, and the `printui /e` printer-setup path. Both were superseded
  — the setup is per user in the database, and the printer dialog now edits a private copy of the driver's
  settings for the job only.
- **Files:**
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml` (+`.xaml.cs`) — three tabs, InfoBar,
    columned lists, advanced expander
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs` — `SelectedTabIndex`,
    `SettingsTargetIndex`, per-tab `_filters`, `Parts`/`SelectedPart`/`CardSheetLabels`,
    `SelectableItems`
  - `Module_Receiving/Models/Model_ReceivingLabelOverlayItem.cs` — `ReceivedDate`, `ShapeName`,
    `ColorName`, `ReceivedText`, `SelectionKey`, `CopyForCutOut`, per-column `Show*`
  - `Module_Receiving/Contracts/IService_ReceivingLabelOverlay.cs`,
    `Module_Receiving/Services/Service_ReceivingLabelOverlay.cs` — carry the receipt date through; dead
    print-settings and printer-setup members removed
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs` — dead `OpenPrinterSetup` removed
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Models/Model_ReceivingLabelOverlayItemTests.cs`
    (new)
  - Removed: `Module_Receiving/Helpers/Helper_LabelOverlayPrintSettingsStore.cs` and its tests

#### The print button shows what the print is doing (2026-11-04)

- **What changed:** pressing **Print the marking** used to freeze the dialog until the driver had taken the
  job, with nothing on screen to say anything was happening. The button is now a DevWinUI
  **ProgressButton**: a determinate progress bar across it and an animated ring at its side, with the text
  counting the pages ("Printing sheet 3 of 14…").
- **The progress is the printer's own pace**, not a timer: one step is reported each time the driver takes
  a page, so what the bar shows is how much of the job has actually been handed over.
- **The freeze is fixed at the cause:** the print ran synchronously on the UI thread, wrapped in an
  already-completed task, so the window could not repaint at all. It now runs on the thread pool, which is
  what lets the bar and the ring animate.
- **Files:**
  - `Module_Receiving/Models/Model_LabelOverlayPrintProgress.cs` (new)
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs` — reports a page per `PrintPage`
  - `Module_Receiving/Services/Service_ReceivingLabelOverlay.cs` — `Task.Run` around the device work
  - `Module_Receiving/Contracts/IService_ReceivingLabelOverlay.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs` — `IsPrinting`, `PrintProgress`,
    `PrintingStatus`
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml`

### Changed

#### Each Label Overlay output keeps its own printer setup, per user (2026-11-04)

- **What changed:** the tag overlay and the cut-out card sheet now keep **separate** printer setups, so the
  overlay can run from one tray while the cards run from another. Switching between them in the dialog shows
  that output's own settings rather than carrying the other one's across.
- **Stored per user, in the settings database.** The setup was previously a JSON file on the PC. Which tray
  and which media a person wants is a preference, not a property of the machine, so it now lives against the
  user like every other preference.
- **The Windows default printer settings are never touched.** The **Printer setup…** button used to open the
  printer's *global* preferences; it now opens the driver's dialog against a private `DEVMODE` block which is
  kept with that output and re-applied to each job. Tray, duplex and quality are therefore per output, and
  nothing chosen here can change how another program prints. A block saved against a printer that has since
  been swapped out is refused rather than forced.
- **This also addresses the printer not being remembered.** The saved choice was being overwritten by the
  printer-list rebuild and by a blank write that could land after a good one; both are closed, and the value
  now survives closing and reopening the dialog.
- **Files:**
  - `Module_Receiving/Helpers/Helper_PrinterDevMode.cs` (new)
  - `Module_Receiving/Contracts/IService_ReceivingLabelOverlaySettings.cs`,
    `Services/Service_ReceivingLabelOverlaySettings.cs` (new)
  - `Module_Receiving/Models/Model_LabelOverlayListFilter.cs`, `Model_LabelOverlayPrintSettings.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Settings/ReceivingSettingsKeys.cs`,
    `Module_Settings.Core/Defaults/settings.manifest.json`

### Fixed

#### Cut-out cards: every card boxed, every tile filled, and a print dialog to allocate copies (2026-11-05)

- **What changed:** a printed cut-out sheet now carries a **border on all four sides of every card**, all
  **eight cards are filled**, the grid is **sized to the page it is actually printed on**, and **Print
  opens the print dialog** so pages can be allocated and the printer's other settings changed before the
  job goes out.
- **Borders on the outer edges:** the grid used to be drawn as internal lines plus one rectangle around
  the whole sheet. A line drawn on the page edge is half outside the area the printer can mark, and that
  half is what went missing, so the outer cards came out with no border. Each card is now boxed in its own
  right, pulled in by half a pen width so no part of the stroke can be clipped.
- **All eight cards filled:** the sheet stopped drawing at the last selected lot, so one selected lot
  printed one card and seven blanks. A short run now repeats its cards to the end of the sheet, so one lot
  prints eight identical cards, and every selected lot still appears at least once.
- **Sized to the printed page:** the drawable area is now taken from the page the driver is about to mark
  (`Graphics.VisibleClipBounds`, in inches) and narrowed to that page, then the grid is stretched to fill
  it inside the trimming margin. A driver that reports one printable area and clips at another can no
  longer leave the sheet short of the right or bottom edge.
- **Print dialog, seeded with this output's settings:** **Print** now opens the printer's own dialog with
  the setup the current output was saved with — printer, tray, media, duplex, quality and the copies it
  was last printed with — so anything changed there starts from a known point. Cancelling sends nothing.
  What comes back is kept with this output and re-applied to later jobs; the printer's stored defaults are
  never written, so nothing chosen here leaks into another program.
- **Copies:** a **Copies** box in the print bar, 1 to 50, reading and writing whichever count the current
  output uses — the card sheet's sheets, or the tag run's copies — and the print dialog's copy count wins
  for that run. Card sheets are still repeated in code, so the driver is told one copy and cannot double
  the run.
- **Files:**
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs`
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrintDialog.cs` (new)
  - `Module_Receiving/Helpers/Helper_PrinterDevMode.cs`
  - `Module_Receiving/Models/Model_LabelOverlayPrintSettings.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Helpers/Helper_LabelOverlayPrinterCardSheetTests.cs`,
    `Unit/Module_Receiving/Models/Model_LabelOverlayPrintSettingsTests.cs`

#### Label Overlay: the chosen tray is now used, and the pages can be previewed (2026-11-05)

- **What changed:** the tray chosen in the printer setup is now what the app actually prints from, the
  printer settings block is read correctly, and **Print opens a preview** where the pages can be seen
  before any paper is spent.
- **Why the tray kept going back to Bypass Tray:** the app asked the printer driver to merge its saved
  settings back into the driver's own defaults before showing the setup dialog. The receiving room's
  Konica driver ignores that request — measured against it, a block asking for `Auto` and three copies
  came back as the printer's defaults, tray `4` (Bypass) and one copy — so the dialog always opened on
  the printer's default and the saved choice looked forgotten. The saved block itself was never wrong:
  applied to a print job, the same block feeds from `Auto`.
- **Tray is now a setting the app owns:** a **Paper tray** picker on the Printer Settings tab, filled
  from the trays the printer reports (`Auto`, `Tray1`…`Tray4`, `LCT`, `Bypass Tray` on the shop Konica),
  saved per output and named in the setup summary. At print time it is applied through the page settings,
  which is carried into the job's own settings block. Verified against `\\mtmanu-dc01\Receiving Printer
  Expo`: printer default tray `4`, job tray `7` (`Auto`).
- **The printer settings block was being misread:** the fields after `dmFields` are two bytes each in the
  SDK structure and were being read as four, which shifted every later field and turned a 1200 dpi
  resolution into "197808 copies" and a form name into "tray #5242881" — the nonsense in the saved-as
  message. The block now reads correctly (`landscape, 3 copies, tray #7, duplex long edge`), and reading
  it is what lets a tray chosen inside the driver's own dialog flow back into the app.
- **Print preview:** **Print** now shows the real pages — the same document the printer receives, drawn
  by the same code — with page navigation, zoom and page-count controls. **Print…** in the preview opens
  the print dialog on this output's own printer, tray, media and copies, so other settings can still be
  changed before the job goes, and the pages on screen are the pages that print. Closing the preview
  sends nothing. The preview is built from a `PrintDocument` the app configures, so the driver no longer
  reports "This app doesn't support print preview".
- **Printing is restartable for preview:** the page counters reset at the start of every pass, so the
  preview's render does not leave the print run starting on the wrong sheet.
- **Files:**
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs`
  - `Module_Receiving/Helpers/Helper_PrinterDevMode.cs`
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrintPreview.cs` (new)
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrintDialog.cs`
  - `Module_Receiving/Models/Model_LabelOverlayPrintSettings.cs`,
    `Model_LabelOverlayPaperSourceOption.cs` (new)
  - `Module_Receiving/Contracts/IService_ReceivingLabelOverlay.cs`,
    `Services/Service_ReceivingLabelOverlay.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Helpers/Helper_PrinterDevModeTests.cs` (new),
    `Unit/Module_Receiving/Models/Model_LabelOverlayPrintSettingsTests.cs`

#### Cut-out sheets are one full page per lot, never a mixed page (2026-11-05)

- **What changed:** the cut-out card sheet now prints **one full page for each selected lot**, repeated as
  many times as the duplicates box asks for. Three lots duplicated twice give six pages, each page eight
  identical cards — the first lot's two pages, then the second lot's two, then the third's.
- **Why:** the previous fix filled each sheet by repeating the run's cards, which produced a page mixing
  Black Cross, Orange Heart and Teal Triangle cards. Once those are cut up they cannot be told apart,
  and a mixed page cannot be handed to anyone as one lot.
- **One count instead of two:** the separate "Pages to print" and "Copies" boxes are now a single
  **Duplicates per lot** setting, saved with the rest of the output's setup and shown in the print bar.
  `CardSheetPages` is gone from the saved profile; an old profile simply loses that value and starts from
  one duplicate.
- **Status line:** a finished card run now reads back as *"3 lot(s) on 6 page(s), 2 page(s) of each lot,
  sent to …"*, so a stack off the printer can be counted against what was asked for.
- **Files:**
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs`
  - `Module_Receiving/Models/Model_LabelOverlayPrintSettings.cs`
  - `Module_Receiving/Services/Service_ReceivingLabelOverlaySettings.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Helpers/Helper_LabelOverlayPrinterCardSheetTests.cs`,
    `Unit/Module_Receiving/Models/Model_LabelOverlayPrintSettingsTests.cs`

#### Startup no longer stalls on "Creating user session" (2026-11-04)

- **What changed:** the splash screen could sit on **Creating user session…** for a very long time. Preparing
  a session made sure every setting in the manifest had a value, and it did that **one database round-trip
  at a time, for all 280 settings** — including the 231 that are system-wide and therefore never stored
  against a user. Each of those trips also passes through the logging and audit pipeline, so the cost was
  far more than the query itself.
- **The cause, in one line:** only the 49 user-scoped settings ever need a per-user row, and the rest were
  being asked for anyway.
- **The fix:** seeding now covers only what the current scope actually resolves — the user's own settings
  when a user is given, the system set when none is — and the queries run eight at a time. That is roughly
  forty times fewer waits than before, and the settings cache is a `ConcurrentDictionary`, so the parallel
  queries are safe.
- **Files:**
  - `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`
  - `Module_Settings.Core/Interfaces/IService_SettingsCoreFacade.cs`
  - `Module_Settings.Core/Models/Model_SettingsSeedProgress.cs` (new)

### Added

#### The splash screen shows what the current step is doing (2026-11-04)

- **What changed:** the splash screen keeps its main progress bar and percentage exactly as they were, so
  overall startup still reads the same. A **second, smaller progress bar** now sits under it with a line of
  text saying what the step in hand is actually doing — for example *"Preparing your settings (12 of 49)…"*.
- **Why:** a step that takes ten seconds used to be one unchanging message, which reads as a hang. The
  second bar makes progress visible inside the step rather than only between steps.
- **The step bar appears only while a step has something to report** and clears itself when the overall
  section moves on, so it never sits at 100% through the rest of startup.
- **Wired up for:** the Dunnage image sync (reading the folder, copying new images) and session creation
  (checking permissions, then each setting prepared).
- **Files:**
  - `Module_Shared/ViewModels/ViewModel_Shared_SplashScreen.cs`
  - `Module_Shared/Views/View_Shared_SplashScreenWindow.xaml`
  - `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`
  - `Module_Core/Contracts/Services/IService_UserLoginCoordinator.cs`,
    `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`
  - `docs/help/startup.html`

### Added

#### Label Overlay mode adds a cut-out card sheet and a saved print setup (2026-11-03)

- **What changed:** The **Label Overlay** mode on the Receiving Mode selection screen (right of
  Reconciliation) adds the lot marking to tags that were already printed. This release adds a second
  output — a sheet of eight cut-out marking cards — plus a print setup that is remembered on the PC.
- **Cut-out card sheet:** one 8.5 x 11 page holds eight cards, **four across and two down**, with dashed
  scissor lines between them and around the grid. Each card carries the **part number across the top**,
  the filled circle with its glyph and the words ("Blue Star") underneath, and the **lot along the
  bottom**. There is no "Lot Code" heading on a card. The cards are drawn inside the page margin rather
  than to the edge, so a printer that cannot reach the paper edge still prints the top and bottom lines,
  and more than eight cards continue onto a second page.
- **Print setup saved locally:** printer, format, orientation, paper size and the card-sheet margin are
  written to `%LOCALAPPDATA%\MTM_Receiving_Application\label-overlay-print.json` and restored the next
  time the screen opens. A local file rather than the settings database, because the values describe the
  printer standing next to the operator, not the operator. A missing, unreadable or hand-edited file
  falls back to the tag defaults instead of failing.
- **Landscape is now a setting, and the double rotation is fixed:** the overlay printed portrait because
  the code set a landscape-shaped paper size **and** the landscape flag, which asks the driver to turn a
  sheet that has already been turned. The paper is now always handed to the driver in its portrait form
  and the rotation is left to the landscape flag, which prints the 11 x 8.5 tag the right way round.
  Landscape remains the default; portrait is available for anyone who needs it.
- **Printer properties:** a **Printer setup…** button opens the printer driver's own dialog for tray,
  media and quality, which Windows remembers itself.
- **Advanced settings panel:** "Where the marking lands" has moved into a collapsed **Advanced settings**
  expander together with the print setup, so the screen opens on the printer, the format and the tag list.
  The expander's header still shows the current setup ("Letter · Landscape · 11" x 8.5"") without opening.
- **Black-on-black fixed:** a dark fill now gets a **white glyph**, so a black square on a black circle is
  readable instead of being an invisible shape on an invisible shape.
- **Files:**
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs`
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrintSettingsStore.cs`
  - `Module_Receiving/Models/Model_LabelOverlayPrintSettings.cs`, `Model_LabelOverlayPaperOption.cs`,
    `Model_LabelOverlayCard.cs`
  - `Module_Receiving/Enums/Enum_LabelOverlayPrintFormat.cs`
  - `Module_Receiving/Contracts/IService_ReceivingLabelOverlay.cs`,
    `Services/Service_ReceivingLabelOverlay.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml`, `View_Receiving_ModeSelection.xaml`

#### Pages to print, and the cut lines that were being clipped (2026-11-03)

- **What changed:** the first printed card sheet came back with only the top and left scissor lines, and
  no way to ask for more than one sheet. Both are fixed.
- **Missing right and bottom cut lines:** the grid was offset by the requested margin **and** sized from
  `MarginBounds` while the drawing origin already sat at the printable-area corner, so the last column and
  row fell past the paper's reachable edge and were clipped away. The drawing origin is now pinned to the
  page corner (`OriginAtMargins` with zero margins, which is what the measured tag geometry assumes) and
  the grid is laid out inside a rectangle resolved from the printer's own reachable area.
- **Pages to print:** a **Pages to print** box in the Advanced settings panel, 1 to 50, saved with the
  rest of the setup. **Every page carries the same cards**, so four pages of three markings gives four
  sheets to cut up rather than three blank ones. More than eight markings still continue onto further
  pages, and the pages setting multiplies the whole set.
- **Printable area is only believed when it agrees with the page:** several drivers — the Microsoft PDF
  printer among them — report the printable area in the paper's own orientation even for a landscape job.
  Taken at face value that would squeeze a landscape sheet to 8.5 inches across. It is now ignored when it
  is the wrong way round or larger than the page, so the requested margin is used instead.
- **Caption moved closer to the circle:** the words under the marking ("Orange Heart") sat eight percent of
  the diameter below the ring and crowded the tag's own description line. The gap is now four percent.
- **Shape glyphs centred on the circle:** the marking was drawn by centring the text's em box, and an em box
  carries leading above and below the ink plus side bearings either side, so every shape sat high and left
  of the ring — most visibly on the lightning bolt, moon, star and diamond. Each glyph's ink is now measured
  once and the drawing origin is offset by the difference, which centres the shape itself. The measurement
  is the same for the tag overlay and the cut-out cards because both draw through the same routine.
- **All shapes now draw at one size:** the ink is fitted to 60% of the circle's diameter, so a star and a
  square read at the same weight instead of however large the font happens to draw each character.
- **Blank sheets fixed:** the overlay printed correctly to PDF but came off the printer blank. The drawing
  origin had been moved with `OriginAtMargins`, which put every mark outside the printable area on the shop
  printers. Measured against both receiving printers, GDI+ already maps page inches one-to-one onto the
  page's own corner with the default origin, so the origin is now left alone and nothing is translated.
  Verified on the real drivers: the circle lands at 1.185" from the left and 4.075" from the top on both
  `Receiving Printer Expo` and `Material Handling Office`, to within a hundredth of an inch.
- **Refused rather than sent blank:** if the chosen printer has no media of the size the layout needs, the
  print is now refused with a message naming the printer's own paper, instead of feeding blank sheets.
- **Every print is now described in the log:** printer, paper, orientation, page bounds, printable area,
  hard margin and the resolved page size are written before the job goes out, so a blank sheet can be told
  apart from one laid out for a page that is not loaded.
- **Files:**
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrinter.cs`
  - `Module_Receiving/Helpers/Helper_LabelOverlayPrintSettingsStore.cs`
  - `Module_Receiving/Models/Model_LabelOverlayPrintSettings.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Views/View_Receiving_LabelOverlayDialog.xaml`

#### Heat / Lot Management gains a Filter window for dates, order and columns (2026-11-03)

- **What changed:** A **Filters** button in the tool's top card opens a modal window that decides which
  lots the list shows, in what order, and with which columns — and the printed sheet follows the same
  choices. Printing stays one click and reproduces what is on screen.
- **Dates:** two date pickers plus a **Quick date range** menu carrying the app's nine standard presets,
  built from one shared list so it cannot drift from Receiving Analytics. A lot is listed when any of its
  skids was received inside the range, and the **Skids** count and **Last received** date describe the
  skids inside that window.
- **Order:** most recently received, heat / lot number, most skids, supplier, part number, or shape and
  color, each with a highest/lowest switch and a heat-number tie-break.
- **Columns:** a checkbox per column, driving both the grid and the report from one catalog.
- **Saved per user in the database:** five new user-scoped `ShipRecTools` settings
  (`LotMarking.List.FromDate`, `.ToDate`, `.SortField`, `.SortDescending`, `.VisibleColumnKeys`). A saved
  range came back selected after a restart.
- **Database:** `sp_Receiving_HeatLotKeys_GetAll` now takes optional `p_from_date` and `p_to_date` and
  applies them before grouping. Both default to NULL and a NULL pair reads every row as before, so
  existing callers are unaffected. **The procedure must be deployed** using
  `Database/Scripts/Deploy/sp_Receiving_HeatLotKeys_GetAll_Deploy.sql`. Until it is, an older database
  silently ignores the parameters; the application filters the returned rows itself so the list is still
  correct, but the server-side counts are only exact once deployed.
- **Files:**
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarkingFilterWindow.cs`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarkingFilterDialog.xaml`, `.xaml.cs`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_LotMarking.cs`
  - `Module_ShipRec_Tools/Services/Service_Tool_LotMarking.cs`
  - `Module_ShipRec_Tools/Services/Service_ShipRecToolsSettings.cs`
  - `Module_ShipRec_Tools/Data/Dao_Tool_LotMarking.cs`
  - `Module_ShipRec_Tools/Helpers/Helper_DateRangePresets.cs`
  - `Module_ShipRec_Tools/Models/Model_Tool_LotMarkingFilter.cs`, `Model_Tool_LotMarkingSortOption.cs`,
    `Model_Tool_LotMarkingColumnCatalog.cs`
  - `Module_Settings.Core/Defaults/settings.manifest.json`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_HeatLotKeys_GetAll.sql`
  - `Database/Scripts/Deploy/sp_Receiving_HeatLotKeys_GetAll_Deploy.sql`

#### Receiving Analytics split into ERP Data and App Data tabs (2026-10-29)

- **What changed:** The Receiving Analytics page is now two tabs. **ERP Data** is today's page,
  unchanged, and stays the tab that opens. **App Data** is new: it reads the app's own
  `receiving_history` instead of Infor Visual and answers what the dock actually did. It carries its
  own date range and its own filters, so switching tabs does not disturb the other tab's state.
- **What App Data shows**, in the pinned order, one panel per page behind a navbar with the filter
  bar above it:
  **Activity** (loads, labels and quantity by category, switchable between day, week and
  month), **On Time** (early, on time and late against the PO due date, with the on-time
  percentage coloured against the configured target), **Suppliers** (supplier ranking
  by labels, quantity or loads, with the leading suppliers' share and a remainder), and **Coverage**
  (two receipt-count coverage lines over the same dates, and the app's
  category split beside Infor Visual's over the same five categories). Each panel owns the full area
  below the navbar, so none of them scrolls, and reloading or clearing leaves you on the page you
  were reading. Each page carries one plain sentence saying what its figures are read from.
  Saturdays and Sundays are not read: no weekend day appears on a day axis and weekend
  receipts count towards no total, with the Infor Visual side of the coverage chart dropping the same
  weekend dates. The Coverage page compares only the part numbers our own receipts carry for the
  selected dates, because Infor Visual records material that never flows through the app; on live data
  that limits a thirty-day comparison from 1,791 Infor Visual lines to 291. Both sides of the
  comparison count purchase-order lines on the date each was last received, the way Infor Visual dates
  a line, rather than counting labels: the same window holds 970 labels but only 271 PO lines, so
  labels would have reported 333 per cent coverage where the line unit reports 93. The category mix on
  the same page uses that same cohort and unit, and the ERP Data tab keeps reading all of Infor Visual.
- **Why:** Infor Visual knows what arrived at the plant; only the app knows what an operator
  completed and whether it was on time. The comparison panels are what prove the app is capturing
  the real work.
- **Notes:** Both halves of the category mix count receipts, one app label against one Infor Visual
  receipt line, which is the same alignment rule the coverage panel uses. App Data uses four new
  stored procedures in the `sp_Reporting_ReceivingAnalyticsApp_*` family; the app database stays
  read-only and Infor Visual is still opened with read-only intent. Nothing on the page names an
  employee. New setting `ReceivingAnalytics.OnTimeTarget`, administrator scope, default **95**
  per cent; with no value configured the percentage is shown plainly with no colour. Known limits
  are inherited from the unchanged ERP query.
- Files changed:
  - `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalytics.xaml`
  - `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs`
  - `Module_ShipRec_Tools/Services/Service_Tool_ReceivingAnalyticsApp.cs`
  - `Module_ShipRec_Tools/Data/Dao_Tool_ReceivingAnalyticsApp.cs`
  - `Module_ShipRec_Tools/Helpers/Translation_ReceivingAnalyticsCategory.cs`
  - `Module_ShipRec_Tools/Settings/ShipRecToolsSettingsKeys.cs`
  - `Module_Settings.Core/Defaults/settings.manifest.json`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay.sql`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay.sql`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount.sql`
  - `Database/Database_Deployment/Sql_Files/StoredProcedures/Reporting/sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals.sql`
  - `Module_ShipRec_Tools/docs/ReceivingAnalyticsAppDataTab_FeatureUpdate.md`

#### Packing list scan probe tool (2026-10-22)

- **What changed:** Added `tools/PackingListScanProbe`, a standalone .NET 10 console tool that
  OCRs scanned packing lists, bills of lading, ERP screenshots, and supplier emails using the
  in-box `Windows.Media.Ocr` and `Windows.Data.Pdf` APIs, and prints the part numbers, purchase
  orders, quantities, heats, dates, and warehouses it finds. Usage is documented in
  `PACKING_LIST_SCAN_PROBE.md`.
- **Why:** To evaluate scan-to-receive capture for the receiving workflow against real MTM
  documents before wiring any of it into the app. `docling-parse` was evaluated first and cannot
  do this: it parses born-digital PDFs and returns zero text on copier scans.
- **Notes:** The tool is not referenced by the solution and the app project excludes `tools\**`;
  `docling-parse` and full `docling` measurements are recorded in the new document. No app,
  schema, or stored procedure change.

#### WIP added to the dunnage Inventory Type options (2026-09-24)

- **What changed:** The **Add New Dunnage Part** and **Edit Dunnage Part** dialogs now offer
  **WIP** alongside Not Inventoried, Adjust In, Receive In, and Both.
- **Why:** Users need to flag dunnage that is consumed by work in progress instead of being
  received into stock.
- **Notes:** No schema or stored procedure change was needed.
  `dunnage_requires_inventory.inventory_method` is a free-text `VARCHAR(100)` column, and both part
  stored procedures only special-case blank and `NOT INVENTORIED`, so WIP is stored and shown
  as-is in the part details card, the review step, and saved history.
- Files changed:
  - `Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml`
  - `Module_Dunnage/Views/View_Dunnage_EditPartDialog.xaml`

#### Application windows open maximized (2026-09-10)

- Every top-level application window now opens maximized, so users no longer have to maximize
  windows manually. Applies to the main window, the Settings window, the Volvo shipment history
  detail window, and the shared icon selector.
- Added a `Maximize()` extension to `Module_Core/Helpers/UI/Helper_WindowExtensions.cs`, matching
  the existing `SetWindowSize`, `CenterOnScreen`, and `SetFixedSize` helpers rather than repeating
  presenter logic in each window constructor.
- Each window still sets an explicit pre-maximize size, so un-maximizing restores a sensible
  window size instead of the framework default.
- The new-user setup dialog is a `ContentDialog`, so it automatically fills the main window and
  needed no separate change.
- The splash screen is intentionally excluded. It declares
  `SetFixedSize(disableMaximize: true, disableMinimize: true)` for its branded startup layout, and
  forcing it to maximize would override that design decision.
- Files changed:
  - `Module_Core/Helpers/UI/Helper_WindowExtensions.cs`
  - `MainWindow.xaml.cs`
  - `Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs`
  - `Module_Shared/Views/View_Shared_IconSelectorWindow.xaml.cs`
  - `Module_Volvo/Views/View_Volvo_ShipmentHistoryDetailWindow.cs`

### Changed

#### Heat / Lot Management modal windows move to DevWinUI, and WindowsAppSDK moves to 2.4.0 (2026-11-03)

- **What changed:** The **Part numbers that receive markings** list and the **Change this marking**
  editor are now DevWinUI `ModalWindow` windows instead of hand-made `Window` hosts. Windows disables
  the main window for as long as either is open, so neither can be worked behind, and each shows the
  system title bar with a normal close button.
- **Why:** The previous hosts blocked the main window by switching its hit testing off and released
  that in a single close path, so a close that missed it left the app painted but deaf to every click
  and only a restart cleared it. They also extended content into the title bar without drawing caption
  buttons, leaving them faint and easy to miss.
- **Dependency bump:** `Microsoft.WindowsAppSDK` 2.2.0 -> **2.4.0**, which resolves WinUI 2.3.6, the
  version `DevWinUI` **10.9.1** requires. Runtime 2.4.0, DevWinUI 10.9.1 and DevWinUI.Base 10.9.1 are
  the resolved versions.
- **Rebuild note:** the first build after the bump produced an incomplete output folder and the app
  failed at startup with `Class not registered (0x80040154)`. A clean rebuild produced a complete
  output and the app started normally.
- **Files:**
  - `MTM_Receiving_Application.csproj`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarkingPartsWindow.cs`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarkingChangeWindow.cs`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarkingPartsDialog.xaml`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarkingChangeDialog.xaml`
  - `Module_ShipRec_Tools/Views/View_Tool_LotMarking.xaml.cs`

#### App Data range picks through the shared Date Range flyout (2026-11-03)

- **What changed:** The **App Data** tab of **Receiving Analytics** no longer puts two date pickers
  inline in the filter bar. Its range is now chosen in the same **Date Range** flyout the ERP Data tab
  and Delivery Schedule use: click **Date Range**, pick a From and To date or a quick **Date Type**
  preset, then click **Apply Date Range**.
- **Why:** Every other range control in the tool opens a flyout, so the App Data tab was the odd one
  out, and the inline pickers crowded the filter bar out of room for the period, unit and ranking
  controls.
- **Notes:** The presets are Last 7 Days, Last 30 Days, Last 90 Days, This Month, Last Month, This
  Quarter, Last Quarter, This Year and Last 12 Months. All of them look backwards, because the tab
  only reads receipts the app has already completed, and all of them stay bounded so a preset never
  asks for an open-ended read. The last day named is still included, and the range stays the App Data
  tab's own, so the ERP Data tab's dates never move.
- **Files:**
  - `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml`
  - `Module_ShipRec_Tools/Views/View_Tool_ReceivingAnalyticsApp.xaml.cs`
  - `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsApp.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/ViewModels/ViewModel_Tool_ReceivingAnalyticsAppTests.cs`
  - `Module_ShipRec_Tools/docs/ReceivingAnalyticsAppDataTab_FeatureUpdate.md`

#### Weight/Quantity boxes accept digits only and update Current Total while typing (2026-10-08)

- **Symptom:** On the guided **Receiving - Enter Weight & Quantity** step, the **Current Total**
  readout did not move until a box was committed (Enter, spin-button click, or clicking away), so
  users could not watch the running total while keying loads.
- **Cause:** Each load row used a `NumberBox` with `AcceptsExpression="True"`. `NumberBox` only
  commits `Value` on Enter, spin-button click, or focus change, so the bound
  `Model_ReceivingLoad.WeightQuantity` — and therefore `CurrentTotal` — stayed behind the typed
  text. `AcceptsExpression` also allowed expressions such as `2+3`, and unparseable text resolved
  to `NaN`, which the decimal converter silently turned into `0`.
- **Fix:** Each load row is now a `TextBox` (`InputScope="Number"`) bound per keystroke
  (`UpdateSourceTrigger=PropertyChanged`) through the new `Converter_DecimalToBlankText`, so
  `WeightQuantity` — and the `CurrentTotal` the view model already recalculates from
  `Model_ReceivingLoad.PropertyChanged` — update on every digit. A `BeforeTextChanging` handler
  cancels any keystroke containing a non-digit character, so an unparseable value can never reach
  the model and Current Total can never be computed from bad input.
- **Notes:** Weight/Quantity stays whole-number entry, matching the `Enter whole number`
  placeholder and the label-side rounding in `Dao_ReceivingLabelData`. The blank sentinel is `0`,
  so `Converter_DecimalToBlankText` renders `0` as an empty box, mirroring
  `Converter_IntToBlankText`. Focus-on-step-entry now targets the first `TextBox`.
- Files changed:
  - `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml`
  - `Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs`
  - `Module_Core/Converters/Converter_DecimalToBlankText.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Core/Converters/Converter_DecimalToBlankTextTests.cs`

#### Package Type counts start empty and update EA per Package while typing (2026-09-24)

- **Symptom:** The guided **Receiving - Enter Package Type** step opened with `1` already shown in
  every package-count box, so Auto-Fill had nothing blank to fill and users had to clear each box
  before typing real counts. The **EA per Package** readout also only changed after a box was
  committed (Enter, Tab, or clicking away).
- **Cause:** `Model_ReceivingLoad.OnPartIDChanged` forced `PackagesPerLoad` to `1` whenever a part
  ID was assigned, and the guided workflow assigns the part ID while it generates the loads. The
  readout then lagged because `NumberBox.ValueChanged` only fires on Enter, spin-button click, or
  focus change, and the `NumberBox` template never feeds typed text back to `Value`/`Text`.
- **Fix:** Assigning a part ID no longer seeds the package count, so counts stay at the blank
  sentinel (`0`) and the boxes render empty for Auto-Fill. Each package-count box is now a
  `TextBox` bound per keystroke (`UpdateSourceTrigger=PropertyChanged`), which makes
  `EA per Package` recalculate as digits are typed. Blank rows still fall back to a single package
  when the step is left (guided Package Type, or Manual Entry save), keeping the previous end
  result. Blank text handling comes from the new `Converter_IntToBlankText`, which maps `0` to an
  empty string and treats empty, unparseable, or non-positive text as `0`.
- **Trade-off:** a `TextBox` has no arrow-key stepping and no built-in numeric validation, so
  non-digit text leaves the count blank instead of being overwritten. This matches how the Manual
  Entry grid already handles the same column.
- Files changed:
  - `Module_Receiving/Models/Model_ReceivingLoad.cs`
  - `Module_Receiving/Views/View_Receiving_PackageType.xaml`
  - `Module_Receiving/Views/View_Receiving_PackageType.xaml.cs`
  - `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
  - `Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs`
  - `Module_Core/Converters/Converter_IntToBlankText.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Core/Converters/Converter_IntToBlankTextTests.cs`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Models/Model_ReceivingLoadTests.cs`

### Fixed

#### Maximized startup no longer flashes the theme or skips the startup page (2026-09-10)

- **Symptom:** After windows were changed to open maximized, the main window became visible
  during startup, visibly switched from the default theme to the saved theme, and opened on the
  empty `Dashboard` placeholder instead of the Receiving workflow mode-selection page.
- **Cause:** `MainWindow` maximized itself in its constructor, before `Application` activated the
  window. Maximizing a WinUI window that has not been activated yet makes the OS show it
  immediately, so it painted with the default theme while startup was still running. The early
  show also consumed the first activation, so the first `Activated` event that drives the startup
  navigation never reached the handler and `_hasNavigatedOnStartup` stayed `false`.
- **Fix:** Removed the constructor-level maximize and applied it in
  `Service_OnStartup_AppLifecycle` immediately after `App.MainWindow?.Activate()`. The window
  stays hidden until it is activated (restoring the previous theme timing and startup navigation)
  and is maximized in the same dispatcher turn, so it is never painted at the restore size.
- Files changed:
  - `MainWindow.xaml.cs`
  - `Module_Core/Services/Startup/Service_OnStartup_AppLifecycle.cs`

#### Ship/Rec Tools no longer resizes the main window (2026-09-10)

- **Symptom:** Opening Ship/Rec Tools from another module, or selecting any tool card, changed
  the size of the main window.
- **Cause:** `View_ShipRecTools_Main.UpdateActiveViewStatus` called `RestoreMainWindowSize()` on
  every visible-tool change, which resized the maximized main window back to 1450 x 900.
- **Fix:** Removed `RestoreMainWindowSize()` and its call sites. The resize was redundant — it
  re-applied the size the main window already sets during startup.
- Files changed:
  - `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs`

#### Receiving history delete no longer raises a false missing-row error (2026-09-10)

- **Symptom:** Removing loads from Receiving Edit Mode failed with
  `InvalidOperationException: No receiving history row matched the delete request for load '1'`.
  The failure surfaced to the user as `Failed to save receiving data`.
- **Impact:** The selected history rows were not deleted and the transaction rolled back, so no
  data was lost. The real cost was that receiving history rows could not be removed at all.
- **Cause:** `sp_Receiving_Load_Delete` ends with `SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;`.
  MySQL reports the row count of a stored procedure's **final** statement when the procedure is
  invoked, and `SET` always reports `0`. `Dao_ReceivingLoad.DeleteLoadsAsync` read
  `AffectedRows <= 0` as "no matching row" and threw, even though the preceding `DELETE` had
  succeeded.
- **Fix:** When `AffectedRows <= 0`, `DeleteLoadsAsync` now re-checks the row with
  `ReceivingHistoryRowExistsAsync` and throws only when the row is genuinely still present. This
  matches the guard already used by `UpdateLoadsAsync` in the same DAO and by
  `Dao_ReceivingLabelData`.
- **Files changed:** `Module_Receiving/Data/Dao_ReceivingLoad.cs`

### Added

#### Results pages say when a label still needs its colored symbol (2026-11-05)

- **What changed:** when a receipt's part number is on the list of parts that get the lot shape
  treatment, the results page now carries a card naming those labels, saying the tag has to be printed
  in **LabelView first** and then printed over with the colored symbol, and offering a button that opens
  the Label Overlay on them.
- **The card sits between the entry and the buttons** on the Review page, so it reads as a note about the
  receipt in hand rather than a warning pushed above everything else.
- **The card names the parts and lots, not every tag:** `Helper_ReceivingLotMarkingNeeds.Summarize`
  groups the labels by part and lists each lot once — *MMC0000848 · lots XRC123, XRC321, XRC698* — while
  the button says how many labels that comes to. A receipt of five tags of one part used to repeat the
  part number five times.
- **Where it shows:** the **Review** page, for the guided and non-PO flows, and the **finish** page, for
  every flow — manual entry saves straight from the entry grid and never shows Review, so the finish page
  is the first place it could appear.
- **The button on Review saves first:** the overlay reads the label queue, and a queued row only exists
  once the receipt is written, so **Save, then print the symbol over N labels** saves the receipt, waits
  for the save to finish, and only then opens the overlay. The lots are captured before the save because
  the workflow clears the session once the receipt is written. A save that fails opens nothing.
- **The button on the finish page** opens the overlay directly: the receipt is already saved there, so
  the tags are in the queue.
- **Auto-selection is by part and heat/lot together**, so a second lot of the same part is not ticked,
  and every label of the requested lot is. A tag that is no longer in the queue is simply not found, and
  the overlay says so rather than printing something else.
- **One place decides it:** `Helper_ReceivingLotMarkingNeeds` reads the enabled part list once and builds
  the list of labels, used by both pages. A list that cannot be read hides the card rather than guessing,
  because a wrong reminder sends an operator hunting for a tag that needs nothing.
- **Files:**
  - `Module_Receiving/Helpers/Helper_ReceivingLotMarkingNeeds.cs` (new)
  - `Module_Receiving/Models/Model_ReceivingLotMarkingNeed.cs` (new)
  - `Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs`,
    `ViewModels/ViewModel_Receiving_Workflow.cs`,
    `ViewModels/ViewModel_Receiving_LabelOverlay.cs`
  - `Module_Receiving/Views/View_Receiving_Review.xaml(.cs)`,
    `Views/View_Receiving_Workflow.xaml(.cs)`
  - `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Helpers/Helper_ReceivingLotMarkingNeedsTests.cs`
    (new), `Unit/Module_Receiving/ViewModels/ViewModel_Receiving_LabelOverlayPreselectionTests.cs` (new),
    `Unit/Module_Receiving/ViewModels/ViewModel_Receiving_WorkflowTests.cs`

### Added

#### Dunnage parts can be marked as needing no printed label (2026-11-05)

- **What changed:** a dunnage part can now be put on a **no printed label** list from the **New Part** and
  **Edit Part** screens. When a dunnage receipt is saved, any load whose part is on that list skips the
  label queue and is written **straight to `dunnage_history`** instead of `dunnage_label_data`, so nothing
  is left waiting in LabelView for a tag that will never be printed.
- **Why a separate table:** the flag lives in `dunnage_no_label_parts` rather than as a column on
  `dunnage_parts`, so the part insert and update procedures keep their existing parameter lists and the
  part row every other query reads stays exactly as it was. The flag row follows its part: renaming a part
  carries it along, and deleting the part removes it.
- **One save, one transaction:** the labelled loads and the no-label loads are written through two
  procedures that take the same parameters
  (`sp_Dunnage_LabelData_Insert` and the new `sp_Dunnage_History_Insert`) inside a **single transaction**,
  so a save lands whole or not at all whichever half a load belongs to. A row written straight to history
  is shaped exactly like one that was queued and archived later.
- **A list that cannot be read queues everything.** Marking is decided by
  `Helper_DunnageNoLabelParts.Split`, which matches part numbers ignoring case and surrounding spaces. If
  the list cannot be read, the save falls back to queueing every label, because a label that was queued
  need not have been but one that was skipped cannot be recovered.
- **The part details panel says so**, so an operator can see why no tag came out for a part.
- **Files:**
  - `Database/Database_Deployment/Sql_Files/Schemas/52_Table_dunnage_no_label_parts.sql` (new),
    `.../StoredProcedures/Dunnage/sp_Dunnage_NoLabelParts_GetAll.sql`,
    `sp_Dunnage_NoLabelParts_Set.sql`, `sp_Dunnage_History_Insert.sql` (new)
  - `.../StoredProcedures/Dunnage/sp_Dunnage_Parts_UpdateWithReferences.sql`,
    `sp_dunnage_parts_delete.sql`, `.../Schemas/31_Table_dunnage_label_data.sql`
  - `Module_Dunnage/Data/Dao_DunnageNoLabelPart.cs` (new),
    `Module_Dunnage/Helpers/Helper_DunnageNoLabelParts.cs` (new)
  - `Module_Dunnage/Data/Dao_DunnageLabelData.cs`, `Module_Dunnage/Services/Service_MySQL_Dunnage.cs`,
    `Module_Dunnage/Contracts/IService_MySQL_Dunnage.cs`
  - `Module_Dunnage/Models/Model_DunnagePart.cs`, `Model_DunnagePartDialogDraft.cs`,
    `Module_Dunnage/ViewModels/ViewModel_Dunnage_PartSelectionViewModel.cs`
  - `Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml(.cs)`,
    `View_Dunnage_EditPartDialog.xaml(.cs)`, `View_Dunnage_PartSelectionView.xaml`
  - `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`,
    `Database/Database_Deployment/sql-object-catalog.json`
  - `MTM_Receiving_Application.Tests/Unit/Module_Dunnage/Helpers/Helper_DunnageNoLabelPartsTests.cs`
    (new), `Unit/Module_Dunnage/Services/Service_MySQL_DunnageTests.cs`

## Verification Evidence

### Reported affected rows versus actual deletion

A throwaway probe ran against the test schema (`mtm_receiving_application_test`) to isolate the
affected-rows behavior of a `CALL`. Both variants deleted their target row.

| Call body                                                       | Rows reported by client | Rows actually remaining |
| --------------------------------------------------------------- | ----------------------- | ----------------------- |
| `SET FOREIGN_KEY_CHECKS=0; DELETE ...; SET FOREIGN_KEY_CHECKS=back;` | 0                       | 0                       |
| `DELETE ...;`                                                   | 1                       | 0                       |

### Deployed procedure matches the repository

The live `sp_Receiving_Load_Delete` definition was read from the database and compared against
`Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql`.
The trailing `SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;` is present in both, so the bug
is in the stored procedure's shape, not in a deployment drift.

### Introducing change

The trailing `SET` was appended by commit `1e4119e1` on 2026-07-20, which added foreign-key-check
handling across the Receiving and Volvo procedures. Any caller that infers row existence from the
affected-rows count of those procedures inherited the same defect.

## Related Risk

61 stored procedures under
`Database/Database_Deployment/Sql_Files/StoredProcedures/` end with a
`SET FOREIGN_KEY_CHECKS` restore rather than a data-modifying statement. Every DAO that calls one
of these procedures and branches on `AffectedRows` is exposed to the same false-negative result.

Call sites already guarded against this and known to be safe:

- `Dao_ReceivingLoad.UpdateLoadsAsync` — re-checks row existence when `AffectedRows <= 0`.
- `Dao_ReceivingLabelData` — re-checks row existence and falls back to an idempotent path.

`Dao_ReceivingLoad.DeleteLoadsAsync` was the only receiving delete path missing the guard and is
resolved by this change.

## Follow-Up

- [ ] Audit remaining DAO call sites that branch on `AffectedRows` for the 61 procedures with a
      trailing `SET FOREIGN_KEY_CHECKS` restore, prioritizing delete and update paths.
- [ ] Decide whether to fix the shape of the stored procedures themselves. Moving the
      `FOREIGN_KEY_CHECKS` restore into the `EXIT HANDLER` and terminating the body with the
      data-modifying statement would restore meaningful affected-rows reporting for every caller.
      This is a database-schema change and requires explicit approval before implementation.

## See Also

- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql`
- <https://keepachangelog.com/en/1.1.0/>
