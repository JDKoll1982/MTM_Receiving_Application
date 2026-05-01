"""Discover candidate quantity fields in a running Infor Visual session.

This script connects to the Visual desktop client process and prints the top-level
windows plus candidate Edit controls that could correspond to the quantity field.
It does not modify any data. The goal is to replace stale hard-coded HWND values
with a repeatable discovery step.

Requirements:
    pip install pywinauto

Typical usage:
    python discover_visual_quantity_field.py
    python discover_visual_quantity_field.py --dump-identifiers
    python discover_visual_quantity_field.py --title-re "Receiver|Purchase"
"""

from __future__ import annotations

import argparse
import re
import sys
from dataclasses import dataclass
from typing import Iterable, List, Sequence

from pywinauto import Application
from pywinauto.findwindows import ElementNotFoundError


DEFAULT_PROCESS_NAME = "VMRCVENT.EXE"
DEFAULT_EDIT_CLASS = "Edit"


@dataclass
class ControlSnapshot:
    """A minimal serializable view of a discovered control."""

    handle: int
    class_name: str
    control_id: int
    rectangle: str
    window_text: str


def build_parser() -> argparse.ArgumentParser:
    """Create the command-line parser for the discovery tool."""
    parser = argparse.ArgumentParser(
        description=(
            "Inspect a running Infor Visual client and list candidate Edit controls."
        )
    )
    parser.add_argument(
        "--process-name",
        default=DEFAULT_PROCESS_NAME,
        help="Process image name to connect to. Defaults to VMRCVENT.EXE.",
    )
    parser.add_argument(
        "--title-re",
        default=".*",
        help="Regex used to filter top-level Visual windows by title.",
    )
    parser.add_argument(
        "--class-name",
        default=DEFAULT_EDIT_CLASS,
        help="Child control class to search for. Defaults to Edit.",
    )
    parser.add_argument(
        "--max-controls",
        type=int,
        default=25,
        help="Maximum number of candidate controls to print per window.",
    )
    parser.add_argument(
        "--dump-identifiers",
        action="store_true",
        help=(
            "Print pywinauto control identifiers for matching windows. Useful when "
            "you want a stable selector instead of a stale HWND."
        ),
    )
    return parser


def connect_to_visual(process_name: str) -> Application:
    """Connect to the first running Visual process that matches the executable."""
    try:
        return Application(backend="win32").connect(path=process_name)
    except ElementNotFoundError as exc:
        raise RuntimeError(
            f"Could not connect to a running Visual process named {process_name}."
        ) from exc


def get_matching_windows(app: Application, title_pattern: str) -> List:
    """Return top-level windows whose title matches the supplied regex."""
    pattern = re.compile(title_pattern, re.IGNORECASE)
    matches = []

    for window in app.windows():
        title = window.window_text() or ""
        if pattern.search(title):
            matches.append(window)

    return matches


def snapshot_control(control) -> ControlSnapshot:
    """Capture the small set of control properties needed for triage."""
    rectangle = control.rectangle()
    return ControlSnapshot(
        handle=int(control.handle),
        class_name=control.class_name(),
        control_id=int(control.control_id()),
        rectangle=(
            f"({rectangle.left},{rectangle.top})-({rectangle.right},{rectangle.bottom})"
        ),
        window_text=control.window_text() or "",
    )


def find_candidate_controls(window, class_name: str) -> List[ControlSnapshot]:
    """Find descendant controls that match the requested Win32 class name."""
    candidates: List[ControlSnapshot] = []

    for child in window.descendants(class_name=class_name):
        candidates.append(snapshot_control(child))

    return candidates


def format_control_rows(rows: Sequence[ControlSnapshot], limit: int) -> Iterable[str]:
    """Yield human-readable lines for the discovered controls."""
    for row in rows[:limit]:
        yield (
            f"    hwnd=0x{row.handle:08X} class={row.class_name} "
            f"id={row.control_id} rect={row.rectangle} text={row.window_text!r}"
        )


def print_window_report(windows: Sequence, class_name: str, limit: int) -> None:
    """Print a report of the matching windows and candidate child controls."""
    if not windows:
        print("No Visual windows matched the supplied title filter.")
        return

    for window in windows:
        print(
            f"Window: title={window.window_text()!r} "
            f"hwnd=0x{int(window.handle):08X} class={window.class_name()}"
        )
        candidates = find_candidate_controls(window, class_name)
        print(f"  Candidate {class_name} controls: {len(candidates)}")
        for line in format_control_rows(candidates, limit):
            print(line)
        print()


def dump_identifiers(windows: Sequence) -> None:
    """Print control identifier trees for deeper selector analysis."""
    for window in windows:
        print(f"=== Control identifiers for {window.window_text()!r} ===")
        window.print_control_identifiers()
        print()


def main(argv: Sequence[str]) -> int:
    """Run the discovery workflow and return a process exit code."""
    parser = build_parser()
    args = parser.parse_args(argv)

    try:
        app = connect_to_visual(args.process_name)
    except RuntimeError as exc:
        print(str(exc), file=sys.stderr)
        return 1

    windows = get_matching_windows(app, args.title_re)
    print_window_report(windows, args.class_name, args.max_controls)

    if args.dump_identifiers and windows:
        dump_identifiers(windows)

    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))