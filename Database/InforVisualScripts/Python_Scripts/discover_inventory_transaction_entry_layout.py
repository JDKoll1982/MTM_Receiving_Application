"""Discover anchor points for Inventory Transaction Entry.

This script captures the live field locations for the Inventory Transaction Entry
workflow hosted by ``VMINVENT.EXE``. It accepts either the direct child window
title or the parent host frame title used by Visual.

Usage:
    .\\.venv32\\Scripts\\python.exe Database\\InforVisualScripts\\Python_Scripts\\discover_inventory_transaction_entry_layout.py
    .\\.venv32\\Scripts\\python.exe Database\\InforVisualScripts\\Python_Scripts\\discover_inventory_transaction_entry_layout.py --list-windows
"""

from __future__ import annotations

import argparse
import ctypes
import json
import sys
import time
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Sequence

from pywinauto import Application
from pywinauto.findwindows import ElementNotFoundError


PROCESS_NAME = "VMINVENT.EXE"
TARGET_WINDOW_TITLE = "Inventory Transaction Entry - Infor VISUAL - MTMFG"
HOST_WINDOW_TITLES = {TARGET_WINDOW_TITLE, "Inventory Transfers"}
CONFIG_PATH = Path(__file__).with_name("inventory_transaction_entry_layout.json")
CAPTURE_DELAY_SECONDS = 3
MAX_ATTEMPTS = 3
GA_ROOT = 2


class POINT(ctypes.Structure):
    _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]


class RECT(ctypes.Structure):
    _fields_ = [
        ("left", ctypes.c_long),
        ("top", ctypes.c_long),
        ("right", ctypes.c_long),
        ("bottom", ctypes.c_long),
    ]


user32 = ctypes.windll.user32
user32.GetCursorPos.argtypes = [ctypes.POINTER(POINT)]
user32.GetCursorPos.restype = ctypes.c_bool
user32.WindowFromPoint.argtypes = [POINT]
user32.WindowFromPoint.restype = ctypes.c_void_p
user32.GetAncestor.argtypes = [ctypes.c_void_p, ctypes.c_uint]
user32.GetAncestor.restype = ctypes.c_void_p
user32.GetWindowRect.argtypes = [ctypes.c_void_p, ctypes.POINTER(RECT)]
user32.GetWindowRect.restype = ctypes.c_bool
user32.GetWindowTextLengthW.argtypes = [ctypes.c_void_p]
user32.GetWindowTextLengthW.restype = ctypes.c_int
user32.GetWindowTextW.argtypes = [ctypes.c_void_p, ctypes.c_wchar_p, ctypes.c_int]
user32.GetWindowTextW.restype = ctypes.c_int
user32.GetClassNameW.argtypes = [ctypes.c_void_p, ctypes.c_wchar_p, ctypes.c_int]
user32.GetClassNameW.restype = ctypes.c_int
user32.GetDlgCtrlID.argtypes = [ctypes.c_void_p]
user32.GetDlgCtrlID.restype = ctypes.c_int
@dataclass
class Anchor:
    prompt: str
    root_title: str
    relative_x: int
    relative_y: int
    child_class: str
    child_control_id: int
    screen_x: int
    screen_y: int


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Capture Inventory Transaction Entry field anchors.")
    parser.add_argument("--list-windows", action="store_true", help="List visible VMINVENT windows and exit.")
    return parser


def connect_app() -> Application:
    try:
        return Application(backend="win32").connect(path=PROCESS_NAME)
    except ElementNotFoundError as exc:
        raise RuntimeError(f"Process {PROCESS_NAME} is not running.") from exc


def get_window_text(handle: int) -> str:
    length = user32.GetWindowTextLengthW(handle)
    buffer = ctypes.create_unicode_buffer(max(length + 1, 256))
    user32.GetWindowTextW(handle, buffer, len(buffer))
    return buffer.value


def get_class_name(handle: int) -> str:
    buffer = ctypes.create_unicode_buffer(256)
    user32.GetClassNameW(handle, buffer, len(buffer))
    return buffer.value


def get_rect(handle: int) -> RECT:
    rect = RECT()
    if not user32.GetWindowRect(handle, ctypes.byref(rect)):
        raise RuntimeError(f"Failed to read RECT for handle 0x{handle:08X}.")
    return rect


def is_accepted_title(title: str) -> bool:
    return title in HOST_WINDOW_TITLES


def list_windows(app: Application) -> list[tuple[str, str, str, int]]:
    rows: list[tuple[str, str, str, int]] = []
    for window in app.windows():
        rect = window.rectangle()
        rows.append((window.window_text(), window.class_name(), str(rect), int(window.handle)))
    return rows


def activate_target_window(app: Application) -> None:
    candidates = [w for w in app.windows() if w.window_text() == TARGET_WINDOW_TITLE]
    if not candidates:
        candidates = [w for w in app.windows() if w.window_text() == "Inventory Transfers"]
    if not candidates:
        raise RuntimeError("Could not find an Inventory Transaction Entry host window.")

    candidates[0].set_focus()
    time.sleep(0.3)


def read_anchor(prompt: str, app: Application) -> Anchor:
    for attempt in range(1, MAX_ATTEMPTS + 1):
        print()
        print(prompt)
        print(f"Attempt {attempt} of {MAX_ATTEMPTS}")
        activate_target_window(app)
        input("Press Enter to start the countdown, then move the mouse over the requested target...")

        for seconds in range(CAPTURE_DELAY_SECONDS, 0, -1):
            print(f"Capturing in {seconds}...")
            time.sleep(1)

        point = POINT()
        if not user32.GetCursorPos(ctypes.byref(point)):
            raise RuntimeError("Failed to read the cursor position.")

        child_handle = int(user32.WindowFromPoint(point))
        root_handle = int(user32.GetAncestor(child_handle, GA_ROOT))
        root_title = get_window_text(root_handle)

        if not is_accepted_title(root_title):
            print(f"Captured '{root_title}' instead of one of {sorted(HOST_WINDOW_TITLES)}. Retrying...")
            continue

        root_rect = get_rect(root_handle)

        return Anchor(
            prompt=prompt,
            root_title=root_title,
            relative_x=point.x - root_rect.left,
            relative_y=point.y - root_rect.top,
            child_class=get_class_name(child_handle),
            child_control_id=int(user32.GetDlgCtrlID(child_handle)),
            screen_x=point.x,
            screen_y=point.y,
        )

    raise RuntimeError(f"Failed to capture '{prompt}' after {MAX_ATTEMPTS} attempts.")


def main(argv: Sequence[str]) -> int:
    args = build_parser().parse_args(argv)
    app = connect_app()

    if args.list_windows:
        for title, class_name, rect_text, handle in list_windows(app):
            print(repr(title), class_name, rect_text, f"0x{handle:08X}")
        return 0

    print("Inventory Transaction Entry layout discovery")
    print(f"Accepted window titles: {sorted(HOST_WINDOW_TITLES)}")
    print(f"Config output: {CONFIG_PATH}")

    anchors = {
        "PartIdField": read_anchor("1. Part ID field", app),
        "QuantityField": read_anchor("2. Quantity field", app),
        "FromWarehouseField": read_anchor("3. From Warehouse ID field", app),
        "FromLocationField": read_anchor("4. From Location ID field", app),
        "ToWarehouseField": read_anchor("5. To Warehouse ID field", app),
        "ToLocationField": read_anchor("6. To Location ID field", app),
        "SaveButton": read_anchor("7. Save button", app),
    }

    payload = {
        "process_name": PROCESS_NAME,
        "accepted_titles": sorted(HOST_WINDOW_TITLES),
        "created_at": time.strftime("%Y-%m-%dT%H:%M:%S"),
        "anchors": {name: asdict(anchor) for name, anchor in anchors.items()},
    }
    CONFIG_PATH.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    print()
    print(f"Saved config to {CONFIG_PATH}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
