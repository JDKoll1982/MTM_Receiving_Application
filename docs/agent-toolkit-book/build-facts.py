#!/usr/bin/env python3
"""Scan the repository and emit the suggestion lists used by the prompt builder.

The "fill in the facts" fields are free-text inputs backed by
``<datalist>`` suggestions. Those suggestions have to come from the real
codebase, not from a hand-written list that rots, so this script parses the
repository and writes one JSON file per field:

    assets/facts/module.json            Module_* directory names
    assets/facts/dao.json               DAO classes and service interfaces
    assets/facts/stored-procedure.json  MySQL procedures declared under Database/
    assets/facts/target-file.json       tracked .cs and .xaml paths
    assets/facts/test.json              test project and test file paths
    assets/facts/spec-folder.json       feature spec directories
    assets/facts/manifest.json          counts plus a short digest per field

It also writes ``assets/facts.js`` so the builder still has suggestions when
the pages are opened straight from disk, where fetch is blocked.

Nothing here modifies the codebase. Usage:

    python build-facts.py
    python build-facts.py --check      # report whether the JSON is current
"""

from __future__ import annotations

import argparse
import glob
import hashlib
import io
import json
import os
import re
import sys

SKIP_DIRS = {"bin", "obj", ".git", ".vs", "node_modules", ".serena", "TestResults", "print"}

FIELDS = {
    "module": "module.json",
    "dao": "dao.json",
    "storedProcedure": "stored-procedure.json",
    "targetFile": "target-file.json",
    "test": "test.json",
    "specFolder": "spec-folder.json",
}

PROCEDURE = re.compile(
    r"CREATE\s+(?:DEFINER\s*=\s*\S+\s+)?PROCEDURE\s+`?([A-Za-z0-9_]+)`?", re.IGNORECASE)


def find_repo(start: str):
    """Walk up until a repository root is found, so the script works when moved."""
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


def walk_files(root: str, suffixes: tuple[str, ...]) -> list[str]:
    """Yield repo-relative POSIX paths, skipping build output and tool caches."""
    found: list[str] = []
    for dirpath, dirnames, filenames in os.walk(root):
        dirnames[:] = sorted(
            d for d in dirnames
            if d not in SKIP_DIRS and not d.startswith(".git")
        )
        for name in sorted(filenames):
            if name.lower().endswith(suffixes):
                absolute = os.path.join(dirpath, name)
                found.append(os.path.relpath(absolute, root).replace("\\", "/"))
    return found


def collect(repo: str) -> dict[str, list[str]]:
    facts: dict[str, list[str]] = {}

    # Modules: the folder names the whole application is organised by.
    modules = sorted(
        name for name in os.listdir(repo)
        if os.path.isdir(os.path.join(repo, name)) and name.startswith("Module_")
    )
    facts["module"] = modules

    # DAO and service types, read from file names because that is the convention.
    # Test doubles are excluded: a suggestion list full of Dao_*Tests helps nobody.
    source_paths = walk_files(repo, (".cs",))
    dao = set()
    for path in source_paths:
        if path.startswith("MTM_Receiving_Application.Tests/"):
            continue
        base = os.path.basename(path)[:-3]
        if base.endswith("Tests") or base.endswith("Test"):
            continue
        if base.startswith(("Dao_", "IService_", "Service_")):
            dao.add(base)
    facts["dao"] = sorted(dao)

    # Stored procedures, de-duplicated because schema dumps repeat definitions.
    procedures = set()
    database_root = os.path.join(repo, "Database")
    for path in walk_files(database_root, (".sql",)):
        absolute = os.path.join(database_root, path)
        try:
            text = io.open(absolute, encoding="utf-8", errors="replace").read()
        except OSError:
            continue
        for match in PROCEDURE.finditer(text):
            procedures.add(match.group(1))
    # The repo convention is sp_*. Migration helpers are procedures too, but they
    # are not what a reader means by "stored procedure", so prefer the convention.
    conventional = {name for name in procedures if name.lower().startswith("sp_")}
    facts["storedProcedure"] = sorted(conventional or procedures)

    # Every source file, so the target-file field can suggest a real path.
    facts["targetFile"] = sorted(walk_files(repo, (".cs", ".xaml")))

    # Tests: the project plus each test class file, as repo-relative paths.
    test_root = os.path.join(repo, "MTM_Receiving_Application.Tests")
    tests: list[str] = ["MTM_Receiving_Application.Tests"]
    if os.path.isdir(test_root):
        for path in walk_files(test_root, (".cs",)):
            tests.append("MTM_Receiving_Application.Tests/" + path)
    facts["test"] = sorted(set(tests))

    # Spec directories, newest numbering first so recent work is easiest to pick.
    spec_root = os.path.join(repo, "specs")
    if os.path.isdir(spec_root):
        specs = sorted(
            (name for name in os.listdir(spec_root)
             if os.path.isdir(os.path.join(spec_root, name))),
            reverse=True,
        )
        facts["specFolder"] = ["specs/" + name for name in specs]
    else:
        facts["specFolder"] = []

    return facts


def digest(values: list[str]) -> str:
    joined = "\n".join(values)
    return hashlib.sha256(joined.encode("utf-8")).hexdigest()[:16]


def read_existing(book: str) -> dict:
    path = os.path.join(book, "assets", "facts", "manifest.json")
    if not os.path.isfile(path):
        return {}
    try:
        return json.loads(io.open(path, encoding="utf-8").read())
    except json.JSONDecodeError:
        return {}


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Build suggestion lists for the prompt builder.")
    parser.add_argument("--book", default=os.path.dirname(os.path.abspath(__file__)))
    parser.add_argument("--check", action="store_true",
                        help="report whether the fact files are current")
    args = parser.parse_args(argv)

    book = os.path.abspath(args.book)
    repo = find_repo(book)
    if not repo:
        print("build-facts: no repository root found above " + book, file=sys.stderr)
        return 2

    facts = collect(repo)
    manifest = {key: {"count": len(values), "digest": digest(values)}
                for key, values in facts.items()}

    if args.check:
        existing = read_existing(book)
        stale = []
        for key in facts:
            if not existing.get(key):
                stale.append(key + " (no manifest entry)")
            elif existing[key].get("digest") != manifest[key]["digest"]:
                stale.append("%s (%d recorded, %d now)"
                             % (key, existing[key].get("count", 0), manifest[key]["count"]))
        if stale:
            print("fact files are stale: " + ", ".join(stale))
            print("run: python build-facts.py")
            return 1
        print("fact files are current (%s)"
              % ", ".join("%s %d" % (k, len(facts[k])) for k in facts))
        return 0

    out_dir = os.path.join(book, "assets", "facts")
    os.makedirs(out_dir, exist_ok=True)
    for key, filename in FIELDS.items():
        payload = json.dumps(facts[key], indent=2, ensure_ascii=False) + "\n"
        io.open(os.path.join(out_dir, filename), "w", encoding="utf-8", newline="\n").write(payload)
    io.open(os.path.join(out_dir, "manifest.json"), "w", encoding="utf-8", newline="\n").write(
        json.dumps(manifest, indent=2) + "\n")

    # Offline fallback: a script tag works from file://, fetch does not.
    io.open(os.path.join(book, "assets", "facts.js"), "w", encoding="utf-8", newline="\n").write(
        "/* Generated by build-facts.py from the repository. Offline suggestion lists. */\n"
        "window.MTK_FACTS = " + json.dumps(facts, ensure_ascii=False) + ";\n")

    print("wrote facts: " + ", ".join("%s %d" % (k, len(facts[k])) for k in facts))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
