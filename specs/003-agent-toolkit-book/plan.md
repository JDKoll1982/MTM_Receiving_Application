# Implementation Plan: MTM Agent Toolkit Book

**Feature**: `003-agent-toolkit-book`

**Created**: 2026-11-11

**Size**: oversized

## Scale Note

This change spans roughly twenty new files across a single new documentation subtree, and it
touches no application source. What a reader should watch for is that the content volume is
front-loaded: most of the risk sits in editorial accuracy across 282 entries rather than in the
markup, so the plan spends its detail on the content contract and the drift check rather than on
styling mechanics.

## Summary

Build a static, print-ready HTML book series under `docs/agent-toolkit-book/` that documents every
agent-tooling entry available in this workspace: 13 installed plugins, 71 unique skills, 21 plugin
agents, 4 canvas extensions, 3 MCP servers, and the repo's own 37 skills, 50 agents, 82 prompts,
and 1 hook. The book is organized primarily by job-to-be-done, with a complete alphabetical index
by source as the completeness guarantee.

The approach is deliberately plain. Every page is hand-authored HTML with one shared stylesheet
and one small script, so the series renders and searches with no network access and prints as a
book without a build step. A single read-only Python script parses the committed HTML and compares
the documented entries against the live plugin and repo inventory, reporting missing, stale, and
version-changed entries plus newer versions worth adopting.

No application code, project file, or build output changes.

## Technical Context

| Aspect | Choice |
| --- | --- |
| Deliverable | Static HTML, embedded CSS and JavaScript, no framework |
| Location | `docs/agent-toolkit-book/` |
| Theme | Light editorial only, accent `#1F4E79`, no dark mode |
| Print | US Letter, cover page, contents page, one chapter per page break |
| Search | Inline JSON index filtered client-side, offline |
| Drift check | Read-only Python 3 script parsing committed HTML |
| Runtime impact | None; documentation only |

## Project Structure

```text
docs/agent-toolkit-book/
├── index.html                      cover + contents + how to read the book
├── index-by-source.html            complete alphabetical index of all 282 entries
├── decision-guide.html             "which tool when" with default, reason, escape condition
├── print.html                      concatenated series for one-shot printing
├── assets/
│   ├── book.css                    single stylesheet, screen + print
│   ├── book.js                     nav state, search, copy-to-clipboard
│   ├── search-index.json           entry names + descriptions + anchors
│   └── previews/                   locally copied preview images
├── chapters/
│   ├── spec-driven-development.html
│   ├── dotnet-development.html
│   ├── testing.html
│   ├── winui-platform.html
│   ├── packaging-signing.html
│   ├── database-sql.html
│   ├── code-quality.html
│   ├── security-supply-chain.html
│   ├── documentation-diagrams.html
│   ├── context-planning.html
│   ├── mcp-and-environment.html
│   └── repo-assets.html
└── check-drift.py                  read-only inventory comparison
```

**Structure Decision**: One directory per book series, chapters in a `chapters/` subfolder so the
landing page and the print view stay uncluttered, and shared assets isolated so a chapter file
contains content and nothing else. Every page links only relative paths, which is what makes the
series portable and printable from disk.

## Blocks

### Calls

```text
index.html ──▶ chapters/*.html ──▶ entry anchor
index-by-source.html ──▶ same entry anchor (no duplicate content)
decision-guide.html ──▶ referenced entry anchor
print.html ──embedded──▶ all chapter bodies, nav suppressed
book.js ──fetch──▶ assets/search-index.json (local file, offline)
check-drift.py ──read──▶ chapters/*.html + live inventory ──▶ report only
```

### Screens

- **Cover and contents** — title, subtitle, generation date, inventory totals, chapter list with
  entry counts.
- **Chapter page** — persistent sidebar with all chapters and the current one marked; each entry
  rendered as a headed block with source badge, purpose, use cases, prerequisites, boundaries, and
  a copyable prompt.
- **Source index** — filterable list grouped by plugin then repo path, each row linking to the
  owning chapter anchor.
- **Decision guide** — table of overlap, default choice, reason, escape condition.
- **Print view** — cover, contents, then every chapter with navigation removed and breaks between
  chapters.

## Constitution Check

The constitution governs the WinUI application. This feature adds documentation only, so most
principles apply as "not applicable, and not weakened". Each is assessed rather than skipped.

| Principle | Assessment |
| --- | --- |
| I. MVVM and View Purity | PASS — no view, view model, or XAML is added or altered |
| II. Data Access Integrity | PASS — no DAO, stored procedure, or SQL is added; the book only documents existing rules |
| III. CQRS + Mediator First | PASS — no command, query, or handler is introduced |
| IV. Dependency Injection and Modular Boundaries | PASS — no DI registration or module boundary changes |
| V. Validation, Errors, and Structured Logging | PASS — no application logging path is touched; the drift script prints a plain report |
| VI. Security and Session Discipline | PASS — the book embeds no credential, connection string, or secret, and the drift script is read-only |
| VII. Library-First Reuse | PASS — reuses the browser, Python 3 already on the machine, and existing preview images; adds no third-party dependency |

No violations, so Complexity Tracking is omitted.

## Approach

Content is the deliverable, so the work is sequenced to lock the content contract before any page
is written: a shared entry template and a fixed category taxonomy come first, then the shell and
one exemplar chapter, then the remaining chapters against that exemplar. The drift script is
written against the finished HTML so it parses what actually ships rather than a speculative shape.

## Validation

- Every one of the 282 entries appears exactly once in a chapter and once in the source index.
- `check-drift.py` reports zero drift, exits non-zero on injected drift, and leaves the working
  tree unchanged.
- Every page loads with the network disabled and issues no external request.
- The print view produces a cover page, a contents page, and one chapter per page break at Letter.
