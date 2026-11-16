---
applyTo: '**/.github/skills/mtm-update-application-version/references/*.md'
---

<!-- 
[DOC-META-START]
- File Name: version-source-inventory.md
- Description: Every MTM application version source, its required shape, and how to handle newly discovered sources.
- Last Updated: 2026-11-16
- Quick TOC:
  - Line 22-22: # Version Source Inventory
  - Line 26-26: ## Shapes
  - Line 33-33: ## Known sources
  - Line 48-48: ## Not a source
  - Line 60-60: ## Finding sources that are not in this list
  - Line 77-77: ## When the inventory changes
  - Line 85-85: ## Live database
- Critical Notes: Three-part version in appsettings.json and the seed script; four-part (trailing .0) in the project file and both manifests.
[DOC-META-END]
-->

# Version Source Inventory

Last Updated: 2026-11-16

## Shapes

- **3-part** `major.minor.patch` - application-facing and database-facing values.
- **4-part** `major.minor.patch.0` - Windows packaging values. The fourth field stays `0` for a
  normal release; increment it only when re-shipping an already published version, and only when
  the user explicitly asks for that.

## Known sources

| # | File | Field | Shape | Example |
| --- | --- | --- | --- | --- |
| 1 | `appsettings.json` | `Application.Version` | 3-part | `"Version": "4.7.0"` |
| 2 | `Database/Database_Deployment/Sql_Files/SeedData/05_seed_software_version.sql` | `required_version` value | 3-part | `'4.7.0'` |
| 3 | `MTM_Receiving_Application.csproj` | `<Version>` | 4-part | `<Version>4.7.0.0</Version>` |
| 4 | `MTM_Receiving_Application.csproj` | `<AssemblyVersion>` | 4-part | `<AssemblyVersion>4.7.0.0</AssemblyVersion>` |
| 5 | `MTM_Receiving_Application.csproj` | `<FileVersion>` | 4-part | `<FileVersion>4.7.0.0</FileVersion>` |
| 6 | `appxmanifest.xml` | `<Identity Version>` | 4-part | `Version="4.7.0.0"` |
| 7 | `Package.appxmanifest` | `<Identity Version>` | 4-part | `Version="4.7.0.0"` |

All seven must agree. `appsettings.json` and the seed script share the same 3-part string; the four
packaging fields share the same 4-part string.

## Not a source

These look version-related and must stay untouched:

- `docs/updates/{version}/index.html` - published history; each page keeps its own version.
- `docs/updates/index.html` release rows - historical entries are never rewritten; only the new row
  and the "latest" surfaces change.
- `Module_*/Documentation/**`, `docs/**`, `specs/**` - change logs describe past releases.
- `MTM_Receiving_Application.Tests/Package.appxmanifest` - test project identity, independent of
  the application release.
- `bin/`, `obj/`, `.vs/`, `TestResults/` - generated output.

## Finding sources that are not in this list

1. Run the snapshot script; it lists every file outside the known set that contains the current
   4-part literal (plus any file containing the current 3-part literal outside historical docs).
2. Confirm each hit is a real declaration, not documentation or a package version.
3. Update real declarations with the same shape rules above.
4. Report the extra source by path in the final summary so the inventory can be extended.

```powershell
pwsh -File .github/skills/mtm-update-application-version/scripts/Get-MtmVersionSnapshot.ps1 -Path . -AsJson
```

Typical additions to watch for: a second `.appxmanifest`, `Directory.Build.props` with a
`<Version>` property, an installer script (`.iss`, `.wxs`, `.nuspec`), a workflow file that pins a
display version, or a `version.json`. `Set-MtmVersion.ps1` does not write these; update them by
hand and say so.

## When the inventory changes

Adding a source requires three edits in the same change:

1. Add the file to the known-sources table above.
2. Teach `scripts/Set-MtmVersion.ps1` the new replacement.
3. Teach `scripts/Test-MtmVersionConsistency.ps1` the new assertion.

## Live database

`05_seed_software_version.sql` is deployment input only. Updating it does not change the running
MySQL database. Never update the live `software_version` row unless the user asks for that in the
current task, and never do it as part of a routine version bump.
