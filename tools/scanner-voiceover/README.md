# Scanner walkthrough caption and voice tooling

Scripts that build and check the captioned Scanner walkthrough in
`docs/features/receiving/scanner-feature/walkthrough`. All resolve their paths from the repository root,
so they run from anywhere.

| Script | Purpose |
| --- | --- |
| `build_captions.py` | Reads `Scanner_Entire_Location.srt` and burns the cues into `UpdatedVideoEmpty.mp4`, or attaches them as a selectable `mov_text` track. **This is the current build.** |
| `build_voiceover.py` | The earlier narrated pass: synthesises one clip per cue with `edge-tts`, lays each clip at its cue start, normalises to −16 LUFS, and muxes the track into the video. Not used for the captioned deliverable. |
| `verify_fit.py` | Finds each spoken line in a finished track by silence detection and reports its spare margin. Useful only for a narrated build. |

## Requirements

- `ffmpeg` and `ffprobe` on `PATH`; the caption build needs libass (the `subtitles` filter) and the
  `mov_text` encoder.
- `pip install edge-tts` — only for `build_voiceover.py`.

## Usage

```powershell
python tools\scanner-voiceover\build_captions.py              # burned-in captions
python tools\scanner-voiceover\build_captions.py --mode soft  # selectable CC track only
python tools\scanner-voiceover\build_captions.py --mode both  # both files

python tools\scanner-voiceover\build_voiceover.py [voice] [suffix]
python tools\scanner-voiceover\verify_fit.py [--srt PATH] [--narrated PATH] [--min-spare SECONDS]
```

Caption outputs are `Scanner_Entire_Location_captioned.mp4` (burned in) and
`Scanner_Entire_Location_cc.mp4` (selectable track). Scratch audio from the voice build lands in
`walkthrough/.tts-build/`, which is git-ignored and safe to delete.

## Caption style

`build_captions.py` renders white Arial on a semi-transparent black box, centred above the app's footer
bar (`MarginV=52` in libass script units, `MarginL/MarginR=48`). Keep each cue to two short lines; split
a long statement across cues instead of shrinking the type.