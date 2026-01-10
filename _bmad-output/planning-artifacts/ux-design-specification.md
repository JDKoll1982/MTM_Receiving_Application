---
stepsCompleted: [1, 2, 3, 4, 5]
workflowType: ux-design
project_name: MTM_Receiving_Application
user_name: John Koll
date: 2026-01-10
inputDocuments:
  - docs/MTM_Waitlist_Application/README.md
  - docs/MTM_Waitlist_Application/PROJECT_STRUCTURE.md
  - _bmad-output/planning-artifacts/prd.md
  - _bmad-output/planning-artifacts/ui-design.md
  - _bmad-output/planning-artifacts/architecture.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/kickoff-revised-core-first.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/kickoff-stakeholder-version.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/kickoff.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/initial-epics.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/meeting-outline.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/meeting-summary.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/meeting-transcript.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/module-breakdown.md
  - docs/MTM_Waitlist_Application/Documentation/Planning/file-structure-breakdown.md
---

# UX Design Specification MTM_Receiving_Application

**Author:** John Koll
**Date:** 2026-01-10

---

<!-- UX design content will be appended sequentially through collaborative workflow steps -->

## Executive Summary

### Project Vision

Replace “Tables Ready” with an in-house Windows desktop waitlist system that feels familiar (table-first, scan-first), minimizes operator typing, prevents incorrect requests via guided selection + Visual read-only context, and enforces role-based rights — while supporting offline operation and leadership approval gates for rollout.

Version 1.0 is intentionally “close to Tables Ready”:

- No Quality module/workflow in v1.0
- Core request types available immediately (everything except Quality)
- A single primary queue/table surface with modal actions (no multi-page maze)
- Operators can toggle between “My” and “Shared” queues with one click

### Target Users

- Press Operators
  - Primary goal: submit correct requests fast with minimal typing
  - Default view: “My Active” queue (their work and what they’re waiting on)
  - Can toggle to “Shared Queue” for awareness, not for analytics
- Material Handlers
  - Primary goal: clear, sortable site queue with fast completion flow
  - Default view: “Shared Queue” (site-scoped active work), with “Recent (Completed)” for history
- Production Leads/Admin
  - Primary goal: visibility and accountability (analytics rights)
  - Own time standards (operators cannot change), and can see deeper performance fields

### Key Design Challenges

- One table must serve multiple roles without becoming a “junk drawer.”
  - Mitigation: role-gated columns + progressive disclosure (details drawer/modal)
- Dual-scope viewing (My vs Shared) must be obvious and safe.
  - Mitigation: a single segmented toggle labeled clearly; persist last selection per role
- “Recent” is completed history (completed tasks), and can optionally be used as a starting template for a new task.
  - Mitigation: explicit “Recent (Completed)” mode with date filters, search, and clear actions (Reopen vs Use as Template)
- Offline/async behavior must not erode trust.
  - Mitigation: explicit sync status badge; toasts distinguish “Completed” vs “Queued for sync”
- Completion feedback must be fast and non-blocking.
  - Requirement: timed toast confirmation, then remove from Active; item appears in Recent

### UX Trust Requirements (Non-Negotiable)

- Every critical action produces immediate feedback:
  - Completion: timed toast then item removed from Active and visible in Recent (Completed)
  - Offline/async: toast explicitly says “Queued for sync (Offline)” and row shows sync status badge
- “Disappearing” is never mysterious:
  - Users must be able to find completed work immediately in Recent (Completed) via search/filter
- Operators default to “My Active” to prevent overwhelm; Shared Queue is one click away
- Primary actions must be reachable in 1–2 clicks from the table (no deep navigation)

### Reliability / Offline Failure Modes (Design Must Handle)

- Offline actions must preserve user trust:
  - Distinguish “Completed” vs “Queued for sync (Offline)” vs “Sync failed — tap to retry”
  - Provide a persistent sync status indicator (Online/Offline/Syncing/Failed)
  - Ensure completed items remain discoverable in Recent even when pending sync
- Prevent duplicate submissions:
  - Disable submit while busy, show sending state, and prefer idempotent behavior
- Prevent wrong-site confusion:
  - Always display current site context; if unknown, show warning and restrict scope until resolved
- Make “Reopen” explicit:
  - Reopen returns item to Active and resets timer; this must be stated in the UI
- Avoid column overload and role leakage:
  - Role-gated columns and progressive disclosure for details

### Core Interaction Pattern (v1.0)

- One primary table surface is the “home” (TablesReady muscle memory).
- Users switch scope with a single segmented toggle: View = My | Shared.
- Users switch time/status mode with a clear toggle: Show = Active | Recent (Completed).
- Default scope is role-based (Operator = My Active; MH/Lead = Shared Active).
- Actions are modal/flyout from the table (Complete, Reopen, Add Note), not navigation.

### Key UX Architecture Decisions (ADR Summary)

- Single table is the home surface (TablesReady muscle memory).
- Scope is a one-click segmented toggle: View = My | Shared (role-defaulted).
- History is explicit: Show = Active | Recent (Completed); “Recent” means completed history and also supports “Use as Template” for fast re-entry.
- Actions are modal/flyout from the table (not page navigation).
- Offline actions are transparent (Queued for sync / Failed with retry) with a persistent sync indicator.
- Column sets are role-gated; full detail is progressive disclosure (drawer/modal).

### v1.0 Simplicity Rules (Occam)

- The Queue table is the product; avoid multi-page navigation for core work.
- Only two global toggles in the primary UI:
  - View = My | Shared
  - Show = Active | Recent (Completed)
- Keep the table readable:
  - Limit visible columns per role; push extra details into a details drawer/modal.
- Defer non-essential complexity:
  - No Quality in v1.0, no auto-assign controls, no extra dashboards inside the queue view.
- Preserve user trust above all:
  - Timed toasts + explicit offline/sync status and discoverable Recent history.

### Design Opportunities

- Preserve Tables Ready muscle memory:
  - One table as the “home”
  - Click row → act via modal (Complete / Reopen / Add Note), not navigation
- Reduce wrong requests:
  - Push selection lists first, free-text only when needed
  - Use press/site/work order context (Visual read-only) to constrain choices where possible
- Make “fast hands” users happy:
  - Keyboard-first table navigation, type-to-filter, and single-action modals
- Make the system “explain itself” for rollout:
  - First-run coach marks for the My/Shared toggle and Recent definition
  - Consistent terminology across roles

## Core User Experience

### Defining Experience

The core experience of MTM_Receiving_Application (Waitlist v1.0) is: creating a waitlist task quickly, correctly, and confidently.

This means the primary loop is:

1) User opens the queue (table-first home)
2) User adds a task (New Request modal)
3) User immediately sees the task appear in the correct queue scope (My/Shared) with clear status
4) Downstream roles complete work and the task transitions from Active → Recent (Completed) with traceability

### Platform Strategy

- Platform: Windows desktop application (in-house only)
- Primary input: mouse + keyboard (optimized for fast “table-first” workflows)
- Offline: required (actions can queue for sync)
- Core UI model: single table home + two toggles (View: My|Shared, Show: Active|Recent (Completed)) + modal actions

### Effortless Interactions

These must be “zero thought”:

- Add a task in seconds with minimal typing (selection-first; free-text only when needed)
- Immediate confirmation that the task was created:
  - Online: toast “Created” and row visible instantly
  - Offline: toast “Queued for sync (Offline)” with clear status indicator
- Find the task again quickly:
  - My Active as Operator default
  - Shared Active as MH/Lead default
  - Recent (Completed) searchable for “where did it go?” moments

### Critical Success Moments

- The “Send” moment: user submits a task and instantly trusts it went to the right place
- The “Visibility” moment: the task appears where expected (My vs Shared; Active vs Recent)
- The “Completion” moment: completion produces a timed toast and the item transitions predictably to Recent (Completed)

### Experience Principles

1) Table-first familiarity: keep the queue as the home surface
2) Minimum typing: selection-first workflows, avoid free-form entry unless necessary
3) Immediate trust feedback: every action yields clear toast + predictable row state
4) Role-safe clarity: operators can see Shared Queue without analytics leakage
5) Offline honesty: never fake success; queued/syncing/failed states are explicit

## Desired Emotional Response

### Primary Emotional Goals

- Confidence: users trust the system will work every time.
- Speed: the system feels immediate and responsive.
- Accuracy: users believe the request is correct and visible to the right people.

### Emotional Journey Mapping

- First use / first week:
  - Feel: “This is simple and familiar (TablesReady-like). I can do this.”
- During core action (adding a task):
  - Feel: fast, focused, no doubt (“I hit send, it worked.”)
- Immediately after adding a task:
  - Feel: confidence that a material handler will see it almost immediately.
- During daily use:
  - Feel: in control; predictable results; low mental load.
- When something goes wrong (offline/sync failed/wrong-site):
  - Feel: reassurance (“It’s handled / queued / I know what to do next.”)

### Micro-Emotions

- Confidence over confusion
- Trust over skepticism
- Calm focus over anxiety
- Satisfaction over frustration
- Control over helplessness

### Design Implications

- Confidence/Trust → immediate, explicit feedback:
  - Timed toasts for create/complete/reopen with clear wording (Created / Completed / Queued for sync / Failed with retry).
  - Predictable row movement (Active ↔ Recent) so “disappearing” is never mysterious.
- Speed → low-friction interaction:
  - Minimal typing; selection-first request modal; keyboard-friendly table.
  - Avoid extra screens; keep work on the table surface.
- Accuracy → visibility + context clarity:
  - Request appears immediately in the expected scope (My vs Shared).
  - Site context is always visible; wrong-site uncertainty triggers clear warnings.
- Reassurance during failure → “what now?” answers:
  - Persistent sync status indicator.
  - Clear retry path for failed sync; no silent failures.

### Emotional Design Principles

1) Never leave users guessing (always show status and next step).
2) Make success feel instant (fast feedback + visible proof in the table).
3) Keep it familiar under pressure (TablesReady-like table-first workflow).
4) Be honest about offline/sync states (reassurance comes from transparency).

## UX Pattern Analysis & Inspiration

### Inspiring Products Analysis

#### Tables Ready (Current System)

What it does well:

- Table-first scanning: users can see the state of work at a glance.
- Low ceremony: the queue is the product; minimal navigation.
- Familiar mental model: “it’s just a list, sorted by urgency/time.”

Lessons to adopt:

- Keep the queue as the home surface.
- Prioritize scanability and speed over “pretty” UI.

Risks to improve:

- Reduce ambiguity when items disappear or change state.
- Reduce manual typing/error entry through selection-first flows.

#### Infor Visual (ERP)

What it does well:

- Authority: users trust it as “the truth” (even if it’s painful).
- Structured data: clear fields and strict formats.

Lessons to adopt:

- Clear labeling and consistent field naming.
- Strong validation and predictable outcomes (no hidden magic).

Patterns to avoid:

- Dense screens, heavy terminology, and “ERP intimidation.”
- Deep navigation where users get lost.

#### Microsoft Teams

What it does well:

- Reassurance through status: delivered/sent/failed patterns (users know what happened).
- Fast feedback loops: quick confirmation moments and lightweight notifications.
- Recoverability: users can retry, re-open context, and find history.

Lessons to adopt:

- Toasts and status indicators should clearly communicate success vs pending vs failed.
- History must be searchable and trustworthy (Recent = completed history, easy to find, and also usable as a template to create a new task).

### Transferable UX Patterns

**Navigation & IA**

- “One home surface” pattern: single primary table/queue as the app’s anchor.
- Minimal top-level controls: keep focus on the work, not the app chrome.

**Interaction Patterns**

- Selection-first task creation (minimize typing) using modal forms.
- Predictable state transitions: Active ↔ Recent (Completed) with traceability.
- Clear feedback language: Created / Completed / Queued for sync / Failed (tap to retry).

**Visual Patterns**

- High-contrast table readability with clear row states (e.g., urgency/red timing).
- Explicit scope indicators (My vs Shared) and site context always visible.

### Anti-Patterns to Avoid

#### “VS Code intimidation” (Non-developer UX anti-pattern)

Avoid:

- Dense multi-pane layouts with tiny controls
- Icon-only actions without labels
- Hidden shortcut-driven commands as the primary path
- Developer-like terminology (“workspaces”, “panels”, “commands”)

Instead:

- Label-first UI with clear call-to-action buttons.
- Simple toggles (View: My|Shared, Show: Active|Recent) and visible system status.

### Design Inspiration Strategy

**What to Adopt**

- Tables Ready: table-first, scan-first, minimal navigation.
- Teams: reassurance via explicit statuses and lightweight confirmation feedback.

**What to Adapt**

- Visual’s “structured truth” into a simplified, role-friendly modal flow and table detail model.

**What to Avoid**

- Any developer-tool aesthetic or complexity that intimidates non-developers.
