# Module_Receiving Workflow Consolidation - UI Mockup

## Step 1: Order & Part Selection

```
┌─────────────────────────────────────────────────────────────────┐
│ Receiving - Order & Part Selection                    [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Purchase Order Information ────────────────────────────┐  │
│  │                                                           │  │
│  │  PO Number:  [________________]  [Non-PO Item ☐]       │  │
│  │                                                           │  │
│  │  Part Selection:                                          │  │
│  │  ┌─────────────────────────────────────────────────┐   │  │
│  │  │ [Search parts...]                    [Search]    │   │  │
│  │  └─────────────────────────────────────────────────┘   │  │
│  │                                                           │  │
│  │  Selected Part:                                          │  │
│  │  ┌─────────────────────────────────────────────────┐   │  │
│  │  │ Part ID:     ABC-12345                            │   │  │
│  │  │ Part Type:   Steel Plate                          │   │  │
│  │  │ PO Line:     001                                   │   │  │
│  │  │ Description: 10x20 Steel Plate                    │   │  │
│  │  └─────────────────────────────────────────────────┘   │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌─ Load Configuration ────────────────────────────────────┐  │
│  │                                                           │  │
│  │  Number of Loads:  [___]  (Minimum: 1)                   │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  [< Back]                                    [Next >]    │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

**Features:**
- PO Number entry with validation
- Non-PO checkbox (hides PO field when checked)
- Part search with autocomplete
- Part details card (shown after selection)
- Number of loads input with validation
- Clear visual separation of sections

---

## Step 2: Load Details Entry

```
┌─────────────────────────────────────────────────────────────────┐
│ Receiving - Load Details Entry                      [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Part: ABC-12345 - Steel Plate | PO: PO-12345 | Loads: 3     │
│                                                                 │
│  ┌─ Load 1 ────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  Weight/Quantity:  [________]  lbs                       │  │
│  │  Heat Lot Number:   [________]                            │  │
│  │  Package Type:      [Dropdown ▼]                         │  │
│  │  Packages Per Load: [____]                                │  │
│  │                                                           │  │
│  │  [✓ Valid]  [Copy to All Loads]                         │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌─ Load 2 ────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  Weight/Quantity:  [________]  lbs                       │  │
│  │  Heat Lot Number:   [________]                            │  │
│  │  Package Type:      [Dropdown ▼]                         │  │
│  │  Packages Per Load: [____]                                │  │
│  │                                                           │  │
│  │  [⚠ Missing Heat Lot]  [Copy to All Loads]              │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌─ Load 3 ────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  Weight/Quantity:  [________]  lbs                       │  │
│  │  Heat Lot Number:   [________]                            │  │
│  │  Package Type:      [Dropdown ▼]                         │  │
│  │  Packages Per Load: [____]                                │  │
│  │                                                           │  │
│  │  [⚠ Missing Weight]  [Copy to All Loads]                │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌─ Bulk Actions ──────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  [Copy Load 1 to All]  [Clear All]                     │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  [< Back]                                    [Next >]    │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

**Features:**
- Expandable or scrollable sections for each load
- Inline validation indicators (✓ Valid, ⚠ Warning, ✗ Error)
- "Copy to All Loads" button per load
- Bulk actions section
- Context bar showing current part/PO info
- Real-time validation feedback

**Alternative Layout (Data Grid):**
```
┌─────────────────────────────────────────────────────────────────┐
│ Load Details Entry                                    [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Part: ABC-12345 - Steel Plate | PO: PO-12345                 │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │ Load │ Weight │ Heat Lot │ Package Type │ Packages │ Status│ │
│  ├──────┼────────┼──────────┼─────────────┼──────────┼───────┤ │
│  │  1   │ [____] │ [______]  │ [Dropdown ▼]│ [____]   │  ✓   │ │
│  │  2   │ [____] │ [______]  │ [Dropdown ▼]│ [____]   │  ⚠   │ │
│  │  3   │ [____] │ [______]  │ [Dropdown ▼]│ [____]   │  ⚠   │ │
│  └───────────────────────────────────────────────────────────┘ │
│                                                                 │
│  [Copy Row 1 to All]  [Clear All]                              │
│                                                                 │
│  [< Back]                                          [Next >]    │
└─────────────────────────────────────────────────────────────────┘
```

---

## Step 3: Review & Save

```
┌─────────────────────────────────────────────────────────────────┐
│ Receiving - Review & Save                           [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Summary ────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  Part: ABC-12345 - Steel Plate                           │  │
│  │  PO Number: PO-12345                                      │  │
│  │  Number of Loads: 3                                       │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌─ Load Details ───────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │ Load 1  │ Weight: 500 lbs │ Heat Lot: HL-001       │ │  │
│  │  │         │ Package: Pallet  │ Count: 10              │ │  │
│  │  │         │ [Edit]                                  │ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  │                                                           │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │ Load 2  │ Weight: 750 lbs │ Heat Lot: HL-002       │ │  │
│  │  │         │ Package: Pallet  │ Count: 15              │ │  │
│  │  │         │ [Edit]                                  │ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  │                                                           │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │ Load 3  │ Weight: 600 lbs │ Heat Lot: HL-003       │ │  │
│  │  │         │ Package: Pallet  │ Count: 12              │ │  │
│  │  │         │ [Edit]                                  │ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  [< Back]                          [Save]                │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

**During Save:**
```
┌─────────────────────────────────────────────────────────────────┐
│ Receiving - Saving...                               [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Save Progress ──────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  Saving to local CSV...                                  │  │
│  │  ████████████████████░░░░░░░░  60%                       │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**After Save (Success):**
```
┌─────────────────────────────────────────────────────────────────┐
│ Receiving - Complete                                 [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Save Results ────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  ✓ Success! 3 loads saved successfully.                   │  │
│  │                                                           │  │
│  │  Save Details:                                            │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │ Local CSV:    ✓ Saved                               │ │  │
│  │  │ Network CSV:  ✓ Saved                               │ │  │
│  │  │ Database:     ✓ Saved (3 loads)                      │ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │              [Start New Entry]                            │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

**After Save (Partial Failure):**
```
┌─────────────────────────────────────────────────────────────────┐
│ Receiving - Complete                                 [Help] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─ Save Results ────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │  ⚠ Partial Success - 3 loads saved with warnings.        │  │
│  │                                                           │  │
│  │  Save Details:                                            │  │
│  │  ┌─────────────────────────────────────────────────────┐ │  │
│  │  │ Local CSV:    ✓ Saved                               │ │  │
│  │  │ Network CSV:  ✗ Failed (Network unavailable)        │ │  │
│  │  │ Database:     ✓ Saved (3 loads)                      │ │  │
│  │  └─────────────────────────────────────────────────────┘ │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                                                           │  │
│  │              [Start New Entry]                            │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

**Features:**
- Summary section with key information
- Expandable/collapsible load details
- Edit buttons to return to previous steps
- Save button with progress indicator
- Results display with status icons
- Clear success/failure messaging
- "Start New Entry" button after completion

## Design Principles

1. **Progressive Disclosure:** Show only what's needed at each step
2. **Visual Hierarchy:** Clear section separation and grouping
3. **Validation Feedback:** Inline indicators and error messages
4. **Accessibility:** Keyboard navigation, screen reader support
5. **Responsive Layout:** Adapts to different screen sizes
6. **Consistent Styling:** Matches existing WinUI 3 design system

## Responsive Considerations

- **Large Screens:** Side-by-side layout for load details
- **Medium Screens:** Stacked sections with scrolling
- **Small Screens:** Collapsible sections, single column layout
