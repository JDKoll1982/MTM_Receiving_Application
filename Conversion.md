/speckit-companion-specify Move page navigation out of the Receiving workflow pages into a persistent bottom action bar on the main window, and delete the Receiving mode-selection page.

CONTEXT
This is the MTM Receiving Application: WinUI 3 / .NET 10 desktop, MVVM (View -> ViewModel -> Service -> DAO -> Database), x:Bind only, CommunityToolkit.Mvvm, MySQL via stored procedures, Infor Visual SQL Server read-only. Read .github/copilot-instructions.md and the architecture instructions before planning.

TODAY
- MainWindow.xaml has an inner grid (around line 217) with two rows: the header bar (HeaderBarBorder, HeaderBackButton, HeaderStepBar) and the content frame (ContentFrame). The shell already resolves a page's header title through IViewModel_HeaderTitleProvider and its step bar through IViewModel_HeaderStepBarProvider (see MainWindow.UpdateHeader / TrackHeaderStepBar).
- Each guided Receiving page draws its own navigation row inside Module_Receiving/Views/View_Receiving_Workflow.xaml (ReceivingWorkflowNavigationGrid): "Mode Selection", "Clear Label Data", Back (PreviousStepCommand), Next (NextStepCommand), and a Help icon.
- Module_Receiving/Views/View_Receiving_ModeSelection.xaml is the page that picks Guided / Manual / Edit / Overlay and lets the user tick a default. The per-user default is already stored via IService_UserPreferences.UpdateDefaultReceivingModeAsync ("guided" | "manual" | "edit").

GOAL
Add a third row to the right-hand content column of the main window - header bar, page content, then a shell action bar - so every page's navigation sits in the same place instead of inside the page. The action bar is shell-owned and must not reference any module's ViewModel types directly.

ACTION BAR LAYOUT
Left-justified group (entry-mode buttons, shown for Receiving pages only):
1) "Guided" -> enter guided mode. A checkbox with no text (tooltip: make Guided my default) sits after it. Divider.
2) "Manual" -> enter manual mode. Checkbox with no text. Divider.
3) "Edit" -> enter edit mode. Checkbox with no text. Divider.
4) "Overlay" -> opens the label overlay window (View_Receiving_LabelOverlayWindow).

Right-justified group:
5) "Back" - back arrow, returns to the previous page/step when the current page supports going back.
6) "Next" - advances when the current page supports it and its validation passes; disabled otherwise.
   Divider.
7) "Clear Label Data" - trash-can icon, clears the current user's label data (existing ResetLabelDataAsync / CanClearLabelData rules), red text.
8) "Help" - help for the current page (existing ShowHelpAsync).

PER-PAGE AVAILABILITY (Receiving guided walk; numbers refer to the list above)
- Enter PO Number: 1,2,3,4,6,7,8 (no Back)
- Select Part: 1,2,3,4,5,6,7,8
- Enter Load Information: 1,2,3,4,5,6,7,8
- Enter Weight & Quantity: 1,2,3,4,5,6,7,8
- Enter Heat & Lot: 1,2,3,4,5,6,7,8
- Enter Package Type: 1,2,3,4,5,6,7,8
- Review & Save: 1,2,3,4,5,7,8, plus "Save to Database" in the position Next normally occupies. Remove the "Add another Part/PO" button - it is never used.
- Result (Complete or Failed): 1,2,3,4,7,8, plus "Start New Entry" in the position Back normally occupies. Remove the "Mode Selection" button.

REQUIREMENTS
- Delete Module_Receiving mode selection: the View and ViewModel, the ModeSelection workflow step, ReturnToModeSelectionCommand, its step-bar entry, and every "Mode Selection" button. Entering Receiving goes straight to the user's default mode (or a sensible fallback - see Open Questions).
- The four mode buttons replace that page: clicking one enters that mode immediately (same behaviour the mode page performed). The checkbox beside a button is a separate control that only persists the per-user default; it must not navigate.
- Shell must stay module-agnostic: define a contract (for example IViewModel_ShellActionProvider or an IService_ShellActions resolved by MainWindow) that a page's ViewModel implements or supplies so the shell can decide which of the eight buttons are visible and enabled for the current page, without MainWindow referencing Module_Receiving types. Register anything new in Infrastructure/DependencyInjection, not App.xaml.cs.
- Enable/disable rules must match today's behaviour, at minimum: Next disabled until the PO's part list is loaded on the PO step (IService_ReceivingWorkflow.IsPoPartListLoaded) and until a part is chosen on the part step; Back hidden where the step does not support it.
- Existing keyboard shortcuts (Ctrl+Left / Ctrl+Right for Back/Next, plus any others in View_Receiving_Workflow) must keep working and must respect the same visible/enabled rules as the buttons.
- Remove the old in-page navigation row from View_Receiving_Workflow.xaml; keep the receipt header card and step content untouched.
- All UI text that users can edit today must stay in settings (ReceivingSettingsKeys / IService_ReceivingSettings), including button labels and tooltips.
- No business logic in code-behind; Views bind to ViewModels with x:Bind.

OPEN QUESTIONS TO RESOLVE BEFORE OR DURING SPECIFY
- Which page in the app counts as "no default mode" now that mode selection is gone - should Receiving fall back to Guided, or should the action bar start with nothing selected until the user picks a mode?
- Do the four mode buttons appear on non-Receiving pages (hidden, disabled, or replaced by that module's own actions)?
- Does the new action bar replace the existing label-button footer (PrimaryLabelButtonsPanel in MainWindow) or sit in its own row above/below it? Where do both fit at compact widths?
- Are the default checkboxes mutually exclusive (radio-like: ticking one clears the others)?
- Should "Clear Label Data" appear on Manual Entry and Edit Mode pages, which are Receiving pages but not part of the guided walk?
- Does the Overlay button belong on the Result page (the matrix above excludes it)?
- Should Back/Next remain available through the existing shell header back button (HeaderBackButton) as well, or does the header keep only its title/step bar?

ACCEPTANCE CRITERIA
- Every Receiving page shows the same action bar position, with exactly the buttons listed in the matrix for that page.
- The Receiving entry point opens the default mode with no mode-selection page anywhere in navigation, settings, or help.
- Ticking a mode's checkbox persists the default for that user and survives an app restart; it is reflected the next time Receiving opens.
- Next is disabled in the exact cases it is disabled today, with the same messages/validation.
- Review & Save offers "Save to Database" where Next normally sits; Result offers "Start New Entry" where Back normally sits; "Add another Part/PO" and "Mode Selection" buttons are gone.
- No module type is referenced from MainWindow for these actions; the shell depends only on the new contract.
- Build clean and the existing test suite green; add focused tests for the contract's visibility/enabled decisions per page.