"""Set the first-line Quantity Received value in Infor Visual.

This script automates the Purchase Receipt Entry grid in the running
``VMRCVENT.EXE`` client. It targets the first visible row in the
``Quantity Received`` column and attempts to edit the in-place ``Edit`` control
with control id ``32791``.

The implementation uses the live findings from this repository's inspection work:

* main window title: ``Purchase Receipt Entry - Infor VISUAL - MTMFG``
* grid class: ``Gupta:ChildTable``
* live cell editor class: ``Edit``
* live cell editor control id: ``32791``
* first-row quantity cell offset inside the grid: approximately ``(421, 45)``
* first-row selector offset inside the grid: approximately ``(15, 45)``

Requirements:
    pip install pywinauto

Examples:
    .\\.venv\\Scripts\\python.exe Database\\InforVisualScripts\\Python_Scripts\\set_visual_quantity_received.py --value 2000
    .\\.venv\\Scripts\\python.exe Database\\InforVisualScripts\\Python_Scripts\\set_visual_quantity_received.py --value 2000 --dry-run
"""

from __future__ import annotations

import argparse
import sys
import time
from dataclasses import dataclass
from typing import Sequence

from pywinauto import Application, keyboard, mouse
from pywinauto.findwindows import ElementNotFoundError


DEFAULT_PROCESS_NAME = "VMRCVENT.EXE"
DEFAULT_WINDOW_TITLE = "Purchase Receipt Entry - Infor VISUAL - MTMFG"
DEFAULT_TABLE_CLASS = "Gupta:ChildTable"
DEFAULT_EDITOR_CLASS = "Edit"
DEFAULT_EDITOR_CONTROL_ID = 32791
DEFAULT_QUANTITY_VALUE = "2000"
DEFAULT_CELL_OFFSET_X = 421
DEFAULT_CELL_OFFSET_Y = 45
DEFAULT_ROW_SELECTOR_OFFSET_X = 15
DEFAULT_POST_CLICK_DELAY = 0.25


@dataclass
class TargetCell:
    """A resolved target point and its owning grid window."""

    x: int
    y: int
    grid_handle: int
    row_selector_x: int


def build_parser() -> argparse.ArgumentParser:
    """Create the command-line parser for the setter script."""
    parser = argparse.ArgumentParser(
        description=(
            "Activate the row 1 Quantity Received cell in Purchase Receipt Entry "
            "and set its value."
        )
    )
    parser.add_argument(
        "--value",
        default=DEFAULT_QUANTITY_VALUE,
        help="Quantity value to write into the first visible row.",
    )
    parser.add_argument(
        "--process-name",
        default=DEFAULT_PROCESS_NAME,
        help="Visual process image name. Defaults to VMRCVENT.EXE.",
    )
    parser.add_argument(
        "--window-title",
        default=DEFAULT_WINDOW_TITLE,
        help="Exact Purchase Receipt Entry window title.",
    )
    parser.add_argument(
        "--editor-control-id",
        type=int,
        default=DEFAULT_EDITOR_CONTROL_ID,
        help="Control id of the active in-place quantity editor.",
    )
    parser.add_argument(
        "--cell-offset-x",
        type=int,
        default=DEFAULT_CELL_OFFSET_X,
        help="Horizontal offset from the grid origin to the row 1 quantity cell.",
    )
    parser.add_argument(
        "--cell-offset-y",
        type=int,
        default=DEFAULT_CELL_OFFSET_Y,
        help="Vertical offset from the grid origin to the first-line quantity cell.",
    )
    parser.add_argument(
        "--row-selector-offset-x",
        type=int,
        default=DEFAULT_ROW_SELECTOR_OFFSET_X,
        help="Horizontal offset from the grid origin to the first-line selector.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Resolve the target cell and print actions without clicking or typing.",
    )
    return parser


def connect_to_visual(process_name: str) -> Application:
    """Connect to the running Visual desktop client."""
    try:
        return Application(backend="win32").connect(path=process_name)
    except ElementNotFoundError as exc:
        raise RuntimeError(
            f"Could not connect to a running Visual process named {process_name}."
        ) from exc


def resolve_main_window(app: Application, window_title: str):
    """Return the exact Purchase Receipt Entry window wrapper."""
    for window in app.windows():
        if window.window_text() == window_title:
            return window

    raise RuntimeError(
        f"Could not find a Visual window titled {window_title!r}."
    )


def resolve_grid(window):
    """Return the most likely Gupta child table used for line entry."""
    tables = []
    for child in window.descendants(class_name=DEFAULT_TABLE_CLASS):
        rect = child.rectangle()
        if rect.width() > 0 and rect.height() > 0:
            tables.append((rect.width() * rect.height(), child))

    if not tables:
        raise RuntimeError(
            "Could not find a usable Purchase Receipt Entry grid."
        )

    tables.sort(key=lambda item: item[0], reverse=True)
    return tables[0][1]


def resolve_target_cell(
    grid,
    offset_x: int,
    offset_y: int,
    row_selector_offset_x: int,
) -> TargetCell:
    """Translate the known first-line offsets into screen coordinates."""
    rect = grid.rectangle()
    return TargetCell(
        x=rect.left + offset_x,
        y=rect.top + offset_y,
        grid_handle=int(grid.handle),
        row_selector_x=rect.left + row_selector_offset_x,
    )


def find_visible_editor(window, control_id: int):
    """Return the visible in-place quantity editor when it exists."""
    for child in window.descendants(class_name=DEFAULT_EDITOR_CLASS):
        rect = child.rectangle()
        if child.control_id() == control_id and rect.left >= 0 and rect.top >= 0:
            return child

    return None


def activate_quantity_cell(cell: TargetCell, delay_seconds: float) -> None:
    """Anchor the first line, then enter edit mode for its quantity cell."""
    mouse.click(button="left", coords=(cell.row_selector_x, cell.y))
    time.sleep(delay_seconds)
    mouse.click(button="left", coords=(cell.x, cell.y))
    time.sleep(delay_seconds)
    mouse.double_click(button="left", coords=(cell.x, cell.y))
    time.sleep(delay_seconds)


def try_set_via_editor(window, control_id: int, value: str) -> bool:
    """Try to populate the active in-place editor directly."""
    editor = find_visible_editor(window, control_id)
    if editor is None:
        return False

    editor.set_edit_text(value)
    return True


def fallback_set_via_keyboard(value: str) -> None:
    """Write the value using keyboard input when the direct editor path is absent."""
    keyboard.send_keys("^a")
    time.sleep(0.1)
    keyboard.send_keys(value, with_spaces=True)


def main(argv: Sequence[str]) -> int:
    """Run the quantity-cell activation and update workflow."""
    parser = build_parser()
    args = parser.parse_args(argv)

    try:
        app = connect_to_visual(args.process_name)
        window = resolve_main_window(app, args.window_title)
        grid = resolve_grid(window)
        cell = resolve_target_cell(
            grid,
            args.cell_offset_x,
            args.cell_offset_y,
            args.row_selector_offset_x,
        )
    except RuntimeError as exc:
        print(str(exc), file=sys.stderr)
        return 1

    print(
        f"Window: {window.window_text()!r} | hwnd=0x{int(window.handle):08X} "
        f"class={window.class_name()}"
    )
    print(
        f"Grid: hwnd=0x{int(grid.handle):08X} | rect={grid.rectangle()} | "
        f"row_selector=({cell.row_selector_x}, {cell.y}) | "
        f"target_cell=({cell.x}, {cell.y})"
    )

    if args.dry_run:
        print(
            f"Dry run: would select the first line and set Quantity Received "
            f"to {args.value!r}."
        )
        return 0

    window.set_focus()
    time.sleep(DEFAULT_POST_CLICK_DELAY)
    activate_quantity_cell(cell, DEFAULT_POST_CLICK_DELAY)

    if try_set_via_editor(window, args.editor_control_id, args.value):
        print(f"Set Quantity Received to {args.value!r} via active editor.")
        return 0

    fallback_set_via_keyboard(args.value)
    print(
        f"Set Quantity Received to {args.value!r} via keyboard fallback after "
        "activating the cell."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))