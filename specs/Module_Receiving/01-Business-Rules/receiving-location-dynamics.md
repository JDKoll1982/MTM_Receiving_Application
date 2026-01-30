# Receiving Location Dynamics

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Part Number Dynamics](./part-number-dynamics.md), [Data Flow](../00-Core/data-flow.md)

## Overview

Receiving Locations specify where incoming materials are stored upon receipt. This rule defines the auto-population behavior, default fallbacks, session-based overrides, and Settings integration for managing location defaults.

## Concept

**Default Receiving Location ("Auto-Pull"):**
- Every inventoried part in Infor Visual has an expected default receiving location
- Location is automatically populated when user enters a valid part number
- User may override the location during receiving
- Overrides persist within the current session for that specific part

## Default Location Sources

### Primary Source: Infor Visual (ERP)

**Query Path:**
1. User enters Part Number (e.g., `MMC0001000`)
2. System queries Infor Visual Part Master table
3. Retrieves `DefaultReceivingLocation` field
4. Auto-populates Receiving Location field

**Example:**
```
Part Number: MMC0001000
Default Location (from ERP): V-C0-01
Result: Receiving Location auto-populated with "V-C0-01"
```

### Secondary Source: Application Settings

**Override Path:**
1. If part has custom location set in Settings → Part Number Management
2. Settings override takes precedence over ERP default
3. Auto-populate with Settings value

**Settings Configuration:**
```
Settings → Part Number Management
  → Select Part: MMC0001000
    → Edit
      → Default Receiving Location: V-C0-02 (Custom Override)
      → Save
```

**Result:**
```
Part Number: MMC0001000
Default Location (from Settings): V-C0-02 (overrides ERP)
Result: Receiving Location auto-populated with "V-C0-02"
```

### Fallback: "RECV" (Missing Default)

**Fallback Condition:**
- Part not found in Infor Visual, OR
- Part exists but DefaultReceivingLocation is null/empty

**Fallback Action:**
```
Part Number: MMC9999999
Default Location (from ERP): Not found
Fallback: RECV
Result: Receiving Location auto-populated with "RECV"
```

**Visual Indicator:**
- Show warning icon next to "RECV"
- Tooltip: "Default location not found. Using fallback location RECV."

## Auto-Populate Behavior

### Initial Population

**On Part Entry (Wizard or Manual Mode):**

**Step 1:** User enters Part Number  
**Step 2:** System queries location sources (Settings → ERP → Fallback)  
**Step 3:** Auto-populate Receiving Location field  
**Step 4:** Display location with source indicator

**Example Flow:**
```
1. User types: MMC0001000
2. System queries: Settings (none), then ERP (V-C0-01)
3. Auto-populate: V-C0-01
4. Display: "V-C0-01" (from ERP)
```

### Session-Based Override Persistence

**Session Scope:**
- A "session" is the current receiving workflow (Guided or Manual Mode)
- Session begins when user enters Mode Selection
- Session ends when user:
  - Completes and saves transaction, OR
  - Returns to Mode Selection, OR
  - Exits application

**Override Rule:**
If user changes Receiving Location during the current session, retain the user's last choice for that specific part rather than overwriting with default.

#### Override Example Session

**Step 1: First Part Entry**
```
User enters: MMC0001000
System auto-populates: V-C0-01 (from ERP default)
User accepts: V-C0-01
```

**Step 2: User Overrides Location**
```
User changes location to: RECV (manual override)
System marks: MMC0001000 → RECV (session override)
```

**Step 3: Different Part Entry**
```
User enters: MMC0002000
System auto-populates: V-C0-02 (from ERP default for this part)
User accepts: V-C0-02
```

**Step 4: Re-Enter First Part**
```
User re-enters: MMC0001000
System recalls session override: RECV (NOT V-C0-01)
Auto-populate: RECV (session override takes precedence)
```

**Step 5: Session Ends**
```
User completes transaction and starts new entry
Session overrides cleared
Next time MMC0001000 is entered: V-C0-01 (fresh default)
```

### Multi-Part Session Example (Comprehensive)

**Scenario:** User receiving multiple parts in single session

```
Action 1: Enter MMC0001000
  → Auto: V-C0-01 (default)
  → User accepts: V-C0-01

Action 2: User manually changes to RECV
  → Session stores: MMC0001000 → RECV

Action 3: Enter MMC0002000
  → Auto: V-C0-02 (default for this part)
  → User accepts: V-C0-02

Action 4: Re-enter MMC0001000 (same session)
  → Auto: RECV (session override, NOT V-C0-01)
  → User accepts: RECV

Action 5: Enter MMC0003000
  → Auto: V-C0-03 (default)
  → User changes to: RECV
  → Session stores: MMC0003000 → RECV

Action 6: Re-enter MMC0002000 (same session)
  → Auto: V-C0-02 (no override for this part)
  → User accepts: V-C0-02

Action 7: Complete transaction, start new session
  → Session overrides cleared

Action 8: Enter MMC0001000 (new session)
  → Auto: V-C0-01 (fresh default, override forgotten)
```

## Settings Integration

### Part Number Management

**Access Path:**
```
Settings
  → Part Number Management
    → Select Part Number (dropdown or search)
      → Edit
```

**Configurable Fields:**
- **Default Receiving Location** (text input)
  - Validate against known Infor Visual locations
  - If invalid, show warning but allow save
  - Override ERP default when set

**Validation:**
```csharp
public async Task<bool> ValidateReceivingLocation(string location)
{
    // Query Infor Visual for valid locations
    var validLocations = await GetValidLocationsFromERP();
    
    if (!validLocations.Contains(location))
    {
        ShowWarning($"Location '{location}' not found in Infor Visual. Save anyway?");
        return await UserConfirms();
    }
    
    return true;
}
```

### Settings Workflow

**Step 1: Navigate to Settings**
```
User: Settings → Part Number Management
```

**Step 2: Select Part**
```
User: Select MMC0001000 from dropdown
```

**Step 3: Edit Default Location**
```
Current Default: V-C0-01 (from ERP)
User changes to: V-C0-05
System validates: V-C0-05 exists in ERP ✓
Save: Success
```

**Step 4: Verify Override**
```
Next receiving entry for MMC0001000:
  → Auto-populate: V-C0-05 (Settings override)
  → ERP default V-C0-01 is ignored
```

## Receiving Location Validation

### Valid Location Format

**Infor Visual Location Pattern:**
- Format varies by facility
- Common patterns:
  - `V-C0-01` (Vertical storage, Column 0, Row 01)
  - `RECV` (General receiving area)
  - `QC-HOLD` (Quality Control hold area)
  - `SHIP` (Shipping area)

**Validation Rules:**
- Max length: 20 characters
- Valid characters: A-Z, 0-9, hyphen (-)
- Case-insensitive (stored as uppercase)
- No special characters except hyphen

### Location Existence Validation (Optional)

**Query Infor Visual:**
```sql
SELECT LocationCode
FROM LocationMaster
WHERE LocationCode = @LocationInput
  AND Active = 1
```

**If Not Found:**
- Show warning: "Location not found in Infor Visual. Verify location code."
- Allow user to:
  - Correct the location
  - Proceed anyway (with warning logged)

## User Interface Requirements

### Receiving Location Field

**Display Properties:**
- **Label:** "Receiving Location"
- **Placeholder:** "Auto-populated from part default"
- **Max Length:** 20 characters
- **Auto-Complete:** Suggest valid locations from ERP
- **Case Conversion:** Auto-convert to uppercase on blur

**Visual Indicators:**
- **Default (ERP):** Normal text, no icon
- **Default (Settings Override):** Blue info icon, tooltip "Custom default from Settings"
- **Fallback (RECV):** Yellow warning icon, tooltip "Default not found. Using RECV."
- **Session Override:** Green checkmark icon, tooltip "Custom location (session override)"

### Location Source Tooltip

**On Hover:**
```
Source: Infor Visual (ERP)
Default Location: V-C0-01
Override in Settings: Settings → Part Number Management
```

**After Manual Change:**
```
Custom Location: RECV
Original Default: V-C0-01
This override applies to this session only.
To change permanently: Settings → Part Number Management
```

## Edge Cases

### Case 1: Part Not in ERP

**Scenario:** Part MMC9999999 doesn't exist in Infor Visual

**Behavior:**
- Auto-populate: RECV (fallback)
- Show warning: "Part not found in ERP. Using fallback location RECV."
- User can manually change location

### Case 2: ERP Default is Empty

**Scenario:** Part exists but `DefaultReceivingLocation` field is NULL

**Behavior:**
- Auto-populate: RECV (fallback)
- Show warning: "Default location not set for this part. Using RECV."
- Suggest: "Set default in Settings → Part Number Management"

### Case 3: Settings Override Doesn't Exist in ERP

**Scenario:** User sets custom default "CUSTOM-01" in Settings, but location doesn't exist in ERP

**Behavior:**
- Auto-populate: CUSTOM-01 (Settings override takes precedence)
- Show warning: "Location CUSTOM-01 not found in ERP. Verify location code."
- Allow user to proceed or change

### Case 4: Multiple Session Overrides

**Scenario:** User overrides locations for 5 different parts in same session

**Behavior:**
- Session cache stores all 5 overrides
- Each part retains its specific override
- All overrides cleared when session ends

### Case 5: Session Ends Before Save

**Scenario:** User overrides location, then exits without saving

**Behavior:**
- Session overrides are lost
- Next session starts fresh with defaults
- No persistence of unsaved overrides

## Implementation Notes

### Session Override Storage

**Data Structure:**
```csharp
public class ReceivingSession
{
    public Dictionary<string, string> LocationOverrides { get; set; } = new();
    
    public void SetLocationOverride(string partNumber, string location)
    {
        LocationOverrides[partNumber] = location;
    }
    
    public string GetLocationForPart(string partNumber, string defaultLocation)
    {
        return LocationOverrides.TryGetValue(partNumber, out var override) 
            ? override 
            : defaultLocation;
    }
    
    public void ClearOverrides()
    {
        LocationOverrides.Clear();
    }
}
```

### Auto-Populate Logic

**Pseudocode:**
```csharp
public async Task<string> GetReceivingLocationForPart(string partNumber)
{
    // 1. Check session override first
    if (Session.LocationOverrides.ContainsKey(partNumber))
    {
        return Session.LocationOverrides[partNumber];
    }
    
    // 2. Check Settings override
    var settingsOverride = await SettingsService.GetLocationOverride(partNumber);
    if (!string.IsNullOrEmpty(settingsOverride))
    {
        return settingsOverride;
    }
    
    // 3. Query ERP default
    var erpDefault = await ERPService.GetDefaultReceivingLocation(partNumber);
    if (!string.IsNullOrEmpty(erpDefault))
    {
        return erpDefault;
    }
    
    // 4. Fallback to RECV
    return "RECV";
}
```

## Testing Scenarios

### Test 1: Standard ERP Default

**Given:**
- Part MMC0001000 exists in ERP
- ERP DefaultReceivingLocation: V-C0-01
- No Settings override
- No session override

**Expected:**
- Auto-populate: V-C0-01
- Source indicator: "From Infor Visual (ERP)"

### Test 2: Settings Override

**Given:**
- Part MMC0001000 exists in ERP
- ERP DefaultReceivingLocation: V-C0-01
- Settings override: V-C0-05
- No session override

**Expected:**
- Auto-populate: V-C0-05
- Source indicator: "Custom default from Settings"

### Test 3: Session Override

**Given:**
- Part MMC0001000 exists in ERP
- ERP DefaultReceivingLocation: V-C0-01
- User previously changed to RECV in current session
- Session override: RECV

**Expected:**
- Auto-populate: RECV
- Source indicator: "Custom location (session override)"

### Test 4: Fallback (Part Not Found)

**Given:**
- Part MMC9999999 does NOT exist in ERP
- No Settings override
- No session override

**Expected:**
- Auto-populate: RECV
- Warning: "Part not found in ERP. Using fallback location RECV."

### Test 5: Session Override Cleared on New Session

**Given:**
- Previous session: User changed MMC0001000 to RECV
- Previous session ended (transaction saved)
- New session started

**Expected:**
- Auto-populate: V-C0-01 (ERP default, override forgotten)
- Source indicator: "From Infor Visual (ERP)"

## Related Documentation

- [Part Number Dynamics](./part-number-dynamics.md) - Part number entry triggers location lookup
- [Data Flow](../00-Core/data-flow.md) - Location data in transaction flow
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md)
- [Manual Entry Mode Specification](../02-Workflow-Modes/003-manual-mode-specification.md)
- [Hub Orchestration](../02-Workflow-Modes/004-hub-orchestration-specification.md) - Session management
