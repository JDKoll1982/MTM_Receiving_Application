"""Render the Scanner walkthrough voiceover and mux it into the recording.

Reads the cue sheet (SRT) and the silent recording that sit in
docs/features/receiving/scanner-feature/walkthrough, synthesises one clip per cue,
lays each clip at its cue start, normalises loudness, then muxes the track into a
narrated copy of the video.

Requires: ffmpeg/ffprobe on PATH, and `pip install edge-tts`.

    python tools/scanner-voiceover/build_voiceover.py
    python tools/scanner-voiceover/build_voiceover.py en-US-BrianNeural brian
"""

import asyncio
import os
import re
import subprocess
import sys
import wave
from pathlib import Path

import edge_tts

REPO = Path(__file__).resolve().parents[2]
WALKTHROUGH = REPO / "docs" / "features" / "receiving" / "scanner-feature" / "walkthrough"

SRT = WALKTHROUGH / "Scanner_Entire_Location.srt"
VIDEO = WALKTHROUGH / "UpdatedVideoEmpty.mp4"
BUILD = WALKTHROUGH / ".tts-build"
SR = 48000

VOICE = sys.argv[1] if len(sys.argv) > 1 else "en-US-AndrewNeural"
SUFFIX = sys.argv[2] if len(sys.argv) > 2 else "andrew"


def parse_srt(path):
    raw = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
    cues = []
    for block in [b for b in raw.strip().split("\n\n") if b.strip()]:
        lines = block.split("\n")
        m = re.match(
            r"(\d+):(\d+):(\d+),(\d+)\s*-->\s*(\d+):(\d+):(\d+),(\d+)", lines[1]
        )
        sh, sm, ss, sms, eh, em, es, ems = (int(g) for g in m.groups())
        cues.append(
            {
                "index": int(lines[0].strip()),
                "start": sh * 3600 + sm * 60 + ss + sms / 1000,
                "end": eh * 3600 + em * 60 + es + ems / 1000,
                "text": " ".join(l.strip() for l in lines[2:]).strip(),
            }
        )
    return cues


async def synth_all(cues, voice, folder):
    folder.mkdir(parents=True, exist_ok=True)
    for cue in cues:
        mp3 = folder / f"cue_{cue['index']:02d}.mp3"
        if mp3.exists() and mp3.stat().st_size > 1000:
            continue
        last_error = None
        for attempt in range(4):
            try:
                await edge_tts.Communicate(cue["text"], voice).save(mp3)
                last_error = None
                break
            except Exception as exc:  # network hiccup: retry
                last_error = exc
                await asyncio.sleep(1.5 * (attempt + 1))
        if last_error is not None:
            raise RuntimeError(f"cue {cue['index']} failed: {last_error}")


def decode(mp3, wav_path):
    subprocess.run(
        ["ffmpeg", "-hide_banner", "-loglevel", "error", "-y", "-i", str(mp3),
         "-ac", "1", "-ar", str(SR), "-c:a", "pcm_s16le", str(wav_path)],
        check=True,
    )
    with wave.open(str(wav_path), "rb") as w:
        return w.readframes(w.getnframes()), w.getnframes() / w.getframerate()


def stretch(wav_path, out_path, tempo):
    subprocess.run(
        ["ffmpeg", "-hide_banner", "-loglevel", "error", "-y", "-i", str(wav_path),
         "-filter:a", f"atempo={tempo:.4f}", "-ac", "1", "-ar", str(SR),
         "-c:a", "pcm_s16le", str(out_path)],
        check=True,
    )
    with wave.open(str(out_path), "rb") as w:
        return w.readframes(w.getnframes()), w.getnframes() / w.getframerate()


def main():
    cues = parse_srt(SRT)
    video_duration = float(
        subprocess.run(
            ["ffprobe", "-v", "error", "-show_entries", "format=duration",
             "-of", "csv=p=0", str(VIDEO)],
            check=True, capture_output=True, text=True,
        ).stdout.strip()
    )

    folder = BUILD / SUFFIX
    asyncio.run(synth_all(cues, VOICE, folder))

    prepared = []
    for i, cue in enumerate(cues):
        wav = folder / f"cue_{cue['index']:02d}.wav"
        pcm, dur = decode(folder / f"cue_{cue['index']:02d}.mp3", wav)
        window = (cues[i + 1]["start"] if i + 1 < len(cues) else video_duration) - cue["start"]
        tempo = 1.0
        if dur > window - 0.15 and window > 0.5:
            tempo = min(dur / (window - 0.15), 1.18)
            pcm, dur = stretch(wav, folder / f"cue_{cue['index']:02d}_fit.wav", tempo)
        prepared.append({**cue, "pcm": pcm, "dur": dur, "window": window, "tempo": tempo})
        print(f"cue {cue['index']:02d}  start {cue['start']:6.2f}  speech {dur:6.2f}  "
              f"window {window:6.2f}  tempo {tempo:.3f}")

    total = int(round(max(video_duration, max(c["start"] + c["dur"] for c in prepared)) * SR))
    track = bytearray(total * 2)
    for cue in prepared:
        offset = int(round(cue["start"] * SR)) * 2
        data = cue["pcm"][: len(track) - offset]
        track[offset:offset + len(data)] = data

    out_wav = folder / "voiceover.wav"
    with wave.open(str(out_wav), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(bytes(track))

    out_audio = WALKTHROUGH / "Scanner_Entire_Location_voiceover.m4a"
    subprocess.run(
        ["ffmpeg", "-hide_banner", "-loglevel", "error", "-y", "-i", str(out_wav),
         "-af", "loudnorm=I=-16:TP=-1.5:LRA=11,aresample=48000", "-ar", "48000", "-ac", "2",
         "-c:a", "aac", "-b:a", "192k", str(out_audio)],
        check=True,
    )

    out_video = WALKTHROUGH / "Scanner_Entire_Location_narrated.mp4"
    subprocess.run(
        ["ffmpeg", "-hide_banner", "-loglevel", "error", "-y", "-i", str(VIDEO),
         "-i", str(out_audio), "-map", "0:v:0", "-map", "1:a:0",
         "-c:v", "copy", "-c:a", "copy", "-shortest", "-movflags", "+faststart",
         str(out_video)],
        check=True,
    )

    print(f"\nwrote {out_audio.name} and {out_video.name}  ({total / SR:.2f}s)")


if __name__ == "__main__":
    main()
