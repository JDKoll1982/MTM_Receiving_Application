# Privilege Level UI Access Suggestions

Last Updated: 2026-03-23
Status: Research-backed recommendations derived from the current spec and external authorization guidance
Companion To: `docs/Modules/Privilege-Level-UI-Access-Spec.md`

## Purpose

This document is the companion to the current privilege access spec. The spec maps the current codebase, gaps, assumptions, and candidate implementation areas. This document answers a different question:

- based on the current MTM codebase
- based on the repo's architecture constraints
- based on common RBAC and authorization guidance from widely used sources

what would most teams likely do next?

This is not a formal approval matrix. It is a reasoning document that turns the spec into practical design suggestions.

## Sources Reviewed

The suggestions below were informed by the current spec plus the following external sources reviewed during this pass:

1. OWASP Authorization Cheat Sheet
2. NIST RBAC project overview
3. Microsoft ASP.NET Core role-based authorization guidance
4. Microsoft ASP.NET Core policy-based authorization guidance
5. Microsoft Entra app roles guidance
6. Microsoft Azure architecture guidance for tenant-aware authorization and audit concerns
7. Auth0 RBAC documentation
8. Okta RBAC guidance
9. Aserto authorization pattern guidance
10. Permit.io RBAC implementation guidance
11. Open Policy Agent access-control comparison guidance

## Executive Summary

If this codebase were being modernized the way most teams would do it today, the likely direction would be:

1. Keep the four coarse privilege levels: `User`, `Supervisor`, `Admin`, `Developer`
2. Centralize authorization decisions in one shared application service
3. Deny by default for privileged surfaces
4. Enforce authorization in more than one layer
5. Use roles for broad access and permissions/policies for sensitive actions
6. Avoid rewriting the database model unless there is a strong business reason
7. Start with a simple first release for the Users page, then evolve only if needed

That is the strongest common pattern across the sources reviewed.

## Second-Pass Summary

This second pass improves the original suggestions document in three ways:

1. it turns general recommendations into explicit proposed answers to the current spec assumptions
2. it separates what should likely be approved now from what should likely be deferred
3. it gives a more concrete target architecture for how authorization would actually fit this repo

The main conclusion from this second pass is:

- the first document was directionally correct
- the most useful next step is not more theory
- the most useful next step is to make a few deliberate decisions and keep phase 1 intentionally small

## Recommended Default Decisions

If the goal is to move forward without over-engineering, these are the default decisions I would recommend.

| Topic                                            | Recommended Default                                                                                   | Why This Is The Safer Majority Choice                                                          |
| ------------------------------------------------ | ----------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Business-facing privilege model                  | Keep `User`, `Supervisor`, `Admin`, `Developer`                                                       | The current model is already understandable and maps well to common enterprise privilege tiers |
| Underlying storage model                         | Keep role tables as-is                                                                                | Most teams preserve existing flexible storage until the business proves it is insufficient     |
| Phase-1 Users UI                                 | Expose one editable effective privilege level                                                         | Simpler for admins, lower risk for rollout, still compatible with the current schema           |
| Enforcement pattern                              | Centralized decision service used from multiple layers                                                | This is the strongest shared recommendation across security and platform guidance              |
| Default behavior on unmapped privileged surfaces | Deny                                                                                                  | Safer than permissive defaults and easier to audit                                             |
| Audit baseline                                   | Log privilege changes, denied elevated actions, destructive admin actions, sensitive credential edits | This is the normal enterprise expectation for privileged operations                            |
| Placeholder pages                                | Defer implementation, keep documented                                                                 | Most teams would not expand scope to placeholder permissions pages in the first rollout        |

## Recommended Answers To The Current Spec Assumptions

This section translates the current spec's assumptions into suggested approval answers.

### A-01: Single Effective Privilege In Users UI

Recommended answer:

- approve the UI as single effective privilege for phase 1
- do not approve a schema rewrite to force single-role-at-database-level yet

Reasoning:

- most teams optimize the first admin UX for clarity, not maximum theoretical flexibility
- most teams also avoid collapsing a flexible schema before they know they need to

Suggested wording for approval:

- the Users page will manage one effective privilege level in phase 1
- the existing role tables remain in place
- future multi-role editing can be added later if real business demand appears

### A-02: UI Hiding Must Be Paired With Hard Enforcement

Recommended answer:

- approve

Reasoning:

- this has the strongest support from both the codebase review and the external guidance
- the repo already has multiple non-visual access paths, including host services, code-behind, and service-location patterns

Suggested wording for approval:

- privileged surfaces must be protected by both UI behavior and non-UI authorization checks

### A-03: Session Should Expose Effective Privilege

Recommended answer:

- approve, with one refinement

Refinement:

- use session-cached privilege for UI decisions and quick access checks
- still re-check sensitive operations at execution time through the shared authorization service

Reasoning:

- this balances responsiveness with safety
- it also aligns well with the repo's desktop-app shape

### A-04: Placeholder Permissions Pages Are Not Phase-1 Targets

Recommended answer:

- approve

Reasoning:

- most teams would not spend the first implementation wave on placeholder pages when the real risk is already concentrated in existing navigation and privileged commands

### A-05: Existing Role Tables Are Sufficient Unless One-Role-Only Must Be Enforced At DB Level

Recommended answer:

- approve

Reasoning:

- this is the most conservative and least disruptive path
- it preserves future optionality
- it avoids spending the first release on migrations instead of enforcement

## Approve Now vs Defer Now

### Approve Now

These are the decisions I would recommend locking in now.

1. Keep the four coarse privilege levels
2. Add one shared authorization service
3. Deny by default for privileged surfaces
4. Enforce authorization in shell, host, ViewModel/command, and service layers
5. Make the Users page phase-1 UI single-select for effective privilege
6. Keep the existing role schema for now
7. Add transactional privilege replacement logic
8. Add audit logging for privileged changes and denied elevated operations
9. Add anti-lockout rules such as no self-deactivation
10. Defer placeholder permissions pages

### Defer Now

These are valid ideas, but I would not recommend making them part of the first approved implementation wave.

1. full multi-role editing UI in the Users page
2. full ABAC or relationship-based authorization across the app
3. replacing the current role schema
4. removing all service-locator usage as part of phase 1
5. externalizing all policy decisions into a separate policy engine immediately

## Suggested Target Architecture

If the project wants a design that most teams would recognize as maintainable, the likely target shape is this:

```text
Session / Current User Context
   -> Shared Authorization Service
      -> Policy Methods / Permission Decisions

Main Shell / Settings Shell / Window Hosts
   -> ask shared authorization service for visibility and navigation decisions

ViewModels / Commands
   -> ask shared authorization service for execution decisions

Business Services
   -> ask shared authorization service before sensitive mutations

DAOs
   -> remain data-access only
```

Important consequence for this repo:

- the authorization service belongs in the service layer
- the ViewModels should consume it
- the DAOs should not become policy engines

That aligns with the repo's architectural rules and the external guidance at the same time.

## Recommended Authorization Vocabulary

To keep the code understandable, most teams would gradually move from direct role-name checks to named policy decisions.

Examples of better decision names for this repo:

- `CanOpenSettingsWindow`
- `CanOpenUsersAndPrivileges`
- `CanEditUserPrivilege`
- `CanEditVisualCredentials`
- `CanUseDeveloperTools`
- `CanEnterReceivingEditMode`
- `CanDeleteDunnageAdminData`
- `CanEditVolvoHistory`

This is usually clearer than scattering checks like `IsAdmin || IsDeveloper` across many files.

## Suggested Policy Style For This Repo

The codebase is not an ASP.NET Core API, but the Microsoft guidance is still useful because the core pattern is the same: roles by themselves are not the final shape of the business rule.

The likely best pattern here is:

- coarse role determines the broad tier
- named policy method decides the actual operation

Example:

- `Admin` may broadly manage settings
- `CanEditVisualCredentials` still remains its own explicit decision
- `Developer` may access diagnostics, but not automatically every admin workflow unless intentionally granted

## What Most Teams Would Do About Multi-Role Support

This is one of the biggest decision points in the spec.

The majority practical answer is usually not purely “single role” or purely “full multi-role UI.” It is a phased compromise.

### Likely majority pattern

1. keep storage flexible
2. expose a simpler admin UI first
3. define one effective privilege level for the major app tiers
4. reserve true multi-role or fine-grained permission assignment for future need

Why this usually wins:

- admins can reason about it quickly
- it reduces mistakes in the first release
- it avoids premature UI complexity
- it avoids premature schema change

## What Most Teams Would Do About Service-Locator Risk

The current repo still has service-location paths.

The majority pragmatic response would be:

- do not block the authorization project on removing all service-locator usage
- design the authorization enforcement so that even service-located views still call the same shared authorization service
- then treat service-locator cleanup as a later maintainability task

This is the typical incremental modernization path rather than an all-at-once rewrite.

## What Most Teams Would Do About Auditing

If the app is going to manage privileges, most teams would quickly move from generic logging to explicit authorization audit events.

Recommended audit event families:

1. privilege assignment changed
2. privilege assignment denied
3. privileged navigation denied
4. destructive admin command executed
5. destructive admin command denied
6. Visual credential updated
7. settings shell opened for elevated pages
8. developer tool launched

Recommended minimum fields:

- actor user
- target user if applicable
- action attempted
- allow or deny result
- before and after privilege when applicable
- timestamp
- module or page name

## What Most Teams Would Test First

The likely first authorization test set would not try to cover the entire app. It would cover the first privileged seams.

Recommended first test targets:

1. `Admin` can edit another user's privilege
2. non-admin cannot edit another user's privilege
3. user cannot open `Users & Privileges`
4. non-developer cannot open Developer Tools
5. self-deactivation is blocked
6. privilege replacement is atomic
7. hidden UI is not the only protection and command-level denial still works

## Second-Pass Recommendation On Implementation Order

The original suggestions already had a reasonable order. This second pass sharpens it.

### Recommended Phase Order

1. Decide the five assumptions in the spec using the recommendations above
2. Create the shared authorization service and privilege-resolution model
3. Protect settings entry points and the `Users & Privileges` page
4. Implement Users-page privilege editing with audit logging and anti-lockout rules
5. Protect Developer Tools
6. Protect the next-highest-risk edit and admin surfaces in Dunnage, Receiving, Volvo, and Reporting
7. Expand tests and only then revisit placeholders or future-state redesigns

## Minority But Valid Alternatives

There are valid alternatives that some teams would choose, but they are not the most likely majority path for this repo right now.

### Alternative 1: Force one role per user in the database immediately

Why some teams do it:

- simpler mental model
- easier admin UI

Why I would not recommend it first here:

- the current schema already supports more flexible assignment
- there is no strong evidence yet that the business needs a schema-level restriction

### Alternative 2: Full policy engine from day one

Why some teams do it:

- stronger long-term separation of policy and code

Why I would not recommend it first here:

- too much architectural movement for the first rollout
- the app can get most of the safety benefit with a shared in-app authorization service first

### Alternative 3: Role checks only, no permission vocabulary

Why some teams do it:

- fastest to implement

Why I would not recommend it first here:

- the repo already shows multiple high-risk surfaces where named decisions will age better than raw role comparisons

## Third-Pass Summary

This third pass shifts the document one step closer to implementation readiness.

It adds:

1. a recommended phase-1 contract
2. concrete acceptance criteria for the first rollout
3. explicit boundaries for what phase 1 should and should not solve
4. the highest implementation risks and the mitigations most teams would use

The main conclusion from the third pass is:

- the project likely has enough design guidance already
- the most important remaining task is to prevent scope creep while building a safe first release

## Recommended Phase-1 Contract

If I had to define a clean and realistic first implementation boundary for this repo, I would recommend this exact contract.

### Phase-1 Scope

Phase 1 should do all of the following:

1. resolve the current user's effective privilege through one shared authorization service
2. protect the Settings entry path and `Users & Privileges` navigation path
3. protect all privileged actions in the Users page
4. add a single effective privilege selector for the selected user
5. perform privilege replacement transactionally
6. log privilege changes and denied privileged operations
7. block self-deactivation and obvious self-lockout scenarios

### Phase-1 Non-Goals

Phase 1 should explicitly not try to do all of the following:

1. redesign the entire authorization model for every module
2. implement full multi-role editing UX
3. replace the schema
4. remove all `App.GetService<T>()` usage
5. introduce a full external policy engine
6. fully implement placeholder permissions pages

That line between scope and non-goals is what will keep the rollout realistic.

## Recommended Phase-1 Acceptance Criteria

The first implementation should not be considered done unless all of these are true.

### Functional Acceptance Criteria

1. an `Admin+` user can open the Users page and see the privilege editor for the selected user
2. a non-admin user cannot successfully reach or use the Users-page privilege-changing path
3. the selected user's privilege can be changed through one atomic operation
4. the privilege displayed in the page reflects the effective privilege used by the app
5. self-deactivation is blocked
6. last-admin or self-lockout scenarios defined for phase 1 are blocked

### Enforcement Acceptance Criteria

1. hiding the UI is not the only protection
2. code-behind-triggered command execution cannot bypass authorization
3. host services cannot bypass authorization for privileged settings access
4. ViewModel command execution rejects unauthorized requests
5. sensitive service methods reject unauthorized requests even if the caller reached them indirectly

### Audit Acceptance Criteria

1. every privilege change is logged
2. every denied privilege-change attempt is logged
3. every denied access attempt to the privileged settings path is logged
4. audit records contain actor, target, action, result, timestamp, and context

## Recommended Authorization Service Contract

Most teams would do better with a small, explicit contract first rather than a giant authorization abstraction.

Suggested initial responsibilities:

1. resolve current effective privilege
2. answer broad navigation questions
3. answer sensitive action questions
4. provide deny reasons suitable for logs and optionally for user-safe messaging

Suggested initial decision families:

1. navigation decisions
   - can open settings
   - can open users and privileges
   - can open developer tools
2. user-management decisions
   - can add user
   - can edit user
   - can deactivate user
   - can edit user privilege
   - can edit Visual credentials
3. future module decisions
   - can enter receiving edit mode
   - can access dunnage admin surface
   - can edit Volvo history

Most teams would keep this contract small at first and expand it only as module rollout proceeds.

## Recommended Decision Order Inside The App

For a sensitive action, the likely safest order is:

1. determine current user context
2. resolve effective privilege
3. evaluate named authorization decision
4. if denied, stop safely and log
5. if allowed, continue to the business operation

That order should be the same whether the action came from:

- visible navigation
- hidden page reachability
- code-behind event
- host service
- service-located view

## Highest Risks In The First Rollout

### Risk 1: UI-only protection sneaks back in

Why it is likely:

- WinUI projects often start with visibility toggles because they are easy

Mitigation:

- require matching command and service-layer checks for every privileged action included in phase 1

### Risk 2: The Users ViewModel keeps too much authorization logic

Why it is likely:

- the current Users ViewModel already mixes UI and direct data access responsibilities

Mitigation:

- move authorization decisions into the shared authorization service
- move privilege write orchestration into a service layer, not the ViewModel

### Risk 3: Self-lockout rules remain ambiguous

Why it is likely:

- privilege editing almost always exposes edge cases around self-modification

Mitigation:

- define the exact phase-1 rules before implementation begins
- at minimum, block self-deactivation and block any action that removes the last administrative recovery path

### Risk 4: Audit logs exist but are too weak to investigate problems

Why it is likely:

- teams often log only a message string, not structured event details

Mitigation:

- define the audit fields up front
- keep the event names consistent

### Risk 5: The authorization service becomes a dumping ground

Why it is likely:

- once a central service exists, everything gets pushed into it

Mitigation:

- keep phase-1 decisions narrow and named
- do not move data access or business mutation logic into the authorization service

## Recommended Phase-1 Edge-Case Rules

These are the minimum edge-case rules I would recommend deciding explicitly.

1. can an admin lower their own privilege?
   - recommended default: no, not in phase 1
2. can an admin deactivate themselves?
   - recommended default: no
3. can an admin edit another admin?
   - recommended default: yes, subject to anti-lockout rules
4. can a developer edit users just because they are developer?
   - recommended default: no, unless explicitly approved
5. if a user has multiple roles in storage, which one drives the displayed effective privilege?
   - recommended default: the managed privilege family resolves to one effective app privilege for UI purposes

## Recommended Rollout Sequence By File Category

Most teams would reduce risk by changing files in this order.

1. service contract and implementation
2. DI registration
3. session or startup privilege resolution plumbing
4. settings host and shell gating
5. Users ViewModel and service orchestration
6. Users XAML and code-behind protection
7. tests

This order is safer than starting in XAML because it establishes the enforcement path first.

## Minimum Documentation Standard For The Implementation

When implementation begins, the first rollout should leave behind enough documentation that later module expansion is consistent.

Recommended minimum documentation artifacts:

1. shared authorization service contract summary
2. list of phase-1 policy decision names
3. audit event names and required fields
4. phase-1 edge-case rules for self-management and lockout prevention

## Third-Pass Bottom Line

The recommended majority path is now more concrete:

1. approve the current four privilege tiers
2. approve the assumptions with the answers recommended above
3. implement a narrow phase-1 authorization slice centered on the Users page and settings entry points
4. make the first rollout atomic, auditable, and anti-lockout safe
5. defer broader redesign until the first slice is stable

If the project follows that path, it will get the biggest security and maintainability improvement without turning the first authorization rollout into a full-platform rewrite.

## Fourth-Pass Summary

This fourth pass focuses on release governance rather than feature design.

It adds:

1. go or no-go criteria for phase 1
2. rollback expectations if the rollout misbehaves
3. a short list of unresolved questions that should be answered before coding begins
4. implementation anti-patterns to actively avoid during the first rollout
5. a final execution checklist that a reviewer could use before approving implementation start

The main conclusion from the fourth pass is:

- the design direction is now sufficiently mature
- the remaining risk is mostly execution discipline, not missing theory

## Go / No-Go Criteria For Phase 1

Most teams benefit from deciding in advance what must be true before phase 1 is allowed to start, and what must be true before it is allowed to ship.

### Go Criteria Before Implementation Starts

Implementation should start only after these are explicitly agreed:

1. A-01 through A-05 have an approved disposition
2. the four privilege tiers are accepted as the app's business-facing model
3. self-deactivation behavior is explicitly defined
4. self-demotion and last-admin protection behavior is explicitly defined
5. the phase-1 non-goals remain accepted so scope does not expand mid-implementation

### Go Criteria Before Phase 1 Is Considered Ready To Ship

Phase 1 should not ship unless these are all true:

1. privileged settings entry paths are protected
2. Users-page privilege editing is protected at UI and non-UI layers
3. privilege replacement is atomic
4. anti-lockout rules work as designed
5. audit logging exists and is reviewable
6. the relevant tests pass
7. no new MVVM violations were introduced

### No-Go Conditions

I would treat these as stop conditions for the first rollout:

1. privilege changes can succeed from an unauthorized command path
2. an admin can accidentally remove the last recovery-capable administrative path
3. audit records are missing actor, action, result, or target context
4. implementation requires ViewModels to talk directly to DAOs for authorization data
5. phase 1 expands into placeholder pages or broad module redesign without explicit re-approval

## Rollback Guidance

Authorization changes are high-impact even when they are correct in principle. Most teams define rollback posture up front.

### Recommended Rollback Trigger Conditions

Rollback should be considered immediately if any of these occur in test or early rollout validation:

1. legitimate admins are blocked from recovering access
2. unauthorized users retain access to newly protected surfaces
3. privilege changes write partial state
4. navigation and execution decisions disagree in user-visible ways
5. audit logging fails for the protected actions introduced in phase 1

### Recommended Rollback Shape

The safest rollback shape for this repo is likely:

1. preserve the existing schema
2. keep the new privilege-replacement stored procedure isolated rather than rewriting many existing ones
3. keep the shared authorization service additive and removable
4. avoid coupling phase-1 rollout to large, unrelated refactors

That makes rollback mostly a matter of disabling the new protected path rather than unwinding broad architectural change.

## Open Questions That Still Matter

The document already recommends answers to the current assumptions. These are the specific questions I would still want clearly answered before implementation begins.

1. should `Developer` ever imply `Admin` behavior, or should those remain separate by default?
2. should admins be able to edit other admins in phase 1, assuming anti-lockout rules hold?
3. if a user has more than one role in storage today, what exact precedence rule determines the displayed effective privilege?
4. should denied privileged navigation produce a user-facing message, silent redirect, or both?
5. should phase 1 include only settings-shell gating, or also main-shell filtering for obviously privileged top-level entry points?

These are not blockers to understanding the direction, but they are important for avoiding implementation ambiguity.

## Implementation Anti-Patterns To Avoid

These are the concrete mistakes I would actively watch for during implementation and review.

1. adding privilege logic directly into XAML converters or code-behind as the primary enforcement path
2. duplicating raw role comparisons across many ViewModels instead of using named authorization decisions
3. making the authorization service depend on UI types
4. moving business mutation logic into the authorization service
5. using multiple independent DAO calls for privilege replacement instead of one transactional path
6. introducing phase-1 schema changes just to simplify UI work
7. mixing developer-tool permissions into general admin checks without an explicit decision
8. treating hidden navigation as sufficient security

## Recommended Reviewer Checklist For Implementation PRs

When implementation starts, the first PRs should be reviewed against a short, strict checklist.

### Architecture Checklist

1. ViewModels do not call DAOs directly for authorization or privilege writes
2. the shared authorization service sits in the service layer
3. DAOs remain data-access only
4. the privilege replacement path is centralized

### Enforcement Checklist

1. privileged navigation is checked
2. privileged commands are checked
3. host-service access paths are checked
4. sensitive service operations are checked

### Safety Checklist

1. self-deactivation is blocked if that is the approved rule
2. anti-lockout rule is enforced
3. privilege changes are atomic
4. audit events are emitted for allow and deny cases where required

### Scope Checklist

1. no placeholder permissions pages were pulled into phase 1 without re-approval
2. no full multi-role editor was added opportunistically
3. no broad service-locator cleanup was folded into the same change set unless explicitly approved

## Recommended Cut Line If Schedule Pressure Appears

If implementation time becomes constrained, most teams would preserve the protection boundary and cut optional convenience features first.

Recommended cut order:

1. keep shared authorization service
2. keep protected settings entry path
3. keep protected Users-page privilege edit path
4. keep atomic write behavior
5. keep audit logging
6. cut lower-value convenience items before cutting enforcement

Examples of features to cut before cutting enforcement:

1. richer privilege display formatting
2. non-essential UI polish in the Users page
3. broader module rollout beyond Settings and the first high-risk surfaces

## Recommended Decision Log Entries

If the project wants the implementation to remain stable across future passes, these decisions should be recorded explicitly somewhere durable once approved.

1. the business meaning of each privilege tier
2. the approved answer to A-01 through A-05
3. the self-management and anti-lockout rules
4. whether `Developer` is independent from `Admin`
5. the exact boundaries of phase 1

This reduces the chance that future follow-up work silently reopens decisions that were already made.

## Fourth-Pass Bottom Line

After four passes, the document now supports four distinct jobs:

1. explaining the likely majority authorization direction
2. recommending concrete answers to the current spec assumptions
3. defining an implementation-ready phase-1 scope and contract
4. defining rollout governance so the first release stays safe and controlled

The strongest final recommendation remains:

1. approve the assumption answers
2. keep phase 1 narrow
3. centralize authorization decisions
4. enforce in multiple layers
5. make privilege changes atomic and auditable
6. protect against lockout before broadening scope

That is still the most practical path most teams would choose for this repo, and it is now documented in a way that is much closer to direct execution.

Across security and IAM guidance, privileged mutations should be logged consistently.

For this repo, the high-value audit events are:

- privilege changes
- denied access attempts for privileged actions
- user deactivation/reactivation
- Visual credential updates
- destructive admin actions
- developer tool execution

#### 7. Authorization logic should be tested explicitly

Most sources emphasize tests for allow and deny behavior. In this repo that likely means:

- unit tests for privilege decisions
- ViewModel tests for hidden or disabled command paths
- integration tests for sensitive service operations when feasible

### Medium Consensus

These ideas are common, but the exact implementation varies more between teams.

#### 8. Start with RBAC, then add finer-grained checks only where needed

Many teams still begin with RBAC because it is simple and understandable. They add permission- or resource-level rules only for the cases where roles alone are too blunt.

That fits this repo well.

Suggested interpretation:

- use the four privilege levels for broad UI/module gating
- add finer checks only for places like self-demotion, destructive deletes, developer tools, credentials, and history editing

#### 9. Keep role assignment flexible in storage, even if the UI is simpler at first

Many platforms and standards assume that users can have multiple roles. The current schema already supports that. The majority pattern would usually avoid collapsing the schema too early.

That does not mean the first UI must expose multi-role editing.

The practical compromise most teams would accept is:

- first release: one selected effective privilege level in the Users page
- storage: continue using the existing role tables
- implementation rule: for the privilege family being managed here, keep exactly one assigned privilege role per user unless future requirements say otherwise

This preserves future flexibility without overcomplicating the first rollout.

#### 10. Session caching is fine for visibility, but sensitive actions may still need authoritative checks

A common pattern is:

- cache effective privilege in the session for UI visibility and quick checks
- re-check for high-risk operations when the action actually runs

In this desktop application, that is a reasonable compromise between responsiveness and safety.

## What The Majority Would Probably Do In This Repo

### Suggestion 1: Introduce one shared authorization service before touching most UI files

This is the most important suggestion.

Before editing many screens, most teams would create a shared service that answers questions like:

- can this user see this module?
- can this user open this settings page?
- can this user run this command?
- can this user mutate this sensitive record?

For this repo, that service should sit above DAOs and below ViewModels, consistent with the existing architecture rules.

Recommended shape:

```text
View / Window Host / ViewModel
    -> shared authorization service
    -> business/service layer
    -> DAO
```

Not recommended:

- ViewModels calling role DAOs directly
- XAML-only visibility checks with no command protection
- role-name checks copied across many modules

### Suggestion 2: Keep the current four privilege levels as the business-facing model

The current `User`, `Supervisor`, `Admin`, `Developer` model is easy to explain and already appears aligned with how the app behaves conceptually.

Most teams would keep that as the visible business model instead of inventing many new role names immediately.

Suggested interpretation:

- `User`: standard operational work
- `Supervisor`: elevated operational control
- `Admin`: configuration and user-management authority
- `Developer`: diagnostic and engineering-only tooling

This is simple enough for administrators to understand and aligns with least privilege.

### Suggestion 3: Treat module visibility and action permission as related but separate concerns

This distinction appears in many real systems.

A common pattern is:

- module visibility: broad role-based decision
- action permission: narrower policy decision

Example for this repo:

- a user may see the Volvo module
- but only a supervisor may edit shipment history
- only an admin may access Volvo connection-string settings

That is cleaner than trying to make the top-level role solve every single action by itself.

### Suggestion 4: Gate authorization in four layers, but keep the decision logic centralized

Based on both the codebase and the external guidance, the likely best pattern here is:

1. Shell layer
   - `MainWindow`
   - Settings shell and module shells
2. Host/navigation layer
   - window host services
   - DI-driven page resolution paths
3. ViewModel/command layer
   - `CanExecute`-style guards
   - explicit authorization checks at execution time
4. Service/business layer
   - final protection for sensitive operations

The majority view is not to put different logic in each layer. The majority view is to ask the same centralized decision service from multiple layers.

### Suggestion 5: Make the first Users-page release intentionally narrow

Most teams would not try to solve all authorization problems in the first implementation.

The likely first slice would be:

- show current effective privilege for the selected user
- allow `Admin+` to change that privilege
- prevent self-demotion below required admin capability unless explicitly designed
- prevent self-deactivation
- audit every privilege change

That is enough to prove the model without expanding into every placeholder settings page immediately.

### Suggestion 6: Do not rewrite the schema unless the business model forces it

The current schema already supports roles.

Most teams would avoid a schema rewrite at this stage unless one of these becomes true:

- the business explicitly requires exactly one role per user forever
- external identity integration requires a different shape
- reporting or performance requirements prove the current model is inadequate

For now, the more common approach would be:

- keep `settings_roles`
- keep `settings_user_roles`
- add a replace-style write path for the privilege level managed by the Users page

### Suggestion 7: Prefer a transactional replace operation for privilege changes

The common operational expectation is that privilege changes are atomic.

Most teams would prefer one operation that says:

- remove the old privilege assignment for this privilege family
- assign the new one
- log the change
- fail as one unit if anything goes wrong

That is safer than issuing independent delete and insert operations from the UI.

### Suggestion 8: Add separation-of-duty rules for the highest-risk combinations

This came up in OPA and is common in enterprise authorization design.

For this repo, possible separation-of-duty or guardrail rules include:

- no self-deactivation
- no self-removal from the last admin-capable path
- developer tooling should not automatically imply unrestricted user-management authority unless deliberately chosen
- if conflicting future roles are introduced, reject unsafe combinations

Not every app needs formal separation-of-duty on day one, but most teams add at least the obvious anti-lockout rules.

### Suggestion 9: Prefer app-defined authorization semantics over identity-provider-specific group semantics

Several sources recommend app roles or app-defined permissions over raw tenant-specific group names when consistency matters.

Applied here, that suggests:

- keep the app's privilege semantics inside the application domain
- do not tie the design too tightly to external group naming conventions
- if external identity is ever introduced, map external claims/groups into app privilege decisions instead of letting external shape dictate internal behavior

### Suggestion 10: Treat `Developer` as a capability boundary, not just another admin flavor

In many teams, developer or support tooling becomes the most weakly controlled privilege because it feels internal.

The majority safer pattern is:

- explicitly isolate developer tools
- treat diagnostic/database-test surfaces as sensitive
- log usage
- keep them out of normal admin workflows unless there is a deliberate business reason to combine them

## Suggestions Ranked By Confidence

### High Confidence

These are the suggestions I would be comfortable recommending immediately.

1. Add a shared authorization service first
2. Keep the current four business-facing privilege levels
3. Deny by default for privileged surfaces
4. Enforce authorization in shell, command, and service layers
5. Use a transactional replace flow for privilege changes
6. Audit privilege changes and denied privileged actions
7. Prevent self-deactivation and accidental admin lockout

### Medium Confidence

These are good suggestions, but they depend more on your business expectations.

1. Expose one effective privilege level in the Users page while keeping the underlying schema flexible
2. Cache effective privilege in session for UI gating but re-check high-risk actions when they execute
3. Keep module visibility broad and action permissions narrower
4. Separate `Developer` tooling from ordinary admin work even if both are elevated

### Lower Confidence / Future-State Ideas

These are valid, but I would not make them phase-1 defaults unless the project explicitly wants them.

1. Move to a fully external policy engine
2. Replace RBAC with full ABAC/ReBAC for the entire application
3. Rewrite the current schema into a different authorization model immediately
4. Rework all legacy service-locator usage as part of the first privilege rollout

Those may be worthwhile later, but they are not what most teams would do first in a desktop line-of-business application that already has working role tables.

## Specific Recommendations For The Current Spec

### Recommendation A: Approve A-02 and A-03

These appear the strongest and safest assumptions:

- UI hiding must be paired with hard enforcement
- session or a shared service should expose effective privilege

Those two assumptions are strongly supported by both the codebase findings and the external guidance.

### Recommendation B: Keep A-05 unless business rules say otherwise

The existing role tables are already capable enough for the first release. Most teams would not replace them yet.

### Recommendation C: Handle A-01 with a phased compromise

My suggested position is:

- admin UI exposes one selected effective privilege level for phase 1
- underlying schema remains role-based and reusable
- implementation treats privilege level as one managed role assignment set for this feature

That is probably the best balance between simplicity and future flexibility.

### Recommendation D: Defer placeholder permissions pages

Most teams would document them now and implement them later. That matches the current spec's phased approach.

## Suggested Implementation Priorities

If the goal is to move from planning to implementation with the least risk, the likely majority path would be:

1. Build shared authorization service and privilege-resolution model
2. Gate Settings shell entry points and Users page commands
3. Add Users-page privilege editing with audit logging
4. Gate Developer Tools and other clearly sensitive pages
5. Gate workflow edit/admin surfaces in receiving, dunnage, reporting, and Volvo
6. Add broader tests for allow and deny scenarios

## What I Would Avoid

Based on both the codebase and the external guidance, I would avoid these approaches:

- sprinkling direct role checks throughout many ViewModels
- relying only on hidden buttons or hidden pages
- letting code-behind or host services bypass privilege logic
- coupling business authorization rules directly to raw DAO access from ViewModels
- forcing a major schema rewrite before the first authorization rollout is proven
- assuming that because a service is registered in DI, it is actually enforcing anything

## Bottom Line

The current spec is already pointing in the right direction.

The strongest research-backed recommendation is this:

- keep the current four privilege levels
- add one shared authorization service
- deny by default
- enforce in multiple layers
- start with the Users page and the settings shell
- keep the database model stable unless business requirements prove otherwise

That is the path most teams would likely choose because it is:

- understandable
- auditable
- safer than UI-only gating
- compatible with the current codebase
- incrementally adoptable without a major rewrite

## Reference Notes

The reasoning above was informed by a combined reading of:

- OWASP guidance on least privilege, deny by default, server-side enforcement, logging, and authorization testing
- NIST guidance on RBAC as the common enterprise baseline, plus RBAC/ABAC tradeoff awareness
- Microsoft guidance on roles, policies, centralized authorization services, and app roles
- Auth0 and Okta guidance on roles as permission containers, additive role assignment, and least privilege
- Aserto, Permit, and OPA guidance on central policy evaluation, role-to-permission mapping, enforcement at multiple layers, and separation-of-duty concerns
