---
name: mtm-update-application-version
description: 'Choose the correct semantic version bump from a change set, update every MTM application version source plus the software_version SQL seed, publish an end-user update page under docs/updates, validate with a build, and commit only the version files. Use when asked to bump, change, cut, or publish the application version, prepare release notes, or correct the version shown in the app. Keywords: version bump, semver, release, release notes, app version, appsettings Version, csproj FileVersion, appxmanifest Version, docs/updates, software_version.'
license: MIT
---

<!-- 
[DOC-META-START]
- File Name: SKILL.md
- Description: MTM application version release skill — analyze a change set, select the semantic bump, update all version sources and the software_version seed, publish an end-user update page, validate, and commit version-only files.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 30-30: # MTM Application Version Release
  - Line 36-36: ## When to use this skill
  - Line 45-45: ## Stop conditions
  - Line 58-58: ## Step 0 - Settle the inputs
  - Line 69-69: ## Step 1 - Inspect the change set
  - Line 85-85: ## Step 2 - Choose the bump
  - Line 99-99: ## Step 3 - Apply the version to every source
  - Line 115-115: ## Step 4 - Publish the end-user update page
  - Line 132-132: ## Step 5 - Validate
  - Line 147-147: ## Step 6 - Commit
  - Line 155-155: ## Step 7 - Report
  - Line 166-166: ## Guardrails
  - Line 177-177: ## Resources
- Critical Notes: Never bump without a real change set, keep 3-part and 4-part sources in sync, reuse the shared docs/updates assets, and only commit version-owned files.
[DOC-META-END]
-->

# MTM Application Version Release

Turn a reviewed change set into a complete, consistent version release: new version everywhere it
is declared, a published end-user update page, a passing build, and a commit limited to the files
this workflow owns.

## When to use this skill

- "Bump the app version", "cut a release", "publish 4.8.0", "write the release notes".
- "The version in the app/installer is wrong" — repair mode, still follow Steps 3 and 5.
- A change set is finished and the user asks what the next version should be.

Do not use it for marketing site copy, database-only migrations that leave the application version
alone, or changes that are still in progress and need review first.

## Stop conditions

Stop and report instead of editing when any of these is true:

- The change set contains no meaningful application or deployment change (for example, only
  `.github/`, `bin/`, `obj/`, or generated artifacts changed). Report that no bump is warranted.
- The user supplied a target version that is lower than or equal to the current version.
- Required inputs cannot be resolved and the user declines to choose (see Step 0).
- The working tree contains unrelated unstaged work that would be swept into the commit.

When a stop condition fires, do not create `docs/updates/{version}`, do not edit version sources,
and do not commit.

## Step 0 - Settle the inputs

Decide these two things before analyzing anything.

1. **Change set.** If the user supplied a commit hash, the change set is everything since that
   hash, including uncommitted work: `git diff <hash>`, `git diff --staged`, `git status`.
   If no hash was supplied, ask once whether they want a base commit; if they decline, use the
   uncommitted working tree only.
2. **Target version (optional).** If the user supplied an explicit version, use it and skip
   bump selection in Step 2. Otherwise derive the bump.

## Step 1 - Inspect the change set

```powershell
pwsh -File .github/skills/mtm-update-application-version/scripts/Get-MtmVersionSnapshot.ps1 -Path . -AsJson
git status --short
git diff --stat
git diff --staged --stat
```

Read snapshot output first: it reports the current version in every declared source, any drift
between them, and any file that contains the version but is not in the known list.

Then read the actual diffs and group them by intent: new capability, behavior fix, UX polish,
documentation, deployment/schema, breaking change. Treat `.github/`, `bin/`, `obj/`, and generated
artifacts as noise when judging scope.

## Step 2 - Choose the bump

Apply the highest bump that any part of the change set requires. Never go lower than the current
version and never invent prerelease or build-metadata suffixes.

| Bump | Use when the change set contains | Examples |
| --- | --- | --- |
| PATCH | Fixes, copy changes, refactors, docs, small polish | wrong total, layout fix, log noise |
| MINOR | New backward-compatible capability | new page, tool, setting, report, additive schema |
| MAJOR | Breaking or migration-required change | removed feature, renamed contract, incompatible schema, required operator steps |

Explain the choice in one or two sentences, naming the specific changes that drove it. Full edge
cases: [semver decision rules](./references/semver-decision-rules.md).

## Step 3 - Apply the version to every source

```powershell
pwsh -File .github/skills/mtm-update-application-version/scripts/Set-MtmVersion.ps1 -Path . -Version <newVersion>
```

The script updates the known sources with the correct shape per file: 3-part `major.minor.patch`
in `appsettings.json` and the `software_version` seed, 4-part `major.minor.patch.0` in the project
file and both manifests. Use it instead of hand-editing; it validates the input and reports every
file it changed or skipped.

Then handle anything the snapshot flagged as an unexpected version source: update it as well and
name it in the final report. Never rewrite version strings inside `docs/updates/{oldVersion}/` —
published pages are history. Source inventory and shape rules:
[version source inventory](./references/version-source-inventory.md).

## Step 4 - Publish the end-user update page

1. Start from [version page template](./templates/version-page-template.html) and write
   `docs/updates/{version}/index.html`.
2. Add a `docs/updates/index.html` release row using
   [release index row](./templates/release-index-row.html), newest first, and refresh the hero
   badge, the `data-copy-text` value, and the "Latest release at a glance" cards.
3. Reuse the shared `docs/updates/styles.css` and `docs/updates/app.js`. Do not create version-local
   copies of either, and do not add inline `<style>` or `<script>` blocks.
4. Use `docs/updates/mockup/` as the design reference. Keep its structure and interaction hooks
   unless the change set justifies a small, explainable variation.

Write for operators, not developers: plain language, real screen names, no file paths, no class
names. Describe only changes the diff actually supports and state explicitly whether any user
action is required. Content, accessibility, and index-update rules:
[update page authoring](./references/update-page-authoring.md).

## Step 5 - Validate

```powershell
pwsh -File .github/skills/mtm-update-application-version/scripts/Test-MtmVersionConsistency.ps1 -Path . -ExpectedVersion <newVersion> -PreviousVersion <oldVersion> -RunBuild
pwsh -File .github/skills/mtm-update-application-version/scripts/Test-MtmUpdatePage.ps1 -Path . -Version <newVersion>
```

Both scripts exit non-zero on failure and print one status line per check. Fix every failure and
re-run. The consistency run includes `dotnet build` (Debug, x64) unless `-RunBuild` is dropped;
dropping it is only acceptable for repair-mode runs where no build-affecting file changed.

Manual confirmation that scripts cannot make: the chosen bump matches the real scope, the page
describes only real changes, and no stale old-version string survives outside historical release
pages. Full checklist: [validation checklist](./references/validation-checklist.md).

## Step 6 - Commit

- Stage only the version-owned files: the version sources, `docs/updates/{version}/index.html`,
  `docs/updates/index.html`, and the shared update assets if they genuinely changed.
- Never `git add -A`; unrelated worktree changes stay untouched.
- Commit message: `Bump application version to {version}`.
- Report the resulting commit hash. Do not push unless the user asks.

## Step 7 - Report

Return these items in order, brief and factual:

1. Previous version and new version.
2. Why that bump was chosen.
3. Every file updated, flagging any unexpected version source.
4. Location of the new update page and whether `docs/updates/index.html` was updated.
5. Validation results, including the build outcome.
6. Commit hash.

## Guardrails

- Prefer `major.minor.patch`; never guess prerelease identifiers or build metadata.
- Mixed-category change sets take the highest required bump.
- Ignore `.github/`, `bin/`, `obj/`, and generated artifacts when judging bump scope.
- Never invent user-facing changes that the change set does not support.
- Do not create a new release page when no bump is warranted.
- Do not modify the live MySQL `software_version` row or run migrations unless the user explicitly
  asks in the current task; editing the seed script does not deploy it.
- Do not touch DAO, ViewModel, or XAML code as part of a version release.

## Resources

| Resource | Use |
| --- | --- |
| [Get-MtmVersionSnapshot.ps1](./scripts/Get-MtmVersionSnapshot.ps1) | Read-only report of every version source plus unlisted occurrences |
| [Set-MtmVersion.ps1](./scripts/Set-MtmVersion.ps1) | Apply a validated version to all known sources |
| [Test-MtmVersionConsistency.ps1](./scripts/Test-MtmVersionConsistency.ps1) | Cross-source consistency, stale-string scan, optional build |
| [Test-MtmUpdatePage.ps1](./scripts/Test-MtmUpdatePage.ps1) | Structure, asset, accessibility, and index checks for the release page |
| [version page template](./templates/version-page-template.html) | Starting point for `docs/updates/{version}/index.html` |
| [release index row](./templates/release-index-row.html) | Row snippet for `docs/updates/index.html` |
| [semver decision rules](./references/semver-decision-rules.md) | Bump selection edge cases |
| [version source inventory](./references/version-source-inventory.md) | Every source, its shape, and how to handle new ones |
| [update page authoring](./references/update-page-authoring.md) | Content, design hooks, accessibility, index updates |
| [validation checklist](./references/validation-checklist.md) | Final gate before commit |
| [MtmVersionShared.ps1](./scripts/MtmVersionShared.ps1) | Internal helper library dot-sourced by the four scripts; not run directly |
