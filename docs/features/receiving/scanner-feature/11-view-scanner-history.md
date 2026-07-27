# Scanner History View

Last Updated: 2026-07-21

This view presents completed and interrupted sends, with drill-down into item-level results.

## View Purpose

- support user review after incomplete send or stop
- provide confidence before manual retry of waiting items
- provide auditable history of sent versus failed outcomes

## Required Regions

- Filter bar region: date range, status, user, search trigger
- Send list region: history entries with status summaries
- Send detail region: selected send metadata and item results
- Export region: summary export and support handoff notes

## Textual Mockup

Top Filter Bar:

- Date From
- Date To
- Status dropdown
- User dropdown
- Apply and Clear actions

Middle Split Layout:

- Left panel send list showing Time, Operator, Status, Items Sent, Failed, Waiting
- Right panel selected send details showing profile, app window snapshot, child screen context, stop reason, failure summary

Bottom Detail Table:

- item results table with columns for #, Item ID, From Warehouse, From Location, To Warehouse, To Location, Status, Attempts, Issue Type, Details

## Interaction Notes

- selecting a send loads item results on demand
- incomplete sends highlighted for quick triage
- immutable sent records clearly labeled as non-editable history
- history details must be sufficient for operator retry decisions without returning to the original workbench session

## Binding And UI Rules

- use x:Bind for filters, list selection, and commands
- keep formatting logic in view-model projections when possible
- keep detail rendering passive and data-driven

## Use Or Modify Guidance

Use existing:

- shared list and details view patterns used in other receiving/reporting pages
- shared status messaging for load and filter failures

Modify or extend:

- receiving module route map for history page
- scanner history query service to support this view's filter model

Do not modify:

- workbench active run state from history interactions
