# MCP Guide 02 — Complete MCP Tool Catalog (No Tools Omitted)

This document catalogs **every MCP tool available in this environment** (all tool names starting with `mcp_`).

For each tool:

- **What it does**
- **When to use it** (use cases)
- **How to invoke it** in VS Code (without guessing hidden schemas)
- **Safety notes**
- **Example prompts**

> Tip: In VS Code Chat, you can invoke a tool explicitly by typing `#` and selecting the tool.
> VS Code will show a confirmation dialog with the tool’s **input schema** (fields + required values). Use that as the source of truth.

## Universal invocation pattern (works for every MCP tool)

1. In Copilot Chat, type `#` and pick the tool (e.g., `#mcp_filesystem_search_files`).
2. Fill in the required fields in the tool UI.
3. Approve the tool call.

When I include parameter names below, they reflect the most common schema for these servers.
If VS Code shows different fields, follow the UI.

---

## Filesystem MCP tools (`mcp_filesystem_*`)

### `mcp_filesystem_list_allowed_directories`

- **Does:** Lists directories the filesystem server is allowed to access.
- **Use cases:** Verify the server is correctly sandboxed; debug "access denied" errors.
- **How to invoke:** Call with no parameters.
- **Safety:** Read-only.
- **Example prompts:**
  - “#mcp_filesystem_list_allowed_directories”
  - “Before touching files, list allowed directories.”

### `mcp_filesystem_read_text_file`

- **Does:** Reads a text file (optionally first/last N lines).
- **Use cases:** Inspect config files, logs, source code, markdown, JSON.
- **How to invoke:** Provide `path`, optionally `head` or `tail` (not both).
- **Safety:** Read-only.
- **Example prompts:**
  - “Use #mcp_filesystem_read_text_file to read `${workspaceFolder}/README.md`.”
  - “Read the last 200 lines of `${workspaceFolder}/logs/app.log`.”

### `mcp_filesystem_read_file` (deprecated)

- **Does:** Legacy alias for reading text content.
- **Use cases:** Compatibility with older prompts.
- **How to invoke:** Same as `read_text_file`.
- **Safety:** Read-only.
- **Example prompts:**
  - “#mcp_filesystem_read_file (use only if a prompt expects it).”

### `mcp_filesystem_read_multiple_files`

- **Does:** Reads multiple text files in one call.
- **Use cases:** Compare configs; summarize multiple docs; review related code.
- **How to invoke:** Provide `paths` as an array of file paths.
- **Safety:** Read-only.
- **Example prompts:**
  - “Use #mcp_filesystem_read_multiple_files to read `appsettings.json`, `App.xaml.cs`, and `README.md`.”
  - “Read all files in this list and summarize differences.”

### `mcp_filesystem_read_media_file`

- **Does:** Reads binary media (image/audio) and returns base64 + MIME.
- **Use cases:** Pull an image asset for analysis; verify media exists.
- **How to invoke:** Provide `path` to a supported media file.
- **Safety:** Read-only.
- **Example prompts:**
  - “Use #mcp_filesystem_read_media_file to load `${workspaceFolder}/Assets/SplashScreen.png`.”
  - “Confirm this audio file exists and return its MIME type.”

### `mcp_filesystem_list_directory`

- **Does:** Lists directory entries.
- **Use cases:** Explore unknown repo structure; confirm outputs/artifacts exist.
- **How to invoke:** Provide `path` to a directory.
- **Safety:** Read-only.
- **Example prompts:**
  - “#mcp_filesystem_list_directory for `${workspaceFolder}`.”
  - “List the contents of the `docs/` folder.”

### `mcp_filesystem_list_directory_with_sizes`

- **Does:** Lists entries and sizes.
- **Use cases:** Find large artifacts; audit repo bloat; identify big logs.
- **How to invoke:** Provide `path` and (optionally) `sortBy`.
- **Safety:** Read-only.
- **Example prompts:**
  - “List sizes in `${workspaceFolder}/bin` using #mcp_filesystem_list_directory_with_sizes.”
  - “Sort by size and tell me the top 10 biggest files.”

### `mcp_filesystem_directory_tree`

- **Does:** Returns a recursive JSON tree for a directory.
- **Use cases:** Create documentation of project structure; quick repo overview.
- **How to invoke:** Provide `path`, optionally `excludePatterns`.
- **Safety:** Read-only.
- **Example prompts:**
  - “Generate a directory tree for `${workspaceFolder}` excluding `bin/**` and `obj/**`.”
  - “Create a tree for `src/` only.”

### `mcp_filesystem_search_files`

- **Does:** Searches for files/dirs matching a pattern.
- **Use cases:** Find `appsettings.json`; locate XAML; locate `*.csproj`.
- **How to invoke:** Provide `path` (root), `pattern`, optional `excludePatterns`.
- **Safety:** Read-only.
- **Example prompts:**
  - “Search for `**/*.csproj` under `${workspaceFolder}`.”
  - “Find `mcp.json` files excluding `**/bin/**` and `**/obj/**`.”

### `mcp_filesystem_get_file_info`

- **Does:** Gets file/directory metadata.
- **Use cases:** Confirm timestamps; check file sizes; verify type.
- **How to invoke:** Provide `path` to file/folder.
- **Safety:** Read-only.
- **Example prompts:**
  - “Get file info for `${workspaceFolder}/App.xaml`.”
  - “Check whether `${workspaceFolder}/docs` is a file or directory.”

### `mcp_filesystem_create_directory`

- **Does:** Creates a directory recursively.
- **Use cases:** Scaffold new feature folder; create output folder.
- **How to invoke:** Provide `path`.
- **Safety:** Writes to disk (idempotent).
- **Example prompts:**
  - “Create `${workspaceFolder}/docs/mcp`.”
  - “Create a `Reports/` folder if missing.”

### `mcp_filesystem_write_file`

- **Does:** Creates or overwrites a file.
- **Use cases:** Generate docs; create prompt files; produce config files.
- **How to invoke:** Provide `path` and `content`.
- **Safety:** Destructive (overwrites).
- **Example prompts:**
  - “Write a `docs/mcp/README.md` file with the following content…”
  - “Generate a `.vscode/mcp.json` template (do not overwrite if it exists — if overwrite is needed, ask first).”

### `mcp_filesystem_edit_file`

- **Does:** Applies targeted edits to a text file.
- **Use cases:** Patch JSON safely; update README sections; refactor snippets.
- **How to invoke:** Provide `path` plus one or more edits (often `oldText` → `newText`).
- **Safety:** Potentially destructive depending on edit; preview if supported.
- **Example prompts:**
  - “Update `.vscode/mcp.json` to add a new server entry (surgical edit).”
  - “Replace a config value across the file while preserving formatting.”

### `mcp_filesystem_move_file`

- **Does:** Moves/renames a file or directory.
- **Use cases:** Rename docs; reorganize folders; move generated output.
- **How to invoke:** Provide `source` and `destination`.
- **Safety:** Mutating.
- **Example prompts:**
  - “Rename `MCP.md` to `docs/mcp.md`.”
  - “Move generated artifacts into `out/`.”

---

## Playwright Browser MCP tools (`mcp_playwright_browser_*`)

### `mcp_playwright_browser_install`

- **Does:** Installs the Playwright browser binaries required by the server.
- **Use cases:** Fix “browser not installed” errors; first-time setup.
- **How to invoke:** Usually no parameters; VS Code will show any options.
- **Safety:** Downloads binaries.
- **Example prompts:**
  - “#mcp_playwright_browser_install”
  - “Install browsers and confirm success.”

### `mcp_playwright_browser_evaluate`

- **Does:** Runs JavaScript in the page context (or on a specific element).
- **Use cases:** Extract DOM values; run custom scripts; compute derived data.
- **How to invoke:** Provide the JS expression/script and (if offered) a selector/element handle.
- **Safety:** Can run arbitrary JS on pages; use on trusted sites.
- **Example prompts:**
  - “Navigate to the page, then evaluate `document.title`.”
  - “Extract JSON embedded in a `<script>` tag.”

### `mcp_playwright_browser_drag`

- **Does:** Performs drag-and-drop between two page elements.
- **Use cases:** Kanban board moves; UI testing; reordering.
- **How to invoke:** Provide source and target selectors.
- **Safety:** Mutates page state.
- **Example prompts:**
  - “Drag the first item to the end of the list.”
  - “Move a card between columns and confirm it moved.”

### `mcp_playwright_browser_fill_form`

- **Does:** Fills multiple form fields.
- **Use cases:** Automate login flows; fill multi-step forms; test validation.
- **How to invoke:** Provide a mapping of fields/selectors to values.
- **Safety:** Can submit personal data; use caution.
- **Example prompts:**
  - “Fill username/password fields and click submit.”
  - “Populate required fields with edge-case values to test validation.”

### `mcp_playwright_browser_file_upload`

- **Does:** Uploads files via the browser file chooser.
- **Use cases:** Test import features; upload docs/images.
- **How to invoke:** Provide the upload control selector and local file path.
- **Safety:** Sends local files to a website.
- **Example prompts:**
  - “Upload `${workspaceFolder}/testdata/report.csv` to the file input.”
  - “Upload an image and verify preview appears.”

### `mcp_playwright_browser_console_messages`

- **Does:** Returns browser console messages.
- **Use cases:** Debug front-end errors; verify warnings; triage script failures.
- **How to invoke:** Usually no parameters; may support filtering.
- **Safety:** Read-only.
- **Example prompts:**
  - “After reproducing the bug, show console messages.”
  - “List console errors only.”

---

## GitHub MCP tools (local GitHub server via MCP)

### `mcp_githublocal_get_file_contents`

- **Does:** Reads a file/directory from a GitHub repo.
- **Use cases:** Pull README; inspect a file at a ref; browse repo tree.
- **How to invoke:** Use tool UI to provide repo owner/name + path + ref.
- **Safety:** Read-only.
- **Example prompts:**
  - “Read `/README.md` from `owner/repo` on `main`.”
  - “List files under `/src`.”

### `mcp_githublocal_create_or_update_file`

- **Does:** Creates or updates a file in a GitHub repo.
- **Use cases:** Automated doc updates; config changes; templating.
- **How to invoke:** Use tool UI to specify repo + branch/ref + path + content + commit message.
- **Safety:** Writes to repo; ensure correct branch.
- **Example prompts:**
  - “Create `docs/mcp.md` on a new branch and open a PR (if your GitHub tools support it).”
  - “Update a file with a new section while preserving the rest.”

### `mcp_githublocal_list_commits`

- **Does:** Lists commits for a branch/tag/SHA.
- **Use cases:** Audit changes; find regression window; release notes.
- **How to invoke:** Provide repo + ref; optionally limit.
- **Safety:** Read-only.
- **Example prompts:**
  - “List the last 20 commits on `main`.”
  - “Show commits between two SHAs.”

### `mcp_githublocal_get_teams`

- **Does:** Lists GitHub teams you’re a member of (org-scoped).
- **Use cases:** Determine org access; automate team-based workflows.
- **How to invoke:** Provide org (if prompted).
- **Safety:** Read-only.
- **Example prompts:**
  - “List teams in `MyOrg`.”
  - “Which teams do I have access to that can review this repo?”

### `mcp_githublocal_list_issue_types`

- **Does:** Lists supported issue types for an org.
- **Use cases:** Ensure correct issue type when creating issues.
- **How to invoke:** Provide org.
- **Safety:** Read-only.
- **Example prompts:**
  - “List issue types for `MyOrg`.”
  - “Which issue type should I use for a defect vs feature?”

### `mcp_githublocal_assign_copilot_to_issue`

- **Does:** Assigns Copilot to a GitHub issue.
- **Use cases:** Delegate implementation tasks to Copilot coding agent.
- **How to invoke:** Provide repo + issue number.
- **Safety:** Writes to issue assignment.
- **Example prompts:**
  - “Assign Copilot to issue #123 and ask it to implement the fix.”
  - “Assign Copilot only after the acceptance criteria are clear.”

### `mcp_githublocal_add_comment_to_pending_review`

- **Does:** Adds a comment to your latest **pending** PR review.
- **Use cases:** Build a multi-comment review before submitting.
- **How to invoke:** Provide repo + PR + file/line context or general comment (per tool UI).
- **Safety:** Writes review comments.
- **Example prompts:**
  - “Add a review comment noting a nullability issue in a file.”
  - “Add a comment suggesting a unit test.”

---

## GitHub MCP tools (remote GitHub server via MCP)

### `mcp_githubremote_create_or_update_file`

- **Does:** Creates or updates a file in a GitHub repo using the remote GitHub MCP.
- **Use cases:** Same as local create/update, but via hosted server.
- **How to invoke:** Provide repo + ref + path + content + commit message (per tool UI).
- **Safety:** Writes to repo.
- **Example prompts:**
  - “Update docs on a branch and open a PR (if supported).”
  - “Create a new file with a template.”

### `mcp_githubremote_assign_copilot_to_issue`

- **Does:** Assigns Copilot to a GitHub issue using the remote GitHub MCP.
- **Use cases:** Delegate issue to Copilot coding agent.
- **How to invoke:** Provide repo + issue number.
- **Safety:** Writes to issue assignment.
- **Example prompts:**
  - “Assign Copilot to issue #123 and request a draft PR.”
  - “Assign Copilot after you’ve tagged the issue type.”

---

## Serena / OraiOS MCP tools (`mcp_oraios_*`)

### `mcp_oraios_serena_initial_instructions`

- **Does:** Returns Serena’s instructions manual.
- **Use cases:** Learn the tool’s symbol navigation workflow; onboarding.
- **How to invoke:** Call with no parameters.
- **Safety:** Read-only.
- **Example prompts:**
  - “#mcp_oraios_serena_initial_instructions”
  - “Summarize Serena’s recommended workflow.”

### `mcp_oraios_serena_onboarding`

- **Does:** Generates onboarding instructions if onboarding hasn’t been performed.
- **Use cases:** First-time setup for a repo; ensure Serena context is created.
- **How to invoke:** Call with no parameters.
- **Safety:** Read-only.
- **Example prompts:**
  - “#mcp_oraios_serena_onboarding”
  - “Onboard Serena, then locate entry points and DI configuration.”

### `mcp_oraios_serena_think_about_collected_information`

- **Does:** Reflection step to validate whether gathered info is sufficient.
- **Use cases:** After multi-step searches; before edits/refactors.
- **How to invoke:** Call with no parameters.
- **Safety:** Read-only.
- **Example prompts:**
  - “After exploring 5 files, run #mcp_oraios_serena_think_about_collected_information.”
  - “Decide whether more context is needed before implementing changes.”
