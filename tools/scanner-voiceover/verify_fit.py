"""Measure every voiceover line against the time it has before the next line starts.

Reads the cue sheet (SRT) and the finished narrated video, finds each spoken line by
silence detection, and reports the spare margin. Exits non-zero if any line overruns
its window by less than the required margin.

    python tools/scanner-voiceover/verify_fit.py
    python tools/scanner-voiceover/verify_fit.py --narrated path\\to\\video.mp4 --min-spare 0.5
"""

import argparse
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
WALKTHROUGH = REPO / "docs" / "features" / "receiving" / "scanner-feature" / "walkthrough"

DEFAULT_SRT = WALKTHROUGH / "Scanner_Entire_Location.srt"
DEFAULT_AUDIO = WALKTHROUGH / "Scanner_Entire_Location_narrated.mp4"


def parse_srt(path):
    raw = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
    cues = []
    for block in [b for b in raw.strip().split("\n\n") if b.strip()]:
        lines = block.split("\n")
        m = re.match(r"(\d+):(\d+):(\d+),(\d+)\s*-->\s*(\d+):(\d+):(\d+),(\d+)", lines[1])
        sh, sm, ss, sms, eh, em, es, ems = (int(g) for g in m.groups())
        cues.append(
            {
                "n": int(lines[0].strip()),
                "start": sh * 3600 + sm * 60 + ss + sms / 1000,
                "end": eh * 3600 + em * 60 + es + ems / 1000,
                "text": " ".join(l.strip() for l in lines[2:]),
            }
        )
    return cues


def probe_duration(path):
    return float(
        subprocess.run(
            ["ffprobe", "-v", "error", "-show_entries", "format=duration",
             "-of", "csv=p=0", str(path)],
            check=True, capture_output=True, text=True,
        ).stdout.strip()
    )


def speech_intervals(path, noise="-45dB", min_silence=0.15):
    log = subprocess.run(
        ["ffmpeg", "-hide_banner", "-nostats", "-i", str(path), "-map", "0:a:0",
         "-af", f"silencedetect=noise={noise}:d={min_silence}", "-f", "null", "NUL"],
        capture_output=True, text=True, errors="ignore",
    ).stderr
    starts = [float(x) for x in re.findall(r"silence_start: ([\d.]+)", log)]
    ends = [float(x) for x in re.findall(r"silence_end: ([\d.]+)", log)]
    total = probe_duration(path)

    silences = [(s, ends[i] if i < len(ends) else total) for i, s in enumerate(starts)]

    intervals, cursor = [], 0.0
    for s, e in silences:
        if s > cursor + 0.01:
            intervals.append((cursor, s))
        cursor = max(cursor, e)
    if total > cursor + 0.01:
        intervals.append((cursor, total))
    return intervals, total


def overlap(a1, a2, b1, b2):
    return max(0.0, min(a2, b2) - max(a1, b1))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--srt", default=str(DEFAULT_SRT))
    parser.add_argument("--narrated", default=str(DEFAULT_AUDIO))
    parser.add_argument("--min-spare", type=float, default=0.25)
    args = parser.parse_args()

    cues = parse_srt(Path(args.srt))
    intervals, total = speech_intervals(Path(args.narrated))

    print(f"{'#':>3} {'starts':>8} {'window':>7} {'audio':>7} {'spare':>7}  verdict")
    print("-" * 56)
    failures = 0
    tightest = None
    for i, cue in enumerate(cues):
        end = cues[i + 1]["start"] if i + 1 < len(cues) else total
        window = end - cue["start"]
        spoken = sum(overlap(cue["start"], end, a, b) for a, b in intervals)
        spare = window - spoken
        ok = spare >= args.min_spare
        if not ok:
            failures += 1
        if tightest is None or spare < tightest[1]:
            tightest = (cue["n"], spare)
        print(f"{cue['n']:>3} {cue['start']:>7.2f}s {window:>7.2f} {spoken:>7.2f} {spare:>7.2f}  "
              f"{'PASS' if ok else 'FAIL'}")

    print("-" * 56)
    print(f"cues: {len(cues)}   audio track: {total:.2f}s   failures: {failures}")
    print(f"tightest margin: cue {tightest[0]} with {tightest[1]:.2f}s spare")
    print(f"total silence: {total - sum(b - a for a, b in intervals):.2f}s")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
