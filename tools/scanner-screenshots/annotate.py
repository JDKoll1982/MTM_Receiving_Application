"""Annotate the raw Scanner screenshots into the images the help pages use.

The raw captures live in the repository root (Scanner-*.png, 1920x1032). Each scene below says
how to crop the capture and which callouts to draw on it, then writes the result into
docs/Tutorial_Images/Module_Scanner so docs/help/scanner/*.html can reference a stable path.

Re-run this after taking fresh captures - no hand-editing of the PNGs:

    python tools/scanner-screenshots/annotate.py
    python tools/scanner-screenshots/annotate.py --scene workbench-part --preview

Requires Pillow (`pip install pillow`), which the repo already uses for doc tooling.
"""

from __future__ import annotations

import argparse
from dataclasses import dataclass, field
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

REPO = Path(__file__).resolve().parents[2]
IMAGE_ROOT = REPO / "docs" / "Tutorial_Images" / "Module_Scanner"
PREVIEW_ROOT = Path(__file__).resolve().parent / "out"
SOURCE_ROOT = Path(__file__).resolve().parent / "source"

# Palette matching the app's dark theme so the marks read as part of the UI.
ACCENT = (79, 168, 255)        # accent blue - focus rectangles and badges
ACCENT_SOFT = (26, 62, 102)    # translucent fill behind a focus rectangle
WARN = (255, 185, 0)           # amber - things worth reading twice
GOOD = (76, 195, 138)          # green - the happy path
INK = (18, 22, 28)             # badge text
PANEL = (32, 37, 44)           # label chip background
WHITE = (245, 247, 250)

FONT_BOLD = Path("C:/Windows/Fonts/arialbd.ttf")
FONT_REGULAR = Path("C:/Windows/Fonts/arial.ttf")


def font(size: int, bold: bool = True) -> ImageFont.FreeTypeFont:
    path = FONT_BOLD if bold else FONT_REGULAR
    return ImageFont.truetype(str(path), size)


@dataclass
class Callout:
    """A numbered marker plus the rectangle it points at, in source-image pixels."""

    box: tuple[int, int, int, int]
    number: int
    color: tuple[int, int, int] = ACCENT
    radius: int = 10
    # Override the badge position (source pixels, top-left of the 30px circle) when the default
    # left-edge placement would sit on top of another control.
    badge: tuple[int, int] | None = None


@dataclass
class Scene:
    name: str
    source: str
    output: str
    # Crop applied before annotating, in source pixels. None keeps the whole capture.
    crop: tuple[int, int, int, int] | None = None
    callouts: list[Callout] = field(default_factory=list)
    # A soft dark veil everywhere except these boxes, used to pull the eye to one region.
    focus: tuple[int, int, int, int] | None = None
    caption: str | None = None


# --- Shared geometry -------------------------------------------------------
# Measurements taken from the 1920x1032 captures: the page header, the segmented page buttons,
# the Search By switch beside them, the entry row, the table header, and the footer action bar.
HEADER = (345, 112, 1877, 190)
PAGE_BUTTONS = (345, 196, 585, 232)
SEARCH_BY = (590, 196, 770, 232)
ENTRY_LABEL = (356, 254, 476, 282)
ENTRY_BOX = (356, 286, 1866, 332)
TABLE_HEADER = (345, 336, 1877, 364)
FOOTER = (352, 926, 1877, 996)
FOOTER_LEFT = (362, 930, 900, 992)
FOOTER_AUTO = (455, 930, 900, 992)
FOOTER_RIGHT = (1655, 940, 1862, 984)
SINGLE_LOCATION = (352, 292, 520, 326)
BANNER = (345, 142, 1877, 190)

SCENES: list[Scene] = [
    # -------- Workbench, part-number mode --------
    Scene(
        name="workbench-part",
        source="Scanner-Workbench-Part.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-Part.png",
        callouts=[
            Callout(HEADER, 1),
            Callout(PAGE_BUTTONS, 2),
            Callout(SEARCH_BY, 3, badge=(604, 158)),
            Callout(ENTRY_BOX, 4),
            Callout(TABLE_HEADER, 5),
            Callout(FOOTER, 6),
        ],
    ),
    Scene(
        name="workbench-part-entry",
        source="Scanner-Workbench-Part.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-Part-Entry.png",
        crop=(300, 186, 1900, 340),
        callouts=[
            Callout(PAGE_BUTTONS, 1),
            Callout(SEARCH_BY, 2, badge=(604, 158)),
            Callout(ENTRY_LABEL, 3),
            Callout(ENTRY_BOX, 4),
        ],
    ),
    # -------- Workbench, location mode --------
    Scene(
        name="workbench-location",
        source="Scanner-Workbench-Location.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-Location.png",
        callouts=[
            Callout(SEARCH_BY, 1, badge=(604, 158)),
            Callout(SINGLE_LOCATION, 2),
            Callout(ENTRY_BOX, 3),
            Callout(TABLE_HEADER, 4),
        ],
    ),
    # -------- Parts-at-location picker --------
    Scene(
        name="workbench-picker",
        source="Scanner-Workbench-Location-List.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-Picker.png",
        callouts=[
            Callout((496, 224, 900, 258), 1),
            Callout((498, 282, 604, 313), 2),
            Callout((1035, 342, 1395, 388), 3),
            Callout((498, 770, 1420, 814), 4),
        ],
    ),
    Scene(
        name="workbench-picker-checked",
        source="Scanner-Workbench-Location-List-Checked.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-Picker-Checked.png",
        callouts=[
            Callout((520, 342, 1410, 390), 1, color=GOOD),
            Callout((498, 770, 1420, 814), 2),
        ],
    ),
    # -------- Current list, one line waiting for a destination --------
    # This capture carries the info banner, which pushes everything down by one row height.
    Scene(
        name="workbench-list",
        source="Scanner-Workbench-Location-LineData.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-List.png",
        callouts=[
            Callout((345, 190, 1877, 230), 1, color=WARN),
            Callout((345, 372, 1877, 456), 2),
            Callout((455, 934, 900, 992), 3, color=WARN, badge=(410, 904)),
            Callout((1650, 934, 1866, 990), 4),
        ],
    ),
    Scene(
        name="workbench-action-bar",
        source="Scanner-Workbench-Location-LineData.png",
        output="View_Scanner_Workbench/View_Scanner_Workbench-ActionBar.png",
        crop=(340, 906, 1900, 1014),
        callouts=[
            Callout((360, 932, 900, 998), 1),
            Callout((908, 932, 1104, 998), 2),
            Callout((1648, 936, 1868, 994), 3),
        ],
    ),
    # -------- History --------
    Scene(
        name="history",
        source="Scanner-History.png",
        output="View_Scanner_History/View_Scanner_History-Main.png",
        callouts=[
            Callout(HEADER, 1),
            Callout((345, 194, 1877, 232), 2, color=WARN),
            Callout((345, 238, 585, 280), 3),
            Callout((345, 298, 1877, 358), 4),
            Callout((345, 380, 1877, 430), 5),
        ],
    ),
    Scene(
        name="history-filters",
        source="Scanner-History.png",
        output="View_Scanner_History/View_Scanner_History-Filters.png",
        crop=(346, 292, 1890, 362),
        callouts=[
            Callout((358, 296, 962, 356), 1),
            Callout((966, 296, 1536, 356), 2),
            Callout((1542, 296, 1702, 356), 3),
            Callout((1706, 296, 1848, 356), 4),
        ],
    ),
    # -------- Settings --------
    Scene(
        name="settings",
        source="Scanner-Settings.png",
        output="View_Scanner_Settings/View_Scanner_Settings-Main.png",
        callouts=[
            Callout(HEADER, 1),
            Callout((345, 192, 1877, 238), 2, color=WARN),
            Callout((345, 285, 1125, 735), 3),
            Callout((1165, 210, 1878, 945), 4),
            Callout((348, 952, 475, 1002), 5),
        ],
    ),
    Scene(
        name="settings-editor",
        source="Scanner-Settings.png",
        output="View_Scanner_Settings/View_Scanner_Settings-Editor.png",
        crop=(1120, 288, 1892, 945),
        callouts=[
            Callout((1180, 296, 1440, 326), 1),
            Callout((1180, 332, 1866, 878), 2),
            Callout((1180, 884, 1502, 930), 3),
        ],
    ),
]

def round_rect(draw: ImageDraw.ImageDraw, box, radius, **kwargs):
    draw.rounded_rectangle(box, radius=radius, **kwargs)


def draw_badge(draw: ImageDraw.ImageDraw, x: int, y: int, number: int, color, size: int = 30):
    draw.ellipse((x, y, x + size, y + size), fill=color, outline=(0, 0, 0), width=2)
    label = str(number)
    f = font(19)
    width = draw.textlength(label, font=f)
    draw.text((x + (size - width) / 2, y + size / 2 - 11), label, font=f, fill=INK)


def annotate(scene: Scene, preview: bool) -> Path:
    source = next(
        (candidate for candidate in (SOURCE_ROOT / scene.source, REPO / scene.source) if candidate.exists()),
        None,
    )
    if source is None:
        raise SystemExit(
            f"missing capture {scene.source} - put it in tools/scanner-screenshots/source/ "
            "or the repository root"
        )

    image = Image.open(source).convert("RGBA")
    offset_x, offset_y = 0, 0
    if scene.crop:
        left, top, right, bottom = scene.crop
        image = image.crop(scene.crop)
        offset_x, offset_y = left, top

    if scene.focus:
        veil = Image.new("RGBA", image.size, (0, 0, 0, 0))
        overlay = ImageDraw.Draw(veil)
        overlay.rectangle((0, 0, image.width, image.height), fill=(0, 0, 0, 130))
        fx1, fy1, fx2, fy2 = scene.focus
        overlay.rectangle(
            (fx1 - offset_x, fy1 - offset_y, fx2 - offset_x, fy2 - offset_y), fill=(0, 0, 0, 0)
        )
        image = Image.alpha_composite(image, veil)

    # Highlights first so the badges stay on top of them.
    layer = Image.new("RGBA", image.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)

    for callout in scene.callouts:
        x1, y1, x2, y2 = callout.box
        x1, y1, x2, y2 = x1 - offset_x, y1 - offset_y, x2 - offset_x, y2 - offset_y
        round_rect(
            draw,
            (x1, y1, x2, y2),
            callout.radius,
            outline=callout.color + (255,),
            width=3,
            fill=callout.color + (34,),
        )

    image = Image.alpha_composite(image, layer)
    draw = ImageDraw.Draw(image)

    for callout in scene.callouts:
        x1, y1, x2, y2 = callout.box
        x1, y1, x2, y2 = x1 - offset_x, y1 - offset_y, x2 - offset_x, y2 - offset_y
        if callout.badge:
            badge_x, badge_y = callout.badge[0] - offset_x, callout.badge[1] - offset_y
        # Sit the badge on the left edge, centred, so it never covers the label a box encloses.
        # When there is no room to the left, tuck it just inside the top-left corner instead.
        elif x1 >= 40:
            badge_x = x1 - 34
            badge_y = int((y1 + y2) / 2) - 15
        else:
            badge_x = max(x1 - 12, 2)
            badge_y = max(y1 - 12, 2)
        draw_badge(draw, badge_x, badge_y, callout.number, callout.color)

    if scene.caption:
        f = font(22)
        pad = 10
        width = draw.textlength(scene.caption, font=f)
        draw.rounded_rectangle(
            (12, image.height - 52, 12 + width + pad * 2, image.height - 12),
            radius=8,
            fill=PANEL + (235,),
            outline=ACCENT + (255,),
            width=2,
        )
        draw.text((12 + pad, image.height - 46), scene.caption, font=f, fill=WHITE)

    root = PREVIEW_ROOT if preview else IMAGE_ROOT
    target = root / scene.output
    target.parent.mkdir(parents=True, exist_ok=True)
    image.convert("RGB").save(target, "PNG", optimize=True)
    return target


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--scene", action="append", help="Annotate only these scenes.")
    parser.add_argument(
        "--preview",
        action="store_true",
        help="Write into tools/scanner-screenshots/out instead of the docs image folder.",
    )
    parser.add_argument("--list", action="store_true", help="List scene names and outputs.")
    args = parser.parse_args()

    if args.list:
        for scene in SCENES:
            print(f"{scene.name:28} {scene.source:44} -> {scene.output}")
        return

    selected = [s for s in SCENES if not args.scene or s.name in args.scene]
    if not selected:
        raise SystemExit("no scenes matched")

    for scene in selected:
        print(f"wrote {annotate(scene, args.preview)}")


if __name__ == "__main__":
    main()
