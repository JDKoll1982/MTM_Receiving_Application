# Feature Specification: MTM Agent Toolkit Book

**Feature Branch**: `003-agent-toolkit-book`

**Created**: 2026-11-11

**Status**: Draft

**Input**: Produce a book-styled HTML reference covering every installed Copilot CLI plugin,
skill, agent, canvas extension, MCP server, and repo-local Copilot asset in this workspace,
organized so a developer can find the right tool for a task, copy a working prompt, and print
the whole thing as a physical book.

## User Scenarios & Testing

This book exists because the workspace now carries 269 distinct agent-tooling entries across
thirteen plugins, four canvas extensions, and the repo's own `.github` assets. Nothing currently
describes them in one place, so the only way to discover a tool is to already know it exists.

Coverage inventory this specification is measured against:

| Source | Count |
| --- | --- |
| Installed plugins | 13 |
| Plugin skills (72 directories, 71 unique) | 71 |
| Plugin agents | 21 |
| Plugin canvas extensions | 4 |
| Repo skills (`.github/Skills`) | 37 |
| Repo agents (`.github/agents`, excluding README) | 49 |
| Repo prompts (`.github/prompts`, incl. subfolders) | 82 |
| Repo hooks (`.github/hooks`) | 1 |
| MCP servers | 3 |
| **Total entries** | **269** |

### User Story 1 - Find the right tool for a job by browsing categories (Priority: P1)

A developer who needs to do something ("write tests for this DAO", "package an MSIX") opens the
book and browses by job-to-be-done rather than by which plugin happens to own the skill. Category
chapters group unlike-named tools that solve the same problem, and every entry states what the
tool does, when to reach for it, what it needs, and what it will not do.

**Why this priority**: Discovery is the entire point of the book. Without categorization the
reader must already know a plugin name to find anything, which reproduces the problem the book
solves. This story alone delivers a usable reference.

**Independent Test**: Open the book root, click into the Testing category, and confirm it lists
every testing-related entry from both `dotnet-test` and `testing-automation`, each with a
complete entry body. No other category needs to exist for this to pass.

**Acceptance Scenarios**:

1. **Given** the book is generated, **When** a reader opens the root page, **Then** every
   job-to-be-done category appears in a persistent sidebar with a visible entry count.
2. **Given** a category page, **When** the reader scans it, **Then** each entry shows name,
   source plugin, version, one-line purpose, and three to five concrete use cases.
3. **Given** a skill that exists in two plugins (`csharp-nunit`), **When** the reader views its
   category entry, **Then** both sources are named and the duplication is called out rather than
   rendered as two indistinguishable entries.

---

### User Story 2 - Look up any entry by its source and name (Priority: P2)

A developer who already knows they want "the `msbuild` skill" or "whatever is in `winappcli`"
switches to the source-organized index and finds the entry alphabetically under its plugin,
without knowing which category it landed in.

**Why this priority**: Categorization is an editorial judgment and will not match every reader's
mental model. A source index guarantees no entry is unreachable even when the category guess is
wrong.

**Independent Test**: From any page, open the index, search for a plugin name, and confirm the
plugin's full skill, agent, and canvas list is present and each item links to its detail.

**Acceptance Scenarios**:

1. **Given** the index page, **When** the reader filters by a plugin name, **Then** only that
   plugin's entries remain visible and the result count updates.
2. **Given** an entry that appears in a category chapter, **When** the reader reaches it through
   the source index, **Then** it resolves to the same content rather than a duplicated stub.
3. **Given** a plugin with zero skills and only a canvas (`dependabot-triage`, `github-security`,
   `diagram-viewer`, `speckit-companion`), **When** the reader opens its index entry, **Then**
   the canvas is documented as a canvas with its invocation path, not as an empty skill list.

---

### User Story 3 - Decide between overlapping tools (Priority: P3)

A developer facing an ambiguity — Serena or the `dotnet` plugin for code intelligence, `winui`
or `winappcli` for packaging, `run-tests` or `dotnet test` directly — reads a decision guide
that resolves the overlap with a stated rule instead of a coin flip.

**Why this priority**: Overlap is the most common failure mode in a toolset this size, and it is
where a reference book beats a listing. Several overlaps are already known and recorded in
project memory, so the guide has real decisions to encode rather than hypotheticals.

**Independent Test**: Read the decision guide and confirm each documented overlap pair states a
default choice, the reason, and the escape condition where the other option wins.

**Acceptance Scenarios**:

1. **Given** the decision guide, **When** the reader looks up "which C# language server", **Then**
   the guide states that Serena owns symbol work and the `dotnet` plugin LSP owns diagnostics,
   and that the `dotnet` LSP is dropped first under memory pressure.
2. **Given** the decision guide, **When** the reader looks up "packaging a WinUI app", **Then**
   the guide distinguishes the `winui-packaging` and `winapp-package` skills and states which to
   prefer.
3. **Given** any row in the decision guide, **When** the reader follows its recommendation,
   **Then** the referenced skill name matches an entry that exists in the book.

---

### User Story 4 - Copy a working prompt for a skill or agent (Priority: P4)

A developer who has found the right tool wants the exact invocation text, adapted to this repo's
conventions, without reconstructing it from a skill description.

**Why this priority**: The gap between knowing a skill exists and using it correctly is where
most readers stall. Prompt examples convert a reference into a tool.

**Independent Test**: Copy any prompt example verbatim into a Copilot session and confirm it
names a real skill or agent and references only real repo paths.

**Acceptance Scenarios**:

1. **Given** any skill or agent entry, **When** the reader views it, **Then** at least one
   copy-to-clipboard prompt is present.
2. **Given** a prompt example for a repo-specific tool, **When** the reader inspects it,
   **Then** it references existing workspace paths such as
   `MTM_Receiving_Application.Tests` or `Database/StoredProcedures`.
3. **Given** a prompt example, **When** the reader copies it, **Then** the copied text is plain
   prose with no HTML markup and no XML tags.

---

### User Story 5 - Print the book as a physical book (Priority: P5)

A developer prints the entire series to Letter paper, or to PDF, and receives a cover page, a
table of contents, chapter breaks on fresh pages, no clipped tables, and legible monospace
prompts.

**Why this priority**: The explicit requirement is printability. It is ranked below discovery
because a screen-only book still delivers most value, whereas a printable book with missing
entries delivers none.

**Independent Test**: Print the series to PDF at US Letter and confirm the cover page, contents,
chapter page breaks, and absence of horizontal clipping.

**Acceptance Scenarios**:

1. **Given** print output, **When** the reader opens the PDF, **Then** page one is a cover page
   carrying the book title, subtitle, date, and inventory counts.
2. **Given** print output, **When** a chapter begins, **Then** it starts on a fresh page and its
   navigation chrome is suppressed.
3. **Given** a wide entry table in print, **When** rendered on Letter, **Then** no column is
   clipped and no cell overflows the margin.
4. **Given** print rendering, **When** the reader views any page, **Then** the light palette is
   used regardless of the on-screen theme state.

---

### User Story 6 - Know when the book has drifted or a tool has updates (Priority: P6)

A developer returns weeks later. A check script compares the book against the live plugin
inventory and reports, without modifying anything, which entries are missing, stale, or newly
available — including newer plugin versions and newer skill sets worth adopting.

**Why this priority**: A reference that silently rots is worse than no reference, because it
misleads. Ranked last only because the book must exist first.

**Independent Test**: Inject a synthetic new skill into a temp fixture inventory, run the check,
and confirm the report names it as missing from the book without any file being changed.

**Acceptance Scenarios**:

1. **Given** the book and live inventory agree, **When** the check runs, **Then** it exits
   successfully and reports zero drift.
2. **Given** a plugin has been updated to a newer version, **When** the check runs, **Then** the
   version delta is reported as an update to consider.
3. **Given** a skill exists in inventory but not in the book, **When** the check runs, **Then**
   it is reported as missing, and the check makes no edits to any book file.
4. **Given** an entry documents a skill no longer present in inventory, **When** the check runs,
   **Then** it is reported as stale.

---

### Edge Cases

- A skill name appears in two plugins (`csharp-nunit`, shipped by both `csharp-dotnet-development`
  and `testing-automation`). The book must present one entry with both sources and must not
  inflate its own entry count by double-counting.
- A plugin ships zero skills and only a canvas (`dependabot-triage`, `github-security`,
  `diagram-viewer`, `speckit-companion`). These must render as canvas entries with an invocation
  path, never as an empty chapter or a bare "0 skills" row.
- A plugin ships a preview image while others do not. Missing previews must degrade to a
  styled placeholder rather than a broken image reference.
- A skill description exceeds a comfortable length. Long descriptions must wrap and truncate
  predictably in both screen and print without pushing other columns off page.
- The reader has no network access. The book must render and search entirely offline, with no
  external stylesheet, font, script, or image request.
- A plugin is uninstalled after the book is written. The check script must report the stale entry
  and must not delete it automatically.
- A repo prompt lives in a subfolder of `.github/prompts`. Its entry must retain its subfolder
  grouping so the reader can tell `quality/` prompts from root-level ones.
- Search returns no matches. The page must say so explicitly rather than showing an empty region.
- The inventory contains an entry whose name differs only by case or punctuation from another.
  Matching must not silently merge distinct entries.
- A reader opens a deep-linked entry URL directly. The page must load standalone, with navigation
  intact and the entry scrolled into view.

## Requirements

### Functional Requirements

**Discovery and organization**

- **FR-001**: The book MUST document all 269 unique inventoried entries: 13 plugins, 71 unique
  plugin skills, 21 plugin agents, 4 plugin canvas extensions, 3 MCP servers, 1 plugin language
  server, 1 repo hook, 37 repo skills, 49 repo agents, and 82 repo prompts. The total is 269
  rather than the 282 estimated during specification because repo agents are counted once each
  (the count of 50 included a README), and because `csharp-nunit`, shipped by both
  `csharp-dotnet-development` and `testing-automation`, is documented once under both sources.
- **FR-002**: The book MUST be organized primarily by job-to-be-done category chapters, and MUST
  additionally provide a complete index organized by source.
- **FR-003**: Each category chapter MUST state its scope and MUST list every entry that belongs to
  it, with no entry omitted for length reasons.
- **FR-004**: Each entry MUST display its name, its source (plugin with version, or repo path),
  a one-line purpose, and a set of concrete use cases.
- **FR-005**: Each entry MUST state any prerequisite the tool depends on, such as the WinApp CLI
  requirement for the `winui-*` and `winapp-*` skills.
- **FR-006**: Each entry MUST state its boundary — what the tool does not do or when not to use it —
  wherever the source material defines one.
- **FR-007**: The book MUST identify duplicate skill names across plugins and present each
  duplicate once, naming all sources.
- **FR-008**: The book MUST document the four canvas extensions as canvases, describing what each
  surface shows and how a reader opens it.
- **FR-009**: The book MUST document the three MCP servers, including which are project-scoped
  and which are user-scoped.

**Navigation**

- **FR-010**: Every page MUST present the same persistent navigation allowing movement to the
  root, any category chapter, the source index, the decision guide, and the print view.
- **FR-011**: Every page MUST indicate the reader's current location within the book.
- **FR-012**: The book MUST provide client-side search over entry names and descriptions that
  works with no network access.
- **FR-013**: Search results MUST link directly to the matching entry, and the target page MUST
  scroll that entry into view.
- **FR-014**: Entry pages MUST be reachable by direct URL without requiring navigation from the
  root first.

**Decision support**

- **FR-015**: The book MUST include a decision guide resolving at minimum these overlaps: Serena
  versus the `dotnet` plugin language server; `winui` versus `winappcli`; `winui-packaging` versus
  `winapp-package`; `winui-setup` versus `winapp-setup`; `winui-ui-testing` versus
  `winapp-ui-automation`; `run-tests` versus invoking `dotnet test` directly; and the repo's
  FluentAssertions test rules versus the plugin-provided test skills.
- **FR-016**: Each decision-guide row MUST state the default choice, the reason, and the condition
  under which the alternative wins.
- **FR-017**: Every skill or agent named in the decision guide MUST correspond to an entry present
  in the book.

**Prompt examples**

- **FR-018**: Each skill and agent entry MUST include at least one copy-ready prompt example.
- **FR-019**: Prompt examples MUST reference real workspace paths and MUST follow the repo's
  architecture language, including the View to ViewModel to Service to DAO flow and the
  stored-procedure-only MySQL rule.
- **FR-020**: Each prompt example MUST be copyable with a single action and MUST copy plain text
  containing no markup.

**Printing**

- **FR-021**: The book MUST provide a cover page rendered in print output, carrying the title,
  subtitle, generation date, and inventory counts.
- **FR-022**: The book MUST provide a table of contents in print output listing every chapter.
- **FR-023**: In print, each chapter MUST begin on a fresh page.
- **FR-024**: In print, navigation, search, and interactive controls MUST be suppressed.
- **FR-025**: Print output MUST use US Letter geometry with margins such that no table column is
  clipped and no content exceeds the printable area.
- **FR-026**: Print output MUST always use the light palette, ignoring any on-screen state.
- **FR-027**: Tables, long skill names, and monospace prompt blocks MUST wrap or truncate
  predictably in print rather than overflowing.

**Appearance**

- **FR-028**: The book MUST present a publishing-quality reading experience with consistent
  typographic hierarchy, restrained color, and deliberate whitespace.
- **FR-029**: The book MUST use the light editorial palette with steel blue `#1F4E79` as the accent.
- **FR-030**: The book MUST embed every style, script, font decision, and image locally, issuing
  no external network request at any point.
- **FR-031**: The book MUST render correctly in a current Chromium-based browser at widths from
  a narrow laptop window through a full desktop display.

**Drift detection**

- **FR-032**: A check script MUST compare the book against the live plugin and repo inventory and
  report entries that are missing, stale, or changed in version.
- **FR-033**: The check script MUST additionally report newer available plugin versions and newer
  skill sets as updates to consider.
- **FR-034**: The check script MUST be read-only and MUST NOT modify, create, or delete any book
  or inventory file.
- **FR-035**: The check script MUST exit with a distinct non-zero status when drift is detected.
- **FR-036**: The check script MUST report a human-readable summary, not only a machine status.

### Key Entities

- **Book Series**: The set of pages composing the book — a root page, category chapters, the
  source index, the decision guide, and the print view — sharing one navigation model.
- **Chapter**: A navigable unit of the book with a title, an optional scope statement, and an
  ordered list of entries. Chapters are either category-based or source-based.
- **Entry**: A single documented tool. Attributes: display name, kind (skill, agent, canvas, MCP
  server, hook, or prompt), source plugin and version or repo path, purpose, use cases,
  prerequisites, boundaries, prompt examples, and an optional preview image.
- **Category**: An editorial grouping of entries by job-to-be-done, carrying a label and a
  derived entry count.
- **Inventory Snapshot**: The captured set of installed plugins with versions plus repo-local
  asset counts that the book claims to cover, used as the drift baseline.
- **Drift Report**: The read-only comparison of a snapshot against live inventory, listing
  missing entries, stale entries, version deltas, and available updates.
- **Cover Page**: The print-only title page carrying title, subtitle, generation date, and
  inventory totals.

## Success Criteria

### Measurable Outcomes

- **SC-001**: All 269 inventoried entries appear in the book, verified by an automated count that
  reports zero missing.
- **SC-002**: A reader can reach any entry from the root page in two interactions or fewer.
- **SC-003**: A task-oriented reader can identify the correct tool for a described job without
  consulting any source outside the book, validated against at least ten representative jobs
  spanning testing, UI, database, spec workflow, security, and documentation.
- **SC-004**: Every skill and agent entry carries at least one prompt example, verified by an
  automated check reporting zero entries without one.
- **SC-005**: Printing the full series to US Letter produces a document with a cover page, a
  contents page, and one chapter per page break, with no clipped table columns.
- **SC-006**: The book issues zero external network requests, verified by loading every page with
  the network disabled.
- **SC-007**: The check script reports zero drift against a freshly generated book, and correctly
  reports a synthetic missing entry, a synthetic stale entry, and a synthetic version bump in a
  single run.
- **SC-008**: The check script leaves the working tree unchanged, verified by comparing repository
  status before and after a run.
- **SC-009**: Client-side search returns the expected entry in the first three results for at
  least twenty sampled queries spanning skill names, plugin names, and plain-English job
  descriptions.

## Assumptions

- The book is documentation only. It introduces no runtime dependency, changes no application
  code, and does not alter build output.
- Output location is `docs/agent-toolkit-book/` with a new subfolder per the docs convention.
- The book is a static HTML series with embedded CSS and JavaScript. No framework, bundler, or
  external stylesheet is introduced.
- The light editorial palette with `#1F4E79` accent is the only theme. A dark mode is explicitly
  out of scope.
- Depth is `deep`: every entry receives a full section with purpose, use cases, prerequisites,
  boundaries, and prompt examples.
- The audience is team onboarding, so entries assume a reader who knows C# and WinUI but does not
  know this workspace's tooling.
- Plugin inventory is read from `~/.copilot/installed-plugins/` and repo assets from `.github/`.
  The book does not attempt to discover plugins from marketplaces that are not installed.
- The check script runs locally on Windows and may use PowerShell, Python, or Node.js, consistent
  with tooling already present.
- Category assignment is editorial. Where an entry genuinely spans two categories, it appears in
  its primary category with a cross-reference, and the source index remains the authoritative
  complete list.
- Preview images are used where a plugin ships one; absence is handled, not treated as an error.

## Verbatim Constraints

These are the user's locked decisions for this feature. Treat them as fixed inputs.

- Scope: everything — installed plugins, plugin skills, plugin agents, plugin canvas extensions,
  repo `.github` assets, and MCP servers.
- Structure: multi-page series, one HTML file per category, shared navigation, printable as a book.
- Depth: deep, meaning a full section per item with use cases and prompt examples.
- Audience: team onboarding.
- Output folder: `docs/agent-toolkit-book/`.
- Title: `MTM Agent Toolkit`.
- Include decision guide: yes.
- Include prompt examples: yes.
- Include client-side search: yes.
- Print target: US Letter, one chapter per page, cover page, page-break control.
- Organization: both — job-to-be-done chapters plus an alphabetical index by source.
- Theme: light only, accent `#1F4E79` (industrial steel blue).
- Generation: static HTML, accompanied by a script that reports drift against the live inventory
  and flags updates worth considering.
- Preview images: included.
- Cover page: included.
