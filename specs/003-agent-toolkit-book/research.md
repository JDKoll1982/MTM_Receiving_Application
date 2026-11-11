# Phase 0 Research: MTM Agent Toolkit Book

**Feature**: `003-agent-toolkit-book`

**Created**: 2026-11-11

Genuine unknowns the plan left open, resolved before any page was written.

## Decision 1: Static HTML is the source of truth, not a generation target

**Decision**: Author the chapter HTML directly and commit it. The drift script parses those
committed files to learn what is documented, and compares that against live inventory.

**Rationale**: The user asked for static HTML with a *read-only* check that reports drift. Parsing
the shipped artifact means the check verifies what a reader actually receives. A model that
generated HTML from data would invert the guarantee: the check would validate an intermediate
representation while the output could still be wrong.

**Alternatives considered**: A generator reading a JSON content model. Rejected because it
contradicts the stated decision and because editorial prose across 282 entries is the hard part,
not the markup. A generator that only scaffolds pages, with prose edited afterward, was also
rejected: it creates two sources of truth that silently diverge.

## Decision 2: Twelve job-to-be-done chapters, with a fixed assignment rule

**Decision**: Organize into twelve chapters — spec-driven development, .NET development,
testing, WinUI platform, packaging and signing, database and SQL, code quality, security and
supply chain, documentation and diagrams, context and planning, MCP and environment, and repo
assets. An entry belongs to the chapter matching the job a reader is trying to do, not the plugin
that ships it. Cross-cutting entries appear in their primary chapter with a cross-reference.

**Rationale**: Plugin boundaries are invisible to a reader who does not already know the plugins.
Job boundaries are what someone actually arrives with. Twelve chapters keeps each chapter under
about forty entries, which is navigable in a sidebar and reasonable as a printed chapter.

**Alternatives considered**: One chapter per plugin, rejected because it reproduces the discovery
problem. One chapter per entry kind (skills, agents, canvases), rejected because it separates tools
that work together. A single flat page, rejected as unprintable at this size.

## Decision 3: A fixed entry template with a published CSS class contract

**Decision**: Every entry uses the same element structure and class names: `article.entry` with
`header` carrying `h3.entry-name` and `span.entry-source`, then `p.entry-purpose`, `ul.entry-uses`,
`dl.entry-facts`, and `div.entry-prompt` containing `pre > code`. The drift script reads
`data-entry-name` and `data-entry-source` attributes rather than parsing prose.

**Rationale**: Consistency is the difference between a reference and a pile of pages. Putting the
identity in data attributes also makes drift detection exact and cheap, and keeps the check immune
to wording changes in visible text.

**Alternatives considered**: Parsing headings for entry names. Rejected because headings carry
display decoration and would need fragile normalization.

## Decision 4: Prebuilt JSON search index rather than DOM scanning

**Decision**: Generate `assets/search-index.json` as part of authoring, holding one record per
entry: name, kind, source, chapter, anchor, and a short description. `book.js` loads it once and
filters in memory.

**Rationale**: Scanning the DOM works only on the page you are on. A prebuilt index supports
search-from-anywhere and keeps the per-page script trivial. At 282 records the file stays small
enough to load instantly from disk.

**Alternatives considered**: Fetching and parsing every chapter on load. Rejected as slow and
fragile over `file://`.

## Decision 5: A dedicated `print.html`, plus print rules in the shared stylesheet

**Decision**: Provide `print.html` that inlines the cover, contents, and every chapter body in one
document, so one Print action produces the whole book. The shared stylesheet still carries
`@media print` rules so any single chapter prints acceptably on its own.

**Rationale**: Printing a multi-file series page by page does not produce one document with
continuous page numbering and a single contents page. A combined view does, and it also makes the
"print the series as a book" requirement directly verifiable.

**Alternatives considered**: Relying on browser "print all tabs". Rejected as inconsistent across
browsers and unable to produce a single cover and contents.

## Decision 6: Copy preview images into `assets/previews`

**Decision**: Copy the four plugin preview images into the book's asset folder and reference them
relatively. Entries without an image render a typographic placeholder.

**Rationale**: The absolute paths under the user profile would break on any other machine and
would make the book machine-specific. A relative copy keeps the series portable and offline.

**Alternatives considered**: Referencing the plugin install path directly. Rejected as fragile and
non-portable.

## Decision 7: Python 3 for the drift script

**Decision**: Implement `check-drift.py` in Python 3 with no third-party dependency.

**Rationale**: Python 3.14 is already present on this machine, JSON handling is built in, and the
script needs no Windows-specific behavior. It also matches the scripting already used by the Spec
Kit extension in this repo.

**Alternatives considered**: PowerShell, which is idiomatic for this repo's `tools/` folder.
Rejected only because the parsing work is more concise in Python and the script is a one-off
maintenance tool rather than an operational script.

## Decision 8: Duplicate skills documented once and counted once

**Decision**: `csharp-nunit` is documented as a single entry naming both
`csharp-dotnet-development` and `testing-automation`. The coverage claim stays at 71 unique skills,
and the drift script de-duplicates by name before comparing counts.

**Rationale**: The inventory has 72 skill directories and 71 unique names. Reporting 72 would
overstate coverage; reporting two entries would make the reader think the skill differs by source
when it does not.

**Alternatives considered**: Listing both as separate entries. Rejected as misleading padding.

## Decision 9: Stable anchor scheme

**Decision**: Each entry gets the anchor `<chapter-slug>--<entry-slug>`, for example
`testing--writing-mstest-tests`. The source index and decision guide link to these anchors and
never restate entry content.

**Rationale**: A globally unique anchor prevents collisions between chapters and keeps deep links
valid if a chapter is reordered. Deterministic slugs also let the drift script resolve a link
target without a second pass.

**Alternatives considered**: Per-chapter anchors without a chapter prefix. Rejected because
`winui-setup` and `winapp-setup` are different entries whose bare slugs would collide.

## Decision 10: Verify coverage by script, not by eye

**Decision**: Add a coverage assertion inside `check-drift.py` that counts documented entries and
compares them to the expected per-source totals, failing the run when the book claims a count it
cannot substantiate.

**Rationale**: 282 entries cannot be eyeballed. A count that recomputes from the HTML is the only
claim the book can honestly make, and it fails loudly when a chapter is added without its entries.

**Alternatives considered**: Manual review against the inventory listing. Rejected as unreliable at
this volume and not repeatable after a plugin update.
