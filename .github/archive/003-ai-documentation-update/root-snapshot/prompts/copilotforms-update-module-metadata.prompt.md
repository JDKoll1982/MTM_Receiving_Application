---
name: copilotforms-update-module-metadata
description: "Search the full codebase for a referenced module and update that modules CopilotForms metadata if it is stale."
agent: agent
argument-hint: "Enter a module name like Module_Receiving and optionally mention what changed in that module"
---

# CopilotForms Update Module Metadata

Search the full codebase for the referenced module and refresh that module's CopilotForms metadata so it matches the current code.

## Mission

Keep CopilotForms module metadata accurate after code changes by inspecting the real module code, comparing it to the current metadata, and updating only what is stale or missing.

## Inputs

- Target module: `${input:moduleName:Module_Receiving}`
- Optional context about what changed: `${input:changeNotes:Describe any known workflow, screen, file, or service changes}`

## Scope And Preconditions

1. Treat `${input:moduleName:Module_Receiving}` as the source-of-truth module to inspect.
2. Search the full module codebase, including the module folder, related tests, and nearby docs when they help clarify workflows or ownership.
3. Read the current CopilotForms metadata before editing it.
4. If the module already uses split metadata, update the files under `docs/CopilotForms/data/module-metadata/<ModuleName>/`.
5. If the module is still inline, update `docs/CopilotForms/data/copilot-forms.config.json` unless the user explicitly asks to split it.
6. Do not invent workflows, screens, files, or services that are not supported by the codebase.
7. Keep changes minimal and targeted to the referenced module.

## Workflow

1. Read the current workspace instructions and any relevant prompt or metadata guidance before editing.
2. Search the repository for `${input:moduleName:Module_Receiving}` and inspect the real code paths in that module.
3. Identify the current feature areas, important workflows, views, view models, services, DAOs, models, and user-facing actions.
4. Read the current CopilotForms metadata for that module.
5. Compare code to metadata and update only the stale or missing parts, including when relevant:
   - feature summaries
   - related files
   - related services
   - workflow areas
   - screens
   - UI patterns
   - UI elements
   - user actions
   - preserve rules
   - acceptance criteria
   - validation hints
   - prompt defaults
   - sub-feature hints
6. Preserve the existing metadata structure for that module unless the user explicitly asks for a structural migration, or if during your inspection you find that the current structure is misleading or no longer fits the codebase.
7. Validate that the edited JSON or Markdown files remain syntactically valid.
8. Summarize exactly what metadata changed and why.

## Output Expectations

- Update the metadata files directly when needed.
- Keep the metadata aligned with real code, not guesses.
- If the module does not appear to have CopilotForms metadata yet, say that clearly and add only the smallest necessary metadata structure.
- If the codebase does not provide enough evidence for a metadata claim, call that out instead of inventing it.

## Quality Assurance

- Confirm the module name used in the search matches `${input:moduleName:Module_Receiving}`.
- Confirm the final metadata reflects the current module files and workflows.
- Confirm unrelated modules were not changed.
- Confirm edited JSON remains valid.
