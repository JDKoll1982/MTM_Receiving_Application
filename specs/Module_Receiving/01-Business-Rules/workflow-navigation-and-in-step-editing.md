# Workflow Navigation and In-Step Editing

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Data Flow](../00-Core/data-flow.md), [Guided Mode](../02-Workflow-Modes/001-wizardmode-specification.md), [Hub Orchestration](../02-Workflow-Modes/004-hub-orchestration-specification.md)

## Overview

Workflow Navigation defines how users move between steps in Guided Mode and how they can edit previous step data from the Review screen without restarting the entire workflow. This includes forward navigation, backward navigation, and in-step editing from the Review screen (Step 3).

## Core Navigation Patterns

### Forward Navigation

**Pattern:** User progresses from one step to the next in sequential order.

**Standard Flow:**
```
Mode Selection → Step 1 → Step 2 → Step 3 → Complete
```

**Navigation Controls:**
- **"Next" Button:** Moves to next step
- **Enabled:** When current step validation passes
- **Disabled:** When current step has validation errors

**Validation Before Navigation:**
```csharp
if (CurrentStepIsValid())
{
    SaveSessionState();
    NavigateToNextStep();
}
else
{
    ShowValidationErrors();
    DisableNextButton();
}
```

### Backward Navigation

**Pattern:** User returns to a previous step to review or modify data.

**Backward Flow:**
```
Step 3 → Step 2 → Step 1 → Mode Selection
```

**Navigation Controls:**
- **"Back" Button:** Returns to previous step
- **Always Enabled:** Even when current step has validation errors
- **Data Preservation:** ALL data entered on current step is saved to session

**Key Principle:** Validation errors do NOT block backward navigation.

**Rationale:** Users may need to go back to correct data in a previous step that caused issues in the current step.

### Session State Preservation

**Behavior:** All navigation (forward and backward) preserves complete session state.

**Session State Components:**
- PO Number / Non-PO flag
- Part Number and part details
- Load Count
- All load detail data (Weight, Heat Lot, Package Type, Packages Per Load)
- Auto-fill metadata
- Validation state

**Implementation:**
```csharp
public class ReceivingSession
{
    public Guid SessionId { get; set; }
    public string CurrentStep { get; set; } // "Step1", "Step2", "Step3"
    public string PreviousStep { get; set; }
    public bool IsEditMode { get; set; }
    public string EditReturnTarget { get; set; } // "Step3" when in edit mode
    
    // Step 1 Data
    public string PONumber { get; set; }
    public bool IsNonPO { get; set; }
    public string PartNumber { get; set; }
    public int LoadCount { get; set; }
    
    // Step 2 Data
    public List<LoadDetail> Loads { get; set; }
    public Dictionary<string, AutoFillMetadata> AutoFillTracking { get; set; }
    
    // Step 3 Data
    public bool ReviewedAndConfirmed { get; set; }
}
```

## In-Step Editing from Review Screen

### Purpose

Allows users to correct errors discovered during review (Step 3) without restarting the entire workflow or losing all entered data.

### Edit Mode Activation

**Trigger:** User clicks "Edit" button on Review screen (Step 3)

**Edit Buttons on Review Screen:**
```
┌─────────────────────────────────────────────┐
│ Review Receiving Transaction                │
│ ─────────────────────────────────────────── │
│                                             │
│ Order & Part Information  [Edit Order & Part]
│ • PO Number: PO-123456                      │
│ • Part: MMC0001000 - Steel Coil             │
│ • Load Count: 5                             │
│                                             │
│ Load Details             [Edit Load Details]│
│ • Load 1: 10,000 lbs, HL-001, SKID, 3      │
│ • Load 2: 10,000 lbs, HL-001, SKID, 3      │
│ ...                                         │
│                                             │
│           [Back]  [Save and Complete]       │
└─────────────────────────────────────────────┘
```

### Edit Order & Part Button

**Action:** Returns user to Step 1 in Edit Mode

**Button Behavior:**
- **Tooltip:** "Edit Order & Part Information - Step 1"
- **Click Action:**
  1. Set session flag: `IsEditMode = true`
  2. Set return target: `EditReturnTarget = "Step3"`
  3. Navigate to Step 1
  4. Change "Next" button to "Return to Review"
  5. Show "Edit Mode" indicator

**Step 1 in Edit Mode:**
```
┌─────────────────────────────────────────────┐
│ Step 1: Order & Part Selection - Edit Mode │  ← Visual indicator
│ ─────────────────────────────────────────── │
│                                             │
│ [☐] Non-PO Item                             │
│ PO Number: [PO-123456]                      │
│ Part Number: [MMC0001000]                   │
│ Load Count: [5]                             │
│                                             │
│     [Back]  [Return to Review]              │  ← Button text changed
└─────────────────────────────────────────────┘
```

### Edit Load Details Button

**Action:** Returns user to Step 2 in Edit Mode

**Button Behavior:**
- **Tooltip:** "Edit Load Details - Step 2"
- **Click Action:**
  1. Set session flag: `IsEditMode = true`
  2. Set return target: `EditReturnTarget = "Step3"`
  3. Navigate to Step 2
  4. Change "Next" button to "Return to Review"
  5. Show "Edit Mode" indicator
  6. Optional: Scroll to first error (if validation errors exist)

**Step 2 in Edit Mode:**
```
┌─────────────────────────────────────────────┐
│ Step 2: Load Details - Edit Mode           │  ← Visual indicator
│ ─────────────────────────────────────────── │
│                                             │
│ [Load detail grid with existing data...]   │
│                                             │
│     [Back]  [Return to Review]              │  ← Button text changed
└─────────────────────────────────────────────┘
```

### "Return to Review" Button

**Purpose:** Navigates directly back to Step 3 (Review) after editing, bypassing intermediate steps.

**Behavior:**
- **Enabled:** When all validation on current step passes
- **Disabled:** When validation errors exist on current step
- **Tooltip (when disabled):** "Fix validation errors to return to review"

**Navigation Path:**
- **From Step 1 (Edit Mode):** Step 1 → Step 3 (bypasses Step 2)
- **From Step 2 (Edit Mode):** Step 2 → Step 3 (direct)

**On Click:**
```csharp
if (CurrentStepIsValid())
{
    SaveSessionState();
    session.IsEditMode = false;
    session.EditReturnTarget = null;
    NavigateToStep(3);
    HighlightChangedFields(); // Visual feedback for 3-5 seconds
}
else
{
    ShowValidationErrors();
}
```

### Changed Field Highlighting

**Purpose:** Provides visual confirmation that edits were applied successfully.

**Display:** On Step 3 (Review) after returning from Edit Mode

**Visual Indicator:**
- Changed fields highlighted with subtle background color (e.g., light blue, light yellow)
- Highlight duration: 3-5 seconds
- Fade out animation

**Example:**
```
┌─────────────────────────────────────────────┐
│ Review Receiving Transaction                │
│ ─────────────────────────────────────────── │
│                                             │
│ Order & Part Information  [Edit Order & Part]
│ • PO Number: [PO-654321] ← Highlighted      │
│ • Part: MMC0002000 - Different Coil         │
│ • Load Count: 5                             │
│                                             │
│ Load Details             [Edit Load Details]│
│ • Load 1: 10,000 lbs, HL-001, SKID, 3      │
│ • Load 2: [12,000] lbs, HL-001, SKID, 3 ← Highlighted
│ ...                                         │
│                                             │
│           [Back]  [Save and Complete]       │
└─────────────────────────────────────────────┘
```

## Edit Mode vs. Normal Navigation

### Standard "Back" Button in Edit Mode

**Scenario:** User is in Step 1 (Edit Mode) and clicks standard "Back" button instead of "Return to Review"

**Behavior:** Warning dialog shown

**Dialog:**
```
┌──────────────────────────────────────────────┐
│ ⚠ Navigation Warning                        │
│ ────────────────────────────────────────────│
│                                              │
│ You are in Edit Mode.                       │
│                                              │
│ • Use 'Return to Review' to save changes    │
│   and return to Step 3                      │
│                                              │
│ • Use 'Back' to navigate normally (edit     │
│   context may be lost)                      │
│                                              │
│ Continue with 'Back'?                        │
│                                              │
│          [Cancel]  [Continue]               │
└──────────────────────────────────────────────┘
```

**User Continues:**
- Navigate back normally (e.g., Step 1 → Mode Selection)
- Edit Mode flag cleared
- Changes preserved in session

**User Cancels:**
- Remain on current step
- Edit Mode continues

### Closing Window in Edit Mode

**Scenario:** User attempts to close window or exit while in Edit Mode

**Behavior:** Warning dialog shown

**Dialog:**
```
┌──────────────────────────────────────────────┐
│ ⚠ Unsaved Edits                             │
│ ────────────────────────────────────────────│
│                                              │
│ You have unsaved edits in Edit Mode.        │
│                                              │
│ • Return to review to preserve changes      │
│ • Discard all workflow data and exit        │
│                                              │
│ What would you like to do?                  │
│                                              │
│  [Cancel]  [Discard All Data and Exit]      │
└──────────────────────────────────────────────┘
```

## Load Count Changes in Edit Mode

### Increase Load Count

**Scenario:** User in Edit Mode (Step 1) increases load count from 5 to 8

**Behavior:**
1. Existing loads 1-5: Data preserved
2. New loads 6-8: Empty cells created
3. If auto-fill was previously used: New loads automatically receive copy source data in empty cells
4. Notification: "New loads initialized with Load 1 data (empty cells only)"
5. Step 2 updates grid to show 8 loads total

**Example:**
```
Original: 5 loads with data
User changes Load Count to 8

Step 2 Grid After Change:
- Load 1-5: Existing data unchanged
- Load 6-8: New rows, auto-filled if copy source exists, otherwise empty
```

### Decrease Load Count

**Scenario:** User in Edit Mode (Step 1) decreases load count from 5 to 3

**Behavior:**
1. **WARNING:** Data for loads 4-5 will be lost
2. Confirmation dialog shown

**Confirmation Dialog:**
```
┌──────────────────────────────────────────────┐
│ ⚠ Confirm Load Count Decrease               │
│ ────────────────────────────────────────────│
│                                              │
│ Decreasing load count from 5 to 3 will      │
│ discard data for loads 4 and 5.              │
│                                              │
│ Load 4: 10,000 lbs, HL-001, SKID, 3         │
│ Load 5: 10,000 lbs, HL-001, SKID, 3         │
│                                              │
│ This cannot be undone.                      │
│                                              │
│ Continue?                                    │
│                                              │
│          [Cancel]  [Discard and Continue]   │
└──────────────────────────────────────────────┘
```

**User Confirms:**
- Loads 4-5 deleted
- Load Count set to 3
- Session updated

**User Cancels:**
- Load Count remains 5
- No data lost

## Part Change in Edit Mode

### Scenario

User in Edit Mode (Step 1) changes part selection from MMC0001000 to MMC0002000

### Potential Impacts

**Different Unit of Measure:**
- Original part: Weight in LBS
- New part: Weight in KG
- Result: Load detail weight values may become invalid

**Different Part Type:**
- Original part: Coils (requires diameter)
- New part: Flat Stock (requires length)
- Result: Measurement fields incompatible

### Confirmation Dialog

**When Part Change Detected:**
```
┌──────────────────────────────────────────────┐
│ ⚠ Confirm Part Change                       │
│ ────────────────────────────────────────────│
│                                              │
│ Changing from MMC0001000 to MMC0002000       │
│ may invalidate some load details.           │
│                                              │
│ Affected Fields:                             │
│ • Unit of measure changed (LBS → KG)        │
│ • Part type changed (Coils → Flat Stock)    │
│                                              │
│ Load detail fields will be cleared:         │
│ • Weight/Quantity values                    │
│                                              │
│ The following will be preserved:            │
│ • Heat Lot                                  │
│ • Package Type                              │
│ • Packages Per Load                         │
│                                              │
│ Continue?                                    │
│                                              │
│          [Cancel]  [Change Part]            │
└──────────────────────────────────────────────┘
```

**User Confirms:**
- Part changed to MMC0002000
- Incompatible load fields cleared
- Compatible fields preserved
- User must re-enter cleared fields

**User Cancels:**
- Part remains MMC0001000
- No changes made

## PO/Non-PO Mode Switching in Edit Mode

### Restriction

**Rule:** Cannot switch between PO and Non-PO modes in Edit Mode when editing historical transactions.

**Rationale:** Historical integrity - transaction type (PO vs. Non-PO) is a fundamental attribute that should not change after creation.

### Notification

**When User Attempts to Switch:**
```
┌──────────────────────────────────────────────┐
│ ℹ Cannot Change PO Mode in Edit Mode        │
│ ────────────────────────────────────────────│
│                                              │
│ This transaction was originally created     │
│ with PO-123456.                              │
│                                              │
│ You cannot switch between PO and Non-PO     │
│ modes when editing historical transactions. │
│                                              │
│ To receive this part as Non-PO, create a    │
│ new receiving transaction.                   │
│                                              │
│                      [OK]                    │
└──────────────────────────────────────────────┘
```

## Validation in Edit Mode

### Edit Mode Validation Rules

**Same as Normal Mode:** All validation rules apply identically in Edit Mode

**Difference:** "Return to Review" button instead of "Next" button

**Validation Blocking:**
- **"Return to Review" Button:** Disabled when validation errors exist
- **Tooltip:** "Fix validation errors to return to review"
- **"Back" Button:** Always enabled (validation does NOT block backward navigation)

### Multiple Edit Sessions

**Scenario:** User edits from Step 3, returns to review, edits again, returns to review again, etc.

**Behavior:**
- Each edit session preserves all changes
- Changes accumulate across multiple edit sessions
- Final "Save" operation applies ALL changes from all edit sessions
- Audit trail tracks each edit session

**Example:**
```
Session 1:
  User edits PO Number: PO-123456 → PO-654321
  Returns to review

Session 2:
  User edits Load 2 weight: 10,000 → 12,000
  Returns to review

Session 3:
  User edits Heat Lot for all loads: HL-001 → HL-002
  Returns to review

Final Save:
  All three edits applied:
  • PO Number: PO-654321
  • Load 2 weight: 12,000
  • All heat lots: HL-002
```

## Browser Navigation Handling

### Browser Back Button

**Scenario:** User clicks browser back button while in Edit Mode

**Behavior:**
- Triggers unsaved changes warning
- Same dialog as "Closing Window in Edit Mode"
- Recommended: Disable browser back button during active workflow

### Window Close Event

**Scenario:** User clicks window close (X) button or Alt+F4

**Behavior:**
```javascript
window.addEventListener('beforeunload', (event) => {
    if (session.IsEditMode || session.HasUnsavedData) {
        event.preventDefault();
        event.returnValue = 'You have unsaved edits. Return to review to preserve changes or discard all workflow data?';
    }
});
```

## Edit Mode Scrolling and Navigation Assistance

### Auto-Scroll to Errors

**Scenario:** User returns to Step 2 (Edit Mode) to fix validation errors

**Behavior:**
- Grid automatically scrolls to first load with validation error
- Error row highlighted for 2-3 seconds
- Focus set to first invalid field

### Jump to Next Error

**Feature:** "Jump to Next Error" button available in Edit Mode

**Behavior:**
- Scrolls to next error in sequence
- Wraps to first error after last error
- Visual highlight on current error

## Session State Management

### Edit Mode Session Flags

```csharp
public class ReceivingSession
{
    // Edit Mode Flags
    public bool IsEditMode { get; set; }
    public string EditReturnTarget { get; set; } // "Step3"
    public DateTime? EditModeStarted { get; set; }
    public List<EditHistoryEntry> EditHistory { get; set; }
}

public class EditHistoryEntry
{
    public DateTime EditTimestamp { get; set; }
    public string EditedStep { get; set; } // "Step1" or "Step2"
    public Dictionary<string, FieldChange> Changes { get; set; }
}

public class FieldChange
{
    public string FieldName { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }
}
```

### Clearing Edit Mode

**When Edit Mode Cleared:**
- User clicks "Return to Review" and successfully navigates to Step 3
- User clicks "Save and Complete" on Step 3
- User explicitly exits workflow

**Actions on Clear:**
```csharp
session.IsEditMode = false;
session.EditReturnTarget = null;
session.EditModeStarted = null;
// EditHistory preserved for audit trail
```

## Testing Scenarios

### Test 1: Basic Edit from Review

**Given:** User on Step 3 (Review)  
**When:** User clicks "Edit Order & Part"  
**Expected:**
- Navigate to Step 1
- "Edit Mode" indicator shown
- "Next" button changed to "Return to Review"
- All data preserved  
**Result:** ✓ Success

### Test 2: Return to Review

**Given:** User in Step 1 (Edit Mode), changes PO Number  
**When:** User clicks "Return to Review"  
**Expected:**
- Navigate directly to Step 3
- Changed field highlighted for 3-5 seconds
- Edit Mode cleared  
**Result:** ✓ Success

### Test 3: Load Count Increase in Edit Mode

**Given:** User in Step 1 (Edit Mode), 5 loads exist  
**When:** User changes Load Count to 8  
**Expected:**
- Loads 1-5 preserved
- Loads 6-8 created (empty or auto-filled)
- Notification shown  
**Result:** ✓ Success

### Test 4: Load Count Decrease in Edit Mode

**Given:** User in Step 1 (Edit Mode), 5 loads exist  
**When:** User changes Load Count to 3  
**Expected:**
- Confirmation dialog shown
- On confirm: Loads 4-5 deleted
- On cancel: Load Count remains 5  
**Result:** ✓ Success

### Test 5: Validation Error in Edit Mode

**Given:** User in Step 1 (Edit Mode)  
**When:** User enters invalid data (empty PO Number)  
**Expected:**
- Validation error shown
- "Return to Review" button disabled
- Tooltip explains why disabled  
**Result:** ✓ Success

### Test 6: Multiple Edit Sessions

**Given:** User edits, returns to review, edits again  
**When:** User finally clicks "Save and Complete"  
**Expected:**
- All changes from all edit sessions applied
- Audit trail shows multiple edit entries  
**Result:** ✓ Success

## Related Documentation

- [Data Flow](../00-Core/data-flow.md) - Session state management
- [Guided Mode](../02-Workflow-Modes/001-wizardmode-specification.md) - 3-step workflow
- [Hub Orchestration](../02-Workflow-Modes/004-hub-orchestration-specification.md) - Mode selection and navigation
- [Bulk Copy Operations](./bulk-copy-operations.md) - Auto-fill in Edit Mode
- [Load Number Dynamics](./load-number-dynamics.md) - Load count changes
