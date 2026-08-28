---
name: 'mtm-documentation-prepare'
description: 'Prepare repository-aware context before creating or rewriting MTM documentation.'
agent: 'agent'
tools: ['read_file', 'grep_search', 'semantic_search', 'runSubagent']
argument-hint: 'Describe the documentation target, scope, and any files to prioritize.'
---

<!-- 
[DOC-META-START]
- File Name: mtm-documentation.prepare.prompt.md
- Description: Prepare repository-aware context before creating or rewriting MTM documentation.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 9-12: # Prepare MTM Documentation Work
  - Line 13-16: ## Mission
  - Line 17-23: ## Scope And Preconditions
  - Line 24-29: ## Inputs
  - Line 30-42: ## Workflow
  - Line 43-48: ## Output Expectations
  - Line 49-54: ## Quality Assurance
- Critical Notes: Grounded current-state summary only — never cite nonexistent instruction files or start broad rewrites without owning code anchors.
[DOC-META-END]
-->

# Prepare MTM Documentation Work

Prepare the minimum accurate context needed before creating or rewriting repository documentation.

## Mission

Build a current-state summary grounded in active repo files and the owning code paths.

## Scope And Preconditions

- Use this prompt before writing architecture docs, README content, instruction files, or prompt
  documentation.
- Prefer active sources over archive content.
- Do not start broad rewrites until the owning files and current code anchors are identified.

## Inputs

- User goal for the documentation work
- Optional target files or folders
- Optional modules or features to prioritize

## Workflow

1. Read `README.md`, `.github/README.md`, `.github/copilot-instructions.md`, and `AGENTS.md`.
2. Read the smallest relevant instruction files under `.github/instructions/`.
3. Search the codebase for the owning modules, services, views, or workflows the docs must
   describe.
4. Identify stale or duplicate active docs that overlap the target.
5. Summarize:
   - source-of-truth files
   - real code anchors
   - conflicting or obsolete active docs
   - recommended files to update, merge, archive, or delete

## Output Expectations

- Return a concise preparation summary.
- Include the files that should change next.
- Call out any missing instruction coverage discovered during preparation.

## Quality Assurance

- Use only active documentation unless the task explicitly asks for historical comparison.
- Do not cite nonexistent instruction files.
- Stop and ask for approval if the work would require a major assumption.
