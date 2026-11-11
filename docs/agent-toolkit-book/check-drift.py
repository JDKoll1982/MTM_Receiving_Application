#!/usr/bin/env python3
"""Read-only drift check for the MTM Agent Toolkit book.

Compares the entries documented in the committed HTML against the live plugin and
repo inventory, and reports what is missing, stale, version-changed, or newly
available. It never writes, creates, or deletes a file.

Exit codes
----------
0   no drift
1   drift detected
2   a required input could not be read

Usage
-----
    python check-drift.py
    python check-drift.py --book docs/agent-toolkit-book --json
    python check-drift.py --allow-stale        # after an intentional uninstall
"""

from __future__ import annotations

import argparse
import glob
import json
import os
import re
import sys

TIMEOUT_NOTE = "read-only"


# --------------------------------------------------------------- helpers -----

def read_text(path: str) -> str:
    with open(path, "r", encoding="utf-8", errors="replace") as handle:
        return handle.read()


def attr(chunk: str, name: str) -> str:
    match = re.search(name + r'="([^"]*)"', chunk)
    return match.group(1) if match else ""


def slugify(value: str) -> str:
    return re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")


def find_repo(start: str):
    """Walk up from the book folder looking for a repository root.

    The book normally sits at ``<repo>/docs/agent-toolkit-book``, but the drift
    check must also survive being copied elsewhere for testing. Returning None
    lets the caller skip the repo comparison instead of reporting every repo
    skill as stale.
    """
    current = os.path.abspath(start)
    while True:
        if os.path.isdir(os.path.join(current, ".github")) and (
                os.path.isdir(os.path.join(current, ".git"))
                or os.path.isdir(os.path.join(current, ".specify"))):
            return current
        parent = os.path.dirname(current)
        if parent == current:
            return None
        current = parent


# ---------------------------------------------------------------- reading ----

def read_book(book: str) -> dict:
    chapter_dir = os.path.join(book, "chapters")
    if not os.path.isdir(chapter_dir):
        raise SystemExit(_fail("no chapters folder at " + chapter_dir))

    entries = []
    anchors = {}
    for path in sorted(glob.glob(os.path.join(chapter_dir, "*.html"))):
        text = read_text(path)
        chapter = os.path.splitext(os.path.basename(path))[0]
        anchors[chapter] = set(re.findall(r'id="([^"]+)"', text))
        for chunk in re.findall(r'<article class="entry"(.*?)</article>', text, re.S):
            entries.append({
                "name": attr(chunk, "data-entry-name"),
                "kind": attr(chunk, "data-entry-kind"),
                "source": attr(chunk, "data-entry-source"),
                "anchor": attr(chunk, "id"),
                "chapter": chapter,
            })

    nav_path = os.path.join(book, "assets", "nav.js")
    declared = {}
    if os.path.isfile(nav_path):
        nav = read_text(nav_path)
        for key in ("plugins", "skills", "agents", "canvases", "mcpServers", "total"):
            match = re.search(key + r":\s*(\d+)", nav)
            if match:
                declared[key] = int(match.group(1))

    search_path = os.path.join(book, "assets", "search-index.json")
    search = []
    if os.path.isfile(search_path):
        try:
            search = json.loads(read_text(search_path))
        except json.JSONDecodeError as exc:
            raise SystemExit(_fail(f"search-index.json is not valid JSON: {exc}"))

    return {"entries": entries, "anchors": anchors, "declared": declared,
            "search": search, "chapterDir": chapter_dir}


def read_live() -> dict:
    home = os.path.expanduser("~")
    plugins_root = os.path.join(home, ".copilot", "installed-plugins")
    plugins: dict[str, str] = {}
    skills: set[str] = set()
    agents: set[str] = set()
    canvases: set[str] = set()

    if os.path.isdir(plugins_root):
        for manifest in glob.glob(os.path.join(plugins_root, "*", "*", "plugin.json")):
            try:
                data = json.loads(read_text(manifest))
            except (OSError, json.JSONDecodeError):
                continue
            name = data.get("name") or os.path.basename(os.path.dirname(manifest))
            plugins[name] = str(data.get("version") or "0.0.0")
            plugin_dir = os.path.dirname(manifest)
            for skill in glob.glob(os.path.join(plugin_dir, "skills", "*")):
                if os.path.isdir(skill):
                    skills.add(os.path.basename(skill))
            for agent in glob.glob(os.path.join(plugin_dir, "**", "agents", "*.md"),
                                   recursive=True):
                agents.add(os.path.splitext(os.path.basename(agent))[0].replace(".agent", ""))
            for canvas in glob.glob(os.path.join(
                    plugin_dir, "com.github.copilot", "extensions", "*")):
                if os.path.isdir(canvas):
                    canvases.add(os.path.basename(canvas))

    here = os.path.dirname(os.path.abspath(__file__))
    repo = find_repo(here)
    github = os.path.join(repo, ".github") if repo else ""
    repo_found = bool(repo and os.path.isdir(github))

    repo_skills = {os.path.basename(p) for p in glob.glob(os.path.join(github, "Skills", "*"))
                   if os.path.isdir(p)} if repo_found else set()
    repo_agents = {
        os.path.splitext(os.path.basename(p))[0].replace(".agent", "")
        for p in glob.glob(os.path.join(github, "agents", "*.md"))
        if not os.path.basename(p).lower().startswith("readme")
    } if repo_found else set()
    repo_prompts = [os.path.basename(p) for p in glob.glob(
        os.path.join(github, "prompts", "**", "*.md"), recursive=True)] if repo_found else []
    repo_hooks = [os.path.basename(p) for p in glob.glob(os.path.join(github, "hooks", "*"))
                  if os.path.isfile(p)] if repo_found else []

    mcp = {}
    project_mcp = os.path.join(repo, ".vscode", "mcp.json") if repo else ""
    user_mcp = os.path.join(home, ".copilot", "mcp-config.json")
    if os.path.isfile(project_mcp):
        try:
            mcp.update(json.loads(read_text(project_mcp)).get("servers", {}) or {})
        except json.JSONDecodeError:
            pass
    if os.path.isfile(user_mcp):
        try:
            for key in json.loads(read_text(user_mcp)).get("mcpServers", {}) or {}:
                mcp[key.split("/")[-1]] = key
        except json.JSONDecodeError:
            pass

    return {"plugins": plugins, "skills": skills, "agents": agents, "canvases": canvases,
            "repoSkills": repo_skills, "repoAgents": repo_agents,
            "repoPrompts": repo_prompts, "repoHooks": repo_hooks, "mcp": mcp,
            "repoRoot": repo, "repoFound": repo_found}


def _fail(message: str) -> int:
    print("check-drift: " + message, file=sys.stderr)
    return 2


# -------------------------------------------------------------- comparing ----

def documented_version(entries, plugin: str) -> set[str]:
    pattern = re.compile(r"^\s*" + re.escape(plugin) + r"@([\w.\-+]+)")
    found = set()
    for entry in entries:
        match = pattern.match(entry["source"] or "")
        if match:
            found.add(match.group(1))
    return found


def compare(book: dict, live: dict, allow_stale: bool) -> dict:
    entries = book["entries"]
    names = {e["name"] for e in entries}

    live_names = set(live["skills"]) | set(live["agents"]) | set(live["canvases"])
    missing = sorted(n for n in live_names if n not in names)

    known = live_names | live["repoSkills"] | live["repoAgents"] | {"serena", "playwright",
            "microsoft-learn", "speckit-hook", "dotnet-plugin-lsp"}
    prompt_stems = {os.path.splitext(p)[0] for p in live["repoPrompts"]}
    prompt_slugs = {slugify(stem) for stem in prompt_stems}
    known |= prompt_stems | prompt_slugs
    repo_scoped = (lambda e: (e["source"] or "").startswith((".github", "~/.copilot")))
    stale = sorted(
        e["name"] for e in entries
        if e["kind"] in ("skill", "agent", "canvas", "mcp", "hook")
        and e["name"] not in known
        and not (not live.get("repoFound", True) and repo_scoped(e)))

    version_changed = []
    for plugin, installed in sorted(live["plugins"].items()):
        shown = documented_version(entries, plugin)
        if not shown:
            version_changed.append({"name": plugin, "documented": "(none)", "live": installed})
            continue
        if installed not in shown:
            version_changed.append({"name": plugin, "documented": ", ".join(sorted(shown)),
                                    "live": installed})

    updates = []
    for plugin, installed in sorted(live["plugins"].items()):
        shown = documented_version(entries, plugin)
        if shown and installed not in shown:
            updates.append({"plugin": plugin, "installed": installed,
                            "documented": ", ".join(sorted(shown)),
                            "note": "Installed version differs from the version the book documents; "
                                    "the skill set may have changed."})
    if missing:
        updates.append({"plugin": "(inventory)", "installed": str(len(missing)),
                        "documented": "0",
                        "note": "Skills or agents present on this machine are not yet documented."})

    duplicates = {}
    for entry in entries:
        if entry["kind"] == "skill":
            duplicates.setdefault(entry["name"], []).append(entry["source"])
    duplicates = {k: v for k, v in duplicates.items() if len(v) > 1}

    counts = {
        "entries": len(entries),
        "skills": len({e["name"] for e in entries if e["kind"] == "skill"}),
        "agents": len({e["name"] for e in entries if e["kind"] == "agent"}),
        "canvases": len({e["name"] for e in entries if e["kind"] == "canvas"}),
        "mcpServers": len({e["name"] for e in entries if e["kind"] == "mcp"}),
        "prompts": len({e["name"] for e in entries if e["kind"] == "prompt"}),
    }
    declared = book["declared"]
    count_notes = []
    if declared:
        if declared.get("total") and declared["total"] != counts["entries"]:
            count_notes.append("cover total %d but %d entries are documented"
                               % (declared["total"], counts["entries"]))

    broken_links = []
    missing_assets = []
    book_root = os.path.dirname(book["chapterDir"])
    for path in glob.glob(os.path.join(book["chapterDir"], "*.html")) + \
            glob.glob(os.path.join(book_root, "*.html")):
        text = read_text(path)
        for href in re.findall(r'href="([^"#]*\.html)#([^"]+)"', text):
            chapter_file, anchor = href
            chapter = os.path.splitext(os.path.basename(chapter_file))[0]
            if chapter in book["anchors"] and anchor not in book["anchors"][chapter]:
                broken_links.append({"file": os.path.basename(path), "target": chapter + "#" + anchor})
        # A page whose stylesheet or script path is wrong renders unstyled with
        # no navigation, which is exactly the failure a reader notices first.
        for ref in re.findall(r'(?:href|src)="([^"]+\.(?:css|js|png))"', text):
            if ref.startswith(("http", "//", "data:")):
                continue
            resolved = os.path.normpath(os.path.join(os.path.dirname(path), ref))
            if not os.path.isfile(resolved):
                missing_assets.append({"file": os.path.basename(path), "missing": ref})

    search_mismatch = []
    indexed = {(r.get("anchor") or "") for r in book["search"]}
    for entry in entries:
        if entry["anchor"] and entry["anchor"] not in indexed:
            search_mismatch.append(entry["anchor"])

    # The prompt builder's catalog is hand-curated, so it can name tools that no
    # longer exist. A prompt telling someone to invoke a missing skill is worse
    # than no prompt at all.
    catalog_problems = []
    catalog_path = os.path.join(book_root, "assets", "prompt-catalog.json")
    if os.path.isfile(catalog_path):
        try:
            catalog = json.loads(read_text(catalog_path))
        except json.JSONDecodeError as exc:
            catalog_problems.append("prompt-catalog.json is not valid JSON: %s" % exc)
            catalog = None
        if isinstance(catalog, dict):
            documented = {e["name"] for e in entries}
            categories = {c.get("id") for c in catalog.get("categories", [])}
            if not categories:
                catalog_problems.append("prompt catalog declares no categories")
            seen_ids = set()
            for prompt in catalog.get("prompts", []):
                pid = prompt.get("id", "(no id)")
                if pid in seen_ids:
                    catalog_problems.append("duplicate prompt id: %s" % pid)
                seen_ids.add(pid)
                if prompt.get("category") not in categories:
                    catalog_problems.append("%s uses unknown category %r"
                                            % (pid, prompt.get("category")))
                for tool in prompt.get("uses", []):
                    if tool not in documented:
                        catalog_problems.append("%s names an undocumented tool: %s" % (pid, tool))
                if not prompt.get("steps"):
                    catalog_problems.append("%s has no steps" % pid)

    has_drift = bool(missing or version_changed or count_notes or broken_links
                     or missing_assets or search_mismatch or catalog_problems
                     or (stale and not allow_stale))

    return {
        "missing": missing,
        "stale": stale,
        "versionChanged": version_changed,
        "updatesAvailable": updates,
        "duplicates": duplicates,
        "counts": counts,
        "countMismatch": count_notes,
        "brokenLinks": broken_links,
        "missingAssets": missing_assets,
        "searchIndexMissing": search_mismatch,
        "promptCatalogProblems": catalog_problems,
        "repoInventoryFound": live.get("repoFound", True),
        "countsAsExpected": not count_notes,
        "exitCode": 1 if has_drift else 0,
    }


# ------------------------------------------------------------------- main ----

def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Read-only drift check for the MTM Agent Toolkit.")
    parser.add_argument("--book", default=os.path.dirname(os.path.abspath(__file__)))
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--quiet", action="store_true")
    parser.add_argument("--allow-stale", action="store_true",
                        help="report stale entries without failing")
    args = parser.parse_args(argv)

    book_dir = os.path.abspath(args.book)
    try:
        book = read_book(book_dir)
    except SystemExit as exc:
        return int(exc.code or 2)
    except OSError as exc:
        return _fail("cannot read the book: %s" % exc)

    live = read_live()
    report = compare(book, live, args.allow_stale)

    if args.json:
        print(json.dumps(report, indent=2))
        return report["exitCode"]

    if not args.quiet:
        print("MTM Agent Toolkit - drift check")
        print("  book: %s" % book_dir)
        print("  documented entries: %d (%d skills, %d agents, %d canvases, %d MCP, %d prompts)"
              % (report["counts"]["entries"], report["counts"]["skills"],
                 report["counts"]["agents"], report["counts"]["canvases"],
                 report["counts"]["mcpServers"], report["counts"]["prompts"]))
        print("  live: %d plugins, %d skills, %d agents, %d canvases"
              % (len(live["plugins"]), len(live["skills"]), len(live["agents"]),
                 len(live["canvases"])))
        if not live.get("repoFound", True):
            print("  note: no repository root found above this book, so the .github inventory")
            print("        was skipped and repo-owned entries are not compared.")
        print()

        def section(title, items, fmt):
            if not items:
                return
            print("%s (%d)" % (title, len(items)))
            for item in items:
                print("  - " + fmt(item))
            print()

        section("MISSING - on this machine, not in the book", report["missing"],
                lambda x: x)
        section("STALE - in the book, not on this machine", report["stale"], lambda x: x)
        section("VERSION CHANGED", report["versionChanged"],
                lambda x: "%s: book says %s, installed %s"
                          % (x["name"], x["documented"], x["live"]))
        section("UPDATES TO CONSIDER", report["updatesAvailable"],
                lambda x: "%s: %s" % (x["plugin"], x["note"]))
        section("DUPLICATE SKILL NAMES", report["duplicates"],
                lambda k: "%s -> %s" % (k, ", ".join(report["duplicates"][k])))
        section("COUNT MISMATCH", report["countMismatch"], lambda x: x)
        section("BROKEN LINK", report["brokenLinks"],
                lambda x: "%s -> %s" % (x["file"], x["target"]))
        section("MISSING ASSET", report["missingAssets"],
                lambda x: "%s -> %s" % (x["file"], x["missing"]))
        section("SEARCH INDEX GAP", report["searchIndexMissing"], lambda x: x)
        section("PROMPT CATALOG", report.get("promptCatalogProblems", []), lambda x: x)

        if report["exitCode"] == 0:
            print("No drift. The book matches this machine.")
        else:
            print("Drift detected. Nothing was changed; this check is read-only.")
            if report["stale"] and args.allow_stale:
                print("  (stale entries were allowed by --allow-stale)")

    return report["exitCode"]


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
