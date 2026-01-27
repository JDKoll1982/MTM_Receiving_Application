---
description: Standard workflow for creating and updating per-module documentation sets
applyTo: '**/*.md'
---

# Module Documentation Maintenance

## Purpose and Scope
- Ensure every module has a consistent `Documentation/` subfolder with the required documents.
- Keep documents fresh with an explicit `Last Updated: YYYY-MM-DD` line near the top of every file.
- Support AI editing, bug fixing, end-user guidance, and major refactors with predictable docs.

## Required Folder Structure (per module)
- `Documentation/Overview/`
- `Documentation/How-To-Guides/`
- `Documentation/Support-and-Fixes/`
- `Documentation/Changes-and-Decisions/`
- `Documentation/Big-Changes/`
- `Documentation/AI-Handoff/`
- `Documentation/End-User-Help/`
- `Documentation/Templates/`

## Mandatory Files (per module)
- `Overview/About-This-Module.md`
- `Overview/How-It-Works-at-a-Glance.md`
- `How-To-Guides/Daily-Tasks.md`
- `How-To-Guides/Unusual-Situations.md`
- `Support-and-Fixes/Common-Issues.md`
- `Support-and-Fixes/Checks-and-Health.md`
- `Changes-and-Decisions/Change-Log.md`
- `Changes-and-Decisions/Decisions.md`
- `Big-Changes/Refactor-Plan.md`
- `Big-Changes/Impact-Map.md`
- `AI-Handoff/Editing-Brief.md`
- `AI-Handoff/Guardrails.md`
- `End-User-Help/Quick-Start.md`
- `End-User-Help/FAQ.md`
- `Templates/README.md` (placeholder template copy)

## Content Expectations (plain language)
- Keep documents short, non-technical where possible, and task-focused.
- Each file must include a `Last Updated: YYYY-MM-DD` line within the first five lines.
- Use bullet lists and short paragraphs; avoid code or stack-specific jargon unless unavoidable.
- For change-oriented files (`Change-Log.md`, `Decisions.md`), add dated entries with concise rationale.
- For `Refactor-Plan.md`, include goals, risks, rollback steps, and success criteria.
- For `Editing-Brief.md` and `Guardrails.md`, list do/do-not rules, sensitive areas, and validation steps.

## Creation and Update Workflow
- If the user specifies modules: operate only on those modules.
- If none specified: choose a module with no `Documentation/` folder; if all have one, pick the module with the oldest `Last Updated` stamp in its docs.
- Create missing folders/files per the structure above; do not delete existing content.
- When updating existing docs, preserve useful content, refresh outdated sections, and update `Last Updated`.
- Keep edits minimal and scoped to documentation.

## Freshness and Logging Rules
- Always set `Last Updated: YYYY-MM-DD` to the edit date for every touched file.
- Append change notes to `Changes-and-Decisions/Change-Log.md` summarizing what changed and why.
- When decisions are made, add a brief entry to `Changes-and-Decisions/Decisions.md` with date and rationale.

## Validation Checklist
- Required folders exist in the module `Documentation/` directory.
- All mandatory files exist and contain `Last Updated` near the top.
- Change-Log includes the latest edit summary and date.
- Tone is plain language and code-agnostic.
- No unrelated files were modified.
