---
applyTo: "**/.github/skills/**/SKILL.md, **/.github/skills/**/scripts/*, **/.github/skills/**/references/*, **/.github/skills/**/templates/*"
---

# Skill Authoring Instructions (Repo Standard)

Last Updated: 2026-01-24

These instructions standardize how to create and maintain Agent Skills in this repository.

## 1) Skill scope rules

- Skills live in `/.github/skills/<skill-name>/`.
- Each skill must contain a `SKILL.md` in the skill root.
- Skills may include `scripts/`, `references/`, `assets/`, and `templates/`.
- Prefer one skill per workflow area (do not build “mega-skills”). Split large skills into multiple focused skills.

## 2) Frontmatter (discovery) rules

**CRITICAL:** Copilot decides whether to load a skill primarily from `name` + `description`.

### Required

```yaml
---
name: my-skill-name
description: <WHAT it does>. Use when asked to <WHEN>. Keywords: <common prompt terms>.
license: MIT
---
```

### Constraints

- `name`
  - lowercase
  - hyphens for spaces
  - max 64 chars
  - must match the folder name
- `description`
  - max 1024 chars
  - must include:
    - WHAT the skill does
    - WHEN to use it (specific triggers)
    - Keywords users will say

### Description template

Use this formula:

> `description: <WHAT>. Use when asked to <WHEN>. Keywords: <keyword1>, <keyword2>, <keyword3>.`

## 3) Progressive loading rules

- Keep `SKILL.md` short and action-oriented.
- Move deep explanations, large examples, adjacency rules, and edge cases to `references/`.
- Link resources using relative paths so Copilot can load them on-demand:

```markdown
See [architecture rules](./references/architecture-rules.md)
Run [validator](./scripts/validate-mvvm.ps1)
Start from [template](./templates/viewmodel-template.cs)
```

## 4) Resource folder rules

### `scripts/`
- Use for deterministic operations (audits, validation scans, report generation).
- Prefer PowerShell Core (`pwsh`) for Windows-friendly scripts.
- Follow `.github/instructions/powershell.instructions.md`.
- Scripts must:
  - accept a `-Path` parameter (default `.`)
  - use `Write-Verbose`/`Write-Warning`/`Write-Error` appropriately
  - avoid aliases (`gci`, etc.)
  - avoid storing secrets

### `references/`
- Use for detailed rulebooks and examples.
- Keep content stable and plain.
- Include `Last Updated: YYYY-MM-DD` near the top of every reference doc.

### `templates/`
- Use for scaffolds that Copilot should modify.
- Prefer placeholders like `[FEATURE_NAME]`, `[SERVICE_NAME]`, `[ENTITY]`.
- Keep templates minimal and compile-friendly.

### `assets/`
- Use for static files that Copilot should not modify.

## 5) Repo-specific guardrails (MVVM and data access)

When authoring skills that generate or modify code in this repo, they must respect repo architecture:

- ViewModels must be `partial` and inherit from `ViewModel_Shared_Base` (or project’s accepted base).
- ViewModels must not call DAOs directly.
- DAOs are instance-based and must return `Model_Dao_Result` (never throw).
- MySQL calls use stored procedures only.
- XAML binding must use `{x:Bind}` with explicit Mode.

If a skill conflicts with `.github/copilot-instructions.md`, the repo instructions win.

## 6) Commenting standard

Follow `.github/instructions/self-explanatory-code-commenting.instructions.md`:
- Prefer self-documenting code.
- Comment WHY, not WHAT.
- Avoid redundant or decorative comments.

## 7) Maintenance

- If a skill behavior changes, update:
  - `SKILL.md` (workflow/checklist)
  - any referenced documents
  - scripts/templates as needed
- Update `Last Updated: YYYY-MM-DD` in any changed `references/` docs.
