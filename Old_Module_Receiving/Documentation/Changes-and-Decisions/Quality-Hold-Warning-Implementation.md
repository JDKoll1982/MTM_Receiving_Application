# Quality Hold Warning Implementation

## Summary

Implemented immediate quality hold warnings when restricted parts (MMFSR/MMCSR) are entered in receiving DataGrids. The warning now appears as soon as the user moves to another cell/row, providing instant feedback about quality inspection requirements.

## Changes Made

### 1. New Service: `IService_QualityHoldWarning`

**File:** `Module_Receiving\Contracts\IService_QualityHoldWarning.cs`

- Interface defining quality hold warning functionality
- Methods:
  - `CheckAndWarnAsync()` - Shows warning dialog and returns user acknowledgment
  - `IsRestrictedPart()` - Checks if a part ID requires quality hold

### 2. Service Implementation: `Service_QualityHoldWarning`

**File:** `Module_Receiving\Services\Service_QualityHoldWarning.cs`

- Detects MMFSR (sheet material) and MMCSR (coil material) parts
- Displays prominent warning dialog with:
  - Part ID and restriction type
  - Critical instructions to contact Quality
  - Reminder not to sign paperwork until Quality accepts
  - Options to acknowledge or cancel entry
- Updates `Model_ReceivingLoad` properties when restricted part detected
- Logs all quality hold interactions

### 3. Dependency Injection Registration

**File:** `Infrastructure\DependencyInjection\ModuleServicesExtensions.cs`

- Registered `IService_QualityHoldWarning` as Singleton
- Available throughout the Receiving module

### 4. View Updates: Manual Entry

**Files:**
- `Module_Receiving\Views\View_Receiving_ManualEntry.xaml`
- `Module_Receiving\Views\View_Receiving_ManualEntry.xaml.cs`

**XAML Changes:**
- Fixed `DataGridTextColumn.Foreground` property issue (not supported)
- Converted PartID column to `DataGridTemplateColumn` with custom cell templates
- Added `Foreground` binding to `TextBlock` within template for conditional red text

**Code-Behind Changes:**
- Injected `IService_QualityHoldWarning` service
- Added `_lastCheckedPartID` field to prevent duplicate warnings
- Implemented `CheckQualityHoldOnCellChangeAsync()` method:
  - Triggered on `CurrentCellChanged` event
  - Checks if PartID contains MMFSR or MMCSR
  - Shows warning dialog immediately when moving to another cell
  - If user cancels, clears the PartID and refocuses the cell
  - If user acknowledges, updates load's quality hold flags

### 5. View Updates: Edit Mode

**Files:**
- `Module_Receiving\Views\View_Receiving_EditMode.xaml`
- `Module_Receiving\Views\View_Receiving_EditMode.xaml.cs`

**XAML Changes:**
- Added quality hold converters to resources
- Converted PartID column to `DataGridTemplateColumn` with conditional formatting
- Same red text highlighting for restricted parts

**Code-Behind Changes:**
- Same quality hold checking logic as Manual Entry
- Injected `IService_QualityHoldWarning` service
- Implemented identical `CheckQualityHoldOnCellChangeAsync()` behavior

## Behavior

### When User Enters Restricted Part:

1. **User types** MMFSR or MMCSR part ID in PartID cell
2. **User moves** to another cell/row (Tab, Enter, or mouse click)
3. **Immediate warning** appears with critical quality hold information
4. **User must choose:**
   - **"I Understand - Contact Quality"** → Part ID kept, load marked as quality hold required
   - **"Cancel Entry"** → Part ID cleared, focus returns to PartID cell for correction

### Visual Indicators:

- **Red text color** in PartID column for MMFSR/MMCSR parts (immediate visual feedback)
- **Light red background** on entire row (via existing `LoadingRow` event handler)

### Save Behavior:

- Original save-time quality hold confirmation dialog **still works**
- Acts as final safety check before database save
- Complements the immediate cell-level warnings

## Consistency Across Module

This implementation ensures consistent quality hold warning behavior across:

- **Manual Entry** view (new receiving entries)
- **Edit Mode** view (editing historical entries)

Both views now:
- Show the same warning dialog
- Use the same visual indicators (red text/background)
- Have the same user interaction flow
- Share the same underlying service logic

## Testing Checklist

- [ ] Enter MMFSR part in Manual Entry → Warning appears on cell change
- [ ] Enter MMCSR part in Manual Entry → Warning appears on cell change
- [ ] Cancel warning → PartID clears, focus returns to cell
- [ ] Acknowledge warning → PartID kept, load marked as quality hold
- [ ] Enter normal part → No warning, normal flow
- [ ] Edit Mode: Same behaviors as Manual Entry
- [ ] Save with quality hold parts → Final confirmation dialog still works
- [ ] Visual indicators: Red text appears for restricted parts
- [ ] No duplicate warnings for the same part ID value

## Files Created

1. `Module_Receiving\Contracts\IService_QualityHoldWarning.cs`
2. `Module_Receiving\Services\Service_QualityHoldWarning.cs`

## Files Modified

1. `Infrastructure\DependencyInjection\ModuleServicesExtensions.cs`
2. `Module_Receiving\Views\View_Receiving_ManualEntry.xaml`
3. `Module_Receiving\Views\View_Receiving_ManualEntry.xaml.cs`
4. `Module_Receiving\Views\View_Receiving_EditMode.xaml`
5. `Module_Receiving\Views\View_Receiving_EditMode.xaml.cs`

## Architecture Notes

- Service-based approach allows easy addition to other views
- Reusable across entire Receiving module
- Logging built-in for audit trail
- No breaking changes to existing code
- Follows MVVM pattern with separation of concerns
