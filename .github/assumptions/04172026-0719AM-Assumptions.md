Last Updated: 2026-04-17

# Assumptions Requiring Confirmation

The settings-window focus change can be implemented safely without new assumptions.

The requested version-checker service, new `software_version` database table, startup enforcement, recurring five-minute verification, and version-bump prompt require clarification before implementation continues.

## 1. Source Of Truth Conflict: Compare Against DB vs Update DB To Current App Version

Assumption:
The database should be treated as the authoritative version source for mismatch checks, and the app should only update the `software_version` table during intentional version-change workflows, not automatically every startup before comparing.

Why this assumption is needed:
The request currently contains two conflicting requirements:

1. On startup, compare the running app version to the version saved in MySQL and force-close on mismatch.
2. Set the current application version on the database to the current app version.

If the app always writes its current version to the database during startup, the mismatch check becomes ineffective because the database value would immediately match the running app.

Potential impact if wrong:
If I choose the wrong authority, the app could either:

1. Never detect version mismatches.
2. Force-close valid clients after deployment.
3. Overwrite the deployment target version unexpectedly.

Alternative interpretations considered:

1. The database is authoritative, and startup only reads it.
2. The running app is authoritative, and startup always writes it to the database.
3. Startup writes to the database only when no row exists yet.

## 2. `software_version` Table Shape

Assumption:
`software_version` should be a single-row system table storing one active required version for the entire application environment.

Why this assumption is needed:
The request names a new table but does not define whether it should store:

1. One global row.
2. Multiple rows with history.
3. Separate rows by environment, workstation, branch, or channel.

Potential impact if wrong:
The service and stored procedures could be built around the wrong lookup model and require rework once deployment semantics are clarified.

Alternative interpretations considered:

1. One global row with the required current version.
2. Multiple rows with an `is_active` flag.
3. One row per environment such as Development vs Production.

## 3. Version String Source In The Application

Assumption:
The version checker should use `Application:Version` from `appsettings.json` as the application version source unless you want the assembly informational version or file version to be authoritative instead.

Why this assumption is needed:
The repo already exposes multiple possible version values:

1. `appsettings.json` → `Application.Version` currently set to `1.0.0`.
2. Generated assembly informational version values that include Git metadata.

Potential impact if wrong:
Using the wrong source could create false mismatches or make the version-bump prompt update the wrong files.

Alternative interpretations considered:

1. `appsettings.json` `Application.Version` is the human-controlled deployment version.
2. The assembly informational version is the authoritative runtime version.
3. Both should be updated and normalized to the same semantic version.

## 4. Startup Behavior When No Database Version Exists Yet

Assumption:
If the `software_version` table has no active version row yet, startup should initialize it from the current application version and continue without prompting.

Why this assumption is needed:
The request does not define the bootstrap behavior for a fresh environment or a newly deployed schema.

Potential impact if wrong:
The first startup after deployment could either block the app unnecessarily or silently skip enforcement when it should not.

Alternative interpretations considered:

1. First run seeds the table and continues.
2. First run is treated as a mismatch and closes.
3. First run shows a warning but does not close.

## 5. Prompt File Semantics For Version Changes

Assumption:
The new prompt should analyze uncommitted changes, choose the best semantic version bump, update the application-side version files, and update the SQL deployment artifacts for `software_version`, rather than attempting to modify the live database directly at prompt-creation time.

Why this assumption is needed:
The request says the prompt should update both the application and the `software_version` table, but a prompt file itself is only guidance for future agent execution. It does not directly change the live database unless the future task also includes database deployment steps.

Potential impact if wrong:
The resulting prompt could either under-scope the requested behavior or instruct unsafe direct-database mutations that do not match this repo's stored-procedure and deployment flow.

Alternative interpretations considered:

1. Update application files plus SQL schema/seed/procedure artifacts in-repo.
2. Update application files and issue a live database update immediately.
3. Update only app version files and leave database updates to deployment.

## 6. Version Comparison Semantics

Assumption:
Version comparison should be semantic numeric comparison on normalized `major.minor.patch` values, and any difference in either direction should trigger the forced-close prompt exactly as requested.

Why this assumption is needed:
The request explicitly says mismatches should trigger whether the database version is higher or lower, but it does not specify whether prerelease/build metadata should be ignored or included.

Potential impact if wrong:
Comparisons involving suffixes like `1.2.3-beta` or assembly metadata like `1.0.0+commitsha` could behave unexpectedly.

Alternative interpretations considered:

1. Compare only normalized semantic version core values.
2. Compare the full raw string.
3. Compare assembly informational versions including build metadata.

## Request For Confirmation

Please confirm, correct, or clarify the assumptions above before implementation continues on the version-checker and version-bump prompt.

The most important points to answer are:

1. Should the database be the authoritative required version, or should the app overwrite the database version on startup?
2. Should `software_version` be a single-row global table or something more specific?
3. Should the checker use `Application.Version` from `appsettings.json`, assembly version metadata, or both?
4. When the table is empty, should startup seed it and continue?
5. Should the future version-bump prompt update in-repo SQL artifacts, the live database, or both?
