"""Flag markdown tables whose cells cannot fit their column when exported to PDF.

Why this exists
---------------
pandoc's Typst writer derives each column's width from the dash counts in the markdown
delimiter row, and Typst never breaks a `code` span across a line. A long unbreakable
token in a narrow column therefore bleeds over its neighbour, which is invisible in
markdown and in most editors and only shows up in the rendered PDF.

This script asks pandoc for the column widths it will actually emit, then checks every
cell for a token wider than its column. Run it after editing any table:

    .venv\\Scripts\\python.exe _check_pandoc_tables.py docs/specs/*.md Module_Receiving/*.md

Exit code is 1 when anything is flagged, so it can gate a commit if desired.

Assumptions: 10pt base font (set in _output.yaml), us-letter paper with 1.25in margins,
and the typst PDF engine. Adjust the constants below if those change.
"""

from __future__ import annotations

import re
import shutil
import subprocess
import sys
from pathlib import Path

TEXT_WIDTH_CM = 15.24  # us-letter 21.59cm minus 2 x 1.25in margins
MONO_CM_PER_CHAR = 0.212  # monospace advance at 10pt (0.6em)
PROP_CM_PER_CHAR = 0.176  # proportional average at 10pt (0.5em)
CELL_INSET_CM = 0.40  # table cell padding in the pandoc typst template
TOLERANCE_CM = 0.05  # ignore rounding-level excess

PANDOC_FALLBACKS = [Path.home() / "AppData/Local/Pandoc/pandoc.exe"]
DELIMITER_ROW = re.compile(r"^\s*\|[\s:|-]+\|\s*$")
FENCE = re.compile(r"^\s*(```|~~~)")
# Either the percent form "(12.5%, 50%, ...)" or the equal-width form "columns: 3".
# Pandoc line-wraps long output, so this runs against the whole document, not per line.
COLUMNS_SPEC = re.compile(r"columns:\s*(?:\(([^)]*)\)|\b(\d+)\b(?!\s*[%\d]))")
# Prose is hyphenated by Typst, so a plain word slightly wider than its column is
# cosmetic. Only flag prose that is long enough that hyphenation cannot save it.
PLAIN_WORD_ADVISORY_CHARS = 14


def find_pandoc() -> str:
    if found := shutil.which("pandoc"):
        return found
    for candidate in PANDOC_FALLBACKS:
        if candidate.exists():
            return str(candidate)
    sys.exit("pandoc not found on PATH; install it or set PANDOC_FALLBACKS.")


def column_fractions(md_path: Path, pandoc: str) -> list[list[float]]:
    """Ask pandoc for the column widths it will emit, in document order."""
    result = subprocess.run(
        [pandoc, str(md_path), "-f", "markdown+tex_math_single_backslash", "-s", "-t", "typst"],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
        check=True,
    )
    # Keep only text outside fenced blocks: code fences in the document become typst raw
    # blocks and would otherwise contribute spurious "columns:" matches.
    kept: list[str] = []
    in_fence = False
    for line in result.stdout.splitlines():
        if FENCE.match(line):
            in_fence = not in_fence
            kept.append("")
            continue
        kept.append("" if in_fence else line)

    tables: list[list[float]] = []
    for match in COLUMNS_SPEC.finditer("\n".join(kept)):
        if match.group(2):
            count = int(match.group(2))
            if count:
                tables.append([1.0 / count] * count)
            continue
        cells = [c.strip() for c in re.split(r"[,\n]", match.group(1)) if c.strip()]
        # Only the all-percent form describes a real table; other expressions such as
        # "(1fr,) * ncols" belong to unrelated template output.
        if cells and all(c.endswith("%") for c in cells):
            tables.append([float(c[:-1]) / 100.0 for c in cells])
    return tables


def split_cells(row: str) -> list[str]:
    body = row.strip()
    body = body[1:] if body.startswith("|") else body
    body = body[:-1] if body.endswith("|") else body
    return [cell.strip() for cell in body.split("|")]


def widest_token_cm(cell: str) -> tuple[float, str, bool]:
    """Widest run the renderer cannot break, the run itself, and whether it is code.

    A `code` span is never broken or hyphenated by Typst, so exceeding the column width
    always produces visible bleed. Plain prose is hyphenated, so it is only a concern for
    unusually long words.
    """
    widest, token = 0.0, ""
    for span in re.finditer(r"`([^`]*)`", cell):
        for part in span.group(1).split():
            if (width := len(part) * MONO_CM_PER_CHAR) > widest:
                widest, token = width, f"`{part}`"
    return widest, token, bool(token)


def markdown_tables(md_text: str) -> list[tuple[int, str, list[str]]]:
    """Return (delimiter_line_number, table_label, body_rows) per table."""
    lines = md_text.splitlines()
    tables: list[tuple[int, str, list[str]]] = []
    index, in_fence = 0, False
    while index < len(lines):
        if FENCE.match(lines[index]):
            in_fence = not in_fence
            index += 1
            continue
        if in_fence:
            index += 1
            continue
        is_header = lines[index].lstrip().startswith("|")
        is_delimiter = index + 1 < len(lines) and DELIMITER_ROW.match(lines[index + 1])
        if is_header and is_delimiter:
            label = " | ".join(split_cells(lines[index]))[:60]
            rows, cursor = [], index + 2
            while cursor < len(lines) and lines[cursor].lstrip().startswith("|"):
                rows.append(lines[cursor])
                cursor += 1
            tables.append((index + 2, label, rows))
            index = cursor
            continue
        index += 1
    return tables


def check(md_path: Path, pandoc: str) -> tuple[list[str], list[str]]:
    text = md_path.read_text(encoding="utf-8")
    tables = markdown_tables(text)
    widths = column_fractions(md_path, pandoc)
    errors: list[str] = []
    warnings: list[str] = []

    if len(tables) != len(widths):
        warnings.append(
            f"  note: parsed {len(tables)} markdown tables but pandoc emitted {len(widths)} "
            "column sets; line numbers below may be mispaired"
        )
    if not widths:
        return errors, warnings

    for (line_number, label, rows), fractions in zip(tables, widths):
        for offset, row in enumerate(rows):
            for column, cell in enumerate(split_cells(row)):
                if column >= len(fractions):
                    continue
                available = fractions[column] * TEXT_WIDTH_CM
                needed, token, is_code = widest_token_cm(cell)
                location = (
                    f"  [{label}] line {line_number + offset + 1}, column {column + 1}: "
                    f"{token} needs {needed + CELL_INSET_CM:.2f}cm but the column is "
                    f"{available:.2f}cm"
                )
                if needed + CELL_INSET_CM > available + TOLERANCE_CM:
                    errors.append(f"ERROR {location}")
                else:
                    plain = re.sub(r"`[^`]*`", " ", cell)
                    long_word = max(
                        (len(w) for w in plain.split() if len(w) > PLAIN_WORD_ADVISORY_CHARS),
                        default=0,
                    )
                    if long_word and long_word * PROP_CM_PER_CHAR + CELL_INSET_CM > available:
                        warnings.append(f"WARN  {location} (prose)")
    return errors, warnings


def main(argv: list[str]) -> int:
    targets = [Path(a) for a in argv[1:]] or sorted(Path(".").rglob("*.md"))
    pandoc = find_pandoc()
    failed = False
    for target in targets:
        if not target.is_file():
            continue
        try:
            errors, warnings = check(target, pandoc)
        except subprocess.CalledProcessError as error:
            print(f"{target}: pandoc failed: {(error.stderr or '').strip()[:200]}")
            failed = True
            continue
        for line in (*errors, *warnings):
            print(f"{target}: {line.strip()}")
        if not errors and not warnings:
            print(f"{target}: OK")
        if errors:
            failed = True
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
