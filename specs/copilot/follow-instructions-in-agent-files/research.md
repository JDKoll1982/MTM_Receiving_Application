# Research Document: Receiving Workflow Consolidation

**Feature**: Receiving Workflow Consolidation  
**Date**: 2026-01-24  
**Phase**: 0 - Research & Analysis

## Executive Summary

This document consolidates research findings for the receiving workflow consolidation project, which aims to reduce the wizard from 12 steps to 3 steps while maintaining all functionality and adding new bulk operation capabilities.

## Workflow Analysis

### Overview

The specification defines **9 distinct workflows** organized into 6 user stories:

1. **Workflow 1.1** (US1): Complete 3-Step Guided Workflow - PRIMARY FOUNDATION
2. **Workflows 2.1-2.4** (US2): Bulk copy operations with various options
3. **Workflow 3.1** (US3): Manual Entry Mode with AutoFill
4. **Workflow 4.1** (US4): Edit from Review Screen
5. **Workflow 5.1** (US5): Non-PO Receiving Path
6. **Workflow 6.1** (US6): Real-Time Validation Feedback

### Workflow Dependencies

```
1.1 (Foundation)
├── 2.1 (Bulk Copy)
│   ├── 2.2 (Change Source)
│   ├── 2.3 (Clear AutoFill)
│   └── 2.4 (Force Overwrite)
├── 3.1 (Manual Mode AutoFill)
├── 4.1 (Edit from Review)
├── 5.1 (Non-PO Path)
└── 6.1 (Real-Time Validation)
```

**Implementation Order**:
1. Start with 1.1 (Foundation) - ALL other workflows depend on it
2. Then implement 2.1 (Bulk Copy) - Required by 2.2, 2.3, 2.4
3. All others can be implemented in parallel after their dependencies

### Workflow Characteristics

| Workflow | Nodes | Decision Points | End States | Complexity |
|----------|-------|-----------------|------------|------------|
| 1.1 | 12 | 3 | 2 | High (Foundation) |
| 2.1 | 13 | 2 | 2 | High (Bulk Ops) |
| 2.2 | 6 | 0 | 1 | Low (UI Change) |
| 2.3 | 10 | 1 | 2 | Medium (Reversal) |
| 2.4 | 10 | 1 | 2 | Medium (Override) |
| 3.1 | 12 | 2 | 2 | Medium (Parallel) |
| 4.1 | 15 | 1 | 2 | High (Navigation) |
| 5.1 | 15 | 2 | 2 | High (Alt Path) |
| 6.1 | 9 | 2 | 1 | Medium (Enhancement) |

### UI Components Required

Based on workflow analysis, the following UI components are needed:

#### Step 1 (Order & Part Selection)
- **Form Fields**: PO Number entry, Part dropdown, Load count spinner
- **Validation**: Real-time PO lookup, Part availability check
- **Navigation**: Next button (enabled when valid)

#### Step 2 (Load Details Entry)
- **Data Grid**: Multi-row grid for all loads
  - Columns: Load #, Weight/Quantity, Heat Lot, Package Type, Packages Per Load
  - Row-level validation indicators
  - Cell highlighting for auto-filled data
- **Dropdown Buttons**:
  - "Copy to All Loads" with options:
    - Copy All Fields to All Loads
    - Copy Weight/Quantity Only
    - Copy Heat Lot Only
    - Copy Package Type Only
    - Copy Packages Per Load Only
  - "Copy Source" selector (default: Load 1)
  - "Clear Auto-Filled Data" with options:
    - Clear All Auto-Filled Fields
    - Clear Auto-Filled Weight Only
    - Clear Auto-Filled Heat Lot Only
    - etc.
- **Selection**: Multi-select loads (Shift+Click, Ctrl+Click)
- **Tooltips**: Error reasons, disabled button explanations
- **Notifications**: Toast messages for copy operations
- **Progress Indicators**: For large bulk operations (>50 loads)

#### Step 3 (Review & Save)
- **Summary Display**: Read-only grid showing all entered data
- **Indicators**: Auto-fill markers, edit capability
- **Actions**:
  - Edit Details button (returns to Step 2)
  - Save button (CSV + Database)
  - Cancel button

### State Management Needs

1. **Workflow State**:
   - Current step (1, 2, or 3)
   - Navigation history for edit mode
   - Edit mode flag

2. **Data State**:
   - PO information (number, part, vendor)
   - Load count
   - Per-load data (weight, heat lot, package type, packages per load)
   - Auto-fill markers per cell (tracks which cells were auto-filled)
   - Validation state per load and per field

3. **Copy Operation State**:
   - Selected copy source load (default: 1)
   - Last copy operation details (for undo/clear)
   - Selected loads for bulk operations

4. **Session State**:
   - Unsaved changes flag
   - Last successful save timestamp
   - Error state and messages

### Data Flow Patterns

1. **Forward Navigation** (Step 1 → Step 2 → Step 3):
   - Validate current step before advancing
   - Preserve all data
   - Initialize new loads with default values

2. **Backward Navigation** (Step 3 → Step 2, Step 2 → Step 1):
   - Preserve all entered data
   - Update load count if changed in Step 1
   - Maintain auto-fill markers

3. **Edit Mode** (Step 3 → Step 2 → Step 3):
   - Set edit mode flag
   - Navigate directly to Step 2 (skip Step 1)
   - Return directly to Step 3 (skip Step 1)
   - Highlight corrected fields

4. **Bulk Copy Operations**:
   - Read source load data
   - Identify empty cells in target loads
   - Apply data only to empty cells
   - Mark cells as auto-filled
   - Display summary notification

5. **Validation**:
   - Real-time validation on field blur
   - Aggregate validation before navigation
   - Per-load and per-field error tracking

### Error Handling Requirements

1. **Validation Errors**:
   - Display inline error messages with field
   - Show summary error count at top
   - Prevent navigation until resolved
   - Disable copy operations from invalid loads

2. **Save Errors**:
   - Preserve all entered data
   - Display error message with recovery options
   - Log error details for support
   - Offer retry mechanism

3. **Data Conflicts**:
   - Warn before overwriting (force overwrite scenario)
   - Confirm destructive operations (clear auto-fill)
   - Notify when preserving existing data

## Technical Decisions

### Decision 1: MVVM Architecture with CQRS

**Decision**: Use ViewModel pattern with MediatR for command/query separation  
**Rationale**:
- Aligns with constitution principle III (CQRS + Mediator First)
- Simplifies unit testing (mock IMediator)
- Clear separation of concerns
- Supports pipeline behaviors for validation/logging

**Alternatives Considered**:
- Direct service injection: Violates constitution
- Static service locator: Violates constitution

### Decision 2: WinUI 3 Data Grid for Step 2

**Decision**: Use WinUI 3 DataGrid with x:Bind for load details entry  
**Rationale**:
- Native WinUI component with good performance
- Supports multi-column editing
- Built-in selection and validation
- Aligns with constitution principle I (View Purity)

**Alternatives Considered**:
- Custom grid control: Over-engineering
- ItemsControl with templates: Less performant for 100+ loads

### Decision 3: Stored Procedures for All Database Operations

**Decision**: All database operations use stored procedures  
**Rationale**:
- Constitution principle II (Data Access Integrity)
- Security (no raw SQL injection risk)
- Performance (compiled execution plans)
- Easier to version and test

**Alternatives Considered**: None - constitution mandates this approach

### Decision 4: FluentValidation for Input Validation

**Decision**: Use FluentValidation for all command/query validation  
**Rationale**:
- Constitution principle V (Validation, Errors, Structured Logging)
- Constitution principle VII (Library-First Reuse)
- Testable validation logic
- Clear validation rules

**Alternatives Considered**:
- Data annotations: Less flexible
- Custom validators: Reinventing the wheel

### Decision 5: Auto-Fill Tracking with Metadata

**Decision**: Track auto-filled cells using separate metadata collection  
**Rationale**:
- Enables "Clear Auto-Filled Data" functionality
- Supports visual indicators in UI
- Allows selective clearing
- Minimal impact on core data model

**Alternatives Considered**:
- Flag in each load object: Pollutes domain model
- No tracking: Cannot implement clear auto-fill feature

## Technology Stack Clarifications

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**:
- WinUI 3 (Windows App SDK 1.6+)
- CommunityToolkit.Mvvm
- MediatR
- FluentValidation
- Serilog

**Storage**:
- MySQL 8.0 (mtm_receiving_application) - READ/WRITE
- SQL Server (Infor Visual) - READ ONLY

**Testing**: xUnit with FluentAssertions  
**Target Platform**: Windows 10/11 Desktop  
**Project Type**: WinUI 3 Desktop Application  
**Performance Goals**:
- UI responsiveness: < 100ms for all user interactions
- Bulk copy operations: < 1 second for up to 100 loads
- Save operations: < 3 seconds for complete workflow

**Constraints**:
- Read-only access to Infor Visual database
- Must support offline data entry (save to CSV)
- Must maintain backward compatibility with existing data format
- No internet connectivity required

**Scale/Scope**:
- Up to 100 loads per receiving transaction
- Concurrent users: 5-10
- Data retention: All historical records
- Estimated 50-100 transactions per day

## Implementation Strategy

### Phase 1 Priority: User Story 1 (Workflow 1.1)

Implement the foundation workflow first:
- Step 1: Order & Part Selection
- Step 2: Load Details Entry (basic grid, no bulk operations)
- Step 3: Review & Save

This provides immediate value and establishes the core architecture.

### Phase 2: Bulk Operations (Workflows 2.1-2.4)

Add efficiency features:
- Copy to All Loads dropdown
- Copy source selection
- Clear auto-filled data
- Force overwrite

### Phase 3: Additional Modes & Features (Workflows 3.1, 5.1)

- Manual Entry Mode
- Non-PO receiving path

### Phase 4: Enhancements (Workflows 4.1, 6.1)

- Edit from review screen
- Real-time validation feedback

## Open Questions

None - all technical context is clarified above.

## Next Steps

Proceed to Phase 1: Design & Contracts
- Generate data-model.md
- Create contracts/ for API specifications
- Generate quickstart.md for testing scenarios
