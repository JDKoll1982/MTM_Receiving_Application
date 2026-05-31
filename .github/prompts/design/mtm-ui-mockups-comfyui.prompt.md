---
name: mtm-ui-mockups-comfyui
description: 'Generate ComfyUI text prompts (positive + negative) for AI-image-based UI mockups of the MTM Receiving Application, including accurate Volvo 2021 brand identity.'
agent: ask
argument-hint: 'Specify the module name and view — e.g. "Volvo ShipmentEntry" or "Receiving LabelEntry"'
---

# ComfyUI UI Mockup Prompt Generator

**Purpose**: Produce ready-to-paste ComfyUI positive and negative prompts that generate
photorealistic desktop UI screenshots for MTM Receiving Application views.

---

## Instructions for AI Agent

Given a module name and view name, output:
1. A **positive prompt** block
2. A **negative prompt** block
3. **Recommended ComfyUI settings** (sampler, steps, CFG, resolution)

Apply the base template first, then layer on the module-specific additions below.
Always validate the Volvo brand identity rules before generating any Volvo module prompt.

---

## Base Positive Prompt Template

```
professional Windows 11 desktop application UI screenshot, WinUI 3 fluent design,
[MODULE_NAME] module, [VIEW_DESCRIPTION],
light theme, white (#FFFFFF) background, primary blue (#0078D4) accent,
Segoe UI font, rounded corners 4px, subtle elevation shadows,
title bar with module name top-left, left sidebar navigation highlighted item,
main content area with [KEY_ELEMENTS],
crisp pixel-perfect rendering, 1920x1080, 8k, ultra-sharp,
professional enterprise manufacturing software
```

## Base Negative Prompt

```
blurry, low quality, pixelated, distorted, JPEG artifacts, watermark, signature,
dark mode, dark theme, macOS, iOS, Android, mobile layout, tablet UI,
cartoon, illustration, painting, sketch, hand-drawn, anime,
neon colors, rainbow gradient, cluttered, messy, inconsistent spacing,
Windows XP, Windows 7 Aero glass, glossy plastic buttons,
old-style dialog boxes, serif body text, Times New Roman
```

---

## Recommended ComfyUI Settings

| Setting        | Value                           | Notes                                    |
|----------------|---------------------------------|------------------------------------------|
| Model          | FLUX.1-dev or Juggernaut XL     | FLUX for crispness; Juggernaut for realism |
| Sampler        | DPM++ 2M Karras or Euler        |                                          |
| Steps          | 30–40                           | 40+ for final output                     |
| CFG Scale      | 7.0–8.5                         | Higher = more prompt-adherent            |
| Resolution     | 1920 × 1080                     | 16:9, matches WinUI target               |
| Batch size     | 4                               | Pick the best composition                |
| Upscaler       | 4x-UltraSharp or RealESRGAN     | Apply after generation                   |
| ControlNet     | Canny or Depth (strength 0.6)   | Use a layout sketch for composition      |

---

## Module-Specific Positive Prompt Additions

### Receiving Module

```
receiving workflow step N of 10, blue step progress indicator top,
PO number input "PO-XXXXXX" in monospace font, green validation checkmark,
package type selection cards (Box icon, Pallet icon, Loose Parts icon),
label generation button, navigation Previous Next buttons in footer
```

### Dunnage Module

```
dunnage type selection cards with geometric shape icons, 7-step workflow progress bar,
material specification dynamic form fields, spec input text boxes,
type card with icon name description layout, admin data grid with CRUD buttons
```

### Volvo Module — Brand Identity Rules (2021–present)

> **CRITICAL**: The current Volvo brand symbol is a **flat, monochrome black roundel** —
> a circle outline with a single diagonal arrow exiting the circle at ~45° upper-right
> (the ancient iron/Mars alchemical symbol ♂). There are NO gradients, NO chrome, NO 3D
> effects, NO blue banner. The wordmark is `VOLVO` in wide-spaced all-caps serif,
> flat black on white. The 3D chrome + blue-badge version is pre-2021 and must NOT appear.
> For the shared footer label-launch cluster in this app, the Volvo button uses the
> **wordmark only inside the square tile**; do not use the roundel for that button.

```
monochrome black and white color scheme throughout — no blue (#0078D4) accent,
black header bar or white header with black text, no blue highlight on sidebar item,

shared shell footer label-launch button cluster:
  four compact shortcut buttons in one horizontal row,
  every button uses the same matching bordered square icon tile,
  each tile is 35x35px with background #F8FAFB,
  1.5px border in #374151,
  3px corner radius on the inner square,
  subtle outer button frame with 6px corner radius,
  identical padding and visual weight on all four buttons,
  no circular badges, no floating icons, no mismatched shapes,

  button 1 — Receiving:
    same exact bordered square tile as the other three buttons,
    use a single centered bold green "R",
    font family: Segoe UI Variable Display Bold or closest WinUI system sans,
    uppercase letterform, vertically and horizontally centered,
    color #16A34A,
    large dominant glyph sized to read clearly at small scale,
    clean industrial receiving feel,
    this button represents the full-size 8.5 x 11 receiving label,
    keep the treatment simple and document-like rather than decorative,

  button 2 — Mini-Receiving:
    same exact bordered square tile as the other three buttons,
    tiny red lowercase word "mini" positioned above a bold green "R",
    "mini" font family: Segoe UI Semibold or closest WinUI system sans,
    "mini" color #DC2626, very small but still legible,
    "R" font family: Segoe UI Variable Display Bold or closest WinUI system sans,
    "R" color #16A34A,
    compact stacked composition with tight vertical spacing,
    this button represents the smaller 6 x 4 thermal mini receiving label,
    the whole mark should feel tighter, shorter, and more compact than the full receiving button,

  button 3 — Dunnage:
    same exact bordered square tile as the other three buttons,
    use a single centered bold blue "D",
    font family: Segoe UI Variable Display Bold or closest WinUI system sans,
    uppercase letterform, vertically and horizontally centered,
    color #1D4ED8,
    broad stable letter shape with a protective packaging feel,
    simple, crisp, and slightly heavy so it feels grounded,

  button 4 — Volvo:
    use the word "Volvo" only inside the same matching square tile,
    do NOT use the Volvo roundel, circle-and-arrow, truck emoji, or any vehicle icon,
    set the wordmark in Clarendon Text Bold as the closest public font reference,
    if Clarendon Text is unavailable, use the closest bracketed-serif Clarendon-style font,
    title case exactly: "Volvo",
    dark charcoal or black text (#111111 to #000000),
    optically centered within the square,
    slightly reduced tracking so the word fits cleanly inside the 35x35 tile,
    preserve the recognizable Clarendon-style serif details and bracketed serifs,
    keep the wordmark crisp and readable at icon scale,
    no blue wordmark, no all-caps VOLVO, no roundel,
    this button should feel premium and automotive while still matching the other square buttons,

  separators:
    thin vertical divider between the Receiving pair and Dunnage,
    thin vertical divider between Dunnage and Volvo,

shipment entry form layout:
  two-column header row: shipment date DatePicker left, shipment number read-only right,
  divider line below header,
  scrollable parts list table with columns:
    Part Number (monospace "V-EMB-XXX"), Description, Requested Qty (numeric spinner),
    Actual Qty (numeric spinner), Skid Count (numeric spinner), Unit (text),
    Component Explosion toggle chevron per row,
  expanded component explosion sub-row shows:
    indented child rows: Component Part#, Component Qty per skid, Total Qty (read-only),
    light grey (#F5F5F5) background on sub-rows, left border accent 2px,

discrepancy tracking section below parts list:
  checkbox "Record Discrepancy" — when checked reveals 4 fields:
    Discrepancy Type dropdown, Expected Qty, Received Qty, Notes textarea,
  optional section collapses when unchecked,

footer action bar (sticky bottom):
  left: "+ Add Part" secondary outlined button,
  center: "Generate Labels" primary filled button (black fill white text),
  right: "Save — Pending PO" split button with dropdown arrow,

status chip top-right of form: "DRAFT" grey pill badge,
form field labels above inputs, 8px spacing between fields,
Segoe UI font 14px body 12px labels, subtle 1px border (#E0E0E0) on inputs
```

**Volvo-specific negative additions** (append to base negative):

```
blue accent color, blue buttons, blue sidebar highlight,
Volvo roundel in footer button, Mars symbol, iron symbol, circle-and-arrow icon,
3D chrome Volvo badge, silver metallic, blue banner, old Volvo emblem pre-2021,
colorful branding, gradient symbol, glossy roundel, truck emoji, vehicle icon,
floating icon without square border, circular badge, mismatched button frames
```

### Reporting Module

```
date range pickers start date end date, module selection checkboxes list,
report data grid with sortable columns, CSV export button, email copy button,
summary statistics panel, filter toolbar
```

---

## Volvo Brand Symbol — Standalone Icon Generation

Use these prompts to generate **only the 2021 Volvo roundel** as a standalone icon image.
Do NOT blend this into module UI views — use only for isolated icon/asset generation.

### Positive Prompt

```
Volvo Cars 2021 brand identity redesign, isolated logo on pure white background,

roundel geometry:
  perfect circle, stroke only — no fill inside, stroke weight 3-4% of diameter,
  single straight arrow shaft starting at the 3 o'clock position on the circle edge
  and extending diagonally to the upper-right at exactly 45 degrees,
  arrowhead at tip — two short tick marks angled inward forming a V-notch,
  arrow shaft length outside the circle equals approximately 40% of circle diameter,
  uniform stroke weight throughout — no tapering, no calligraphic variation,

wordmark:
  "VOLVO" in all-caps, wide letter-spacing (+200 tracking),
  thin square-serif typeface (slab serif, hairline stroke weight),
  centered directly below the roundel,
  wordmark total width approximately equal to circle diameter,
  font size approximately 30-35% of circle diameter,

overall style:
  flat 2D vector graphic, brand identity sheet, pure graphic design,
  solid black (#000000) on pure white (#FFFFFF) — no other colors anywhere,
  no fill, no gradients, no shadows, no glow, no textures, no depth cues,
  generous white margin on all sides, logo occupies ~60% of canvas centered,

ultra-crisp edges, vector-perfect rendering, 4k, print-ready
```

### Negative Prompt

```
3D effect, chrome, metallic, silver, glossy, reflective, embossed, raised, beveled,
gradient, drop shadow, inner shadow, outer glow, depth cue, texture,
blue color, colored background, dark background, grey background, off-white tint,
old Volvo logo pre-2021, blue rectangle banner beneath circle, blue shield shape,
solid filled disc (must be circle outline only — no filled circle),
multiple arrows, curved arrow, horizontal arrow, vertical arrow,
arrow pointing left, arrow pointing down, arrow pointing lower-right,
sans-serif wordmark, rounded font, bold font, condensed font, italic font,
photorealistic car, automobile photograph, vehicle, truck, engine part,
blurry, pixelated, JPEG noise, low resolution, hand-drawn, sketch, watermark
```

### Logo-Specific Settings

| Setting         | Value                      | Notes                                       |
|-----------------|----------------------------|---------------------------------------------|
| Model           | FLUX.1-dev                 | Best for precise geometric line art         |
| Sampler         | Euler                      |                                             |
| Steps           | 40                         | More steps = crisper strokes                |
| CFG Scale       | 6.0–7.0                    | Lower CFG avoids over-sharpening            |
| Resolution      | 1024 × 1024 (square)       | Square canvas for logo isolation            |
| Batch size      | 8                          | Select the geometrically cleanest output    |
| Post-processing | Vectorize in Inkscape      | Convert raster output to SVG via auto-trace |

---

## Full Ready-to-Paste Examples

### Volvo — ShipmentEntry View

**Positive:**
```
professional Windows 11 desktop application UI screenshot, WinUI 3 fluent design,
Volvo shipment entry form view, light theme white background,
monochrome black accent, no blue accent,
shipment date picker field, part number input "V-EMB-XXX" monospace,
skid count numeric input, component explosion calculated read-only table below,
discrepancy tracking optional checkbox expanding to 4 fields,
add part button, generate labels primary button, save pending PO button in footer,
shared shell footer label-launch button cluster with four matching bordered square buttons,
Receiving button with green R, Mini-Receiving button with red mini above green R,
Dunnage button with blue D, Volvo button using the word "Volvo" only,
Volvo wordmark set in Clarendon Text Bold or closest Clarendon-style serif,
title case "Volvo", centered inside the same 35x35 bordered square tile,
left sidebar with Shipment Entry item active, clean enterprise UI,
Segoe UI font, subtle card shadows, 1920x1080, ultra-sharp, 8k
```

**Negative:**
```
blurry, dark mode, macOS, mobile, 3D chrome Volvo badge, blue banner Volvo logo,
old Volvo emblem, gradient, neon, watermark, hand-drawn, cartoon, low quality,
Volvo roundel in footer button, floating icon without square border, mismatched button frames
```

---

### Volvo — POEntry (History Grid) View

**Positive:**
```
professional Windows 11 desktop application screenshot, WinUI 3 fluent design,
Volvo history and PO entry view, light theme white background,
monochrome black accent, no blue highlight,
full-width data grid — columns: Date, Shipment#, Status, PO Number, Receiver,
status badges: "pending_po" orange pill, "completed" green pill,
inline PO number text input and receiver number text input per row,
toolbar with filter input and date range at top, row action buttons Edit Archive,
shared shell footer label-launch button cluster with four matching bordered square buttons,
Volvo footer button uses the word "Volvo" in Clarendon Text Bold or closest Clarendon-style serif,
title case wordmark centered inside the same square bordered tile as the other three buttons,
footer with export button, Segoe UI font, subtle row striping, 1920x1080, 8k
```

**Negative:**
```
blurry, dark mode, macOS, mobile, 3D chrome Volvo logo, gradient, cartoon, watermark,
Volvo roundel in footer button, truck emoji, circular badge, mismatched square borders
```

---

## Usage Steps

1. Copy the relevant positive block into ComfyUI's **CLIP Text Encode (Prompt)** node.
2. Copy the negative block into **CLIP Text Encode (Negative Prompt)** node.
3. Set sampler/steps/CFG per the table above.
4. Optionally wire a **ControlNet Canny** node using a hand-sketched layout as the control image.
5. Generate 4 images, select the best composition.
6. Pass through **4x-UltraSharp** upscaler for final resolution.
7. Save output to `specs/[N]-[MODULE]-module/mockups/comfyui/`.
