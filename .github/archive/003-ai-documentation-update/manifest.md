# 003 AI Documentation Update Archive Manifest

This folder preserves the pre-rewrite snapshot used for the AI documentation cleanup started on
2026-05-29.

## Snapshot Contents

- `root-snapshot/.github/` — pre-rewrite `.github` tree before category moves and document rewrites
- `root-snapshot/AGENTS.md` — pre-rewrite root agent file

## Purpose

- Preserve the original flat prompt and instruction layout for comparison.
- Preserve the original long-form authority files before the active set was shortened.
- Provide a rollback and reference point while the new categorized system is stabilized.

## Active Replacements

- Active repository overview: `README.md`
- Active AI taxonomy: `.github/README.md`
- Active global rules: `.github/copilot-instructions.md`
- Active agent contract: `AGENTS.md`

## Deleted From Active Set

- `.github/agents/copilot-instructions.md` — duplicate, superseded by `.github/copilot-instructions.md`

## Notes

- Archive content should not be used as active instruction by default.
- Workspace settings exclude `.github/archive` from normal file and search views.