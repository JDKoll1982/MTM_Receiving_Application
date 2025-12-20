# Comprehensive Prompt for Recreating Penpot Mockup Generator

**Role:** You are an expert Tooling Engineer and Automation Specialist with deep knowledge of WinUI 3 XAML and the Penpot file specification. You are detail-oriented, rigorous about file integrity, and prefer robust, modular code over quick scripts.

**Objective:** Create a robust PowerShell automation solution (main script `Generate-Penpot-v2.ps1` and supporting converter modules) that parses WinUI 3 XAML files and generates pixel-perfect, valid `.penpot` import files. 

**Context:** The previous implementation (`Generate-Penpot.ps1`) suffered from "white-on-white" visibility issues, massive canvas scaling bugs, and fragile JSON generation. We are restarting with a modular approach to ensure every XAML control is visually represented correctly in the Penpot design tool.

## 0. Chain of Thought & Planning (Required)
Before writing any code, you must:
1.  **Analyze the Architecture:** Plan the folder structure for the converter modules (e.g., `Converters/Convert-Grid.ps1`, `Converters/Convert-Button.ps1`).
2.  **Define the Data Flow:** Trace how a XAML element is read -> parsed -> mapped to a shape -> converted to JSON -> added to the Page -> zipped.
3.  **Identify Edge Cases:** How will you handle nested Grids? Missing RowDefinitions? Unknown controls?
4.  **Validation Strategy:** How will you verify the JSON is valid *before* zipping it?

## Reference Material & Research
Use these resources to guide your implementation.

**1. Penpot File Format (v3)**
*   **Structure:** ZIP archive containing granular JSON files.
*   **Source Code References:**
    *   `backend/src/app/binfile/v3.clj` (Export logic)
    *   `common/src/app/common/types/shape.cljc` (Shape definitions)
*   **Key Concepts:**
    *   **Manifest:** `manifest.json` at root.
    *   **Hierarchy:** Bidirectional linking. Parent has `shapes: [child_id]`, Child has `parent-id: parent_id`.
    *   **Coordinates:** Relative to parent.
    *   **Text:** ProseMirror JSON structure, not plain strings.

**2. WinUI 3 / XAML Controls**
*   **Repository:** `https://github.com/microsoft/microsoft-ui-xaml`
*   **Control Templates:** Look for `Generic.xaml` to understand visual trees.
*   **Layout Logic:**
    *   **Grid:** Supports `Pixel`, `Auto` (content-based), and `*` (weighted remaining space) sizing.
    *   **StackPanel:** Linear accumulation of child dimensions.
    *   **Defaults:** Implicit 1x1 Grid if definitions missing.

## 1. Input Data (Source)
You will parse XAML files located in `Views/Receiving/*.xaml`.
- **Key Elements to Parse:** 
    - **Containers:** `Grid`, `StackPanel`, `Border`, `ScrollViewer`, `RelativePanel`, `SplitView`, `VariableSizedWrapGrid`, `Pivot`, `ItemsControl`, `Canvas`.
    - **Controls:** `TextBlock`, `TextBox`, `Button`, `CheckBox`, `RadioButton`, `ComboBox`, `ListView`, `GridView`, `DatePicker`, `TimePicker`, `ToggleSwitch`, `Slider`, `ProgressBar`, `ProgressRing`, `CalendarView`, `ColorPicker`, `DropDownButton`, `FlipView`, `HyperlinkButton`, `ListBox`, `NumberBox`, `PasswordBox`, `PersonPicture`, `RatingControl`, `RichEditBox`, `TeachingTip`, `TreeView`.
    - **Visuals:** `Image`, `FontIcon`, `SymbolIcon`, `InfoBar`, `CommandBar`, `NavigationView`, `MediaPlayerElement`, `WebView2`, `InkCanvas`, `Shape` (Rectangle, Ellipse, Line, Path).
- **Properties to Extract:** `Width`, `Height`, `Margin`, `Padding`, `Background`, `Foreground`, `Text`, `CornerRadius`, `Grid.Row/Column`, `HorizontalAlignment`, `VerticalAlignment`, `Visibility`.
- **Layout Logic:** You must approximate the XAML layout engine.
    - **Grid Layout Engine:**
        -   **Priority 1 (Pixel):** Reserve fixed sizes first (e.g., `100`).
        -   **Priority 2 (Auto):** Estimate based on content (e.g., TextBlock height ~20px, Button ~32px).
        -   **Priority 3 (Star `*`):** Distribute remaining space proportionally among star rows/cols.
        -   **Defaults:** If definitions missing, assume 1 Row / 1 Col of size `*`.
    - **StackPanel Engine:**
        -   Vertical: `y = previous_child_y + previous_child_height + spacing`.
        -   Horizontal: `x = previous_child_x + previous_child_width + spacing`.

## 2. Output Format (Target)
The output must be a valid `.penpot` file, which is a **ZIP archive** with a specific internal structure.

### Internal ZIP Structure
```text
/
├── manifest.json                  # Global metadata
└── files/
    └── {file-uuid}/               # Project Root
        ├── page-index.json        # List of pages
        ├── pages/
        │   ├── {page-uuid}.json   # Page metadata
        │   └── {page-uuid}/       # Shape definitions folder
        │       ├── {shape-uuid}.json
        │       └── ...
```

### JSON Schemas (Strict Compliance Required)

**A. `manifest.json`**
```json
{
  "version": 3,
  "origin": "https://design.penpot.app",
  "files": [
    {
      "id": "{file-uuid}",
      "name": "MTM Receiving Mockups",
      "revn": 1,
      "version": 1
    }
  ]
}
```

**B. Page JSON (`pages/{page-uuid}.json`)**
```json
{
  "id": "{page-uuid}",
  "name": "Page Name",
  "file-id": "{file-uuid}",
  "shapes": ["{root-frame-uuid}"], 
  "background-color": "#E5E5E5"
}
```

**C. Shape JSON (`pages/{page-uuid}/{shape-uuid}.json`)**
Every element (Frame, Rect, Text) is a separate JSON file.

**Critical Rules for Shapes:**
1.  **Hierarchy is Bidirectional:**
    -   **Parent** must list child UUIDs in its `shapes` array: `"shapes": ["child-1-uuid", "child-2-uuid"]`
    -   **Child** must reference parent: `"parent-id": "{parent-uuid}"`
    -   **Frame ID**: If inside a frame (like a Board), child must set `"frame-id": "{board-uuid}"`.
2.  **Coordinates:** `x` and `y` are relative to the **parent**.
3.  **Colors:** Use visible, contrasting colors (e.g., `#E0E0E0` for backgrounds, `#333333` for text). **Do not use white-on-white.**
4.  **Text Content:** Text is NOT a simple string. It uses a structured object:
    ```json
    "content": {
      "type": "doc",
      "content": [
        {
          "type": "paragraph",
          "content": [
            {
              "type": "text",
              "text": "Actual Text Here"
            }
          ]
        }
      ]
    }
    ```

## 3. Implementation Strategy (PowerShell)

1.  **Setup Phase:**
    -   Create a temporary directory structure.
    -   Generate UUIDs for File, Page, and Root Board.
    -   Write `manifest.json` and `page-index.json`.

2.  **Parsing & Generation Phase:**
    -   Read the XAML file content.
    -   **Root Board:** Create a "frame" shape (1400x900) to act as the canvas.
    -   **Recursive Parsing:** Iterate through XAML elements.
        -   Maintain a `CurrentY` and `CurrentX` context for StackPanels.
        -   **Container Mapping:**
            -   `Grid`, `StackPanel` -> Penpot `group` or `frame`.
            -   `Border` -> Penpot `rect` (with `r1`, `r2`, `r3`, `r4` for corner radius).
        -   **Control Mapping (Visual Structure):**
            -   `TextBlock`, `RichEditBox` -> Penpot `text`.
            -   `Button`, `HyperlinkButton`, `DropDownButton` -> Group (Rect + Text).
            -   `TextBox`, `PasswordBox`, `NumberBox` -> Group (Rect + Placeholder Text).
            -   `CheckBox` -> Group (Square Rect 20x20 + Check Icon + Text Label).
            -   `RadioButton` -> Group (Circle Stroke 20x20 + Inner Circle Fill 10x10 + Text Label).
            -   `ToggleSwitch` -> Group (Pill Rect 40x20 [Track] + Circle 12x12 [Thumb] + Header Text + On/Off Label).
            -   `Slider` -> Group (Thin Rect [Track] + Circle 18x18 [Thumb] + Header Text).
            -   `ComboBox`, `DatePicker`, `TimePicker` -> Group (Rect 32px height + Text + Chevron Down/Calendar/Clock Icon).
            -   `ListView`, `GridView`, `ListBox`, `TreeView` -> Group containing 3-5 repeated placeholder items (Rect + Text) to simulate a list/grid.
            -   `NavigationView`, `SplitView` -> Group (Pane Rect + Hamburger Menu Icon + List of Menu Items [Icon + Text] + Content Frame).
            -   `Image`, `PersonPicture`, `MediaPlayerElement`, `WebView2` -> Rect with a cross pattern or specific placeholder label.
            -   `FontIcon`, `SymbolIcon` -> Text element (use a generic symbol or emoji if font not available).
            -   `ProgressBar` -> Group (Track Rect + Fill Rect).
            -   `ProgressRing` -> Group (Circle Stroke with gap or multiple dots).
            -   `InfoBar` -> Group (Colored Rect + Icon + Title + Message).
            -   `CalendarView` -> Group (Grid of days).
            -   `ColorPicker` -> Group (Color Spectrum Image/Rect).
        -   **Fallback:** For any unknown element, generate a generic placeholder Rectangle with the element's name as a label, so no UI part is missing.
    -   **Linkage:** As you generate each child, add its UUID to the parent's `shapes` list.

3.  **Packaging Phase:**
    -   Write all JSON files to the temp directory.
    -   **Validation:** Ensure every JSON file is valid before zipping.
    -   Zip the contents (ensure `manifest.json` is at the root of the zip).
    -   Rename `.zip` to `.penpot`.

## 4. Specific Fixes for Previous Failures
-   **Massive Canvas Issue:** Ensure the Root Board shape has explicit `width` (1400) and `height` (900). Do not use `0.01`.
-   **Missing Elements:** You **MUST** populate the `shapes` array of the parent Frame with the UUIDs of all its children. If this array is empty, the board will appear empty.
-   **Visibility:** Default all backgrounds to `#FFFFFF` (White) and all borders/text to `#000000` (Black) or `#333333` (Dark Gray) to ensure visibility.

## 6. Constraints & "Do Not" Checklist
-   **NO White-on-White:** Never generate a shape with a white background on a white board without a border. Always use `#E0E0E0` or darker for backgrounds, or add a `#000000` stroke.
-   **NO Massive Canvas:** The root frame must be exactly 1400x900 (or the XAML design size). Never use `0.01` or `1.0` as dimensions.
-   **NO Empty Shapes Array:** A parent frame MUST list its children in the `shapes` array. If this is missing, the children will exist in the file but be invisible.
-   **NO Monolithic Script:** Do not put all logic in one file. Use `DotSource` to load converter functions from a `Converters/` subdirectory.
-   **NO BOM:** Write JSON files as UTF8 without BOM to ensure Penpot compatibility.

## 7. Deliverable
Produce a single, self-contained PowerShell script `Generate-Penpot-v2.ps1` that performs this entire process for a given XAML file.
Also provide the code for at least 3 core converter modules (`Convert-Grid.ps1`, `Convert-TextBlock.ps1`, `Convert-Button.ps1`) to demonstrate the pattern.
