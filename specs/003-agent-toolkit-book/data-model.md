# Phase 1 Data Model: MTM Agent Toolkit Book

**Feature**: `003-agent-toolkit-book`

**Created**: 2026-11-11

The book has no runtime database. These entities describe the document model the pages implement
and the drift script reads, so that "the book documents entry X" is a checkable statement rather
than a hope.

## Entry

The atomic unit of the book. One entry per documented tool, rendered as one `article.entry`.

| Field | Type | Source | Rule |
| --- | --- | --- | --- |
| name | string | `data-entry-name` | Matches the skill, agent, canvas, or prompt name exactly as installed. Required. |
| kind | enum | `data-entry-kind` | One of `skill`, `agent`, `canvas`, `mcp`, `hook`, `prompt`, `plugin`. Required. |
| source | string | `data-entry-source` | Plugin name with version, or repo-relative path. Required. |
| purpose | string | visible prose | One sentence stating what the tool does. Required. |
| uses | list of strings | visible list | Three to five concrete use cases. At least one required. |
| prerequisites | list of strings | visible list | Omitted when the tool has none. |
| boundaries | list of strings | visible list | What the tool does not do. Omitted when unspecified. |
| prompt | string | `pre > code` | Copy-ready invocation. Required for `skill` and `agent`, optional otherwise. |
| anchor | slug | `id` attribute | `<chapter-slug>--<entry-slug>`. Unique across the series. |
| crossRefs | list of anchors | visible links | Optional pointers to a related chapter. |

**Validation rules**

- An entry whose kind is `skill` or `agent` must carry a non-empty prompt.
- An entry name must be unique within its chapter; the same name may appear in another chapter only
  when the chapter links to the owning anchor instead of restating it.
- `source` for a plugin entry must include the installed version, because the drift script compares
  versions and cannot do so without one.
- A duplicate name reported by two plugins yields a single entry listing both sources.

## Chapter

A navigable unit that owns entries.

| Field | Type | Rule |
| --- | --- | --- |
| slug | string | URL-safe identifier used in the filename and in every entry anchor prefix. |
| title | string | Display title, matched by the sidebar and the contents page. |
| scope | string | One paragraph stating which jobs the chapter covers and which it defers. |
| order | integer | Position in the sidebar and in the printed book. |
| entries | list of Entry | The chapter body. |
| entryCount | integer | Derived, shown in navigation. Never hand-maintained. |

**State transitions**: a chapter is authored, then verified by the drift script. There is no runtime
state beyond the reader's current location, which is not persisted.

## Category

The editorial grouping that produced a chapter. One category maps to one chapter today; the entity
is kept separate because the decision guide references categories by job name rather than by file.

| Field | Type | Rule |
| --- | --- | --- |
| label | string | Job-to-be-done phrasing, for example "Testing". |
| chapter | Chapter slug | The chapter that owns it. |
| keywords | list of strings | Feeds search so a job phrase finds the chapter. |

## InventorySnapshot

The baseline the book claims to cover. Captured in the drift script and in the cover page totals.

| Field | Type | Rule |
| --- | --- | --- |
| capturedAt | ISO date | Date the inventory was read. |
| plugins | map name to version | Every installed plugin, user scope. |
| pluginSkills | list of names | De-duplicated by name. |
| pluginAgents | list of names | Plugin-provided agents. |
| pluginCanvases | list of names | Plugin-provided canvas extensions. |
| repoSkills | list of names | `.github/Skills` directories. |
| repoAgents | list of names | `.github/agents` markdown files. |
| repoPrompts | list of paths | `.github/prompts`, including subfolders. |
| repoHooks | list of names | `.github/hooks` files. |
| mcpServers | map name to scope | Project-scoped and user-scoped named separately. |
| totalEntries | integer | Derived sum, must equal the cover page claim. |

## DriftReport

The read-only comparison result.

| Field | Type | Rule |
| --- | --- | --- |
| missing | list of entries | In live inventory, absent from the book. |
| stale | list of entries | In the book, absent from live inventory. |
| versionChanged | list of `{name, documented, live}` | Plugin version differs. |
| updatesAvailable | list of entries | Newer plugin or skill set worth adopting. |
| duplicates | list of `{name, sources}` | Same skill name in more than one plugin. |
| countsAsExpected | boolean | Documented totals equal recomputed totals. |
| exitCode | integer | `0` clean, non-zero when any list is non-empty. |

**Validation rules**

- The report is emitted only; the script must not write, create, or delete any book file.
- A version comparison must degrade to "unknown" rather than raise when the book records no version.
- Duplicates are informational and never fail the run on their own.

## CoverPage

Print-only title surface.

| Field | Type | Rule |
| --- | --- | --- |
| title | string | `MTM Agent Toolkit`. |
| subtitle | string | One line describing the book's purpose. |
| generatedOn | ISO date | Date the pages were authored. |
| totals | map | Plugin count, skill count, agent count, canvas count, MCP count, grand total. |

**Validation rule**: every number on the cover page must be reproducible by the drift script from
the committed HTML. A number nobody can recompute is a claim, not a fact.

## Relationships

```text
BookSeries 1 ── * Chapter 1 ── * Entry
Category   1 ── 1 Chapter
InventorySnapshot 1 ── 1 DriftReport
CoverPage  1 ── 1 InventorySnapshot (totals only)
```
