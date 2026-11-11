# Tasks: MTM Agent Toolkit Book

**Feature**: `003-agent-toolkit-book`

**Input**: Design documents from `specs/003-agent-toolkit-book/`

**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/book-contracts.md`

**Tests**: No automated test suite is added. This is a documentation deliverable, so verification
is the coverage assertion and drift check described in `contracts/book-contracts.md`, executed in
Phase 8 and Phase 9.

**Organization**: Phases follow the specification's user stories in priority order, so Phase 3
onward each deliver one independently demonstrable slice of the book.

## Phase 1: Setup

**Wave 1 — independent (different files):**

- [x] T001 Create the book root and asset folders at `docs/agent-toolkit-book/`, `docs/agent-toolkit-book/assets/previews/`, and `docs/agent-toolkit-book/chapters/`, and copy the four plugin preview images into `docs/agent-toolkit-book/assets/previews/` [P]
- [x] T002 Write the design system in `docs/agent-toolkit-book/assets/book.css`: light editorial palette, accent `#1F4E79`, typographic scale, sidebar layout, entry article styling, decision table, and a complete `@media print` block for US Letter with chapter page breaks [P]

**⟶ Wait for Wave 1 to finish, then:**

- [x] T003 Write `docs/agent-toolkit-book/assets/book.js`: sidebar current-page marking, search over a lazily fetched `search-index.json`, copy-to-clipboard for prompt blocks, and graceful empty-result text

**Checkpoint**: The stylesheet and script exist and are the only two shared assets. Nothing renders
yet, which is expected.

## Phase 2: Foundational

**Wave 1 — independent (different files):**

- [x] T004 Write `docs/agent-toolkit-book/index.html` as the cover, "how to read this book", inventory table, and contents list linking all twelve chapters plus the source index and decision guide, following the shell in `contracts/book-contracts.md` [P]
- [x] T005 Write `docs/agent-toolkit-book/chapters/testing.html` as the exemplar chapter: full entry markup for every testing entry, proving the `data-entry-*`, anchor, and prompt contract that the other eleven chapters copy [P]

**⟶ Wait for Wave 1 to finish, then:**

- [x] T006 Build `docs/agent-toolkit-book/assets/search-index.json` from the entries already authored, matching the search index contract field for field

**Checkpoint**: One chapter renders correctly with working navigation and search for its entries.
The markup contract is proven and can be replicated.

## Phase 3: User Story 1 — Browse by job-to-be-done (Priority: P1)

**Goal**: Every category chapter exists, each with complete entries and a scope statement.

**Independent Test**: Open the root, click Testing, and confirm every testing entry from both
`dotnet-test` and `testing-automation` is present with purpose, use cases, and a prompt.

### Implementation

**Wave 1 — independent (different files):**

- [x] T007 Write `docs/agent-toolkit-book/chapters/spec-driven-development.html` covering the Spec Kit command family, the companion canvases, and the repo's specify/plan/tasks/implement agents [P]
- [x] T008 Write `docs/agent-toolkit-book/chapters/dotnet-development.html` covering `csharp-expert`, `csharp-refactoring`, `msbuild`, `setup-local-sdk`, `csharp-async`, `dotnet-best-practices`, `dotnet-upgrade`, `resx-source-generator-migration`, `aspnet-minimal-api-openapi`, and the five `csharp-*` test-framework skills [P]
- [x] T009 Write `docs/agent-toolkit-book/chapters/winui-platform.html` covering the seven `winui-*` skills and the WinUI-relevant `winapp-*` skills (`winapp-find-api`, `winapp-find-ui`, `winapp-frameworks`, `winapp-sandbox`, `winapp-troubleshoot`, `winui-devtools`, `winapp-maui`), plus the `winui-dev` and `winapp` agents [P]
- [x] T010 Write `docs/agent-toolkit-book/chapters/database-sql.html` covering `sql-code-review`, `sql-optimization`, `postgresql-code-review`, `postgresql-optimization`, the `ms-sql-dba` and `postgresql-dba` agents, and this repo's stored-procedure and read-only ERP rules [P]
- [x] T011 Write `docs/agent-toolkit-book/chapters/code-quality.html` covering `winui-code-review`, `csharp-refactoring`, `assertion-quality`, `grade-tests`, `test-anti-patterns`, `test-smell-detection`, `test-tagging`, and `code-testing` [P]
- [x] T012 Write `docs/agent-toolkit-book/chapters/documentation-diagrams.html` covering `drawio`, `md-to-docx`, the `project-documenter` agent, and the `diagram-viewer` canvas [P]

**⟶ Wait for Wave 1 to finish, then:**

- [x] T013 Write `docs/agent-toolkit-book/chapters/packaging-signing.html` covering `winui-packaging`, `winapp-package`, `winapp-signing`, `winapp-identity`, `winapp-manifest`, and `winapp-setup`, including the overlap with `winui-setup` [P]
- [x] T014 Write `docs/agent-toolkit-book/chapters/security-supply-chain.html` covering the `github-security` canvas and its five tools, `dependabot-triage`, `ai-prompt-engineering-safety-review`, and the repo's secret-handling rules [P]
- [x] T015 Write `docs/agent-toolkit-book/chapters/context-planning.html` covering `context-map`, `refactor-plan`, `what-context-needed`, the `context-architect` agent, and the testability family (`find-untested-sources`, `detect-static-dependencies`, `generate-testability-wrappers`, `migrate-static-to-wrapper`, `testability-obstacle`, `test-value-evidence`, `test-gap-analysis`) [P]
- [x] T016 Write `docs/agent-toolkit-book/chapters/mcp-and-environment.html` covering Serena, Playwright, and Microsoft Learn MCP servers, the `.github/hooks/speckit.json` hook, the `dotnet` plugin LSP, and the two-Roslyn-servers routing decision [P]
- [x] T017 Write `docs/agent-toolkit-book/chapters/repo-assets.html` covering all 37 repo skills, 50 repo agents, and 82 repo prompts, grouped by folder and rendered as dense but complete entries [P]

**⟶ Wait for Wave 2 to finish, then:**

- [x] T018 Verify every chapter satisfies the markup contract in `contracts/book-contracts.md`: correct anchor prefix, all four `data-entry-*` attributes, present purpose and uses, and a prompt on every skill and agent entry

**Checkpoint**: All twelve chapters render. A reader can browse any job category and find complete
entries. User Story 1 is independently functional.

## Phase 4: User Story 2 — Look up by source (Priority: P2)

**Goal**: A complete index that guarantees no entry is unreachable.

**Independent Test**: Open the index, filter by a plugin name, and confirm that plugin's full skill,
agent, and canvas list appears, each row linking to a real anchor.

### Implementation

- [x] T019 Write `docs/agent-toolkit-book/index-by-source.html` grouping every entry by plugin then by repo path, with a client-side filter, and linking each row to its chapter anchor
- [x] T020 Add the canvas-only plugins (`speckit-companion`, `dependabot-triage`, `github-security`, `diagram-viewer`) to the index as canvas entries with invocation paths rather than empty skill lists
- [x] T021 Verify every index row resolves to an existing `id` in a chapter file, and that no entry is present in the index but absent from a chapter

**Checkpoint**: Nothing in the inventory is unreachable, even when the category guess is wrong.

## Phase 5: User Story 3 — Decide between overlapping tools (Priority: P3)

**Goal**: Overlaps resolved with a default, a reason, and an escape condition.

**Independent Test**: Read the guide and confirm each overlap row states a default and the
condition where the alternative wins.

### Implementation

- [x] T022 Write `docs/agent-toolkit-book/decision-guide.html` with the seven required overlaps from FR-015 as a single decision table
- [x] T023 Verify every tool named in the decision guide resolves to a real anchor, and that the Serena-versus-`dotnet` row matches the routing decision recorded in `.serena/memories/serena-tool-compatibility.md`

**Checkpoint**: The most common failure mode in a toolset this size has a written answer.

## Phase 6: User Story 4 — Copy a working prompt (Priority: P4)

**Goal**: Every skill and agent entry carries at least one copy-ready prompt.

**Independent Test**: Copy any prompt verbatim and confirm it names a real skill or agent and only
real workspace paths.

### Implementation

- [x] T024 Audit every chapter for entries missing a prompt block and add one, using repo-real paths such as `MTM_Receiving_Application.Tests` and `Database/StoredProcedures`
- [x] T025 Verify the copy button yields plain text with no markup, and that no prompt contains an XML tag

**Checkpoint**: Knowing a tool exists converts into using it correctly.

## Phase 7: User Story 5 — Print as a book (Priority: P5)

**Goal**: One print action yields a cover, contents, and every chapter on fresh pages.

**Independent Test**: Print to PDF at US Letter and confirm the cover, contents, chapter breaks, and
absence of clipping.

### Implementation

- [x] T026 Write `docs/agent-toolkit-book/print.html` inlining the cover, contents, and all twelve chapter bodies with navigation suppressed
- [x] T027 Complete the `@media print` rules in `docs/agent-toolkit-book/assets/book.css`: Letter geometry, chapter `break-before`, table `table-layout: fixed`, `pre` wrapping, and forced light palette
- [x] T028 Verify the print output at Letter has a cover page, a contents page, one chapter per page break, and no clipped table column

**Checkpoint**: The series prints as a physical book.

## Phase 8: User Story 6 — Detect drift and updates (Priority: P6)

**Goal**: A read-only check that reports missing, stale, version-changed, and newer-available tools.

**Independent Test**: Inject a synthetic skill into a fixture and confirm the report names it as
missing while no book file changes.

### Implementation

- [x] T029 Write `docs/agent-toolkit-book/check-drift.py` implementing the CLI and JSON contract, parsing `data-entry-*` from the committed pages and comparing against live plugin and repo inventory
- [x] T030 Add the coverage assertion that recomputes per-source totals from the HTML and fails when they disagree with the cover page claim
- [x] T031 Verify the drift script reports zero drift, exits non-zero on an injected missing entry, reports a version delta, and leaves repository status unchanged

**Checkpoint**: The book cannot rot silently.

## Phase 9: Polish

**Wave 1 — independent (different files):**

- [x] T032 Confirm the cover totals in `docs/agent-toolkit-book/index.html` match the recomputed inventory: 13 plugins, 71 skills, 21 agents, 4 canvases, 3 MCP servers, 282 total [P]
- [x] T033 Load every page with the network disabled and confirm zero external requests and working search [P]

**⟶ Wait for Wave 1 to finish, then:**

- [x] T034 Run the repo Markdown lint over the specification artifacts and confirm no issues, and confirm no application source, project file, or build output was modified

## Dependencies & Execution Order

```text
Phase 1 Setup (T001-T003)
  └─▶ Phase 2 Foundational (T004-T006)
        └─▶ Phase 3 US1 chapters (T007-T018)
              ├─▶ Phase 4 US2 source index (T019-T021)  depends on chapter anchors
              ├─▶ Phase 5 US3 decision guide (T022-T023) depends on chapter anchors
              ├─▶ Phase 6 US4 prompts (T024-T025)        depends on chapters existing
              └─▶ Phase 7 US5 print (T026-T028)          depends on all chapters authored
                    └─▶ Phase 8 US6 drift (T029-T031)    parses the finished HTML
                          └─▶ Phase 9 Polish (T032-T034)
```

- Phase 1 Wave 2 (T003) needs the stylesheet classes introduced in T002.
- Phase 2 Wave 2 (T006) needs entries authored in T005 before an index has anything to describe.
- Phase 3 Wave 2 chapters are independent of Wave 1 chapters: different files, shared only the
  contract proven in T005.
- Phase 8 must run last among functional phases because the drift check parses the shipped HTML,
  not a design document.

## Parallel Example

```text
Wave 1 of Phase 3 can run all six chapters at once:
  T007 spec-driven-development.html
  T008 dotnet-development.html
  T009 winui-platform.html
  T010 database-sql.html
  T011 code-quality.html
  T012 documentation-diagrams.html

Wave 2 starts only after all six land, because T013-T017 reuse the verified markup shape:
  T013 packaging-signing.html
  T014 security-supply-chain.html
  T015 context-planning.html
  T016 mcp-and-environment.html
  T017 repo-assets.html
```

## Implementation Strategy

Deliver the walking skeleton first: T002 through T006 produce a stylesheet, a script, a cover, one
complete chapter, and a search index. That proves every contract before eleven more chapters depend
on it. Then author chapters in two independent waves, add the three meta pages, and finish with the
drift script, which can only be written meaningfully once the HTML it parses exists.

## Phase 10: Converge follow-up

Appended by converge: the specification named verification that the first implement pass did not
actually execute.

**Wave 1 — independent (different files):**

- [x] T035 Prove the drift check fails loudly by injecting synthetic drift into a temporary copy of the book and confirming a missing entry, a stale entry, and a version bump are all reported with exit code 1, while the real tree is unchanged [P]
- [x] T036 Run twenty search queries covering skill names, plugin names, and plain-English job words, and confirm the expected entry appears in the first three results for each [P]
- [x] T037 Correct FR-001 in `specs/003-agent-toolkit-book/spec.md`: the fleet holds 269 unique entries, not 282, because repo agents are counted once and duplicate skill names are collapsed

**⟶ Wait for Wave 1 to finish, then:**

- [x] T038 Confirm after these corrections that `check-drift.py` still reports zero drift and exits 0
