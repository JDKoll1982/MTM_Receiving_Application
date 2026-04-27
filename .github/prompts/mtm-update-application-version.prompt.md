---
description: "Analyze uncommitted MTM changes, choose the best semantic version bump, update the application version plus software_version SQL artifacts, and create an end-user HTML update page."
---

# MTM Application Version Update

## Task

When I ask to change the application version, determine the most appropriate semantic version bump from the current uncommitted changes, then update the application and SQL deployment artifacts consistently and publish an end-user-friendly update page for that version.

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

## Step 4 - Create The End-User Update Page

After choosing the new version, create a versioned update page that explains the current patch in plain language for operators and other end users.

Create or update these files:

1. `docs/updates/{version}/index.html`
2. `docs/updates/index.html`
3. `docs/updates/styles.css`
4. `docs/updates/app.js`

Use the reusable mockup in `docs/updates/mockup/` as the required design and content pattern reference:

1. `docs/updates/mockup/index.html`
2. `docs/updates/mockup/styles.css`
3. `docs/updates/mockup/app.js`

The mockup assets are reference files only. Do not create version-local `styles.css` or `app.js` copies for each release page.
All versioned update pages must reuse the shared assets in `docs/updates/styles.css` and `docs/updates/app.js`.
If the shared assets need changes to support the new page, update the shared files once instead of adding per-version duplicates.

Requirements for the versioned update page:

- Use `../../../Assets/MTMLogo.jpg` in the page header.
- Keep the content end-user focused, not developer focused.
- Summarize only changes that are supported by the actual uncommitted diff.
- Group the update into clear sections such as highlights, what changed, why it matters, and any action the user should take.
- If no user action is required, say so explicitly.
- Reference the shared `../styles.css` and `../app.js` files from each versioned page rather than embedding everything inline.
- Keep the structure and visual pattern aligned with the mockup unless the current change set clearly needs a small, justified variation.

Requirements for `docs/updates/index.html`:

- Add the new version to the Releases table.
- Keep the newest version at the top of the list.
- Use a short end-user-facing summary that matches the actual release page content.
- Do not remove existing release entries unless the user explicitly asks for cleanup.

## Step 5 - Validate Consistency

- Confirm the same version string appears in every updated file.
- Confirm the chosen bump matches the actual scope of the uncommitted changes.
- Confirm the generated HTML page matches the chosen version folder name and references the shared `../styles.css` and `../app.js` files.
- Confirm `docs/updates/index.html` includes the new version entry with the correct link and summary.
- Confirm the page uses the shared MTM logo path correctly.
- Do not guess a prerelease suffix unless the user asked for one.

## Step 6 - Summarize

Return:

1. The previous version.
2. The new version.
3. Why that semantic version bump was chosen.
4. Which files were updated.
5. Where the end-user update page was created.
6. Whether `docs/updates/index.html` was updated.

## Guardrails

- Prefer semantic version core format `major.minor.patch`.
- If the change set mixes multiple categories, choose the highest required bump.
- If there are no meaningful code or deployment changes, say that no version bump is warranted.
- If no version bump is warranted, do not create a new `docs/updates/{version}` folder.
- Do not invent user-facing changes that are not supported by the diff.
- Do not update the live database unless the user explicitly asks for that in the current task.
