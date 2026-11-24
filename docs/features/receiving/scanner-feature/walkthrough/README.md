# Scanner — Entire Location walkthrough

`Scanner_Entire_Location.mp4` — the finished walkthrough of the Scanner workbench moving every part out
of one location in a single pass. **1920×1080, 30 fps, 1:35.17.**

- **The captions are burned into the picture**, so the video reads correctly in any player with the sound
  off. There is no voiceover.
- **Its audio track is silent** — every sample sits at the noise floor.
- **The take ends on an empty list**: four lines, sent one at a time, each confirmed in Infor Visual.

It is published in the in-app help at **Help Center → Scanner → The Workbench → Watch the Walkthrough**
(`docs/help/scanner/workbench.html#video`), which references this file directly, so there is only one
copy to keep current.

## What it shows

| Time | Beat |
| --- | --- |
| 0:00–0:17 | The Workbench at rest: **Search By** on **Location**, empty list, Infor Visual closed so the amber “Inventory Transactions window is closed!” notice shows beside a disabled **Send** |
| 0:17–0:21 | **Search By** flicks to **Part**, then back to **Location** |
| 0:21–0:24 | `RECV` typed in the Location box, **Enter** opens the *Parts at RECV* picker |
| 0:26–0:32 | **Select All**, then **Use Selected** — four lines land in the list |
| 0:37–0:50 | Destinations typed in the **To** boxes: `v-e0-02`, `t-00`, `s-00`, `s-00`, each formatted into an Infor Visual location ID |
| 0:51–0:59 | Infor Visual opened, Send live, first line filled in Inventory Transfers, save prompt answered **Yes** |
| 0:59–1:17 | The remaining three lines sent and confirmed; the list empties at 1:17 |
| 1:17–1:35 | Closing captions over the empty list |

Rows went out in the order **4, 1, 2, 3** — row 4 was the selected row when Send was clicked — which is
why the first “Line … cleared from the current list.” banner names line 4.

## Re-rendering the captions

The source cue sheet (`Scanner_Entire_Location.srt`) and the uncaptioned master were removed once this
cut was finished. To change the captions, restore both and re-run:

```powershell
python tools\scanner-voiceover\build_captions.py --mode burn
```

`build_captions.py` reads `UpdatedVideoEmpty.mp4` plus the SRT and writes the captioned MP4; it needs
libass (the ffmpeg `subtitles` filter) and will not run against this already-captioned file.