# Module_Receiving Workflow Consolidation Plan

## Executive Summary

This document outlines the plan to consolidate the Module_Receiving wizard workflow from **12 steps** down to **3 steps** while maintaining all functionality and improving user experience.

## Current Workflow Analysis

### Current Steps (12 total)
1. **ModeSelection** - Choose between Guided, Manual Entry, or Edit Mode
2. **ManualEntry** - Bulk grid entry mode
3. **EditMode** - Historical data editing mode
4. **POEntry** - Enter PO number or select Non-PO item
5. **PartSelection** - Select part from PO or search
6. **LoadEntry** - Enter number of loads
7. **WeightQuantityEntry** - Enter weight/quantity for each load
8. **HeatLotEntry** - Enter heat lot number for each load
9. **PackageTypeEntry** - Enter package type and count for each load
10. **Review** - Review all entered data
11. **Saving** - Save to CSV and database
12. **Complete** - Show save results

### Current Guided Workflow Path
```
ModeSelection → POEntry → PartSelection → LoadEntry → WeightQuantityEntry → HeatLotEntry → PackageTypeEntry → Review → Saving → Complete
```

## Proposed 3-Step Workflow

### Step 1: Order & Part Selection
**Consolidates:** POEntry + PartSelection + LoadEntry

**Purpose:** Collect all order and part identification information in one step.

**UI Components:**
- PO Number entry field (with Non-PO checkbox)
- Part search/selection (auto-populated if PO selected)
- Number of loads entry
- Part details display (Part ID, Type, PO Line Number)

**Validation:**
- PO Number required (unless Non-PO selected)
- Part selection required
- Number of loads must be ≥ 1

### Step 2: Load Details Entry
**Consolidates:** WeightQuantityEntry + HeatLotEntry + PackageTypeEntry

**Purpose:** Enter all load-specific details in a single, organized interface.

**UI Components:**
- Data grid or expandable sections for each load
- For each load:
  - Weight/Quantity field
  - Heat Lot Number field
  - Package Type dropdown
  - Packages Per Load field
- Bulk entry options (copy to all loads)
- Validation indicators per load

**Validation:**
- Weight/Quantity required and valid for each load
- Heat Lot Number required for each load
- Package Type required for each load
- Packages Per Load required and valid for each load

### Step 3: Review & Save
**Consolidates:** Review + Saving + Complete

**Purpose:** Review all data, save, and show results in one step.

**UI Components:**
- Summary table showing all loads with key details
- Edit buttons to return to previous steps
- Save button (triggers save process)
- Progress indicator during save
- Results display:
  - Success/failure status
  - CSV save status (Local/Network)
  - Database save status
  - Number of loads saved
- "Start New Entry" button after completion

**Validation:**
- All loads must be complete and valid
- Session validation before save

## Benefits of Consolidation

1. **Reduced Navigation:** Users complete workflow in 3 steps instead of 9
2. **Better Context:** Related information grouped together
3. **Faster Completion:** Less clicking between steps
4. **Improved UX:** Logical grouping of related data entry
5. **Maintained Functionality:** All validation and features preserved

## Technical Implementation Strategy

### Phase 1: Create New Consolidated Views
- `View_Receiving_OrderPartSelection.xaml` - Step 1
- `View_Receiving_LoadDetails.xaml` - Step 2
- `View_Receiving_ReviewSave.xaml` - Step 3

### Phase 2: Create New ViewModels
- `ViewModel_Receiving_OrderPartSelection.cs` - Step 1
- `ViewModel_Receiving_LoadDetails.cs` - Step 2
- `ViewModel_Receiving_ReviewSave.cs` - Step 3

### Phase 3: Update Workflow Service
- Modify `Service_ReceivingWorkflow` to support new 3-step flow
- Update `Enum_ReceivingWorkflowStep` enum
- Maintain backward compatibility during transition

### Phase 4: Update Navigation Logic
- Update step transition logic
- Update validation gates
- Update session persistence

### Phase 5: Testing & Migration
- Unit tests for new ViewModels
- Integration tests for workflow
- User acceptance testing
- Migration path for existing sessions

## Backward Compatibility

- Keep old step enums for migration period
- Support loading old session formats
- Provide migration utility if needed

## Risk Mitigation

1. **Data Loss:** Maintain session persistence throughout
2. **User Confusion:** Provide clear UI indicators and help text
3. **Validation Errors:** Show inline validation in consolidated steps
4. **Performance:** Optimize data grid rendering for multiple loads

## Success Metrics

- Reduction in average workflow completion time
- Reduction in user errors
- Improved user satisfaction scores
- Maintained data accuracy
