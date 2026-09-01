---
description: "Analyze a change set (uncommitted work or since a commit hash), choose the best semantic version bump, update every application version source plus software_version SQL artifacts, publish an end-user update page, validate with a build, and commit the version-only files."
argument-hint: "Optionally include a commit hash to compare against (git diff <hash>), or an explicit target version override."
---

<!-- 
[DOC-META-START]
- File Name: mtm-update-application-version.prompt.md
- Description: Analyze a change set (uncommitted work or since a commit hash), choose the best semantic version bump, update every application version source plus software_version SQL artifacts, publish an end-user update page, validate with a build, and commit the version-only files.
- Last Updated: 2026-09-01
- Quick TOC:
  - Line 27-28: # MTM Application Version Update
  - Line 29-35: ## Task
  - Line 36-45: ## Step 0 - Gather Inputs
  - Line 46-56: ## Step 1 - Inspect Current Changes
  - Line 57-70: ## Step 2 - Choose The Best Semantic Version
  - Line 71-97: ## Step 3 - Update All Version Sources
  - Line 98-149: ## Step 4 - Create The End-User Update Page
  - Line 150-165: ## Step 5 - Validate Consistency
  - Line 166-172: ## Step 6 - Commit
  - Line 173-185: ## Step 7 - Summarize
  - Line 186-196: ## Guardrails
- Critical Notes: Choose the highest required semantic bump, keep 3-part and 4-part sources consistent, ignore .github/archive/ and bin/obj when judging scope, and keep release pages end-user focused and accessible.
[DOC-META-END]
-->

# MTM Application Version Update

## Task

When I ask to change the application version, determine the most appropriate semantic version
bump from the selected change set, then update the application and SQL deployment artifacts
consistently, publish an end-user-friendly update page for that version, validate with a build,
and commit the version-only files.

## Step 0 - Gather Inputs

Settle the two inputs that define the work before analyzing anything:

- **Change set.** If the user supplied a commit hash, the change set is `git diff <hash>` — all
  changes since that hash, including uncommitted work. If no hash was supplied, ask the user for
  an optional base commit hash; if they decline, use the uncommitted working tree only.
- **Target version (optional).** If the user supplied an explicit version override, use it and
  skip the bump selection in Step 2. Otherwise derive the bump from the change set.

## Step 1 - Inspect Current Changes

Establish the baseline version and review the selected change set before choosing the bump:

- Read the current version from `appsettings.json` and confirm it matches the other sources.
- Run `git status`, `git diff`, and `git diff --staged` against the selected change set.
- Exclude `.github/archive/` and generated directories (`bin/`, `obj/`) when judging the scope
  of the bump; treat them as noise, not as release content.
- Read the actual diffs and group them by intent (new capability, bug fix, docs, UX polish,
  breaking change).

## Step 2 - Choose The Best Semantic Version

Use these rules unless the user explicitly overrides the target version:

- `PATCH` for bug fixes, copy changes, refactors without new capability, docs-only changes, or
  small UX polish.
- `MINOR` for backward-compatible new capability, new settings, new workflows, new reports,
  new services, or schema additions that do not break existing behavior.
- `MAJOR` for breaking workflow changes, removed features, incompatible schema changes,
  renamed/removed public contracts, or required operator migration steps.

Never select a version lower than the current version. Use strictly `major.minor.patch` — do not
add prerelease or build-metadata suffixes. Explain the reasoning briefly before editing files.

## Step 3 - Update All Version Sources

Scan the repo for the current 3-part and 4-part version strings and update every source found.
Report any source that is not in the known list below. Do not replace version strings inside
`docs/updates/` — Step 4 owns those files (the old versioned page keeps its version as history).

Known sources (the expected minimum set):

1. `appsettings.json`
   - Set `Application.Version` to the chosen version (3-part semver, e.g. `2.1.0`).

2. `Database/Database_Deployment/Sql_Files/SeedData/05_seed_software_version.sql`
   - Update the seeded `required_version` value (3-part semver).

3. `MTM_Receiving_Application.csproj`
   - Update `<Version>`, `<AssemblyVersion>`, and `<FileVersion>` to `{version}.0` (4-part,
     e.g. `2.1.0.0`).

4. `appxmanifest.xml`
   - Update the `Version` attribute on the `<Identity>` element (4-part, e.g. `2.1.0.0`).

5. `Package.appxmanifest`
   - Update the `Version` attribute on the `<Identity>` element (4-part, e.g. `2.1.0.0`).

If the scan finds additional version sources (for example, a new manifest or seed file), update
them too and mention them explicitly in the summary.

## Step 4 - Create The End-User Update Page

After choosing the new version, create a versioned update page that explains the current
release in plain language for operators and other end users.

Create or update these files:

1. `docs/updates/{version}/index.html`
2. `docs/updates/index.html`
3. `docs/updates/styles.css`
4. `docs/updates/app.js`

Use the reusable mockup in `docs/updates/mockup/` as the required design and content pattern reference:

1. `docs/updates/mockup/index.html`
2. `docs/updates/mockup/styles.css`
3. `docs/updates/mockup/app.js`

The mockup assets are reference files only. Do not create version-local `styles.css` or `app.js`
copies for each release page. All versioned update pages must reuse the shared assets in
`docs/updates/styles.css` and `docs/updates/app.js`. If the shared assets need changes to
support the new page, update the shared files once instead of adding per-version duplicates.

Requirements for the versioned update page:

- Use `../../../Assets/MTMLogo.jpg` in the page header.
- Use today's date for the `Released {Month DD, YYYY}` label.
- Keep the content end-user focused, not developer focused.
- Summarize only changes that are supported by the actual change set.
- Follow release-note best practices: state the release date, group changes into clear categories
  (features, fixes, improvements), and note any issues or tickets addressed.
- Group the update into clear sections such as highlights, what changed, why it matters, and any
  action the user should take. If no user action is required, say so explicitly.
- Write in plain, scannable language. Use descriptive link text (never "here" or "click me").
- Use semantic HTML landmarks (`main`, `nav`, `article`), a logical heading hierarchy, and lists
  so the page stays navigable by keyboard and screen reader. Keep buttons and links as native
  interactive elements so they work without custom focus handling.
- Reference the shared `../styles.css` and `../app.js` files from each versioned page rather than
  embedding everything inline.
- Keep the structure and visual pattern aligned with the mockup unless the current change set
  clearly needs a small, justified variation.

Requirements for `docs/updates/index.html`:

- Add the new version to the Releases table and keep the newest version at the top.
- Update the hero `Latest release` badge to the new version.
- Update the `Copy latest version` button `data-copy-text` value to the new version.
- Replace the `Latest release at a glance` highlight section with highlights that match the new
  release page.
- Use a short end-user-facing summary that matches the actual release page content.
- Do not remove existing release entries unless the user explicitly asks for cleanup.

## Step 5 - Validate Consistency

- Confirm the same 3-part version string appears in `appsettings.json` and
  `05_seed_software_version.sql`.
- Confirm the same 4-part version string (`{version}.0`) appears in
  `MTM_Receiving_Application.csproj` (`<Version>`, `<AssemblyVersion>`, `<FileVersion>`),
  `appxmanifest.xml` (`<Identity Version>`), and `Package.appxmanifest` (`<Identity Version>`).
- Grep the non-historical sources and `docs/updates/index.html` to confirm no stale old-version
  strings remain. The old versioned page under `docs/updates/{oldVersion}/` keeps its version
  intentionally.
- Confirm the chosen bump matches the actual scope of the change set.
- Confirm the generated HTML page matches the chosen version folder name and references the
  shared `../styles.css` and `../app.js` files.
- Confirm the page uses the shared MTM logo path correctly.
- Run `dotnet build` (Debug, x64) to validate the version edits compile.

## Step 6 - Commit

- Stage only the version sources and the `docs/updates/` files changed by this workflow. Do not
  sweep unrelated worktree changes into the commit.
- Commit with message `Bump application version to {version}`.
- Report the resulting commit hash.

## Step 7 - Summarize

Return:

1. The previous version.
2. The new version.
3. Why that semantic version bump was chosen.
4. Which files were updated, including any unexpected version sources found by the scan.
5. Where the end-user update page was created.
6. Whether `docs/updates/index.html` was updated.
7. The `dotnet build` result.
8. The commit hash.

## Guardrails

- Prefer semantic version core format `major.minor.patch`; never guess a prerelease or build
  metadata suffix.
- If the change set mixes multiple categories, choose the highest required bump.
- Ignore `.github/archive/` and generated directories when judging bump scope.
- If there are no meaningful code or deployment changes, say that no version bump is warranted.
- If no version bump is warranted, do not create a new `docs/updates/{version}` folder, update
  version sources, or commit.
- Do not invent user-facing changes that are not supported by the change set.
- Do not update the live database unless the user explicitly asks for that in the current task.
