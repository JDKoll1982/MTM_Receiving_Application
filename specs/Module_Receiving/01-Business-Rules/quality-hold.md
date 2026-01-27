# Quality Hold

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Part Number Dynamics](./part-number-dynamics.md), [Data Flow](../00-Core/data-flow.md)

## Overview

Quality Hold is a special receiving procedure required for specific parts that must be inspected by Quality Control before acceptance. This rule defines the configuration, workflow triggers, user prompts, and current manual procedures.

## Concept

**Quality Hold:**
- Certain parts require mandatory Quality Control inspection before acceptance
- These parts are flagged with a Quality Hold requirement
- Receiving personnel must follow specific procedures when receiving these parts
- System prompts users and enforces acknowledgment of procedures

## Configuration

### Part-Level Quality Hold Flag

**Settings Path:**
```
Settings
  → Part Number Management
    → Select Part
      → Edit
        → Quality Hold (Checkbox)
```

**Configuration Options:**
- **☐ Quality Hold:** Part does NOT require Quality Hold (default for most parts)
- **☑ Quality Hold:** Part REQUIRES Quality Hold procedures

**Save Behavior:**
- Quality Hold flag stored in application settings database
- Applied automatically when part is entered in receiving workflow
- Can be changed at any time in Settings

### Settings Workflow Example

**Step 1: Navigate to Settings**
```
User: Settings → Part Number Management
```

**Step 2: Select Part**
```
User: Select MMC0003000 from dropdown or search
```

**Step 3: Configure Quality Hold**
```
Current Setting: ☐ Quality Hold (Not required)
User: Checks box ☑ Quality Hold
Reason: MMC0003000 requires inspection due to critical application
```

**Step 4: Save**
```
System: Saves MMC0003000 → Quality Hold: Required
```

**Step 5: Verify in Receiving**
```
Next time MMC0003000 is entered:
  → System detects Quality Hold requirement
  → Displays Quality Hold procedures
  → Requires user acknowledgment
```

## Workflow Triggers

### Prompt Conditions

Quality Hold procedures are displayed in the following modes:

**1. Wizard Mode (Guided Mode)**
- **Trigger Point:** After user enters Part Number
- **Action:** Display Quality Hold prompt dialog
- **Requirement:** User must acknowledge before proceeding

**2. Manual Mode**
- **Trigger Point:** After user enters Part Number
- **Action:** Display Quality Hold prompt dialog
- **Requirement:** User must acknowledge before proceeding to grid entry

**3. Edit Mode**
- **Trigger Point:** When changing from non-hold part to hold-required part
- **Action:** Display notification that part now requires Quality Hold
- **Note:** Edit Mode relies on user's judgment for next steps (not blocking)

### Prompt Workflow

**Standard Prompt (Wizard/Manual Mode):**

```
┌────────────────────────────────────────────────┐
│  ⚠ Quality Hold Required                      │
│ ────────────────────────────────────────────── │
│                                                │
│ Part: MMC0003000                               │
│                                                │
│ This part requires Quality Hold procedures.   │
│ Please follow the Quality Hold procedures     │
│ before accepting this material.                │
│                                                │
│ ┌────────────────────────────────────────┐   │
│ │ Current Procedures:                     │   │
│ │                                         │   │
│ │ Pre-Receive:                            │   │
│ │ • Purchasing informs Quality Control    │   │
│ │   and Receiving of incoming material    │   │
│ │                                         │   │
│ │ On Delivery:                            │   │
│ │ 1. Stop - Call Quality Control          │   │
│ │ 2. Do NOT unload until QC arrives       │   │
│ │ 3. QC inspects material before acceptance│  │
│ │ 4. Do NOT sign BOL until QC releases    │   │
│ │ 5. Proceed with normal receiving after  │   │
│ │    QC approval                          │   │
│ └────────────────────────────────────────┘   │
│                                                │
│ ☑ I understand and will follow these          │
│   procedures                                   │
│                                                │
│          [Cancel]  [Acknowledge & Continue]   │
└────────────────────────────────────────────────┘
```

**Edit Mode Notification:**

```
┌────────────────────────────────────────────────┐
│  ℹ Quality Hold Requirement Changed            │
│ ────────────────────────────────────────────── │
│                                                │
│ Part: MMC0003000                               │
│                                                │
│ This part now requires Quality Hold.          │
│ Future receiving of this part will require    │
│ following Quality Hold procedures.            │
│                                                │
│ For current edit: Use your judgment to        │
│ determine appropriate next steps.              │
│                                                │
│                      [OK]                      │
└────────────────────────────────────────────────┘
```

## Current Quality Hold Procedures

### Pre-Receive Date (Purchasing Department)

**Responsible:** Purchasing Department

**Steps:**
1. **Customer Notification**
   - Customer informs Purchasing of incoming Quality Hold material
   - Customer provides: Part Number, PO Number, Expected Delivery Date

2. **Internal Communication**
   - Purchasing informs Quality Control department
   - Purchasing informs Receiving department

3. **Documentation (Receiving)**
   - Receiving updates "Incoming Quality Hold" log/section
   - Required Information:
     - Part Number
     - PO Number
     - Expected Delivery Date
     - Supplier Name

**Example Log Entry:**
```
┌───────────────────────────────────────────────────────────┐
│ Incoming Quality Hold Log                                 │
│ ─────────────────────────────────────────────────────────│
│ Part Number:     MMC0003000                               │
│ PO Number:       PO-123456                                │
│ Expected Date:   2026-01-28                               │
│ Supplier:        Acme Steel Co.                           │
│ Logged By:       Purchasing Dept.                         │
│ Logged Date:     2026-01-25 10:30 AM                      │
│ Status:          Pending Delivery                         │
└───────────────────────────────────────────────────────────┘
```

### Date of Delivery (Receiving Department)

**Responsible:** Receiving Clerk

**Steps:**

**1. Stop Unloading**
- Truck arrives with Quality Hold material
- **STOP immediately** - Do NOT begin unloading
- Verify part is on "Incoming Quality Hold" log

**2. Call Quality Control**
- Contact Quality Control department immediately
- Provide: Part Number, PO Number, Supplier, Truck ID
- Request QC inspector to meet at receiving dock

**3. Meet Quality Control**
- QC inspector arrives at designated unloading location
- QC inspector confirms material identity
- QC inspector prepares for inspection

**4. Quality Inspection**
- QC inspector performs material inspection
- Inspection may include:
  - Visual inspection
  - Dimensional verification
  - Material testing (if required)
  - Documentation review
- QC inspector makes acceptance decision

**5. Material Release Decision**

**If APPROVED:**
- QC inspector releases material for acceptance
- Receiving clerk may NOW sign Packing Slip/BOL
- Receiving clerk proceeds with normal receiving process
- Material is received into inventory

**If REJECTED:**
- QC inspector rejects material
- Receiving clerk does NOT sign Packing Slip/BOL
- Material is returned to carrier
- Purchasing is notified of rejection

**6. Complete Receiving (If Approved)**
- Follow standard receiving procedures
- Generate receiving labels
- Enter data into receiving system (this application)
- Store material in designated location
- Update "Incoming Quality Hold" log status to "Completed"

## System Integration

### Quality Hold Check Logic

**Pseudocode:**
```csharp
public async Task<bool> CheckQualityHold(string partNumber)
{
    // 1. Query Settings for Quality Hold flag
    var isQualityHold = await SettingsService.GetQualityHoldFlag(partNumber);
    
    // 2. If Quality Hold required
    if (isQualityHold)
    {
        // 3. Display Quality Hold prompt
        var acknowledged = await ShowQualityHoldPrompt(partNumber);
        
        // 4. Return acknowledgment status
        return acknowledged;
    }
    
    // 5. If not Quality Hold, proceed normally
    return true;
}
```

### Prompt Display Logic

**Wizard Mode / Manual Mode:**
```csharp
private async Task<bool> ShowQualityHoldPrompt(string partNumber)
{
    // 1. Create dialog with procedures
    var dialog = new QualityHoldDialog
    {
        PartNumber = partNumber,
        Procedures = GetCurrentProcedures(),
        Title = "Quality Hold Required"
    };
    
    // 2. Show dialog and wait for user response
    var result = await dialog.ShowAsync();
    
    // 3. Return true if acknowledged, false if canceled
    return result == DialogResult.Acknowledged;
}
```

**Edit Mode:**
```csharp
private async Task ShowQualityHoldNotification(string partNumber)
{
    // 1. Create notification dialog
    var notification = new QualityHoldNotification
    {
        PartNumber = partNumber,
        Message = "This part now requires Quality Hold. Future receiving will require procedures.",
        Title = "Quality Hold Requirement Changed"
    };
    
    // 2. Show notification (non-blocking)
    await notification.ShowAsync();
}
```

## User Interface Requirements

### Quality Hold Indicator (Receiving Workflow)

**Part Information Display:**
```
┌─────────────────────────────────────────┐
│ Part Information                        │
│ ─────────────────────────────────────── │
│ Part Number:     MMC0003000             │
│ Description:     Steel Coil, 0.100" Thick│
│ Part Type:       Coils                  │
│ Default Location: V-C0-01               │
│ Quality Hold:    ⚠ YES (Required)       │  ← Indicator
└─────────────────────────────────────────┘
```

**Visual Indicators:**
- **Not Required:** "No" (normal text, no icon)
- **Required:** "⚠ YES (Required)" (yellow warning icon, bold text)

### Quality Hold Dialog Components

**Required Elements:**
- Part Number display
- Warning icon (⚠)
- Clear title: "Quality Hold Required"
- Full procedure text (scrollable if needed)
- Acknowledgment checkbox: "I understand and will follow these procedures"
- Cancel button
- Acknowledge button (disabled until checkbox checked)

## Validation and Business Rules

### Acknowledgment Requirement

**Wizard Mode / Manual Mode:**
- **Required:** Yes (user must acknowledge)
- **Blocking:** Yes (cannot proceed without acknowledgment)
- **Cancellation:** User can cancel and return to previous step

**Edit Mode:**
- **Required:** No (informational only)
- **Blocking:** No (user judgment)
- **Rationale:** Historical data modification; procedures already completed

### Configuration Validation

**Settings:**
- Any part can be flagged for Quality Hold
- No validation on part type or prefix
- Change takes effect immediately
- No approval required to enable/disable Quality Hold flag

## Edge Cases

### Case 1: User Cancels Quality Hold Prompt

**Scenario:** User sees Quality Hold prompt and clicks "Cancel"

**Behavior:**
- Return to Part Number entry screen
- Part Number field cleared
- User must enter different part or acknowledge procedures

### Case 2: Quality Hold Flag Changed Mid-Session

**Scenario:** User has part open in session, admin changes Quality Hold flag in Settings

**Behavior:**
- Current session: No immediate effect (flag checked at part entry only)
- Next session: New flag value applies
- Edit Mode: Show notification if viewing historical transaction

### Case 3: Quality Hold Part in Edit Mode

**Scenario:** User editing historical transaction for Quality Hold part

**Behavior:**
- Show notification: "This part requires Quality Hold"
- Do NOT block editing (material already accepted/rejected)
- User judgment for next steps

### Case 4: Multiple Quality Hold Parts in Same Session

**Scenario:** User enters multiple different Quality Hold parts in one session

**Behavior:**
- Prompt shown for EACH Quality Hold part
- User must acknowledge for each part separately
- No session-level "always acknowledge" option

## Audit and Compliance

### Quality Hold Acknowledgment Logging

**Log Entry:**
```
Event: Quality Hold Acknowledged
Part Number: MMC0003000
PO Number: PO-123456
User: JDoe
Timestamp: 2026-01-25 14:30:00
Action: User acknowledged Quality Hold procedures
Session ID: {GUID}
```

**Purpose:**
- Compliance tracking
- Audit trail
- Verification that procedures were followed

## Testing Scenarios

### Test 1: Quality Hold Part Entry (Wizard Mode)

**Given:**
- Part MMC0003000 flagged for Quality Hold in Settings
- User in Wizard Mode Step 1

**Expected:**
1. User enters Part Number: MMC0003000
2. System detects Quality Hold requirement
3. Quality Hold dialog appears
4. User must check acknowledgment box
5. User clicks "Acknowledge & Continue"
6. Workflow proceeds to next step

### Test 2: Quality Hold Cancellation

**Given:**
- Quality Hold dialog displayed
- User does NOT want to proceed

**Expected:**
1. User clicks "Cancel"
2. Dialog closes
3. Part Number field cleared
4. User remains on Part Number entry screen

### Test 3: Non-Quality Hold Part

**Given:**
- Part MMC0001000 NOT flagged for Quality Hold
- User in Wizard Mode Step 1

**Expected:**
1. User enters Part Number: MMC0001000
2. System checks Quality Hold flag (not required)
3. No Quality Hold dialog shown
4. Workflow proceeds normally

### Test 4: Edit Mode Quality Hold Notification

**Given:**
- Part MMC0003000 flagged for Quality Hold
- User loading historical transaction in Edit Mode

**Expected:**
1. User loads transaction for MMC0003000
2. System detects Quality Hold requirement
3. Notification dialog appears (informational only)
4. User clicks "OK"
5. Edit Mode workflow continues normally

### Test 5: Multiple Quality Hold Parts

**Given:**
- Parts MMC0003000 and MMC0004000 both flagged for Quality Hold
- User entering multiple parts in same session

**Expected:**
1. User enters MMC0003000 → Quality Hold prompt → User acknowledges
2. User enters MMC0004000 → Quality Hold prompt → User acknowledges
3. Each part requires separate acknowledgment

## Related Documentation

- [Part Number Dynamics](./part-number-dynamics.md) - Part number entry triggers Quality Hold check
- [Data Flow](../00-Core/data-flow.md) - Quality Hold in workflow
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md) - Quality Hold in Wizard workflow
- [Manual Entry Mode Specification](../02-Workflow-Modes/003-manual-mode-specification.md) - Quality Hold in Manual Mode
- [Edit Mode Specification](../02-Workflow-Modes/002-editmode-specification.md) - Quality Hold in Edit Mode
