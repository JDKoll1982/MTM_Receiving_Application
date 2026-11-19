/speckit-companion-specify Move page navigation out of the Receiving workflow pages into a persistent bottom action bar on the main window, and delete the Receiving mode-selection page.

CONTEXT
This is the MTM Receiving Application: WinUI 3 / .NET 10 desktop, MVVM (View -> ViewModel -> Service -> DAO -> Database), x:Bind only, CommunityToolkit.Mvvm, MySQL via stored procedures, Infor Visual SQL Server read-only. Read .github/copilot-instructions.md and the architecture instructions before planning.

TODAY
- MainWindow.xaml hosts the page content in a single Frame (ContentFrame) inside the NavigationView; the header bar (HeaderBarBorder, HeaderBackButton, HeaderStepBar, UserMenuButton) lives in NavigationView.Header and the label-shortcut footer (PrimaryLabelButtonsPanel) lives in the left pane's footer menu items. The shell already resolves a page's header title through IViewModel_HeaderTitleProvider and its step bar through IViewModel_HeaderStepBarProvider (see MainWindow.UpdateHeader / TrackHeaderStepBar); the action bar should follow that same resolve-and-track pattern.
- Each guided Receiving page draws its own navigation row inside Module_Receiving/Views/View_Receiving_Workflow.xaml (ReceivingWorkflowNavigationGrid): "Mode Selection", "Clear Label Data", Back (PreviousStepCommand), Next (NextStepCommand), and a Help icon.
- Module_Receiving/Views/View_Receiving_ModeSelection.xaml is the page that picks Guided / Manual / Edit / Overlay and lets the user tick a default. The per-user default is already stored via IService_UserPreferences.UpdateDefaultReceivingModeAsync ("guided" | "manual" | "edit").

GOAL
Add a shell action bar on the right-hand side of the main window, below the page content, so every page's navigation sits in the same place instead of inside the page. The action bar is shell-owned and must not reference any module's ViewModel types directly.

PLACEMENT (confirmed)
- The action bar belongs to the right side of the UI, directly under the page content. It does not replace, merge with, or move the label-shortcut footer (PrimaryLabelButtonsPanel); that footer stays where it is, in the left navigation pane.
- The two surfaces never compete for the same space: label files on the left, page navigation on the right.
- The action bar is sized to its content (Auto height) and collapses entirely - the content frame then expands to the full remaining height - when the active page supplies no actions.
- At compact widths the action bar wraps/scrolls horizontally instead of hiding buttons, so Back, Next, and Clear Label Data are never pushed out of reach.

ACTION BAR LAYOUT
Left-justified group (entry-mode buttons, shown for Receiving pages only):
1) "Guided" -> enter guided mode. A checkbox with no text (tooltip: make Guided my default) sits after it. Divider.
2) "Manual" -> enter manual mode. Checkbox with no text. Divider.
3) "Edit" -> enter edit mode. Checkbox with no text. Divider.
4) "Overlay" -> opens the label overlay window (View_Receiving_LabelOverlayWindow).

Mode-button rules (confirmed):
- The four mode buttons and their default checkboxes are hidden on every non-Receiving page. Nothing replaces them there - the left group is simply absent, and the bar shrinks to whatever the right group needs (and disappears when that is empty too).
- The three default checkboxes are mutually exclusive, like a radio group: ticking one clears the other two. Unticking the last remaining one is allowed and leaves no stored default, which falls back to Guided on the next launch.

Right-justified group:
5) "Back" - back arrow, returns to the previous page/step when the current page supports going back.
6) "Next" - advances when the current page supports it and its validation passes; disabled otherwise.
   Divider.
7) "Clear Label Data" - trash-can icon, clears the current user's label data (existing ResetLabelDataAsync / CanClearLabelData rules), red text.
8) "Help" - help for the current page (existing ShowHelpAsync).

PER-PAGE AVAILABILITY (Receiving; numbers refer to the list above)
- Enter PO Number: 1,2,3,4,6,7,8 (no Back)
- Select Part: 1,2,3,4,5,6,7,8
- Enter Load Information: 1,2,3,4,5,6,7,8
- Enter Weight & Quantity: 1,2,3,4,5,6,7,8
- Enter Heat & Lot: 1,2,3,4,5,6,7,8
- Enter Package Type: 1,2,3,4,5,6,7,8
- Manual Entry: 1,2,3,4,7,8, plus "Save & Finish" in the position Next normally occupies.
- Edit Mode: 1,2,3,4,7,8, plus "Save & Finish" in the position Next normally occupies.
- Review & Save: 1,2,3,4,5,7,8, plus "Save to Database" in the position Next normally occupies. Remove the "Add another Part/PO" button - it is never used.
- Result (Complete or Failed): 1,2,3,4,7,8, plus "Start New Entry" in the position Back normally occupies. Overlay (4) is included on the Result page. Remove the "Mode Selection" button.
- Any non-Receiving page: no mode buttons, no Receiving-only actions. The bar shows only what that page's own contract offers, and disappears when it offers nothing.

MOVED VS REMOVED - NO DUPLICATES (confirmed)
Every action the bar owns must be deleted from the page that used to draw it, so no control appears twice and no command is bound in two places.

- View_Receiving_Workflow.xaml: delete the whole in-page navigation row (ReceivingWorkflowNavigationGrid), including "Mode Selection", "Clear Label Data", Back, Next, and Help.
- View_Receiving_ManualEntry.xaml: delete the right-aligned navigation StackPanel that holds the "Save & Finish" button. Saving happens only from the bar.
- View_Receiving_EditMode.xaml: delete the "Save & Finish" button from the pager row (the Per page / paging controls stay). Saving happens only from the bar.
- Review & Save: delete "Add another Part/PO"; "Save to Database" moves to the bar's Next slot.
- Result (Complete or Failed): delete "Mode Selection"; "Start New Entry" moves to the bar's Back slot.
- Whatever stays on a page (Add Row, Add Multiple Rows, Remove Row, Auto Fill, file/paging/filter controls in Edit Mode) is not duplicated in the bar, and the bar never re-implements a page's command - it invokes the same command the page used.

REQUIREMENTS
- Delete Module_Receiving mode selection: the View and ViewModel, the ModeSelection workflow step, ReturnToModeSelectionCommand, its step-bar entry, and every "Mode Selection" button. Entering Receiving goes straight to the user's default mode. When no default is stored, Guided is assumed and also written back as the stored default, so Receiving never opens with "nothing selected".
- Clear Label Data appears on Manual Entry and Edit Mode as well as the guided walk, following the existing ResetLabelDataAsync / CanClearLabelData rules.
- Hide the shell header back button for now: HeaderBackButton stays collapsed on every page and no code path makes it visible, so Back exists only in the action bar. Leave IService_HeaderBackNavigation wired so re-enabling it later is a visibility change, not a re-plumb.
- The four mode buttons replace that page: clicking one enters that mode immediately (same behaviour the mode page performed). The checkbox beside a button is a separate control that only persists the per-user default; it must not navigate, and ticking one clears the other two.
- Shell must stay module-agnostic: define a contract (for example IViewModel_ShellActionProvider or an IService_ShellActions resolved by MainWindow) that a page's ViewModel implements or supplies so the shell can decide which of the eight buttons are visible and enabled for the current page, without MainWindow referencing Module_Receiving types. Register anything new in Infrastructure/DependencyInjection, not App.xaml.cs.
- Enable/disable rules must match today's behaviour, at minimum: Next disabled until the PO's part list is loaded on the PO step (IService_ReceivingWorkflow.IsPoPartListLoaded) and until a part is chosen on the part step; Back hidden where the step does not support it.
- Existing keyboard shortcuts (Ctrl+Left / Ctrl+Right for Back/Next, plus any others in View_Receiving_Workflow) must keep working and must respect the same visible/enabled rules as the buttons.
- Remove the old in-page navigation row from View_Receiving_Workflow.xaml; keep the receipt header card and step content untouched.
- Manual Entry and Edit Mode keep their editing toolbars (Add Row, Add Multiple Rows, Remove Row, Auto Fill, file/paging/filter controls) but lose their "Save & Finish" button to the bar's Next slot, with no duplicate left behind.
- Nothing that moves to the bar may remain on its page: no button, no command binding, no keyboard shortcut path that bypasses the bar's visible/enabled rules.
- All UI text that users can edit today must stay in settings (ReceivingSettingsKeys / IService_ReceivingSettings), including button labels and tooltips.
- No business logic in code-behind; Views bind to ViewModels with x:Bind.

CONFIRMED CLARIFICATIONS

1) No default mode
- Question: which page counts as "no default mode" now that mode selection is gone?
- Answer: Guided is the default. If the user has no stored default, Receiving opens in Guided and that choice is persisted as the default.

2) Mode buttons on non-Receiving pages
- Question: hidden, disabled, or replaced by each module's own actions?
- Answer: hidden. The mode group is not shown for other modules, and the action bar disappears / shrinks to auto size while another module is active.

3) Label-button footer vs the new action bar
- Question: replace the PrimaryLabelButtonsPanel footer, or sit in its own row?
- Answer: neither replaces the other. The label-shortcut footer stays in the left navigation pane, and the new action bar lives on the right side of the UI under the page content.

4) Default checkboxes
- Question: mutually exclusive (radio-like)?
- Answer: yes. Ticking one clears the others.

5) Clear Label Data outside the guided walk
- Question: shown on Manual Entry and Edit Mode?
- Answer: yes. Those Receiving pages get Clear Label Data too.

6) Overlay on the Result page
- Question: does the Overlay button belong on Result?
- Answer: yes, Result includes the Overlay button.

7) Shell header back button
- Question: keep Back available through HeaderBackButton as well, or header title/step bar only?
- Answer: remove the shell header back button - hide it for now. Back is offered only by the action bar.

8) Manual Entry and Edit Mode actions
- Question: what do those two Receiving modes get, and what happens to their Save button?
- Answer: buttons 1-4, 7 and 8, plus "Save & Finish" in the position Next would occupy. The Save & Finish button is removed from Manual Entry and from Edit Mode so it exists only in the action bar.

ACCEPTANCE CRITERIA
- Every Receiving page shows the same action bar position, with exactly the buttons listed in the matrix for that page.
- A user with no stored default opens Receiving in Guided mode, and Guided is persisted as their default at that point.
- The Receiving entry point opens the default mode with no mode-selection page anywhere in navigation, settings, or help.
- Ticking a mode's checkbox persists the default for that user and survives an app restart; it is reflected the next time Receiving opens.
- Only one default can be set at a time: ticking a second checkbox clears the first, and the stored value is a single mode.
- Clear Label Data is offered on Manual Entry and Edit Mode as well as on every guided-walk page, and follows the existing CanClearLabelData rules.
- Manual Entry and Edit Mode offer "Save & Finish" in the bar's Next slot, and neither page still shows a Save & Finish button of its own.
- No action appears twice: the old workflow navigation row, the Manual Entry Save & Finish, the Edit Mode Save & Finish, "Add another Part/PO", and "Mode Selection" are all gone from their pages.
- The Result page includes the Overlay button.
- Non-Receiving pages show no mode buttons; the action bar shrinks to auto size or disappears, and the page content then takes the full remaining height.
- The label-shortcut footer stays in the left navigation pane and is not merged into the action bar at any window width.
- HeaderBackButton is never visible on any page; the only Back control is the action bar's Back button.
- Next is disabled in the exact cases it is disabled today, with the same messages/validation.
- Review & Save offers "Save to Database" where Next normally sits; Result offers "Start New Entry" where Back normally sits; "Add another Part/PO" and "Mode Selection" buttons are gone.
- No module type is referenced from MainWindow for these actions; the shell depends only on the new contract.
- Build clean and the existing test suite green; add focused tests for the contract's visibility/enabled decisions per page.