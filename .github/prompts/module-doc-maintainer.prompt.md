---
description: 'Create or refresh per-module documentation using the standard structure and freshness rules.'
agent: agent
model: GPT-5.1-Codex-Max
tools: [edit, read, search]
argument-hint: 'Optional: module names (comma-separated).'
---

# Module Documentation Maintainer

## Mission
Create or update the required documentation set for the target module(s), keeping everything in plain language and ensuring freshness logging.

## Inputs
- Optional module names (comma-separated). If omitted, auto-select:
  1) A module without a `Documentation/` folder; otherwise
  2) The module whose docs have the oldest `Last Updated` stamp.

## Workflow
1. **Pick targets**
   - If user supplied modules, use those.
   - Else list modules and pick based on rules above.
2. **Inspect docs**
   - Check for `Documentation/` and required subfolders/files.
   - Note `Last Updated: YYYY-MM-DD` near the top of each file.
3. **Create or update**
   - Create missing folders/files per standard structure.
   - Preserve existing helpful content; refresh outdated sections.
   - Insert or update `Last Updated: <today>` within first five lines of every touched file.
   - Add a brief dated entry to `Changes-and-Decisions/Change-Log.md` summarizing edits.
4. **Decisions**
   - If you made any meaningful choice (scope, tradeoff), add a dated note to `Changes-and-Decisions/Decisions.md` with one-sentence rationale.
5. **Tone and style**
   - Plain language, task-focused, code-agnostic; keep sections concise.
6. **Report**
   - List modules handled, files created/updated, and any follow-ups needed.

## Output Expectations
- Required files exist with current `Last Updated` lines.
- Change-Log updated with today’s entry for this run.
- Decisions file updated when applicable.

## Safeguards
- Do not delete existing content; augment instead.
- Keep scope to documentation only.
