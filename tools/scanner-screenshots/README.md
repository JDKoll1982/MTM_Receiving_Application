# Scanner help screenshots

Turns raw Scanner screenshots into the annotated images that `docs/help/scanner/*.html` shows.

| Script | What it does |
| --- | --- |
| `annotate.py` | Crops each capture to the region that matters and draws numbered callouts, then writes the result into `docs/Tutorial_Images/Module_Scanner`. This is the step that produces the published images. |
| `capture.ps1` | Optional. Drives the running app with `winapp ui` and screenshots each Scanner page straight into the docs folder. Useful when you would rather not crop by hand. |

## The normal loop

1. Take the screenshots at 1920×1032 and drop them in `tools/scanner-screenshots/source/`
   (or the repository root — `annotate.py` checks both) with these names:

   | File | Screen |
   | --- | --- |
   | `Scanner-Workbench-Part.png` | Workbench, **Search By** on Part |
   | `Scanner-Workbench-Location.png` | Workbench, **Search By** on Location |
   | `Scanner-Workbench-Location-List.png` | *Parts at …* picker, nothing ticked |
   | `Scanner-Workbench-Location-List-Checked.png` | Same picker, first part ticked |
   | `Scanner-Workbench-Location-LineData.png` | One line in the list, **To** still blank |
   | `Scanner-History.png` | History page |
   | `Scanner-Settings.png` | Settings page |

2. Regenerate the annotated images:

   ```powershell
   python tools\scanner-screenshots\annotate.py
   ```

3. Check the result, then commit the changed PNGs. The HTML needs no edits — it points at stable
   filenames such as `View_Scanner_Workbench-Picker.png`.

Add `--preview` to write into `tools/scanner-screenshots/out/` instead of the docs folder, and
`--scene <name>` to redo a single image. `--list` prints every scene and its output path.

## Changing what a callout points at

Each scene in `annotate.py` names a source capture, an output path, and a list of `Callout(box, number)`
entries measured in **source-image pixels** against the 1920×1032 capture. A callout draws a rounded
rectangle plus a numbered badge; the badge sits on the left edge of its box, or exactly where you put it
with `badge=(x, y)` when that would cover something.

Cropped scenes (`crop=(left, top, right, bottom)`) keep the same absolute coordinates for their callouts —
the script subtracts the crop itself.

## Capturing the screens automatically

`capture.ps1` needs the app already running and signed in. It only navigates and screenshots: it never
sends a line to Infor Visual, never edits a quantity, and never answers a save prompt.

```powershell
powershell -ExecutionPolicy Bypass -File tools\scanner-screenshots\capture.ps1 -List
powershell -ExecutionPolicy Bypass -File tools\scanner-screenshots\capture.ps1
powershell -ExecutionPolicy Bypass -File tools\scanner-screenshots\capture.ps1 -All
```

Scenes flagged `live` read real Infor Visual data (the `RECV` location, part `MMC0000994`) and only run
with `-All` or by naming them. The captures land directly in `docs/Tutorial_Images/Module_Scanner`, so
they are already cropped to the window — run `annotate.py` afterwards if you want the callouts baked in.

## Requirements

- `pip install pillow` for `annotate.py`.
- `winapp` on `PATH` (Windows App SDK CLI) for `capture.ps1`.
- Arial from `C:\Windows\Fonts` for the badges.
