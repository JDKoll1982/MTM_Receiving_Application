# Packing List Scan Probe

Standalone console tool that reads a scanned packing list / bill of lading and prints the
receiving-relevant fields it can find. Built to evaluate scan-to-receive capture before any of
it is wired into the WinUI application.

Status: **probe only**. It is not referenced by `MTM_Receiving_Application.slnx`, it is
excluded from the app project's compile globs, and nothing in the app calls it yet.

## Why this exists

`docling-parse` was evaluated first and rejected for this use case. It is a programmatic-PDF
object parser, not an OCR engine: on the DALCO scan it extracted **0 characters** because the
file contains no fonts, only one CCITT image. Full `docling` with OCR, and the Granite-Docling
VLM, were also measured and are documented in [Appendix A](#appendix-a--why-not-docling).

What is left is the in-box Windows OCR stack, which needs no install, no Python, no cloud call,
and no NuGet package.

## What the tool uses

| Piece | API | Notes |
| --- | --- | --- |
| OCR | `Windows.Media.Ocr.OcrEngine` | in-box, offline, no NuGet; max image dimension 10000 px |
| PDF rasterizing | `Windows.Data.Pdf.PdfDocument` | in-box; PDFs are rendered to bitmaps and then OCR'd |
| Image decoding | `Windows.Graphics.Imaging.BitmapDecoder` | JPEG, PNG, TIFF, BMP; EXIF rotation respected |

No third-party dependency is used, so there is nothing to `restore` beyond the SDK itself.

## Measured results on the sample set

Samples live in [MTM_Receiving_Application.Tests/Assets/PackingListSamples/](MTM_Receiving_Application.Tests/Assets/PackingListSamples).

| Sample | Actual document type | Words read | Fields recovered |
| --- | --- | --- | --- |
| `SProd Offic26100207250_0001.jpg` | Internal email screenshot (material transfer) | 137 | parts `MMC0000659`, `MMC0000767`; PO `069993`; all five quantities |
| `SProd Offic26100207250_0002.jpg` | Infor Visual screenshot (warehouse locations) | 75 | part `MMC0000659`; warehouse `002`; 7 location quantities; date `10/2/2026` |
| `SProd Offic26100610400.jpg` | Material Availability Transaction Sheet | 102 | parts `MMC0001153`, `MMF0005114`, `T68048`; quantities `10580`, `1406`, `8500` |
| `SProd Offic26102012520.pdf` | DALCO Metals bill of lading (copier scan) | 371 | **none** — see below |

Every field above was correct. Timing on the development machine: about 250 ms for a
1700x2200 JPEG, about 2.6 s per PDF page at 400 dpi.

### The bill-of-lading failure, in plain terms

That page is a halftone bilevel scan of faint dot-matrix print: broken glyph strokes over a
stipple background. The header and the boilerplate come through; the line-item table does not.
Median denoise, autocontrast, 2x upscaling and bilevel thresholding were all tried, alone and
combined, and none of them recovered the table. Two independent engines agree on this file, so
treat it as a capture-quality limit rather than a parser problem.

Practical consequence: a scanner capture has to be around 300 dpi grayscale or better, from a
document scan mode, not a copier's photo/fax mode. A phone photo of a clean printout beats a
halftone copier scan.

## Requirements on the test PC

- Windows 10 version 1903 or later, or Windows 11. x64.
- .NET 10 SDK.
- An English OCR language pack. The probe uses `en-US` and falls back to the user profile
  languages.

Administrator rights are needed only for the SDK and language pack installs, not for running
the tool.

## Step 1 — Install the .NET 10 SDK

```powershell
winget install Microsoft.DotNet.SDK.10
```

Verify, then open a new terminal so `PATH` refreshes:

```powershell
dotnet --version
```

## Step 2 — Verify the OCR language pack

List the OCR capabilities, as administrator:

```powershell
Get-WindowsCapability -Online | Where-Object { $_.Name -like 'Language.OCR*' }
```

Only rows with `State : Installed` are usable. Install `en-US` if needed, as administrator:

```powershell
$capability = Get-WindowsCapability -Online | Where-Object { $_.Name -like 'Language.OCR*en-US*' }
$capability | Add-WindowsCapability -Online
```

Confirm the engine actually sees it, in Windows PowerShell (not PowerShell 7):

```powershell
[Windows.Media.Ocr.OcrEngine, Windows.Foundation, ContentType = WindowsRuntime]
[Windows.Media.Ocr.OcrEngine]::AvailableRecognizerLanguages
```

If the list is empty you will get `No OCR engine available` from the probe. On images where the
system drive is not `C:`, copy `X:\Windows\OCR` to `C:\Windows\OCR` and re-check.

## Step 3 — Get the files onto the test PC

Either clone the repository:

```powershell
git clone https://github.com/JDKoll1982/MTM_Receiving_Application.git
cd MTM_Receiving_Application
```

or copy just [tools/PackingListScanProbe/](tools/PackingListScanProbe) plus the sample folder
over by hand. Nothing else is required.

## Step 4 — Build

```powershell
dotnet build .\tools\PackingListScanProbe\PackingListScanProbe.csproj -c Release
```

The executable lands in:

```text
tools\PackingListScanProbe\bin\x64\Release\net10.0-windows10.0.19041.0\PackingListScanProbe.exe
```

Being a scratch tool, it is deliberately outside the solution, so `dotnet build` on the
solution will not build it.

## Step 5 — Run it

Three modes. Quote paths that contain spaces.

### Structured fields (the one you usually want)

```powershell
$exe = ".\tools\PackingListScanProbe\bin\x64\Release\net10.0-windows10.0.19041.0\PackingListScanProbe.exe"
& $exe extract ".\MTM_Receiving_Application.Tests\Assets\PackingListSamples\SProd Offic26100610400.jpg"
```

`extract` prints one JSON object for a single input, or an array when several inputs are given.
Warnings are written to stderr, so they still appear in the console when you redirect stdout to a
file:

```powershell
$files = Get-ChildItem -Path .\scans\*.jpg, .\scans\*.pdf
$files | ForEach-Object { & $exe extract $_.FullName } > results.json
```

PowerShell does not expand wildcards for native commands, and a comma-separated array passed
directly to the exe is not split into separate arguments, so use `Get-ChildItem` as shown.

### Readable transcript

```powershell
& $exe text ".\scans\bol.pdf"
```

Prints each OCR line with its page position. This is the fastest way to judge whether a scan is
readable at all before trusting the extractor.

### Debug dump

```powershell
& $exe dump ".\scans\bol.pdf" --crop 0,0.2,0.6,0.6
```

Prints every word with x/y/width/height. Use `--crop` to restrict to a region and cut noise from
logos, terms and signatures.

### Options

| Option | Default | Purpose |
| --- | --- | --- |
| `--dpi N` | `400` | PDF rasterization resolution |
| `--scale N` | `1` | multiplier for image inputs; ignored by PDFs |
| `--page N[,N]` | all | restrict to specific PDF pages, 1-based |
| `--crop l,t,r,b` | none | crop as fractions of width/height, e.g. `0,0.15,0.65,0.6` |

You can also skip the build step:

```powershell
dotnet run --project .\tools\PackingListScanProbe -- extract ".\scans\bol.pdf"
```

The `--` before the arguments is required.

### One PowerShell quirk to know

Invoking the tool by a bare relative path that starts with `tools` fails with
`The module 'tools' could not be loaded`. Always prefix with `.\`, or use the `$exe` variable
shown above.

## Step 6 — Try your own scans

Drop the files anywhere and pass paths. Supported: `.pdf`, `.jpg`, `.jpeg`, `.png`, `.tif`,
`.tiff`, `.bmp`. PDFs are read page by page; a multi-page TIFF is read as its first frame only.

To judge a capture, watch `WordCount` and the warnings:

| Signal | Meaning |
| --- | --- |
| `WordCount` under 20 | capture is unusable; re-scan |
| `Warnings: no part number found` | text came through but no part token was recognized |
| `Warnings: no purchase order found` | normal for internal screenshots and sheets; suspicious on a BOL |
| `Quantities` contains years or phone numbers | expected today; see the limitations |

Suggested experiment matrix for the weekend, one row per document:

1. Copier scan at default settings, saved as PDF.
2. Same page as a 300 dpi grayscale JPEG.
3. Same page photographed with a phone, plain background, no flash.
4. Same page at 600 dpi to compare against 400.

Record `WordCount`, `Kind`, the parts and POs found, and the timing. That tells us the capture
floor for the real feature.

## Reading the output

```json
{
  "Source": "SProd Offic26100610400.jpg",
  "Page": "",
  "WordCount": 102,
  "Kind": "MaterialTransactionSheet",
  "Parts": ["MMC0001153", "MMF0005114", "T68048"],
  "PurchaseOrders": [],
  "Quantities": ["10580", "1406", "8500"],
  "Heats": [],
  "Dates": [],
  "Warehouses": [],
  "Warnings": ["no purchase order found"]
}
```

| Field | Source of the value | Maps to |
| --- | --- | --- |
| `Parts` | tokens matching `MMC`/`MMF`/`MMT`/`MME`/`T` plus 5-8 digits, with `O` folded to `0` inside digits | `Model_ReceivingLine.PartID` |
| `PurchaseOrders` | `PO` or `P/O` followed by 4-7 digits | `Model_ReceivingLine.PONumber` |
| `Quantities` | numeric groups of 4-7 digits, comma separated or plain | `Model_ReceivingLine.Quantity` |
| `Heats` | 9-digit values starting with `0` | `Model_ReceivingLine.Heat` |
| `Dates` | `M/D/YY` or `M/D/YYYY` | `Model_ReceivingLine.Date` |
| `Warehouses` | `Warehouse ID` followed by 3 digits | `Model_ReceivingLine.InitialLocation` |
| `Kind` | keyword match on the OCR text | routing decision only |

`Kind` is a heuristic over four known shapes (`BillOfLading`, `EmailScreenshot`,
`ErpLocationScreenshot`, `MaterialTransactionSheet`); `Unknown` is normal for a new supplier
format and is not an error.

## Known limitations

- **Quantities are not scoped.** They are collected wherever they appear, so phone numbers,
  years and ZIP codes slip in. The real implementation has to bind a quantity to a line, either
  by position or by reconciling against the PO lines loaded from Infor Visual.
- **No table structure.** There is no column model, so PO, part and quantity are not linked to
  each other. On the BOL each line repeats `P/O#`, `PART#`, `HEAT#`; only the token sets survive.
- **Handwriting is not read.** The handwritten heat `158394` on the BOL sample is invisible to
  the engine.
- **Halftone copier scans fail.** See the bill-of-lading section above.
- **No multi-line continuation.** The BOL sample ends with `*** CONTINUED ON PAGE 4 ***`; nothing
  joins pages into one receipt yet.
- **Cold start cost is in the first run.** First invocation after a reboot can take a moment
  while Windows loads the OCR runtime.

## Troubleshooting

| Symptom | Cause and fix |
| --- | --- |
| `No OCR engine available` | no OCR language pack; see Step 2 |
| `0 words` on a PDF | the page is blank, or a render bug; try `text --dpi 600 --page 1` |
| Words found but `Parts` empty | the scan is too faint, or the form uses a part prefix not in the regex list |
| `The module 'tools' could not be loaded` | PowerShell treated the path as a module; prefix with `.\` |
| Build error `NETSDK1045` or a missing workload | wrong SDK version; run `dotnet --list-sdks` and install .NET 10 |
| Nothing runs, no output, exits silently | run the `text` mode first; `extract` only prints JSON on success |
| Antivirus quarantines the freshly built exe | common for unsigned local builds; allow the `tools\PackingListScanProbe\bin` folder |

## Appendix A — why not docling

All numbers measured on this machine against the same four samples.

| Approach | Time | Chars on BOL | Fields recovered on BOL | Footprint |
| --- | --- | --- | --- | --- |
| `docling-parse` 7.23.0 alone | under 1 s | **0** | 0 of 21 | light |
| `docling` 2.142.0 + RapidOCR, full page | 55 s cold, 6 s warm | 2,867 | 3 of 21 | 1.15 GB venv + 1.48 GB models |
| `docling` + RapidOCR, auto mode, scale 3 | about 6 s | 3,010 | 5 of 21 | same |
| Granite-Docling VLM, 2-stage | 56 s | 21 | 0 of 21 | same plus weights |
| Granite-Docling VLM, single-stage | over 5 min, aborted | n/a | n/a | same |
| **Windows.Media.Ocr (this probe)** | **2.6 s** | 371 words | 0 of 21, but 100% on the other three samples | **zero** |

`docling-parse` only handles born-digital PDFs, which is why it returned nothing on a copier
scan. Full `docling` mutilated the table it did read: `5,500` became `000`, `P/O# 72477`
disappeared, `SD25611` became `6035611.1`. Its own raster of that page also came out blank,
while `pypdfium2` rendered it fine.

Where docling would still earn its place: suppliers who email born-digital PDFs. `docling-parse`
is exact and instant on those, and that path needs no OCR at all. It is not a solution for the
scan case.

Reproducing the docling comparison, if you want to re-check it:

```powershell
uv venv .\docling-venv --python 3.12
uv pip install --python .\docling-venv\Scripts\python.exe "docling[rapidocr]"
.\docling-venv\Scripts\python.exe -c "from docling.document_converter import DocumentConverter; d=DocumentConverter().convert(r'.\scans\bol.pdf').document; print(d.export_to_markdown())"
```

Expect a multi-gigabyte download on the first run.

## Appendix B — files in the probe

| File | Purpose |
| --- | --- |
| [PackingListScanProbe.csproj](tools/PackingListScanProbe/PackingListScanProbe.csproj) | `net10.0-windows10.0.19041.0`, x64, no package references |
| [Program.cs](tools/PackingListScanProbe/Program.cs) | OCR engine setup, image and PDF page loading, crop and scale handling, CLI |
| [ScanExtractor.cs](tools/PackingListScanProbe/ScanExtractor.cs) | document-kind detection and the part / PO / quantity / heat / date / warehouse patterns |

`MTM_Receiving_Application.csproj` carries `tools\**` removals for `Compile`, `None`, `Page`
and `EmbeddedResource` so the app project ignores the probe.

## Appendix C — how this would land in the app

Sketch only, nothing implemented:

1. Capture gate first: reject images below roughly 200 dpi, or with very low recognized word
   count, and tell the operator to re-scan. That prevents the BOL failure mode from reaching the
   parser at all.
2. `IService_Receiving_DocumentScan` behind the existing service layer, so the ViewModel calls a
   service and never a DAO, per the project rules.
3. Validate, do not trust: the PO line candidates come from Infor Visual, so fuzzy-match the
   OCR part tokens against that closed set and discard the rest. This is what makes
   `MMC0001130`-style noise harmless.
4. Prefill, never auto-save: populate `PoNumber`, `PartID` and the quantities on
   `ViewModel_Receiving_POEntry` and leave the existing validation and review steps as the gate.
