# Outside Service Reporting Assumptions

1. Assumption: The new `Module_OutsideService` checkbox in Reporting should behave as one module selection in the main Reporting page, but generate three separate report tables/cards underneath it.
   Why this assumption is needed: The current Reporting architecture is `one checkbox -> one module data fetch -> one preview card/table`. Your request reads as `one checkbox -> three phase-specific tables`.
   Potential impact if wrong: I could either under-deliver by producing only one generic Outside Service table, or over-complicate the UI by splitting one module into multiple preview cards when you only wanted grouped rows inside a single card.
   Alternative interpretations considered:

- One Outside Service checkbox produces one preview card with one summary table and one detailed table filtered/grouped by phase.
- One Outside Service checkbox produces three separate preview cards/tables: `Waiting For Setup`, `In Completion`, and `Completed`.

2. Assumption: The three phase buckets should be based on current `line_phase` values from the Outside Service queue model.
   Why this assumption is needed: The request names the buckets behaviorally (`still waiting for setup mode`, `still in completion mode`, `rows completed within the date range`), but the implementation data uses `Initialize`, `Setup`, and `Complete` phase values.
   Potential impact if wrong: The report labels or included rows may not match the language your users expect.
   Alternative interpretations considered:

- Map phases directly as `Initialize`, `Setup`, and `Complete`.
- Relabel them in the report as `Waiting For Setup`, `In Completion`, and `Completed` while still using the stored phase values underneath.

3. Assumption: For open Outside Service rows, the date-range filter should use `created_utc`, while completed rows should still be included when `completed_utc` falls inside the selected date range.
   Why this assumption is needed: Your request explicitly mentions completed rows within the date range, but open rows do not have `completed_utc`, so another timestamp has to define their inclusion.
   Potential impact if wrong: The availability count and report rows may include too many or too few open lines.
   Alternative interpretations considered:

- Filter all rows only by `created_utc`.
- Filter completed rows by `completed_utc` and non-complete rows by `created_utc`.
- Filter setup rows by `scheduled_ship_utc` instead of `created_utc`.

4. Assumption: The Outside Service report detail columns should focus on queue/setup data such as request number, line number, part ID, package count/summary, vendor, BOL, scheduled ship date, contact, notes, and phase.
   Why this assumption is needed: The current Reporting module has module-specific default columns, but your request does not specify which columns the new Outside Service tables should show.
   Potential impact if wrong: The report may omit key operational fields or include noisy fields the team does not want.
   Alternative interpretations considered:

- Reuse the generic Reporting fallback columns.
- Add a module-specific Outside Service default column set tuned to the queue/setup workflow.

Please confirm or correct these assumptions before implementation continues. The two most important confirmations are:

1. Should one Outside Service checkbox generate one card with grouped content, or three separate phase-specific tables/cards?
2. For open rows, which date should control inclusion in the selected date range: `created_utc` or `scheduled_ship_utc`?
