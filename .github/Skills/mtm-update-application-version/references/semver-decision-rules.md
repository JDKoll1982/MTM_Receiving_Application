---
applyTo: '**/.github/skills/mtm-update-application-version/references/*.md'
---

<!-- 
[DOC-META-START]
- File Name: semver-decision-rules.md
- Description: Bump-selection rules and edge cases for MTM application version releases.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 22-22: # Semver Decision Rules
  - Line 26-26: ## Core rule
  - Line 32-32: ## Signals and their bump
  - Line 65-65: ## Tie-breakers
  - Line 77-77: ## Checking the current version
  - Line 82-82: ## Worked examples
  - Line 93-93: ## Reporting the decision
- Critical Notes: Always take the highest bump the change set requires; never lower than current and never a prerelease suffix.
[DOC-META-END]
-->

# Semver Decision Rules

Last Updated: 2026-11-16

## Core rule

Pick the **highest** bump that any change in the set requires. A single breaking change makes the
release `MAJOR` even if ninety other files are fixes. Version numbers only move forward, and the
core format is strictly `major.minor.patch` with no prerelease or build-metadata suffix.

## Signals and their bump

### PATCH - `x.y.Z`

- Behavior corrected with no new capability: wrong totals, sorting, formatting, null handling.
- Copy and label changes, typo fixes, tooltips, log wording.
- Refactors, renames of internal types, dead-code removal with no user-visible change.
- Performance or startup improvements that do not add a feature.
- Dependency updates with no functional change.
- Documentation, comments, and `.github/` instruction work.
- Layout or style polish (spacing, alignment, dialog sizing) with the same workflow.

### MINOR - `x.Y.0`

- Any new screen, page, tab, tool, report, dashboard, or settings section.
- New user-visible behavior on an existing screen (new column, filter, button, bulk action).
- New stored procedure, new MySQL table, or additive columns that existing code tolerates.
- New configuration keys with reliable defaults, new optional settings.
- New workflow that operators can ignore without changing existing habits.
- New integration surface (new printer target, new export format) added alongside the old one.
- Splitting one screen into several, or moving a feature to a new window, while keeping every
  capability reachable.

### MAJOR - `X.0.0`

- A feature, screen, or workflow is removed or replaced with no equivalent path.
- A saved setting, layout, or stored selection is interpreted differently after upgrade.
- A required operator action appears after upgrade: re-enter setup, re-map printers, re-train.
- Database change that breaks existing deployments: dropped/renamed columns, changed stored
  procedure parameter contracts, required data migration the app cannot self-heal.
- Public contract changes that break other consumers (renamed handlers, changed command payloads).
- Version-scheme reset or any intentional break in compatibility with the previous release.

## Tie-breakers

1. **Removal always wins.** If anything a user relied on can no longer be done, choose `MAJOR`.
2. **New capability beats fixes.** A release containing at least one new capability is at least
   `MINOR`, even when most of the diff is bug fixing.
3. **Internal-only stays `PATCH`.** New classes, handlers, or services with no user-visible change
   are implementation detail, not new capability.
4. **Schema is judged by deployment risk, not size.** One additive table is `MINOR`; one renamed
   column without a migration is `MAJOR`.
5. **Repair mode is not a bump.** When the user asks to fix a wrong or inconsistent version with no
   new change set, keep the current number and only align the sources.

## Checking the current version

The baseline is the `Application.Version` value in `appsettings.json`. The snapshot script reports
it along with every other declaration so the decision never relies on memory.

## Worked examples

| Change set | Bump | Why |
| --- | --- | --- |
| Fixed totals rounding, renamed a private helper, updated a tooltip | PATCH | No new capability, no behavior removal |
| Added a Report Filters tab and one additive table | MINOR | New capability, backward compatible |
| Added scanner bulk mode and removed the single-location button | MAJOR | Capability removed from the workflow |
| Only `.github/` instruction files changed | none | No application or deployment change |
| Settings dialog resized, one typo fixed | PATCH | Polish only |
| Renamed the stored procedure the app calls, same parameters, seed updated | MAJOR | Deployment order now matters for every install |

## Reporting the decision

State the bump, then the concrete evidence: "MINOR - adds the Label Overlay print flow and the
Dunnage no-label flag; existing screens behave as before." One or two sentences is enough; the
change set is the justification, not personal preference.
