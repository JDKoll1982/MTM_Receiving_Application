"""Render the walkthrough captions onto the silent take instead of a voiceover.

Reads the cue sheet (SRT) and the silent recording, then either burns each cue into the
picture or attaches it as a selectable closed-caption track. No narration audio is used,
so the captions carry the whole message.

Requires: ffmpeg on PATH with libass (the `subtitles` filter) and the `mov_text` encoder.

    python tools/scanner-voiceover/build_captions.py            # burned-in captions
    python tools/scanner-voiceover/build_captions.py --mode soft # selectable CC track
    python tools/scanner-voiceover/build_captions.py --mode both
"""

import argparse
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
WALKTHROUGH = REPO / "docs" / "features" / "receiving" / "scanner-feature" / "walkthrough"

SRT = WALKTHROUGH / "Scanner_Entire_Location.srt"
VIDEO = WALKTHROUGH / "UpdatedVideoEmpty.mp4"
BURNED = WALKTHROUGH / "Scanner_Entire_Location_captioned.mp4"
SOFT = WALKTHROUGH / "Scanner_Entire_Location_cc.mp4"

# White Arial on a semi-transparent black box, sitting above the app's footer bar with side
# margins so a cue wraps to two or three short lines instead of spanning the frame.
STYLE = (
    "FontName=Arial,FontSize=11,PrimaryColour=&H00FFFFFF,OutlineColour=&HC8000000,"
    "BorderStyle=3,Outline=2,Shadow=0,Alignment=2,MarginV=52,MarginL=48,MarginR=48"
)


def filter_path(path):
    """Escape a Windows path for ffmpeg's filter graph (forward slashes, escaped colon)."""
    return str(path).replace("\\", "/").replace(":", "\\:")


def build_burned():
    subprocess.run(
        [
            "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
            "-i", str(VIDEO),
            "-vf", f"subtitles=filename='{filter_path(SRT)}':force_style='{STYLE}'",
            "-c:v", "libx264", "-preset", "medium", "-crf", "18", "-pix_fmt", "yuv420p",
            "-c:a", "copy", "-movflags", "+faststart", str(BURNED),
        ],
        check=True,
    )
    print(f"wrote {BURNED.name}")


def build_soft():
    subprocess.run(
        [
            "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
            "-i", str(VIDEO), "-i", str(SRT),
            "-map", "0:v:0", "-map", "0:a:0?", "-map", "1:0",
            "-c:v", "copy", "-c:a", "copy", "-c:s", "mov_text",
            "-metadata:s:s:0", "language=eng",
            "-metadata:s:s:0", "title=English",
            "-disposition:s:0", "default",
            "-movflags", "+faststart", str(SOFT),
        ],
        check=True,
    )
    print(f"wrote {SOFT.name}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--mode", choices=["burn", "soft", "both"], default="burn")
    args = parser.parse_args()

    if args.mode in ("burn", "both"):
        build_burned()
    if args.mode in ("soft", "both"):
        build_soft()


if __name__ == "__main__":
    main()
