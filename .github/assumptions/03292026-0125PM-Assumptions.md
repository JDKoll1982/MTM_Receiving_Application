# Reporting Preview Grouping And Tabs Assumptions

1. The current raw per-row detail view remains available as a default option, and the four new combine modes are added in addition to it.
Why this assumption is needed: The request asks to add combine options but does not explicitly say to remove the current row-per-record view.
Potential impact if wrong: The UI would expose an extra mode the user does not want.
Alternative interpretations considered: Replace the current raw view entirely with only the four new modes.

2. The new row-display mode changes the Detailed Activity grid and the copied Detailed Activity output, but it does not replace the existing summary-table logic.
Why this assumption is needed: The request focuses on how rows are shown, while the current summary table is a separate feature with existing daily aggregation logic.
Potential impact if wrong: The summary section may no longer match the user’s intended grouping expectations.
Alternative interpretations considered: Apply the grouping mode to the summary table too, or only to the on-screen detail grid.

3. Grouping is always performed within each selected module tab, never across different modules.
Why this assumption is needed: The preview and copy flow are already module-scoped, and combining across modules would materially change the report structure.
Potential impact if wrong: Users wanting cross-module merged results would still see module-isolated groupings.
Alternative interpretations considered: Merge Receiving, Dunnage, and Volvo rows into shared grouped results when keys match.

4. When grouped rows contain mixed text values outside the active grouping keys, the grouped detail row shows blanks for those mixed fields instead of picking an arbitrary source row.
Why this assumption is needed: A deterministic rule is required for fields like PO number, employee, vendor, and notes when several raw rows collapse into one displayed row.
Potential impact if wrong: Some grouped columns may appear sparse when the user would prefer a first-row value or a "Multiple" marker.
Alternative interpretations considered: Use the first row’s value, or show a literal "Multiple" value for mixed text fields.

5. Lot-based grouping modes place rows without a lot value into a "No Lot" group instead of excluding them.
Why this assumption is needed: The request does not define missing-lot behavior, and exclusion would silently drop visible data.
Potential impact if wrong: Users may see a bucket they did not expect.
Alternative interpretations considered: Exclude rows without lot numbers or disable lot-based grouping when any selected module lacks lot support.

6. Part-based and lot-based grouping primarily target modules with meaningful part-number data; modules without useful part-number semantics continue to render their raw detail rows under those modes.
Why this assumption is needed: Dunnage data does not appear to align cleanly with the requested part-number-based grouping behavior.
Potential impact if wrong: Dunnage may stay ungrouped when the user expects a module-specific grouping translation.
Alternative interpretations considered: Force all modules through the same grouping rules, or disable unsupported modules when a part/lot grouping mode is selected.

7. The large vertical per-module preview stack is replaced with tabbed module navigation in the preview dialog, and the options experience also uses tabs for per-module column choices.
Why this assumption is needed: The request calls for tabs per selected module instead of a large scroll wheel, and the current options overlay also becomes long when multiple modules are selected.
Potential impact if wrong: The implementation may change more UI surface than intended.
Alternative interpretations considered: Use tabs only in the main preview area and keep the options dialog as a scroll-based overlay.

8. A single global "Options" button in the preview dialog replaces the current "Customize Preview" entry point and owns both row-display selection and per-module column selection.
Why this assumption is needed: The request refers to changing the button that brings up column selection into an Options button that includes the new logic.
Potential impact if wrong: Users may want per-module shortcuts that remain visible next to each detail grid.
Alternative interpretations considered: Keep both a global button and per-module shortcuts.

Please confirm, correct, or clarify these assumptions. The current implementation work is proceeding against this baseline because you asked to start implementation, but any of the points above can still be adjusted if needed.