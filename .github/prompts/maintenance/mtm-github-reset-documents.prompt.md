---
name: 'mtm-github-reset-documents'
description: 'Reset or modernize the repository AI-documentation surfaces using the active taxonomy and archive-first workflow.'
agent: 'agent'
tools: ['read_file', 'grep_search', 'list_dir', 'apply_patch', 'run_in_terminal', 'get_errors']
argument-hint: 'Describe which .github surfaces to reset or modernize.'
---

<!-- 
[DOC-META-START]
- File Name: mtm-github-reset-documents.prompt.md
- Description: Reset or modernize the repository AI-documentation surfaces using the active taxonomy and archive-first workflow.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 9-12: # Reset MTM AI Documentation
  - Line 13-17: ## Mission
  - Line 18-24: ## Scope And Preconditions
  - Line 25-29: ## Inputs
  - Line 30-39: ## Workflow
  - Line 40-45: ## Output Expectations
  - Line 46-50: ## Quality Assurance
- Critical Notes: Archive first — confirm an archive snapshot exists before deleting or replacing active duplicates.
[DOC-META-END]
-->

# Reset MTM AI Documentation

Archive first, then simplify and modernize the active `.github` documentation surfaces.

## Mission

Use the active taxonomy to replace stale, duplicated, or flat AI-documentation surfaces with
current focused files.

## Scope And Preconditions

- Use this prompt for `.github` cleanup, prompt and instruction restructuring, or agent-documentation
  resets.
- Keep the active set short and authoritative.
- Preserve historical material under `.github/archive/` before deleting or replacing active files.

## Inputs

- Target surfaces to reset, such as instructions, prompts, agents, READMEs, or settings
- Optional list of files to preserve as active sources of truth

## Workflow

1. Read `.github/README.md`, `.github/copilot-instructions.md`, `.github/prompts/README.md`, and
   `.github/instructions/README.md`.
2. Inventory the target files and classify them as keep, rewrite, merge, archive, or delete.
3. Confirm an archive snapshot exists before deleting active duplicates.
4. Rewrite the highest-authority files first.
5. Update prompt and instruction path references after moves.
6. Validate links, settings, and file discovery behavior.

## Output Expectations

- Produce the updated files directly.
- Keep each rewritten file focused on one responsibility.
- Report which active duplicates were deleted and which files were newly created.

## Quality Assurance

- Do not recreate the old flat taxonomy.
- Do not leave active files that simply duplicate archived content.
- Do not leave stale references to moved prompt or instruction files.
