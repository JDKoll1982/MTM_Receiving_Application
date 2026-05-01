"""Populate Inventory Transaction Entry without keyboard commands.

This script uses anchor metadata created by
``discover_inventory_transaction_entry_layout.py``. It resolves the live
controls nearest those anchors and writes directly to the edit controls with
``set_edit_text``. If a part selection dialog appears, the script selects the
first row and clicks an OK-style button.

Usage:
    .\\.venv32\\Scripts\\python.exe Database\\InforVisualScripts\\Python_Scripts\\set_inventory_transaction_entry.py --dry-run
    .\\.venv32\\Scripts\\python.exe Database\\InforVisualScripts\\Python_Scripts\\set_inventory_transaction_entry.py
"""

from __future__ import annotations

import argparse
import ctypes
import json
import math
import sys
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Sequence

from pywinauto import Application
from pywinauto.keyboard import send_keys
from pywinauto.controls.hwndwrapper import HwndWrapper
from pywinauto.findwindows import ElementNotFoundError


PROCESS_NAME = "VMINVENT.EXE"
TARGET_WINDOW_TITLE = "Inventory Transaction Entry - Infor VISUAL - MTMFG"
HOST_WINDOW_TITLES = {TARGET_WINDOW_TITLE, "Inventory Transfers"}
PART_SELECTION_WINDOW_TITLES = {"Parts"}
IGNORED_SECONDARY_WINDOW_TITLES = {"Gupta Runtime"}
IGNORED_SECONDARY_WINDOW_CLASSES = {
    "AfxFrameOrView110u",
    "ComboLBox",
    "tooltips_class32",
    "ProfUIS-NCSB_ScrollContainer",
    "MSCTFIME UI",
    "IME",
}
CONFIG_PATH = Path(__file__).with_name("inventory_transaction_entry_layout.json")

# Edit these values before running the script.
PART_NUMBER = "21-28841-"
QUANTITY = "5082"
FROM_WAREHOUSE_ID = "002"
FROM_LOCATION_ID = "V-B0-29"
TO_WAREHOUSE_ID = "002"
TO_LOCATION_ID = "R-05"
CLICK_SAVE = False


class POINT(ctypes.Structure):
    _fields_ = [("x", ctypes.c_long), ("y", ctypes.c_long)]


class GUITHREADINFO(ctypes.Structure):
    _fields_ = [
        ("cbSize", ctypes.c_uint),
        ("flags", ctypes.c_uint),
        ("hwndActive", ctypes.c_void_p),
        ("hwndFocus", ctypes.c_void_p),
        ("hwndCapture", ctypes.c_void_p),
        ("hwndMenuOwner", ctypes.c_void_p),
        ("hwndMoveSize", ctypes.c_void_p),
        ("hwndCaret", ctypes.c_void_p),
        ("rcCaret_left", ctypes.c_long),
        ("rcCaret_top", ctypes.c_long),
        ("rcCaret_right", ctypes.c_long),
        ("rcCaret_bottom", ctypes.c_long),
    ]


user32 = ctypes.windll.user32
user32.WindowFromPoint.argtypes = [POINT]
user32.WindowFromPoint.restype = ctypes.c_void_p
user32.BlockInput.argtypes = [ctypes.c_bool]
user32.BlockInput.restype = ctypes.c_bool
user32.GetGUIThreadInfo.argtypes = [ctypes.c_uint, ctypes.POINTER(GUITHREADINFO)]
user32.GetGUIThreadInfo.restype = ctypes.c_bool


@dataclass
class Anchor:
    name: str
    relative_x: int
    relative_y: int
    child_class: str
    child_control_id: int
    root_title: str


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Populate Inventory Transaction Entry fields directly."
    )
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--save", action="store_true", help="Click Save after filling the fields.")
    return parser


def load_config() -> dict:
    if not CONFIG_PATH.exists():
        raise RuntimeError(
            f"Layout config not found at {CONFIG_PATH}. Run the discovery script first."
        )
    return json.loads(CONFIG_PATH.read_text(encoding="utf-8"))


def connect_app() -> Application:
    try:
        return Application(backend="win32").connect(path=PROCESS_NAME)
    except ElementNotFoundError as exc:
        raise RuntimeError(f"Process {PROCESS_NAME} is not running.") from exc


def is_accepted_title(title: str) -> bool:
    return title in HOST_WINDOW_TITLES


def resolve_window(app: Application, preferred_title: str) -> HwndWrapper:
    exact_matches = [w for w in app.windows() if w.window_text() == preferred_title]
    if exact_matches:
        return exact_matches[0]

    fallback = [w for w in app.windows() if is_accepted_title(w.window_text())]
    if fallback:
        direct = [w for w in fallback if w.window_text() == TARGET_WINDOW_TITLE]
        if direct:
            return direct[0]
        return fallback[0]

    raise RuntimeError(f"Could not find a live Inventory Transaction Entry host window for {preferred_title!r}.")


def iter_visible_descendants(window) -> Iterable[HwndWrapper]:
    for child in window.descendants():
        rect = child.rectangle()
        if rect.width() > 0 and rect.height() > 0:
            yield child


def resolve_anchor_control(app: Application, anchor_name: str, anchor_data: dict) -> HwndWrapper:
    anchor = Anchor(
        name=anchor_name,
        relative_x=int(anchor_data["relative_x"]),
        relative_y=int(anchor_data["relative_y"]),
        child_class=str(anchor_data["child_class"]),
        child_control_id=int(anchor_data["child_control_id"]),
        root_title=str(anchor_data["root_title"]),
    )
    window = resolve_window(app, anchor.root_title)
    window_rect = window.rectangle()
    target_x = window_rect.left + anchor.relative_x
    target_y = window_rect.top + anchor.relative_y

    candidates = []
    for child in iter_visible_descendants(window):
        if child.class_name() != anchor.child_class:
            continue
        if child.control_id() != anchor.child_control_id:
            continue
        rect = child.rectangle()
        center_x = rect.left + int(rect.width() / 2)
        center_y = rect.top + int(rect.height() / 2)
        distance = math.hypot(center_x - target_x, center_y - target_y)
        candidates.append((distance, child))

    if candidates:
        candidates.sort(key=lambda item: item[0])
        return candidates[0][1]

    point = POINT(target_x, target_y)
    handle = int(user32.WindowFromPoint(point))
    if handle:
        return HwndWrapper(handle)

    raise RuntimeError(f"Could not resolve control for anchor {anchor_name!r}.")


def resolve_edit_target(control: HwndWrapper) -> HwndWrapper:
    if control.class_name() == "Edit":
        return control

    for child in control.children():
        if child.class_name() == "Edit":
            return child

    for child in control.descendants():
        if child.class_name() == "Edit":
            return child

    raise RuntimeError(
        f"Control {control.class_name()} with id {control.control_id()} does not expose an Edit child."
    )


def set_field_value(control: HwndWrapper, value: str) -> None:
    target = resolve_edit_target(control)
    target.set_focus()
    target.set_edit_text(value)


def focus_field(control: HwndWrapper) -> None:
    target = resolve_edit_target(control)
    target.set_focus()
    time.sleep(0.15)


def click_button(control: HwndWrapper) -> None:
    try:
        control.click()
    except Exception:
        control.click_input()
    time.sleep(0.15)


def set_user_input_blocked(blocked: bool) -> bool:
    try:
        return bool(user32.BlockInput(blocked))
    except Exception:
        return False


def get_focused_handle() -> int:
    info = GUITHREADINFO()
    info.cbSize = ctypes.sizeof(GUITHREADINFO)
    if not user32.GetGUIThreadInfo(0, ctypes.byref(info)):
        return 0
    return int(info.hwndFocus or 0)


def wait_for_focus_loss(control: HwndWrapper, timeout_seconds: float = 5.0) -> None:
    target_handle = int(resolve_edit_target(control).handle)
    deadline = time.time() + timeout_seconds
    while time.time() < deadline:
        if get_focused_handle() != target_handle:
            return
        time.sleep(0.05)


def get_secondary_windows(app: Application, main_handles: set[int]) -> list[HwndWrapper]:
    dialogs: list[HwndWrapper] = []
    for window in app.windows():
        if int(window.handle) in main_handles:
            continue

        if window.window_text().strip() in IGNORED_SECONDARY_WINDOW_TITLES:
            continue

        if window.class_name() in IGNORED_SECONDARY_WINDOW_CLASSES:
            continue

        rect = window.rectangle()
        if rect.width() <= 0 or rect.height() <= 0:
            continue
        dialogs.append(window)
    return dialogs


def maybe_pause_for_blocking_dialog(
    app: Application,
    main_handles: set[int],
    context: str,
    *,
    stop_on_detect: bool = True,
) -> bool:
    dialogs = get_secondary_windows(app, main_handles)
    for dialog in dialogs:
        raw_title = dialog.window_text().strip()
        if not raw_title:
            continue

        title = raw_title
        class_names = {child.class_name() for child in dialog.descendants()}

        if "ListBox" in class_names or "SysListView32" in class_names or "Gupta:ChildTable" in class_names:
            continue

        messages = []
        for child in dialog.descendants():
            if child.class_name() == "Static":
                text = child.window_text().strip()
                if text:
                    messages.append(text)

        print()
        print(f"Blocking dialog detected during {context}: {title}")
        if messages:
            print("Dialog text:")
            for message in messages:
                print(f"  {message}")

        if stop_on_detect:
            raise RuntimeError(
                f"Blocking dialog detected during {context}. Script stopped before continuing."
            )

        return True

    return False


def maybe_handle_part_dialog(app: Application, main_handles: set[int], timeout_seconds: float = 5.0) -> None:
    deadline = time.time() + timeout_seconds
    while time.time() < deadline:
        dialogs = get_secondary_windows(app, main_handles)

        if not dialogs:
            time.sleep(0.15)
            continue

        part_dialogs = [dialog for dialog in dialogs if dialog.window_text().strip() in PART_SELECTION_WINDOW_TITLES]
        if not part_dialogs:
            time.sleep(0.15)
            continue

        dialog = part_dialogs[0]
        list_like = None
        for child in dialog.descendants():
            if child.class_name() in {"ListBox", "SysListView32", "Gupta:ChildTable"}:
                list_like = child
                break

        dialog.set_focus()
        time.sleep(0.2)

        input_blocked = False
        try:
            input_blocked = set_user_input_blocked(True)
            time.sleep(0.1)
            send_keys("{UP}{ENTER}", pause=0.05)
            time.sleep(0.3)
        finally:
            if input_blocked:
                set_user_input_blocked(False)

        close_deadline = time.time() + 5.0
        while time.time() < close_deadline:
            remaining_dialogs = get_secondary_windows(app, main_handles)
            if all(int(window.handle) != int(dialog.handle) for window in remaining_dialogs):
                return
            time.sleep(0.15)

        raise RuntimeError(
            "The part selection window did not close after sending Up and Enter."
        )

    return


def main(argv: Sequence[str]) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    config = load_config()
    app = connect_app()

    anchors = config["anchors"]
    controls = {
        name: resolve_anchor_control(app, name, data) for name, data in anchors.items()
    }

    print("Resolved controls for Inventory Transaction Entry:")
    for name, control in controls.items():
        print(f"{name}: class={control.class_name()} id={control.control_id()} rect={control.rectangle()}")

    if args.dry_run:
        print("Dry run: control resolution succeeded.")
        return 0

    main_handles = {
        int(resolve_window(app, title).handle)
        for title in HOST_WINDOW_TITLES
        if any(window.window_text() == title for window in app.windows())
    }

    if PART_NUMBER:
        set_field_value(controls["PartIdField"], PART_NUMBER)
        focus_field(controls["QuantityField"])
        maybe_handle_part_dialog(app, main_handles)
        maybe_pause_for_blocking_dialog(app, main_handles, "part selection")

    if QUANTITY:
        set_field_value(controls["QuantityField"], QUANTITY)
        maybe_pause_for_blocking_dialog(app, main_handles, "quantity entry", stop_on_detect=True)

    if FROM_WAREHOUSE_ID:
        set_field_value(controls["FromWarehouseField"], FROM_WAREHOUSE_ID)
        maybe_pause_for_blocking_dialog(app, main_handles, "from warehouse entry")

    if FROM_LOCATION_ID:
        set_field_value(controls["FromLocationField"], FROM_LOCATION_ID)
        maybe_pause_for_blocking_dialog(app, main_handles, "from location entry", stop_on_detect=True)

    if TO_WAREHOUSE_ID:
        set_field_value(controls["ToWarehouseField"], TO_WAREHOUSE_ID)
        maybe_pause_for_blocking_dialog(app, main_handles, "to warehouse entry")

    if TO_LOCATION_ID:
        set_field_value(controls["ToLocationField"], TO_LOCATION_ID)
        maybe_pause_for_blocking_dialog(app, main_handles, "to location entry", stop_on_detect=True)

    if CLICK_SAVE or args.save:
        maybe_pause_for_blocking_dialog(app, main_handles, "pre-save validation")
        click_button(controls["SaveButton"])
        print("Fields populated and Save clicked.")
    else:
        print("Fields populated. Save was not clicked.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))