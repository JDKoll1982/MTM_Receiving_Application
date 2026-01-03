# SVG UI Mockup Generation Prompt Template

**Purpose**: This prompt template generates beautiful, accurate SVG UI mockups for WinUI 3 desktop applications following the MTM Receiving Application design system.

---

## Instructions for AI Agent

You are tasked with generating a professional SVG UI mockup for a WinUI 3 desktop application view. Follow ALL specifications below precisely to ensure consistency with the existing design system.

---

## Context & Requirements

### Target Application
- **Platform**: Windows 10/11 Desktop (WinUI 3)
- **Minimum Width**: 1400px
- **Primary Resolution**: 1920x1080
- **ViewBox**: `0 0 1400 900` (standard desktop viewport)

### View Information
- **Module Name**: [MODULE_NAME] (e.g., "Volvo", "Receiving", "Dunnage", "Routing", "Reporting")
- **View Name**: [VIEW_NAME] (e.g., "ShipmentEntry", "POEntry", "TypeSelection", "LabelEntry", "Main")
- **Full View Path**: `View_[MODULE_NAME]_[VIEW_NAME].svg`
- **Target Directory**: `specs/[MODULE_NUMBER]-[MODULE_NAME]-module/mockups/`

### View Purpose
[DESCRIBE WHAT THIS VIEW DOES - e.g., "User enters Volvo shipment data including date, parts, and skid counts. System calculates component explosion and displays requested lines."]

### Key User Actions
[List the primary actions users can take on this view - e.g., "Add parts to shipment", "Enter discrepancy data", "Generate labels", "Save as pending PO"]

---

## Design System Specifications

### Color Palette

**Primary Colors:**
- Primary Blue: `#0078D4` (buttons, selected states, progress bars, links)
- Success Green: `#107C10` (validation, completion, positive feedback)
- Warning Orange: `#FF8C00` (cautions, important notices)
- Error Red: `#D13438` (errors, destructive actions, validation failures)

**Neutral Colors:**
- Background: `#FAFAFA` (page background)
- Surface: `#FFFFFF` (cards, panels, containers)
- Border: `#CCCCCC` (default borders)
- Border Light: `#E0E0E0` (subtle dividers)
- Surface Alt: `#F5F5F5` (alternating rows, secondary surfaces, disabled states)

**Text Colors:**
- Primary Text: `#1A1A1A` (headlines, important data)
- Secondary Text: `#666666` (labels, descriptions, help text)
- Disabled Text: `#999999` (disabled states)

### Typography

**Font Family:**
- Primary: `Segoe UI, sans-serif` (system default)
- Monospace: `Segoe UI Mono, monospace` (for codes, numbers, technical data)

**Font Sizes & Weights:**
- H1 (Page Title): `28px`, `font-weight="600"`, `fill="#1A1A1A"`
- H2 (Section Title): `24px`, `font-weight="600"`, `fill="#1A1A1A"`
- H3 (Card Title): `20px`, `font-weight="600"`, `fill="#1A1A1A"`
- Body Large: `16px`, `font-weight="400"`, `fill="#1A1A1A"`
- Body: `15px`, `font-weight="400"`, `fill="#1A1A1A"`
- Body Small: `14px`, `font-weight="400"`, `fill="#666666"`
- Label: `13px`, `font-weight="600"`, `text-transform="uppercase"`, `fill="#666666"`
- Caption: `12px`, `font-weight="400"`, `fill="#666666"`

### Spacing System (8px Grid)

All spacing must follow 8px increments:
- **4px**: Minimal spacing (icon padding)
- **8px**: Tight spacing (label to input)
- **16px**: Standard spacing (between form elements)
- **24px**: Section spacing (between groups)
- **32px**: Large spacing (between major sections)
- **40px**: Page margins (standard), horizontal padding in title/footer bars
- **60px**: Extra large spacing (between major UI zones), large card padding

---

## Component Specifications

### Title Bar
```svg
<rect y="0" width="1400" height="80" fill="#FFFFFF" filter="url(#shadow)"/>
<text x="40" y="50" font-family="Segoe UI, sans-serif" font-size="28" font-weight="600" fill="#1A1A1A">
  [Module Name] - [View Name]
</text>
```
- Height: `80px`
- Background: `#FFFFFF`
- Box Shadow: `filter="url(#shadow)"` (defined in `<defs>`)
- Padding: `40px` horizontal
- Title: 28px Semi-bold, `#1A1A1A`

### Progress Indicator (if workflow step)
```svg
<g transform="translate(40, 120)">
  <text x="0" y="0" font-family="Segoe UI, sans-serif" font-size="14" font-weight="500" fill="#666666">
    Step [X] of [Y]
  </text>
  <rect x="0" y="12" width="1320" height="6" rx="3" fill="#E0E0E0"/>
  <rect x="0" y="12" width="[PROGRESS_WIDTH]" height="6" rx="3" fill="#0078D4"/>
</g>
```
- Position: Below title bar, `40px` margin from top
- Height: `6px`
- Border Radius: `3px`
- Background: `#E0E0E0`
- Fill: `#0078D4`
- Label: 14px Medium, `#666666`, `16px` above bar

### Content Cards
```svg
<rect width="[WIDTH]" height="[HEIGHT]" rx="8" fill="#FFFFFF" filter="url(#shadow)"/>
```
- Border Radius: `8px`
- Background: `#FFFFFF`
- Box Shadow: `filter="url(#shadow)"`
- Padding: `60px` (large cards), `40px` (medium), `24px` (small)
- Margin: `40px` from edges, `24px` between cards

### Buttons

**Primary Button:**
```svg
<rect width="[WIDTH]" height="46" rx="4" fill="#0078D4"/>
<text x="[CENTER_X]" y="30" font-family="Segoe UI, sans-serif" font-size="16" font-weight="600" fill="#FFFFFF" text-anchor="middle">
  [Button Text]
</text>
```
- Background: `#0078D4`
- Text: `#FFFFFF`, 16px Semi-bold
- Height: `46px`
- Padding: `24px` horizontal (minimum)
- Border Radius: `4px`

**Secondary Button:**
```svg
<rect width="[WIDTH]" height="46" rx="4" fill="#F5F5F5" stroke="#CCCCCC" stroke-width="2"/>
<text x="[CENTER_X]" y="30" font-family="Segoe UI, sans-serif" font-size="16" font-weight="600" fill="#666666" text-anchor="middle">
  [Button Text]
</text>
```
- Background: `#F5F5F5`
- Border: `2px solid #CCCCCC`
- Text: `#666666`, 16px Semi-bold
- Same dimensions as primary

**Success Button:**
- Background: `#107C10`
- Text: `#FFFFFF`, 16px Semi-bold
- Used for final "Save" actions

### Input Fields
```svg
<text x="0" y="0" font-family="Segoe UI, sans-serif" font-size="14" font-weight="600" fill="#333333">
  [Label Text]
</text>
<rect x="0" y="12" width="[WIDTH]" height="56" rx="4" fill="#FFFFFF" stroke="#CCCCCC" stroke-width="2"/>
<text x="20" y="48" font-family="Segoe UI, sans-serif" font-size="16" fill="#1A1A1A">
  [Placeholder/Value]
</text>
```
- Height: `56px`
- Border: `2px solid #CCCCCC`
- Border Radius: `4px`
- Padding: `20px` horizontal
- Font: 16-20px (larger for critical inputs)
- Focus: Border changes to `#0078D4` (show with stroke color)
- Label: 14px Semi-bold, `8px` above field

### Data Grid
```svg
<!-- Header -->
<rect width="[WIDTH]" height="50" fill="#F5F5F5"/>
<text x="20" y="32" font-family="Segoe UI, sans-serif" font-size="13" font-weight="600" fill="#666666" text-transform="uppercase">
  [Column Name]
</text>

<!-- Row -->
<rect width="[WIDTH]" height="60" fill="#FFFFFF"/>
<rect x="0" y="0" width="[WIDTH]" height="1" fill="#E0E0E0"/>
<text x="20" y="38" font-family="Segoe UI, sans-serif" font-size="15" fill="#1A1A1A">
  [Cell Content]
</text>
```
- Header Height: `50px`
- Header Background: `#F5F5F5`
- Header Text: 13px Semi-bold Uppercase, `#666666`
- Row Height: `60px`
- Alternating Rows: `#FFFFFF` and `#FAFAFA`
- Cell Padding: `20px` horizontal, `16px` vertical
- Dividers: `1px solid #E0E0E0`

### Selection Cards (Mode Selection, Package Type)
```svg
<rect width="260" height="280" rx="8" fill="#F5F5F5" stroke="#CCCCCC" stroke-width="2"/>
<!-- Selected: stroke="#0078D4" stroke-width="3" -->
```
- Width: `260px` - `480px` (depends on screen)
- Height: `280px`
- Border Radius: `8px`
- Background: `#F5F5F5`
- Unselected Border: `2px solid #CCCCCC`
- Selected Border: `3px solid #0078D4`
- Icon: `100px x 100px` centered, top area
- Title: 20-22px Semi-bold, centered
- Description: 14-15px, centered, multi-line

### Info Boxes
```svg
<rect width="[WIDTH]" height="[HEIGHT]" rx="6" fill="#F3F9FF" stroke="#0078D4" stroke-width="1"/>
```
- Border Radius: `6px`
- Padding: `24px`
- Types:
  - **Success**: Background `#F3F9F3`, Icon `#107C10`
  - **Warning**: Background `#FFF9F3`, Icon `#FF8C00`
  - **Error**: Background `#FFF4F4`, Icon `#D13438`
  - **Info**: Background `#F3F9FF`, Icon `#0078D4`

### Footer Action Bar
```svg
<rect y="820" width="1400" height="80" fill="#FFFFFF" filter="url(#shadow-top)"/>
```
- Height: `80px`
- Background: `#FFFFFF`
- Box Shadow: `filter="url(#shadow-top)"` (upward shadow)
- Padding: `40px` horizontal
- Button Spacing: `20px` between buttons
- Alignment: Back button left, primary actions right

---

## SVG Structure Template

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1400 900">
  <defs>
    <filter id="shadow">
      <feDropShadow dx="0" dy="2" stdDeviation="4" flood-opacity="0.1"/>
    </filter>
    <filter id="shadow-top">
      <feDropShadow dx="0" dy="-2" stdDeviation="4" flood-opacity="0.1"/>
    </filter>
  </defs>
  
  <!-- Background -->
  <rect width="1400" height="900" fill="#FAFAFA"/>
  
  <!-- Title Bar -->
  [TITLE_BAR_CODE]
  
  <!-- Progress Indicator (if workflow step) -->
  [PROGRESS_INDICATOR_CODE]
  
  <!-- Main Content -->
  [MAIN_CONTENT_CODE]
  
  <!-- Footer Action Bar -->
  [FOOTER_ACTION_BAR_CODE]
</svg>
```

---

## Layout Patterns

### Pattern 1: Mode Selection (Single Choice)
- Title bar with module name
- Progress indicator (Step X of Y)
- Main card with large selection cards in grid (2-3 columns)
- Icons representing each option
- Clear labeling and descriptions
- Footer with Continue button (right-aligned)

### Pattern 2: Single Input Focus
- Title bar
- Progress indicator
- Centered card (800px max width)
- Large, prominent input field
- Real-time validation feedback
- Help text below input
- Footer with Back/Next buttons

### Pattern 3: Multi-Selection Grid
- Title bar
- Progress indicator
- Wide card (1000px+) with grid of options
- Visual icons for each choice
- Hover and selected states clearly differentiated
- Footer with Back/Next

### Pattern 4: Data Review Grid
- Title bar
- Full-width data grid
- Alternating row colors
- Summary info panels below grid
- Action buttons for each row
- Footer with multiple action buttons

### Pattern 5: Form Entry
- Title bar
- Progress indicator
- Form fields in 1-2 column layout
- Labels above inputs
- Inline validation
- Footer with navigation

### Pattern 6: Admin List & Edit
- Title bar (no progress - not a workflow)
- Toolbar with Add/Search/Filter
- Data grid with actions
- Side panel or modal for editing
- No workflow footer

### Pattern 7: Modal/Dialog
- Background overlay: `fill="#000000" fill-opacity="0.5"`
- Centered modal card (600-1000px width)
- Modal header with title and close button
- Content area with form/display
- Modal footer with action buttons

---

## Module-Specific Elements

### Receiving Module
- **PO Number Format**: `PO-XXXXXX` in monospace font
- **Validation Icons**: Green checkmark for valid PO
- **Package Icons**: Box, Pallet, Loose Parts (line-art style)
- **Progress**: 10 steps in workflow

### Dunnage Module
- **Material Icons**: Using Material.Icons.WinUI3 library (show as simple geometric shapes in SVG)
- **Type Cards**: Icon + Name + Description
- **Spec Inputs**: Dynamic form based on type
- **Progress**: 7 steps in standard workflow
- **Admin Screens**: No progress indicator

### Routing Module
- **Label Number**: Auto-increment display
- **Department Auto-fill**: Visual feedback when triggered
- **Duplicate Row**: Visual animation/feedback
- **History Grouping**: Date headers with alternating group colors
- **Progress**: 5 steps (LabelEntry → Review → Print → History)

### Volvo Module
- **Part Numbers**: `V-EMB-XXX` format in monospace
- **Skid Counts**: Numeric input fields
- **Component Explosion**: Calculated display (read-only preview)
- **Discrepancy Tracking**: Optional checkbox with 4-field form
- **Email Preview**: Modal with editable greeting, read-only tables
- **Master Data**: Admin grid with CRUD actions

### Reporting Module
- **Date Range Pickers**: Start and End date inputs
- **Module Checkboxes**: List of modules with record counts
- **Report Data Grid**: Module-specific columns
- **Export Actions**: CSV export, Email copy buttons

---

## Accessibility Guidelines

### Contrast Ratios (WCAG 2.1 AA)
- Normal text: Minimum 4.5:1
- Large text (18pt+): Minimum 3:1
- UI components: Minimum 3:1

### Visual Indicators
- Focus indicators: 2px outline, `#0078D4` (show on interactive elements)
- Disabled states: Reduced opacity (0.6), grayed text (`#999999`)
- Error states: Red border (`#D13438`), error icon
- Success states: Green checkmark (`#107C10`)

---

## View-Specific Requirements

[FOR EACH VIEW, PROVIDE:]

### View: [VIEW_NAME]

**Purpose**: [What this view does]

**Key Elements**:
1. [Element 1 description]
2. [Element 2 description]
3. [Element 3 description]

**User Flow**:
- User enters: [what user enters]
- System displays: [what system shows]
- User can: [what user can do]

**Special Considerations**:
- [Any special layout requirements]
- [Any conditional elements (show/hide based on state)]
- [Any validation feedback]
- [Any help text or tooltips]

**Example Data** (for realistic mockup):
- [Sample data values to display]

---

## Output Requirements

1. **File Name**: `View_[MODULE_NAME]_[VIEW_NAME].svg`
2. **ViewBox**: `0 0 1400 900`
3. **All Elements**: Must use exact colors, fonts, and spacing from design system
4. **Realistic Content**: Use example data that makes sense for the view
5. **Complete Structure**: Include title bar, content, footer (if applicable)
6. **Visual Hierarchy**: Clear distinction between primary and secondary elements
7. **Professional Appearance**: Clean, modern, manufacturing-appropriate aesthetic

---

## Quality Checklist

Before finalizing the SVG, verify:
- [ ] All colors match design system exactly
- [ ] All spacing follows 8px grid
- [ ] Typography sizes and weights are correct
- [ ] Buttons have proper dimensions (46px height)
- [ ] Input fields have proper dimensions (56px height)
- [ ] Data grid rows are 60px height with alternating colors
- [ ] Shadows are applied using filters
- [ ] All text is readable and properly positioned
- [ ] ViewBox is exactly `0 0 1400 900`
- [ ] File name matches convention: `View_[MODULE]_[NAME].svg`

---

## Example: Complete View Structure

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1400 900">
  <defs>
    <filter id="shadow">
      <feDropShadow dx="0" dy="2" stdDeviation="4" flood-opacity="0.1"/>
    </filter>
    <filter id="shadow-top">
      <feDropShadow dx="0" dy="-2" stdDeviation="4" flood-opacity="0.1"/>
    </filter>
  </defs>
  
  <!-- Background -->
  <rect width="1400" height="900" fill="#FAFAFA"/>
  
  <!-- Title Bar -->
  <rect y="0" width="1400" height="80" fill="#FFFFFF" filter="url(#shadow)"/>
  <text x="40" y="50" font-family="Segoe UI, sans-serif" font-size="28" font-weight="600" fill="#1A1A1A">
    [Module Name] - [View Name]
  </text>
  
  <!-- Progress Indicator (if applicable) -->
  <g transform="translate(40, 120)">
    <text x="0" y="0" font-family="Segoe UI, sans-serif" font-size="14" font-weight="500" fill="#666666">
      Step X of Y
    </text>
    <rect x="0" y="12" width="1320" height="6" rx="3" fill="#E0E0E0"/>
    <rect x="0" y="12" width="[PROGRESS]" height="6" rx="3" fill="#0078D4"/>
  </g>
  
  <!-- Main Content Card -->
  <g transform="translate(40, [TOP_MARGIN])">
    <rect width="1320" height="[HEIGHT]" rx="8" fill="#FFFFFF" filter="url(#shadow)"/>
    
    <!-- Content goes here -->
    
  </g>
  
  <!-- Footer Action Bar -->
  <rect y="820" width="1400" height="80" fill="#FFFFFF" filter="url(#shadow-top)"/>
  <g transform="translate(40, 845)">
    <!-- Back button (left) -->
    <rect width="140" height="46" rx="4" fill="#F5F5F5" stroke="#CCCCCC" stroke-width="2"/>
    <text x="70" y="30" font-family="Segoe UI, sans-serif" font-size="16" font-weight="600" fill="#666666" text-anchor="middle">← Back</text>
  </g>
  <g transform="translate(1210, 845)">
    <!-- Primary action (right) -->
    <rect width="140" height="46" rx="4" fill="#0078D4"/>
    <text x="70" y="30" font-family="Segoe UI, sans-serif" font-size="16" font-weight="600" fill="#FFFFFF" text-anchor="middle">Next →</text>
  </g>
</svg>
```

---

**Use this template to generate accurate, beautiful SVG mockups that match the design system perfectly. Always include realistic example data and ensure all specifications are followed precisely.**

