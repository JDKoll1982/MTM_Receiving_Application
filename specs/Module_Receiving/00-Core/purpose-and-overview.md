# Module Receiving - Purpose and Overview

**Category**: Core Specification  
**Last Updated**: 2026-01-25  
**Related Documents**: [Data Flow](./data-flow.md), [Workflow Modes](../02-Workflow-Modes/)

## Purpose

Module_Receiving is the primary application for entering and managing data to generate labels for received goods at manufacturing facilities.

### Core Functionality

The module provides three distinct workflow modes to accommodate different user needs and receiving scenarios:

1. **Module_Receiving.WizardMode (Guided Mode)**
   - Guided data entry flow (≤3 steps)
   - Step-by-step workflow for new receiving labels
   - Best for: New users, occasional receiving, standard transactions
   - Optimized for clarity and ease of use

2. **Module_Receiving.ManualMode**
   - Grid-based manual entry interface
   - Spreadsheet-style bulk data operations
   - Best for: Power users, high-volume receiving (50+ loads)
   - Optimized for speed and efficiency

3. **Module_Receiving.EditMode**
   - Search, view, and modify historical transaction data
   - Full audit trail tracking
   - Re-export capability for label reprinting
   - Best for: Supervisors, data correction, auditing
   - Optimized for data integrity and compliance

## Key Features

### Label Generation
- Creates receiving labels with all required information
- Supports multiple loads per PO line
- Handles complex load composition scenarios
- Generates standardized CSV output for downstream systems

### Data Validation
- Real-time validation during data entry
- Comprehensive business rule enforcement
- Prevents invalid data from being saved
- Provides clear error feedback to users

### Flexible Workflows
- Mode selection based on user preference and use case
- Seamless switching between modes
- Consistent validation across all modes
- Unified session management

### Audit and Compliance
- Complete audit trail for all transactions
- Modification history tracking in Edit Mode
- User and timestamp tracking
- Supports regulatory compliance requirements

## User Personas

### Standard Receiving Clerk
- Uses Guided Mode for daily receiving tasks
- Processes 5-20 loads per transaction
- Benefits from step-by-step guidance
- Occasional user who values clarity

### Power User / High-Volume Clerk
- Uses Manual Entry Mode for bulk receiving
- Processes 50+ loads per transaction
- Proficient with keyboard shortcuts
- Values speed and efficiency over guidance

### Supervisor / Data Corrector
- Uses Edit Mode to correct historical data
- Reviews and modifies completed transactions
- Maintains data quality and compliance
- Requires full audit trail visibility

### Quality Control
- Monitors Quality Hold procedures
- Reviews hold status during receiving
- Ensures compliance with inspection requirements
- Works closely with Receiving team

## Integration Points

### Infor Visual (ERP System)
- Part number validation
- Default receiving locations (auto-pull)
- PO number validation
- Read-only access (no write operations)

### CSV Export System
- Network CSV file generation
- Label printing integration
- Standardized format for downstream systems
- Re-export capability for corrections

### Local History Database
- Transaction persistence
- Audit trail storage
- Historical data queries
- Edit Mode data source

### Settings Module
- Part number management
- Default receiving locations
- Quality Hold configuration
- Part type assignments

## Success Metrics

### Efficiency
- Guided Mode: Complete 3-load transaction in <5 minutes
- Manual Entry Mode: Complete 50-load transaction in <3 minutes
- Edit Mode: Search and load transaction in <5 seconds

### Data Quality
- 100% validation success before save
- 0% invalid data persisted
- Complete audit trail for all modifications

### User Adoption
- 80%+ of power users adopt keyboard shortcuts within 1 week
- Successful mode selection on first use
- Minimal support requests after initial training

## Out of Scope

The following items are explicitly NOT part of Module_Receiving:

- **Direct ERP Integration**: No write operations to Infor Visual
- **Automated Label Printing**: CSV generation only; printing handled by external system
- **Inventory Management**: Receiving data entry only; inventory tracking handled by ERP
- **Purchase Order Creation**: PO data is read-only; creation handled by purchasing system
- **Supplier Management**: Part and supplier data managed in separate system
- **Advanced Analytics**: Basic reporting only; complex analytics handled by BI tools

## Related Documentation

- [Data Flow](./data-flow.md) - Complete data flow architecture
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md) - Full Guided Mode details
- [Manual Entry Mode Specification](../02-Workflow-Modes/003-manual-mode-specification.md) - Full Manual Mode details
- [Edit Mode Specification](../02-Workflow-Modes/002-editmode-specification.md) - Full Edit Mode details
- [Hub Orchestration](../02-Workflow-Modes/004-hub-orchestration-specification.md) - Mode selection and navigation
- [Business Rules](../01-Business-Rules/) - Complete business rule specifications
