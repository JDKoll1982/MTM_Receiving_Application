---
mode: ask
description: "Analyze uncommitted MTM changes, choose the best semantic version bump, and update the application version plus software_version SQL artifacts."
---

# MTM Application Version Update

## Task

When I ask to change the application version, determine the most appropriate semantic version bump from the current uncommitted changes, then update the application and SQL deployment artifacts consistently.

## Step 1 - Inspect Current Changes

Review the full uncommitted worktree before choosing the version bump:

```bash
git status
git diff
git diff --staged
```

Read the actual diffs and group them by intent.

## Step 2 - Choose The Best Semantic Version

Use these rules unless the user explicitly overrides the target version:

- `PATCH` for bug fixes, copy changes, refactors without new capability, docs-only changes, or small UX polish.
- `MINOR` for backward-compatible new capability, new settings, new workflows, new reports, new services, or schema additions that do not break existing behavior.
- `MAJOR` for breaking workflow changes, removed features, incompatible schema changes, renamed/removed public contracts, or required operator migration steps.

Explain the reasoning briefly before editing files.

## Step 3 - Update All Version Sources

Update these files together:

1. `appsettings.json`
   - Set `Application.Version` to the chosen version.

2. `Database/Database_Deployment/Sql_Files/SeedData/05_seed_software_version.sql`
   - Update the seeded `required_version` value.

If additional version sources are discovered later in the repo, update them too and mention them explicitly.

## Step 4 - Validate Consistency

- Confirm the same version string appears in every updated file.
- Confirm the chosen bump matches the actual scope of the uncommitted changes.
- Do not guess a prerelease suffix unless the user asked for one.

## Step 5 - Summarize

Return:

1. The previous version.
2. The new version.
3. Why that semantic version bump was chosen.
4. Which files were updated.

## Guardrails

- Prefer semantic version core format `major.minor.patch`.
- If the change set mixes multiple categories, choose the highest required bump.
- If there are no meaningful code or deployment changes, say that no version bump is warranted.
- Do not update the live database unless the user explicitly asks for that in the current task.
