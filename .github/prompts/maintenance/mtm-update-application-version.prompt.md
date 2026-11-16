---
description: "Loads the mtm-update-application-version skill: choose the semantic version bump from a change set, update every application version source plus the software_version SQL seed, publish an end-user update page, validate with a build, and commit the version-only files."
argument-hint: "Optionally include a commit hash to compare against (git diff <hash>), or an explicit target version override."
---

<!-- 
[DOC-META-START]
- File Name: mtm-update-application-version.prompt.md
- Description: Slash-command entry point that delegates the MTM application version release workflow to the mtm-update-application-version skill.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 19-19: # MTM Application Version Update
  - Line 25-25: ## How to run it
  - Line 36-36: ## What the skill owns
- Critical Notes: All rules, scripts, references, and templates live in .github/skills/mtm-update-application-version/; do not duplicate them here.
[DOC-META-END]
-->

# MTM Application Version Update

This prompt is only the slash-command entry point. The authoritative workflow, scripts, references,
and templates live in the `mtm-update-application-version` skill so there is exactly one copy to
maintain.

## How to run it

1. Load `.github/skills/mtm-update-application-version/SKILL.md`.
2. Execute its Steps 0-7 exactly, including the stop conditions and guardrails.
3. Use the skill's scripts instead of editing version strings by hand:
   - `scripts/Get-MtmVersionSnapshot.ps1` - read current versions and unlisted sources.
   - `scripts/Set-MtmVersion.ps1` - apply the new version to every known source.
   - `scripts/Test-MtmVersionConsistency.ps1` - prove cross-source agreement and build.
   - `scripts/Test-MtmUpdatePage.ps1` - validate the new release page and index entry.
4. Report the summary defined in the skill's Step 7.

## What the skill owns

- Semantic bump selection and its edge cases.
- The full version source inventory and the three-part versus four-part shapes.
- End-user update page structure, content rules, accessibility, and the `docs/updates/index.html`
  updates.
- The validation gates and the commit scope.
