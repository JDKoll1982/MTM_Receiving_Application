# Research: Customer Pull n' Pack Tool

Date: 2026-05-25
Branch: 001-customer-pull-pack

## Decisions

### Decision: Implement the feature as a Module_ShipRec_Tools CQRS vertical slice
Rationale: The constitution requires mediator-first workflows, ViewModel purity, and module-local business logic. The feature naturally breaks into report queries, waitlist commands, queue queries, queue status commands, and print-context queries.
Alternatives considered: Expanding a large multi-method service layer; pushing workflow logic into Views or code-behind.

### Decision: Keep location selection on the report screen and use SUB PARTS ON HAND rows as the selector
Rationale: Review outcomes and mockup updates established the report as the comparison surface for source lines and locations. This reduces back-and-forth with the waitlist window and keeps the waitlist editor focused on confirmation and request details.
Alternatives considered: Location selection inside the waitlist window; a second parallel selector panel as the primary control surface.

### Decision: New requests start with no locations selected
Rationale: Auto-selecting every available location silently overcommits the request context. Starting unselected makes the requester explicitly choose the pull source. Existing linked waitlist work may still preselect saved locations when the user is updating rather than creating.
Alternatives considered: Auto-select all visible locations; auto-select the primary FG location only.

### Decision: Prevent duplicate open waitlist items per source report line
Rationale: One open waitlist item per source line keeps ownership, queue state, and audit trails unambiguous. When duplicate creation is attempted, the system should show a clear message and route the user to the existing item.
Alternatives considered: Allowing multiple open requests for the same source line; warning only while still saving a duplicate.

### Decision: Problem status retains the current owner until explicit handoff or un-assignment
Rationale: The review guide now resolves this as the last ownership edge case. Keeping ownership stable preserves accountability while still allowing explicit handoff through un-assignment.
Alternatives considered: Automatically clearing ownership when a line enters Problem; requiring immediate reassignment on save.

### Decision: Persist waitlist records separately from source ERP demand
Rationale: The constitution and spec both require MTM-managed operational records that remain linked to source lines but do not mutate ERP demand. This implies MySQL-backed MTM persistence with stored procedures and read-only Visual demand queries.
Alternatives considered: Writing execution state back to Infor Visual; embedding queue-only state inside report projections.

### Decision: Model print output as a derived read model from current report or queue context
Rationale: Print workflows depend on current filters, chosen locations, and queue state. A derived read model keeps output deterministic and lets floor copy and pull-list rendering remain independent from what happened to be expanded on screen.
Alternatives considered: Printing only currently visible UI state; recomputing print data inside the View layer.

### Decision: Keep the filter surface collapsed by default but leave the main report actions visible in the header
Rationale: The report page is width-heavy. Collapsing the filter body preserves vertical space, while keeping actions in the expander header avoids hiding the primary report workflow.
Alternatives considered: Flat always-open filter card; separate toolbar above the report; report actions placed below the report surface.

### Decision: Host the in-app report as an embedded crystal-style user control aligned to the legacy grouped layout
Rationale: The review and screenshot pass established that the app view must visually track the legacy crystal report more closely than the earlier simplified grid. An embedded control isolates that layout work from the rest of the page.
Alternatives considered: Keeping the simplified flat grid; reproducing the report directly inside the page without a dedicated control.

### Decision: Use row-highlight selection affordances for request lines and `SUB PARTS ON HAND` rows
Rationale: Review feedback rejected checkbox-first affordances. The whole row is now the intended interaction surface, and selection should be communicated through row highlight.
Alternatives considered: Checkbox columns for both line and location selection; hybrid checkbox plus row highlight treatment.

### Decision: Widen the main shell while Customer Pull n' Pack is active and restore the default size on exit
Rationale: The grouped report needs more horizontal space than the default Ship/Rec host. The shell should widen for this tool but return to the normal application size when the user leaves it.
Alternatives considered: Forcing the report into the default shell width; keeping the wider shell after the user leaves the tool.

### Decision: Treat the current crystal-style report control as a temporary visual-alignment step until live bindings replace sample data
Rationale: The user control currently exists to settle layout and interaction cues, but final implementation still needs real grouped report data and real selection state.
Alternatives considered: Blocking all UI alignment work until the final grouped projection was available; allowing the preview surface to stand indefinitely without explicit follow-up work.

## Workflow Analysis

Mermaid diagrams for these workflows are generated and maintained in the specification. If regeneration tooling is needed, use MermaidProcessor.ps1 -Action Generate.

### Workflow Relationship Validation
- 1.1 and 1.2 are valid mutually exclusive outcomes and correctly conflict with each other.
- 2.1, 2.2, and 2.3 all depend on 1.1 and that dependency exists.
- 3.1 and 3.2 depend on 2.1 and that dependency exists.
- 4.1 depends on 1.1 and 3.1 and both dependencies exist.
- No invalid DEPENDS_ON references were found.
- No missing bidirectional conflict pair remains in the current workflow set.

### Recommended Implementation Order
1. Workflow 1.1 and 1.2: report query, one-customer scope, filters, no-demand state.
2. Workflow 2.1: report selection state, main-screen location selection, batch waitlist upsert.
3. Workflow 2.3: duplicate open-request detection and redirect message path.
4. Workflow 2.2: update existing linked waitlist context with reused saved locations.
5. Workflow 3.1: dedicated queue, ownership transitions, status updates.
6. Workflow 3.2: Problem reason enforcement and owner retention on Problem.
7. Workflow 4.1: defaults restoration and print generation.

### Workflow 1.1: Review Customer Demand And Shortages
UI components required: customer picker, date range inputs, shortage filters, sort controls, grouped report surface, linked-waitlist indicators.
State management needs: selected customer, filter state, sort state, visible row set, no-demand/result mode.
User interactions and event handlers: change customer, adjust filters, change sort, refresh report.
Data flow patterns: read-only query from Visual-backed demand + MTM waitlist linkage overlay.
Error handling requirements: clear no-demand state; non-blocking load failure messaging.

### Workflow 1.2: Handle Customer With No Open Demand
UI components required: no-demand empty state, reset filters action, change customer action.
State management needs: valid customer context with empty results.
User interactions and event handlers: change customer, reset filters, go to settings or back to report.
Data flow patterns: same demand query path as 1.1 with zero results.
Error handling requirements: distinguish true empty state from system failure.

### Workflow 2.1: Create Waitlist From Selected Report Lines
UI components required: report-line multi-select, SUB PARTS ON HAND location buttons, waitlist editor, batch save action.
State management needs: selected source line keys, selected location keys, per-batch customer/parent-part guard, derived requested quantity.
User interactions and event handlers: select line, toggle location, open editor, save batch.
Data flow patterns: source line context -> request editor -> one waitlist record per selected source line.
Error handling requirements: mixed-group validation, no-location exception handling with required note.

### Workflow 2.2: Update Existing Linked Waitlist Work
UI components required: report indicators for linked lines, update action, editor with existing context.
State management needs: current linked waitlist context, saved location reuse, editable requester vs handler fields.
User interactions and event handlers: open existing context, revise details, save update.
Data flow patterns: source line lookup -> linked waitlist fetch -> update command.
Error handling requirements: stale/missing linked item handling; field-level ownership restrictions.

### Workflow 2.3: Block Duplicate Open Waitlist Request
UI components required: duplicate warning message, open-existing-item action.
State management needs: source-line-to-open-waitlist lookup, existing waitlist reference.
User interactions and event handlers: start create action, show duplicate message, navigate to existing item.
Data flow patterns: duplicate check before create command.
Error handling requirements: clear conflict message with deterministic redirect path.

### Workflow 3.1: Work Open Waitlist Queue Item
UI components required: queue filter bar, queue grid/list, detail pane, status actions, owner display, unassign action.
State management needs: selected queue item, queue filters, current owner, audit metadata.
User interactions and event handlers: filter queue, select item, save status, unassign owner.
Data flow patterns: MTM waitlist query -> selected item detail -> status update command.
Error handling requirements: save conflicts resolve last-write-wins while preserving audit fields.

### Workflow 3.2: Save Problem Status With Required Reason
UI components required: Problem reason selector, freeform note field, save action.
State management needs: chosen reason, fallback note, retained owner.
User interactions and event handlers: set Problem, choose preset reason, enter note when needed, save.
Data flow patterns: queue item -> Problem validation -> status update.
Error handling requirements: block save without valid reason/note; preserve current owner on Problem.

### Workflow 4.1: Resume With Saved Defaults And Print Current Work
UI components required: saved defaults, print mode picker, floor copy preview, pull-list preview.
State management needs: last good customer/date context, active filters, chosen print mode, derived print dataset.
User interactions and event handlers: reopen tool, restore defaults, choose print action.
Data flow patterns: saved preferences + current read model -> print-specific read model.
Error handling requirements: printing must not depend on expanded UI state; missing data should be surfaced before print generation.

## Implications For Design
- The data model needs explicit separation between source demand lines, report-side location selection state, and MTM waitlist records.
- Contracts need explicit duplicate-conflict responses and separate queue/status update operations.
- The quickstart must validate report-side location selection, duplicate blocking, Problem ownership retention, and no-location exception behavior.
- The report page design now includes a collapsed filter expander, an embedded crystal-style user control, row-highlight selection cues, and shell resize/restore behavior that must all be preserved when live bindings replace the current preview data.
- The grouped report projection must become a first-class implementation seam so the crystal-style user control can be populated from real `DemandLine` and `LocationOption` data instead of hardcoded sample groups.
