#!/usr/bin/env python3
"""Rebuild the derived pages of the MTM Agent Toolkit book.

The twelve files under ``chapters/`` are hand-authored and are the source of
truth. Three artifacts are derived from them and must be rebuilt after any
chapter edit:

    assets/search-index.json   search records, one per entry
    assets/search-index.js     the same records as an offline fallback
    index-by-source.html       every entry grouped by plugin or repo path
    print.html                 cover, contents, and all chapters in one document

Asset references are emitted root-relative (``assets/...``) because these pages
sit at the book root. Getting that wrong renders the page unstyled with no
navigation, so ``check-drift.py`` verifies it.

Usage
-----
    python build-pages.py
    python build-pages.py --check     # report whether the pages are current
"""

from __future__ import annotations

import argparse
import glob
import html
import io
import json
import os
import re
import sys

CHAPTER_ORDER = [
    "spec-driven-development",
    "dotnet-development",
    "testing",
    "winui-platform",
    "packaging-signing",
    "database-sql",
    "code-quality",
    "security-supply-chain",
    "documentation-diagrams",
    "context-planning",
    "mcp-and-environment",
    "repo-assets",
]

TITLES = {
    "spec-driven-development": "Spec-driven development",
    "dotnet-development": "C# and .NET development",
    "testing": "Testing",
    "winui-platform": "WinUI 3 and the Windows platform",
    "packaging-signing": "Packaging, signing, and distribution",
    "database-sql": "Database and SQL",
    "code-quality": "Code quality and review",
    "security-supply-chain": "Security and supply chain",
    "documentation-diagrams": "Documentation and diagrams",
    "context-planning": "Context, planning, and testability",
    "mcp-and-environment": "MCP servers and environment",
    "repo-assets": "Repo skills, agents, and prompts",
}

DESCRIPTIONS = {
    "spec-driven-development": "Spec Kit pipeline, its agents, and the companion board",
    "dotnet-development": "Language server, refactoring, MSBuild, and framework guidance",
    "testing": "Test generation, execution, coverage, and quality analysis",
    "winui-platform": "XAML patterns, design review, Windows API lookup, UI automation",
    "packaging-signing": "MSIX, certificates, package identity, manifests",
    "database-sql": "Stored procedures, query review, and ERP read-only discipline",
    "code-quality": "Review agents, assertion audits, anti-pattern detection",
    "security-supply-chain": "Scanning alerts, dependency triage, prompt safety",
    "documentation-diagrams": "Word output, draw.io diagrams, and documenter agents",
    "context-planning": "Change planning, context maps, and static-dependency seams",
    "mcp-and-environment": "Serena, Playwright, Microsoft Learn, hooks, and two language servers",
    "repo-assets": "Everything in .github that is not plugin-provided",
}

CHAPTER_KEYWORDS = {
    "spec-driven-development": ["spec", "specs", "specify", "plan", "tasks", "implement", "pipeline", "workflow", "companion", "drift", "living spec", "assess", "bug", "constitution", "checklist", "clarify", "analyze"],
    "dotnet-development": ["csharp", "c#", "dotnet", ".net", "refactor", "msbuild", "build", "sdk", "async", "upgrade", "resx", "language server", "lsp", "diagnostics"],
    "testing": ["testing", "test", "tests", "unit", "integration", "xunit", "assert", "assertion", "coverage", "runner", "filter", "trx", "tdd", "playwright", "mstest", "nunit", "tunit", "fixture"],
    "winui-platform": ["winui", "xaml", "binding", "xbind", "windows", "desktop", "app", "control", "design", "layout", "ui", "automation", "ui automation", "sandbox", "api", "devtools", "maui", "performance", "lookup"],
    "packaging-signing": ["packaging", "package", "msix", "installer", "signing", "sign", "certificate", "pfx", "manifest", "appxmanifest", "identity", "store", "distribution", "setup", "prerequisite", "developer mode"],
    "database-sql": ["database", "sql", "query", "stored procedure", "procedure", "mysql", "sql server", "index", "optimization", "dba", "schema", "erp", "infor", "transaction"],
    "code-quality": ["quality", "review", "assertion", "antipattern", "anti-pattern", "smell", "tagging", "grade", "audit", "refactor", "lint"],
    "security-supply-chain": ["security", "vulnerability", "secret", "scanning", "finding", "ghas", "dependabot", "dependency", "supply chain", "prompt safety"],
    "documentation-diagrams": ["documentation", "docs", "diagram", "drawio", "word", "docx", "markdown", "architecture", "png", "export", "documenter"],
    "context-planning": ["context", "planning", "map", "refactor", "testability", "seam", "wrapper", "static dependency", "gap", "mutation", "crap", "coverage", "platform", "hot reload", "scaffold", "migration", "migrate", "untested", "blind spot"],
    "mcp-and-environment": ["mcp", "server", "serena", "playwright", "learn", "hook", "environment", "language server", "tooling", "setup"],
    "repo-assets": ["repo", "skill", "skills", "agent", "agents", "prompt", "prompts", "speckit", "command", "hook", "instructions"],
}

PRIMARY = {
    "spec-driven-development": ["speckit.specify", "speckit.plan", "speckit.tasks", "speckit.implement", "speckit.companion.auto"],
    "dotnet-development": ["csharp-expert", "csharp-refactoring", "msbuild", "dotnet-best-practices"],
    "testing": ["run-tests", "writing-mstest-tests", "code-testing", "test-engineer", "csharp-xunit"],
    "winui-platform": ["winui-code-review", "winui-design", "winui-dev-workflow", "winapp-find-ui"],
    "packaging-signing": ["winapp-package", "winui-packaging", "winapp-signing", "winui-setup"],
    "database-sql": ["sql-code-review", "sql-optimization", "ms-sql-dba"],
    "code-quality": ["test-anti-patterns", "assertion-quality", "code-testing"],
    "security-supply-chain": ["github-security", "dependabot-triage"],
    "documentation-diagrams": ["drawio", "project-documenter", "md-to-docx"],
    "context-planning": ["context-map", "refactor-plan", "find-untested-sources", "test-gap-analysis"],
    "mcp-and-environment": ["serena", "dotnet-plugin-lsp", "microsoft-learn"],
    "repo-assets": ["agent-performance-audit", "prompt-builder", "winui3-expert"],
}

ASSET_VERSION = "?v=20261111b"


def versioned(page: str) -> str:
    """Stamp a cache-busting query onto every local asset reference.

    Without this, a browser that cached an older book.js keeps running it after
    the file changes, which looks exactly like a missing feature.
    """
    for name in ("book.css", "nav.js", "book.js", "search-index.js"):
        page = page.replace('assets/%s"' % name, 'assets/%s%s"' % (name, ASSET_VERSION))
    return page

FAVICON = '<link rel="icon" href="data:,">\n'
ARTICLE = re.compile(r'<article class="entry"(.*?)</article>', re.S)
PAGE_BODY = re.compile(r'(<div class="page">)(.*?)(\n    </div>\n  </main>)', re.S)


def esc(value: str) -> str:
    return html.escape(value or "", quote=True)


def attr(name: str, chunk: str) -> str:
    match = re.search(name + r'="([^"]*)"', chunk)
    return match.group(1) if match else ""


def plain(fragment: str) -> str:
    text = re.sub(r"<[^>]+>", "", fragment or "")
    return re.sub(r"\s+", " ", html.unescape(text)).strip()


def read_chapters(book: str) -> tuple[list[dict], dict]:
    chapter_dir = os.path.join(book, "chapters")
    records: list[dict] = []
    bodies: dict[str, str] = {}

    for slug in CHAPTER_ORDER:
        path = os.path.join(chapter_dir, slug + ".html")
        if not os.path.isfile(path):
            raise SystemExit("build-pages: missing chapter file: " + path)
        text = io.open(path, encoding="utf-8").read()

        match = PAGE_BODY.search(text)
        bodies[slug] = match.group(2) if match else ""

        primaries = set(PRIMARY.get(slug, []))
        keywords = " ; ".join(CHAPTER_KEYWORDS.get(slug, []))

        for chunk in ARTICLE.finditer(text):
            body = chunk.group(1)
            name = attr("data-entry-name", body)
            purpose = re.search(r'<p class="entry-purpose">(.*?)</p>', body, re.S)
            description = plain(purpose.group(1)) if purpose else ""
            name_words = " ".join(name.replace("-", " ").replace(".", " ").split())
            records.append({
                "name": name,
                "kind": attr("data-entry-kind", body),
                "source": attr("data-entry-source", body),
                "chapter": slug,
                "anchor": attr("id", body),
                # Full text, never truncated: an ellipsis here is a lie in the index.
                "description": description,
                "keywords": name_words + " ; " + keywords,
                "primary": name in primaries,
            })

    return records, bodies


def counts_by_chapter(records: list[dict]) -> dict[str, int]:
    counts = {slug: 0 for slug in CHAPTER_ORDER}
    for record in records:
        counts[record["chapter"]] = counts.get(record["chapter"], 0) + 1
    return counts


def write_search_index(book: str, records: list[dict]) -> None:
    assets = os.path.join(book, "assets")
    payload = json.dumps(records, indent=2, ensure_ascii=False) + "\n"
    io.open(os.path.join(assets, "search-index.json"), "w", encoding="utf-8", newline="\n").write(payload)
    io.open(os.path.join(assets, "search-index.js"), "w", encoding="utf-8", newline="\n").write(
        "/* Generated by build-pages.py from chapters/*.html. Offline search fallback. */\n"
        "window.MTK_INDEX = " + json.dumps(records, ensure_ascii=False) + ";\n")


def write_prompt_catalog(book: str, records: list[dict]) -> list[str]:
    """Validate the hand-curated prompt catalog and emit its offline fallback.

    The catalog is authored by hand, so it can drift from reality. Every tool it
    names must still exist in the book, otherwise the generated prompt tells the
    reader to invoke something that is not installed.
    """
    catalog_path = os.path.join(book, "assets", "prompt-catalog.json")
    if not os.path.isfile(catalog_path):
        return ["assets/prompt-catalog.json is missing"]

    try:
        catalog = json.loads(io.open(catalog_path, encoding="utf-8").read())
    except json.JSONDecodeError as exc:
        return ["assets/prompt-catalog.json is not valid JSON: %s" % exc]

    known = {record["name"] for record in records}
    category_ids = {c.get("id") for c in catalog.get("categories", [])}
    problems: list[str] = []

    seen: set[str] = set()
    for prompt in catalog.get("prompts", []):
        prompt_id = prompt.get("id", "(no id)")
        if prompt_id in seen:
            problems.append("duplicate prompt id: %s" % prompt_id)
        seen.add(prompt_id)
        if prompt.get("category") not in category_ids:
            problems.append("%s has unknown category %r" % (prompt_id, prompt.get("category")))
        for tool in prompt.get("uses", []):
            if tool not in known:
                problems.append("%s names an unknown tool: %s" % (prompt_id, tool))
        if not prompt.get("steps"):
            problems.append("%s has no steps" % prompt_id)

    payload = json.dumps(catalog, ensure_ascii=False)
    io.open(os.path.join(book, "assets", "prompt-catalog.js"), "w", encoding="utf-8", newline="\n").write(
        "/* Generated by build-pages.py. Offline fallback for the prompt builder. */\n"
        "window.MTK_CATALOG = " + payload + ";\n")
    return problems


def group_key(source: str) -> tuple[str, str]:
    if "@" in source and not source.startswith((".", "~")):
        return ("plugin", source.strip())
    parts = source.replace("\\", "/").split("/")
    if source.startswith("~"):
        return ("repo", "~/.copilot")
    if len(parts) >= 2:
        return ("repo", "/".join(parts[:2]))
    return ("repo", source or "repo")


def write_source_index(book: str, records: list[dict]) -> None:
    groups: dict[tuple[str, str], list[dict]] = {}
    for record in records:
        groups.setdefault(group_key(record["source"]), []).append(record)

    blocks = []
    for key in sorted(groups, key=lambda k: (k[0] != "plugin", k[1].lower())):
        kind, label_text = key
        items = sorted(groups[key], key=lambda r: r["name"].lower())
        label = "Plugin" if kind == "plugin" else "Repo path"
        rows = []
        for record in items:
            haystack = " ".join([
                record["name"], record["kind"], record["source"], record["description"],
            ]).lower()
            rows.append(
                '            <tr data-index-row="%s"><td><a href="chapters/%s.html#%s"><code>%s</code></a></td>'
                '<td>%s</td><td>%s</td></tr>' % (
                    esc(haystack), esc(record["chapter"]), esc(record["anchor"]),
                    esc(record["name"]), esc(record["kind"]), esc(record["description"])))
        blocks.append(
            '      <section class="source-group" data-source-group>\n'
            '        <div class="source-head">\n'
            '          <h3>%s</h3>\n'
            '          <span class="source-meta">%s &middot; %d entr%s</span>\n'
            '        </div>\n'
            '        <table class="source-table">\n'
            '          <thead><tr><th style="width:24%%">Entry</th><th style="width:12%%">Kind</th><th>Purpose</th></tr></thead>\n'
            '          <tbody>\n%s\n          </tbody>\n'
            '        </table>\n'
            '      </section>' % (
                esc(label_text), label, len(items), "y" if len(items) == 1 else "ies",
                "\n".join(rows)))

    page = '''<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Index by source &mdash; MTM Agent Toolkit</title>
''' + FAVICON + '''<link rel="stylesheet" href="assets/book.css">
</head>
<body>
<div class="shell">
  <aside class="sidebar" data-nav></aside>
  <main class="main">
    <div class="page">
      <p class="eyebrow">Reference</p>
      <h1>Index by source</h1>
      <p class="lede">Every entry in the book, listed under the plugin or repo path that provides it.
      This is the completeness guarantee: if a tool exists in this workspace and is documented, it is
      on this page.</p>
      <div class="index-filter no-print">
        <input type="search" data-index-filter placeholder="Filter by name, kind, source, or purpose" aria-label="Filter the source index">
        <span class="filter-count" data-index-count></span>
      </div>
%s
      <hr class="rule">
      <p class="callout"><strong>%d entries.</strong> Grouped into %d sources. A skill bundled by two
      plugins appears once, under both names, so the totals here match the cover page.</p>
    </div>
  </main>
</div>
<script src="assets/nav.js"></script>
<script src="assets/search-index.js"></script>
<script src="assets/book.js"></script>
</body>
</html>
''' % ("\n".join(blocks), len(records), len(groups))

    io.open(os.path.join(book, "index-by-source.html"), "w", encoding="utf-8", newline="\n").write(versioned(page))


def write_print_edition(book: str, records: list[dict], bodies: dict[str, str]) -> None:
    counts = counts_by_chapter(records)

    def fix(body: str) -> str:
        body = body.replace('href="../assets/', 'href="assets/')
        body = body.replace('src="../assets/', 'src="assets/')
        for slug in CHAPTER_ORDER:
            body = body.replace('href="%s.html' % slug, 'href="chapters/%s.html' % slug)
        body = re.sub(r'<p class="eyebrow">Chapter \d+</p>\s*', '', body)
        return body.replace('<h1>', '<h1 class="chapter-title">')

    sections = [
        '<section class="chapter" id="%s">\n%s\n</section>' % (slug, fix(bodies[slug]))
        for slug in CHAPTER_ORDER
    ]
    toc = [
        '        <li><a href="#%s"><span class="num"></span><span><span class="label">%s</span></span>'
        '<span class="count">%d entries</span></a></li>' % (slug, TITLES[slug], counts.get(slug, 0))
        for slug in CHAPTER_ORDER
    ]

    page = '''<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>MTM Agent Toolkit &mdash; print edition</title>
''' + FAVICON + '''<link rel="stylesheet" href="assets/book.css">
</head>
<body class="print-edition">
<div class="shell">
  <aside class="sidebar no-print" data-nav></aside>
  <main class="main">
    <div class="page">

      <section class="cover-page">
        <p class="eyebrow">Internal reference</p>
        <h1 class="cover-title">MTM Agent Toolkit</h1>
        <div class="cover-sub">Every plugin, skill, agent, canvas, and MCP server available to this
        workspace, organised by the job you are trying to do.</div>
        <div class="cover-rule"></div>
        <dl class="cover-meta">
          <div><dt>Plugins</dt><dd>13</dd></div>
          <div><dt>Skills</dt><dd>71<small>unique</small></dd></div>
          <div><dt>Agents</dt><dd>21<small>plugin-provided</small></dd></div>
          <div><dt>Canvases</dt><dd>4</dd></div>
          <div><dt>MCP servers</dt><dd>3</dd></div>
          <div><dt>Entries documented</dt><dd>%d</dd></div>
        </dl>
        <p class="no-print" style="margin:22px 0 0"><button class="print-action" type="button" onclick="window.print()">Print this book</button></p>
        <p class="print-hint no-print">Choose US Letter if your print dialog asks. The cover, contents, and all twelve chapters print in order, each chapter starting on a new page.</p>
        <p class="cover-note">Repository: JDKoll1982/MTM_Receiving_Application &middot;
        Generated 2026-11-11 &middot; Print at US Letter</p>
      </section>

      <section class="toc-page">
        <h1>Contents</h1>
        <ul class="contents">
%s
        </ul>
      </section>

%s

    </div>
  </main>
</div>
<script src="assets/nav.js"></script>
<script src="assets/search-index.js"></script>
<script src="assets/book.js"></script>
</body>
</html>
''' % (len(records), "\n".join(toc), "\n\n".join(sections))

    io.open(os.path.join(book, "print.html"), "w", encoding="utf-8", newline="\n").write(versioned(page))


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Rebuild derived book pages.")
    parser.add_argument("--book", default=os.path.dirname(os.path.abspath(__file__)))
    parser.add_argument("--check", action="store_true",
                        help="report whether the derived pages are already current")
    args = parser.parse_args(argv)

    book = os.path.abspath(args.book)
    records, bodies = read_chapters(book)

    if args.check:
        stale = []
        index_path = os.path.join(book, "assets", "search-index.json")
        if not os.path.isfile(index_path):
            stale.append("assets/search-index.json")
        else:
            existing = json.loads(io.open(index_path, encoding="utf-8").read())
            if len(existing) != len(records):
                stale.append("assets/search-index.json (entry count differs)")
            else:
                for old, new in zip(existing, records):
                    if old.get("description") != new.get("description") or old.get("anchor") != new.get("anchor"):
                        stale.append("assets/search-index.json (content differs)")
                        break
        catalog_problems = write_prompt_catalog(book, records)
        if catalog_problems:
            print("prompt catalog problems:")
            for problem in catalog_problems:
                print("  - " + problem)
            return 1
        print("derived pages are current" if not stale else "derived pages need a rebuild: "
              + ", ".join(stale))
        return 0 if not stale else 1

    write_search_index(book, records)
    write_source_index(book, records)
    write_print_edition(book, records, bodies)
    catalog_problems = write_prompt_catalog(book, records)
    print("rebuilt: assets/search-index.json, assets/search-index.js, "
          "assets/prompt-catalog.js, index-by-source.html, print.html (%d entries)" % len(records))
    if catalog_problems:
        print("prompt catalog problems:")
        for problem in catalog_problems:
            print("  - " + problem)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
