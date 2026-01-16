# MCP Guide 03 — MCP Recipes (Cheat Sheet)

This is a **project-agnostic** set of “recipes” that map common tasks to specific MCP servers + tools.

Use it as a quick menu when you don’t want to remember tool names.

## Conventions

- Invoke tools explicitly in VS Code Chat by typing `#` and selecting the tool.
- Always run `mcp_filesystem_list_allowed_directories` first if you intend to read/write files.
- For GitHub actions, prefer `githubRemote` when available; use Docker `githubLocal` when you need local control.

---

## Repo discovery & documentation

### Recipe: Generate a clean repo map

**Goal:** Identify key areas quickly.

- Tools:
  - `#mcp_filesystem_directory_tree` (exclude `bin/**`, `obj/**`, `node_modules/**`)
  - `#mcp_filesystem_search_files` for `README*`, `docs/**`, `*.sln*`, `*.csproj`, `.vscode/mcp.json`
- Output you should produce:
  - A short “where things live” summary
  - A list of entry points (app startup, DI config, main UI)

### Recipe: Create a docs skeleton

**Goal:** Add a minimal docs structure to any repo.

- Tools:
  - `#mcp_filesystem_create_directory` (e.g., `docs/`, `docs/architecture/`, `docs/mcp/`)
  - `#mcp_filesystem_write_file` (create `docs/index.md`, `docs/mcp/README.md`)
- Safety:
  - If `docs/` already exists, ask before overwriting existing docs.

---

## Codebase search & triage

### Recipe: Find where a feature is implemented

**Goal:** Locate relevant code fast.

- Tools:
  - `#mcp_filesystem_search_files` (search for likely filenames)
  - `#mcp_filesystem_read_text_file` (read the top/bottom portions)
  - `#mcp_filesystem_read_multiple_files` (compare related files)
- Tip:
  - Search for UI labels, routes, command names, or model names first.

### Recipe: Spot “hot” directories (large outputs)

**Goal:** Find large folders/files that slow clones or CI.

- Tools:
  - `#mcp_filesystem_list_directory_with_sizes`
  - `#mcp_filesystem_get_file_info`
- Follow-up actions:
  - Propose `.gitignore` updates
  - Move big artifacts out of repo

---

## Safe editing workflows (local files)

### Recipe: Surgical config edit (JSON/YAML/MD)

**Goal:** Change one thing without breaking formatting.

- Tools:
  - `#mcp_filesystem_read_text_file` (confirm exact current text)
  - `#mcp_filesystem_edit_file` (minimal replacement)
- Safety:
  - Avoid broad replacements; keep edits small.

### Recipe: Create a new file safely

**Goal:** Write a new file without overwriting.

- Tools:
  - `#mcp_filesystem_search_files` (confirm it doesn’t exist)
  - `#mcp_filesystem_write_file`
- Safety:
  - If the file exists, stop and ask before overwriting.

---

## GitHub workflows

### Recipe: Read a file from GitHub without cloning

**Goal:** Inspect remote repo content.

- Tools:
  - `#mcp_githublocal_get_file_contents` or `#mcp_githubremote_*` equivalent
- Use cases:
  - Confirm templates exist
  - Compare branches/tags

### Recipe: Update docs in a repo (branch-based)

**Goal:** Create or update a documentation file on a branch.

- Tools:
  - `#mcp_githublocal_create_or_update_file` (or `#mcp_githubremote_create_or_update_file`)
- Safety:
  - Always write to a **non-default branch** unless explicitly instructed.
  - Use clear commit messages.

### Recipe: Assign Copilot coding agent to an issue

**Goal:** Delegate an implementation task.

- Tools:
  - `#mcp_githublocal_assign_copilot_to_issue` or `#mcp_githubremote_assign_copilot_to_issue`
- Best practice:
  - Ensure acceptance criteria are written in the issue first.

### Recipe: Leave a multi-comment PR review (pending review)

**Goal:** Build a structured review before submitting.

- Tools:
  - `#mcp_githublocal_add_comment_to_pending_review`
- Notes:
  - A pending review must already exist.

---

## Browser automation (Playwright)

### Recipe: Install browsers (first run)

**Goal:** Fix “browser not installed” quickly.

- Tools:
  - `#mcp_playwright_browser_install`

### Recipe: Smoke test a web page

**Goal:** Validate the page loads and key elements exist.

- Tools:
  - `#mcp_playwright_browser_evaluate` (check `document.title`, key selectors)
  - `#mcp_playwright_browser_console_messages` (verify no errors)

### Recipe: Fill a form and verify validation

**Goal:** Exercise form behavior.

- Tools:
  - `#mcp_playwright_browser_fill_form`
  - `#mcp_playwright_browser_console_messages`

### Recipe: Upload a file to a web app

**Goal:** Test import/upload paths.

- Tools:
  - `#mcp_playwright_browser_file_upload`
- Safety:
  - Confirm you are allowed to upload the chosen local file.

---

## Serena (code navigation / refactoring)

### Recipe: “Find the right place to change” (symbol-first)

**Goal:** Avoid brute-force reads.

- Tools:
  - `#mcp_oraios_serena_onboarding` (first time in repo)
  - Use Serena to locate symbols + references (via Serena’s toolset)
  - `#mcp_oraios_serena_think_about_collected_information` (before editing)

### Recipe: Pre-refactor checklist

**Goal:** Make safe, minimal changes.

- Steps:
  - Identify the symbol(s) to change
  - Find references/usage sites
  - Confirm intended public API
  - Edit minimal scope
  - Re-run reflection tool to ensure nothing important was missed

---

## Quick “what tool should I use?” index

- “What folders can I access?” → `mcp_filesystem_list_allowed_directories`
- “What files exist here?” → `mcp_filesystem_list_directory` / `mcp_filesystem_directory_tree`
- “Find a file by pattern” → `mcp_filesystem_search_files`
- “Read a file” → `mcp_filesystem_read_text_file`
- “Write a new file” → `mcp_filesystem_write_file`
- “Patch existing file” → `mcp_filesystem_edit_file`
- “Rename/move” → `mcp_filesystem_move_file`
- “GitHub file/commit operations” → `mcp_githublocal_*` / `mcp_githubremote_*`
- “Browser automation” → `mcp_playwright_browser_*`
- “Serena navigation/refactor workflow” → `mcp_oraios_*` + Serena tools
