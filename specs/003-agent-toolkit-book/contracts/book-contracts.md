# Phase 1 Contracts: MTM Agent Toolkit Book

**Feature**: `003-agent-toolkit-book`

**Created**: 2026-11-11

Three contracts a consumer codes against: the page and anchor layout, the entry markup contract, and
the drift script's CLI and JSON shapes. Identifiers pinned in the spec's Verbatim Constraints are
reproduced exactly and never renamed.

## File contract

Every path below is relative to `docs/agent-toolkit-book/` and is reachable without a network.

| Path | Role |
| --- | --- |
| `index.html` | Cover, how to read, and contents |
| `index-by-source.html` | Complete alphabetical index of all entries |
| `decision-guide.html` | Overlap resolutions |
| `print.html` | Single-document print view |
| `assets/book.css` | Only stylesheet in the series |
| `assets/book.js` | Only script in the series |
| `assets/search-index.json` | Search records |
| `assets/previews/` | Locally copied preview images |
| `chapters/<chapter-slug>.html` | One file per chapter |
| `check-drift.py` | Read-only inventory comparison |

**Rule**: a page may reference only paths inside this directory. No absolute path, no `http`
reference, and no path outside the book root is permitted.

## Chapter slugs

Fixed, because entry anchors and the print view both depend on them.

```text
spec-driven-development
dotnet-development
testing
winui-platform
packaging-signing
database-sql
code-quality
security-supply-chain
documentation-diagrams
context-planning
mcp-and-environment
repo-assets
```

## Anchor contract

```text
entry anchor   = "<chapter-slug>--<entry-slug>"
entry slug     = lowercase, non-alphanumeric collapsed to "-", trimmed
chapter anchor = "<chapter-slug>"
```

Examples that must resolve:

- `testing--writing-mstest-tests`
- `winui-platform--winui-setup`
- `packaging-signing--winapp-setup`
- `repo-assets--speckit.specify`

The `winui-setup` and `winapp-setup` pair exists specifically to prove that bare slugs would
collide and that the chapter prefix is load-bearing.

## Markup contract

The drift script parses these attributes and classes. Changing them breaks drift detection, so they
are part of the contract, not styling.

```html
<article class="entry"
         id="testing--writing-mstest-tests"
         data-entry-name="writing-mstest-tests"
         data-entry-kind="skill"
         data-entry-source="dotnet-test@1.0.0">
  <header>
    <h3 class="entry-name">writing-mstest-tests</h3>
    <span class="entry-source">dotnet-test@1.0.0</span>
  </header>
  <p class="entry-purpose">One sentence.</p>
  <ul class="entry-uses">
    <li>A concrete use.</li>
  </ul>
  <dl class="entry-facts">
    <dt>Prerequisites</dt><dd>Example value</dd>
  </dl>
  <div class="entry-prompt">
    <pre><code>Copy-ready prompt.</code></pre>
  </div>
</article>
```

| Attribute or class | Required | Consumed by |
| --- | --- | --- |
| `article.entry` | yes | drift count, print layout |
| `id` | yes | deep links, search results |
| `data-entry-name` | yes | drift comparison |
| `data-entry-kind` | yes | source index grouping, search filtering |
| `data-entry-source` | yes | version comparison |
| `h3.entry-name` | yes | visible title |
| `p.entry-purpose` | yes | search description |
| `ul.entry-uses` | yes | visible use cases |
| `div.entry-prompt` | skill and agent only | copy button |

## Search index contract

`assets/search-index.json` is a JSON array. Field names are fixed.

```json
[
  {
    "name": "writing-mstest-tests",
    "kind": "skill",
    "source": "dotnet-test@1.0.0",
    "chapter": "testing",
    "anchor": "testing--writing-mstest-tests",
    "description": "Write and repair MSTest suites."
  }
]
```

| Field | Rule |
| --- | --- |
| `name` | Unique together with `source` |
| `kind` | Same vocabulary as `data-entry-kind` |
| `chapter` | One of the twelve fixed slugs |
| `anchor` | Must exist as an `id` in the named chapter file |
| `description` | One line, no markup |

## Drift script CLI contract

```text
check-drift.py [--book DIR] [--json] [--quiet] [--allow-stale]
```

| Flag | Behavior |
| --- | --- |
| `--book` | Book root; defaults to the directory containing the script |
| `--json` | Emit the DriftReport as JSON on stdout instead of prose |
| `--quiet` | Suppress the human summary; exit code still carries the verdict |
| `--allow-stale` | Report stale entries without failing; used after an intentional uninstall |

Exit codes:

| Code | Meaning |
| --- | --- |
| 0 | No drift |
| 1 | Drift detected (missing, stale, version change, count mismatch) |
| 2 | The script could not read an input it requires |

**Guarantees**: the script opens files read-only, never writes under the book root, and prints a
human-readable summary unless `--quiet` is passed.

## UI contract for the decision guide

Every row must name a default, a reason, and an escape condition, and every tool named must be a
real anchor.

```html
<table class="decision-table">
  <thead>
    <tr><th>Overlap</th><th>Default</th><th>Why</th><th>Use the other when</th></tr>
  </thead>
  <tbody>
    <tr>
      <td>C# code intelligence</td>
      <td><a href="chapters/dotnet-development.html#dotnet-development--dotnet">dotnet plugin</a></td>
      <td>Owns diagnostics and go-to-definition.</td>
      <td>You need symbol-level renames, so use Serena.</td>
    </tr>
  </tbody>
</table>
```

## Print contract

| Requirement | Mechanism |
| --- | --- |
| Cover page | `print.html` first section, `break-after: page` |
| Contents page | Second section, generated from chapter list |
| One chapter per page break | `.chapter { break-before: page; }` in `@media print` |
| Navigation suppressed | `.sidebar, .toolbar, .search, .copy-btn { display: none; }` |
| No clipping | `table { table-layout: fixed }`, `pre { white-space: pre-wrap }` |
| Letter geometry | `@page { size: letter; margin: 0.75in 0.7in; }` |
| Light palette forced | print rules re-declare light variables regardless of state |
