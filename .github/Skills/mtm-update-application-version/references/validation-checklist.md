---
applyTo: '**/.github/skills/mtm-update-application-version/references/*.md'
---

<!-- 
[DOC-META-START]
- File Name: validation-checklist.md
- Description: Final gate for an MTM version release — automated checks, manual checks, failure handling, and commit staging.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 21-21: # Validation Checklist
  - Line 25-25: ## Automated gates
  - Line 58-58: ## Manual gates
  - Line 69-69: ## Failure handling
  - Line 78-78: ## Commit gate
  - Line 92-92: ## Report template
- Critical Notes: Do not commit until both scripts pass and the build succeeds; stage only version-owned files.
[DOC-META-END]
-->

# Validation Checklist

Last Updated: 2026-11-16

## Automated gates

Run from the repository root, replacing the placeholders.

```powershell
pwsh -File .github/skills/mtm-update-application-version/scripts/Test-MtmVersionConsistency.ps1 -Path . -ExpectedVersion <newVersion> -PreviousVersion <oldVersion> -RunBuild
pwsh -File .github/skills/mtm-update-application-version/scripts/Test-MtmUpdatePage.ps1 -Path . -Version <newVersion>
```

Both print one line per check and exit non-zero when a check fails. A release is not complete
until both exit zero.

What the consistency run proves:

- `appsettings.json` and `05_seed_software_version.sql` carry the same 3-part version.
- `<Version>`, `<AssemblyVersion>`, `<FileVersion>`, and both `<Identity Version>` attributes carry
  the same 4-part version, `{version}.0`.
- No known source still holds the previous version.
- Any file outside the known set that holds the new version is reported.
- The solution builds in Debug x64 when `-RunBuild` is supplied.

What the update-page run proves:

- `docs/updates/{version}/index.html` exists and names the version in its title, heading, and
  copy hook.
- The page loads the shared `../styles.css` and `../app.js`, uses the `../../../Assets/MTMLogo.jpg`
  logo, and has no inline style or script blocks.
- No version-local `styles.css` or `app.js` was added.
- Required sections, ids, filter hooks, and the release date label are present.
- Landmarks, `lang`, viewport, image `alt` values, and descriptive link text pass.
- `docs/updates/index.html` lists the new version first, updates the latest-release badge, the copy
  value, and the at-a-glance heading.

## Manual gates

Neither script can judge these; confirm them yourself.

1. The chosen bump matches the real scope, including the highest-category rule.
2. Every statement on the release page is traceable to the change set. Remove anything invented.
3. The "What you need to do" section is explicit, including the no-action case.
4. Internal-only changes were translated or omitted, never described with code vocabulary.
5. No old-version string survives in a live source; historical pages keep theirs intentionally.
6. The release date is the actual publication date, not a copy of the previous page.

## Failure handling

- Version drift between sources: re-run `Set-MtmVersion.ps1`, then the consistency script.
- Unexpected version source reported: decide whether it is live, update it if so, and name it in the
  summary; if it is historical documentation, leave it and say why.
- Build failure: stop. Version edits do not cause compile errors on their own, so investigate the
  change set before committing anything.
- Update-page failure: fix the page rather than weakening the check.

## Commit gate

Stage exactly these paths, and nothing else:

- The seven known version declarations.
- `docs/updates/{version}/index.html`.
- `docs/updates/index.html`.
- `docs/updates/styles.css` or `docs/updates/app.js` only if they truly changed.
- Any newly discovered live version source, called out in the summary.

Verify with `git diff --cached --stat` before committing, and confirm the staged set contains no
unrelated worktree changes. Commit message: `Bump application version to {version}`. Do not push
unless the user asks.

## Report template

```text
Previous version: {oldVersion}
New version:      {newVersion}
Bump:             {PATCH|MINOR|MAJOR} - {one-line reason}
Sources updated:  {list, flagging any unexpected source}
Update page:      docs/updates/{version}/index.html (index.html updated: yes/no)
Validation:       consistency {pass/fail}, update page {pass/fail}, build {pass/fail}
Commit:           {hash}
```
